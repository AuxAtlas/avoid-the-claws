#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Events.Networking;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets;
using AvoidClaws.code.dotnet.Networking.Packets.State;
using AvoidClaws.code.dotnet.Services;
using Godot;
using LiteNetLib;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Glue.Managers;

public partial class NetworkManager : Node, IService
{
	private const uint NetworkTicksPerSecond = 60;
	private const double MaxSlewAdjust = 0.002d;
	public const uint MaxTickSequence = 512;
	private const int DesiredTicksBuffer = 3;

	public static double TickDeltaTime => 1d / NetworkTicksPerSecond;
	public static float TickDeltaTimeF => 1f / NetworkTicksPerSecond;

	private readonly NetDataWriter _cachedNetWriter = new();
	private readonly Dictionary<KableConnectionId, KableConnection> _kablePeers = new();
	private double _deltaSinceLastNetTick;
	private ushort _ticksSinceStateSent;

	private readonly List<KableConnectionId> _toSkipPeersCache = new();

	private NetworkState? _lastReceivedNetworkState;

	private EventBasedNetListener? _listener;
	private NetManager? _netManager;

	private int _serverPort = 5225;
	private NetworkState? _cachedNetworkState;

	public bool FinishedInitialSync => IsServer || (_lastReceivedNetworkState?.FinishedInitialSync ?? false);

	[Inject]
	protected CoreGame Core { get; } = null!;

	public KableConnectionId MyConnectionId { get; internal set; }

	public PacketHandlersManager PacketHandlersManager { get; protected set; } = null!;


	public bool IsClient => !IsServer;
	public bool IsServer { get; private set; }


	private uint _networkTick;
	private double _networkTimeScaler = 1d;

	public override void _Ready()
	{
		base._Ready();

		_listener = new EventBasedNetListener();
		_netManager = new NetManager(_listener)
		{
			AutoRecycle = true,
			IPv6Enabled = false,
			MaxConnectAttempts = 5
		};

		PacketHandlersManager = new PacketHandlersManager();

		_listener.PeerConnectedEvent += HandlePeerConnected;
		_listener.PeerDisconnectedEvent += HandlePeerDisconnected;
		_listener.NetworkErrorEvent += HandleNetworkError;
		_listener.NetworkReceiveEvent += HandleNetworkReceive;
		_listener.ConnectionRequestEvent += HandleNetworkConnectionRequested;
		_listener.NetworkLatencyUpdateEvent += HandleNetworkLatencyUpdate;
	}


	public override void _ExitTree()
	{
		base._ExitTree();

		_listener?.PeerConnectedEvent -= HandlePeerConnected;
		_listener?.PeerDisconnectedEvent -= HandlePeerDisconnected;
		_listener?.NetworkErrorEvent -= HandleNetworkError;
		_listener?.NetworkReceiveEvent -= HandleNetworkReceive;
		_listener?.ConnectionRequestEvent -= HandleNetworkConnectionRequested;
		_listener?.NetworkLatencyUpdateEvent -= HandleNetworkLatencyUpdate;
	}

	protected void Reset()
	{
		_netManager?.DisconnectAll();
		_netManager?.Stop();
		_kablePeers.Clear();
		_networkTick = 0;
		IsServer = false;
		MyConnectionId = KableConnectionId.Empty;
	}

	public void DisconnectNetwork(string? reason = "Lost connection to host.")
	{
		Reset();
		Core.DisplayedErrorMessages.Clear();

		if (_netManager is not null && _netManager.IsRunning)
		{
			GD.Print($"NetworkManager Disconnected: {reason}");
			Core.CriticalError($"NetworkManager Disconnected: {reason}");
		}

		Core.World.ResetWorld();
		Core.Screens.DisplayMainMenuScreen();
	}

	public NetworkState GetCurrentState()
	{
		NetworkState state = new()
		{
			NetworkTick = _networkTick,
			FinishedInitialSync = true
		};

		foreach (var actor in Core.World.Actors.SpawnedActors)
			state.ObjectStates.Add(actor.GetCurrentState(_networkTick));

		foreach (var controller in Core.World.Controllers.SpawnedControllers)
			state.ObjectStates.Add(controller.GetCurrentState(_networkTick));

		if (IsServer)
			state.ServerKableId = new KableConnectionId(MyConnectionId.Id);

		GetStateCustom(ref state);
		return state;
	}

	protected virtual void GetStateCustom(ref NetworkState state)
	{
	}

	public NetworkState GetStateOrCached()
	{
		if (_cachedNetworkState.HasValue && _cachedNetworkState.Value.NetworkTick == _networkTick)
			return _cachedNetworkState.Value;

		_cachedNetworkState = GetCurrentState();
		return _cachedNetworkState.Value;
	}

	public void IngestNetworkState(NetworkState state)
	{
		var needsReconciliation = false;
		foreach (var objectState in state.ObjectStates)
		{
			var gameObject = Core.World.GetGameObject(objectState.ObjectId);
			if (gameObject is not LivingActor livingActor)
				continue;
			if (livingActor.CheckNeedsNetReconciliation(objectState))
			{
				needsReconciliation = true;
				break;
			}
		}
		if (needsReconciliation)
		{
			ProcessNetReconciliation(state);
		}

		foreach (var objectState in state.ObjectStates)
		{
			var tmpKableObject = Core.World.GetGameObject(objectState.ObjectId);

			tmpKableObject?.IngestNetworkState(objectState);
		}

		_lastReceivedNetworkState = state;
	}

	private void ProcessNetReconciliation(NetworkState state)
	{
		var referenceTick = state.NetworkTick;
		var processingTick = referenceTick;

		foreach (var gameObject in Core.World.GameObjects)
		{
			gameObject.RewindToTick(referenceTick);
		}

		while (processingTick < _networkTick)
		{
			Core.World.ProcessNetTick(processingTick, true);

			processingTick++;
		}
	}

	protected virtual void SetStateCustom(ref NetworkState state)
	{
	}

	private void HandleNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
	{
		var senderConnection = peer.GetKableConnection();
		if (senderConnection is null)
			return;

		PacketHandlersManager.ProcessRawPacket(reader, senderConnection);
	}

	private void HandleNetworkConnectionRequested(ConnectionRequest request)
	{
		if (!IsServer)
		{
			request.Reject();
			return;
		}

		GD.Print("NetworkManager: Net connection requested...");
		request.Accept();
	}

	private void HandleNetworkError(IPEndPoint endPoint, SocketError socketError)
	{
		if (IsServer)
			return;

		Reset();
		Core.CriticalError($"NetworkManager: Networking error: {socketError}");
	}

	private void HandlePeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
	{
		if (IsClient)
		{
			Reset();
			Core.CriticalError($"NetworkManager: lost connection to host.\nReason: {disconnectInfo.Reason}");
			return;
		}

		var foundConnection = peer.GetKableConnection();
		if (foundConnection is null)
			return;

		_kablePeers.Remove(foundConnection.ConnectionId);

		foreach (var gameObject in Core.World.GetAllOwnedBy(foundConnection))
			Core.World.DestroyObject(gameObject.KableId);
	}

	private void HandleNetworkLatencyUpdate(NetPeer peer, int latency)
	{
		var connection = peer.GetKableConnection();
		connection?.LastLatency = latency;
	}

	private void HandlePeerConnected(NetPeer peer)
	{
		GD.Print("NetworkManager: New net peer connected.");
		var connection = new KableConnection(peer, Core.GenerateKableId());
		peer.SetKableConnection(connection);

		// Only add to list if we are the server. Otherwise, the server will send a 'NetworkInitPacket' shortly, telling us its ConnectionId.
		if (IsServer)
		{
			_kablePeers.Add(connection.ConnectionId, connection);
			SendToClientReliableOrdered
			(
				new NetworkInitPacket
				{
					AssignedConnectionId = connection.ConnectionId,
					ServerConnectionId = GetServerConnectionId()
				},
				connection
			);
		}

		Core.EventBus.Publish
		(
			new NetPlayerJoinedEvent
			{
				JoinedNetTick = _networkTick,
				KableConnectionId = connection.ConnectionId
			}
		);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		_netManager?.PollEvents();

		CalculateNetTimeScaler();

		_deltaSinceLastNetTick += delta * _networkTimeScaler;
		while (_deltaSinceLastNetTick >= TickDeltaTime)
		{
			_deltaSinceLastNetTick -= TickDeltaTime;
			_networkTick++;
			Core.ProcessNetTick(_networkTick);
		}
	}

	private void CalculateNetTimeScaler()
	{
		_networkTimeScaler = 1d;

		if (IsServer)
			return;
		if (!_lastReceivedNetworkState.HasValue)
			return;

		long currentOffset = _networkTick - _lastReceivedNetworkState.Value.NetworkTick;

		var offsetError = (int)Math.Clamp(currentOffset - DesiredTicksBuffer, int.MinValue, int.MaxValue);

		var adjustment = Math.Clamp(offsetError * 0.01d, -MaxSlewAdjust, MaxSlewAdjust);
		_networkTimeScaler = 1.0d - adjustment;

		if (Math.Abs(offsetError) > NetworkTicksPerSecond)
		{
			GD.PushWarning("Massive desync detected. Hard resetting clock.");
			_networkTick = _lastReceivedNetworkState.Value.NetworkTick + DesiredTicksBuffer;
		}
	}

	public void ConnectToHost(string hostAddress)
	{
		if (_netManager == null)
		{
			GD.PrintErr("NetworkManager not initialized");
			GetTree().Quit();
			return;
		}

		Reset();
		_netManager?.Start();
		_netManager?.Connect(hostAddress, _serverPort, "ships_prototype");
		GD.Print($"Connecting to host {hostAddress}:{_serverPort}");
	}

	public void HostServer()
	{
		if (_netManager is null || _netManager.IsRunning)
		{
			GD.PrintErr("NetworkManger: Tried to start server more than once.");
			return;
		}

		Reset();

		IsServer = true;
		_netManager.Start(_serverPort);

		Core.World.ChangeMapTo(Core.Resources.MapPrefabs.DevEnvMap);
		var kableConnectionId = Core.GenerateRawKableId();
		MyConnectionId = new KableConnectionId(0);
		GD.Print($"Server KableConnectionId: {MyConnectionId}");

		Core.Resources.LoadingScreenHandle.Visible = false;

		Core.EventBus.Publish
		(
			new NetPlayerJoinedEvent
			{
				JoinedNetTick = _networkTick,
				KableConnectionId = MyConnectionId
			}
		);
	}


	public KableConnectionId GetServerConnectionId()
	{
		if (IsServer)
			return MyConnectionId;

		if (_lastReceivedNetworkState is null)
			throw new InvalidOperationException("NetworkManager not initialized");

		return _lastReceivedNetworkState.Value.ServerKableId;
	}

	public KableConnection? GetKableConnectionFromId(KableConnectionId kableId)
	{
		return _kablePeers.Values.FirstOrDefault(kablePeer => kablePeer.ConnectionId == kableId);
	}

	internal IReadOnlyDictionary<KableConnectionId, KableConnection> GetKablePeersReference()
	{
		return _kablePeers;
	}

	internal void RegisterKableConnection(KableConnection kableConnection)
	{
		_kablePeers.TryAdd(kableConnection.ConnectionId, kableConnection);
	}

	private void SendToClient<T>(T packet, KableConnection client, DeliveryMethod deliveryMethod) where T : IGamePacket
	{
		_cachedNetWriter.Reset();
		PacketHandlersManager.SerializePacket(_cachedNetWriter, packet);
		client.SendPacket(_cachedNetWriter, deliveryMethod);
	}

	public void SendToClientUnreliable<T>(T packet, KableConnection client) where T : IGamePacket
	{
		SendToClient<T>(packet, client, DeliveryMethod.Unreliable);
	}

	public void SendToClientReliableOrdered<T>(T packet, KableConnection client) where T : IGamePacket
	{
		SendToClient<T>(packet, client, DeliveryMethod.ReliableOrdered);
	}

	public void SendToClientReliableUnordered<T>(T packet, KableConnection client) where T : IGamePacket
	{
		SendToClient<T>(packet, client, DeliveryMethod.ReliableUnordered);
	}

	private void SendToAllConnected<T>(T packet, DeliveryMethod deliveryMethod, IReadOnlyList<KableConnectionId>? connectionsToSkip) where T : IGamePacket
	{
		_toSkipPeersCache.Clear();

		if (connectionsToSkip?.Count > 0)
			_toSkipPeersCache.AddRange(connectionsToSkip);

		_cachedNetWriter.Reset();
		PacketHandlersManager.SerializePacket(_cachedNetWriter, packet);

		_kablePeers
			.Where(pair => !_toSkipPeersCache.Contains(pair.Key))
			.ToList()
			.ForEach(x => x.Value.SendPacket(_cachedNetWriter, deliveryMethod));
	}

	public void SendToAllUnreliable<T>(T packet, IReadOnlyList<KableConnectionId>? connectionsToSkip = null) where T : IGamePacket
	{
		SendToAllConnected<T>(packet, DeliveryMethod.Unreliable, connectionsToSkip);
	}

	public void SendToAllReliableOrdered<T>(T packet, IReadOnlyList<KableConnectionId>? connectionsToSkip = null) where T : IGamePacket
	{
		SendToAllConnected<T>(packet, DeliveryMethod.ReliableOrdered, connectionsToSkip);
	}

	public void SendToAllReliableUnordered<T>(T packet, IReadOnlyList<KableConnectionId>? connectionsToSkip = null) where T : IGamePacket
	{
		SendToAllConnected<T>(packet, DeliveryMethod.ReliableUnordered, connectionsToSkip);
	}
}
