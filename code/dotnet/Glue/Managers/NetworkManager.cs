using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Events.Networking;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.Objects;
using AvoidClaws.code.dotnet.Networking.Packets.State;
using AvoidClaws.code.dotnet.Services;
using Godot;
using LiteNetLib;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Glue.Managers;

public partial class NetworkManager : Node, IService
{
    public const uint NETWORK_TICKS_PER_SECOND = 60;
    public const double MAX_SLEW_ADJUST = 0.002d;
    public const uint MAX_TICK_SEQUENCE = 256;
    public const int DESIRED_TICKS_BUFFER = 3;

    public static double TickDeltaTime => 1d / NETWORK_TICKS_PER_SECOND;
    public static float TickDeltaTimeF => 1f / NETWORK_TICKS_PER_SECOND;

    private readonly NetDataWriter _cachedNetWriter = new();
    private readonly Dictionary<KableConnectionId, KableConnection> _kablePeers = new();
    private double _deltaSinceLastNetTick;
    private ushort _ticksSinceStateSent;

    [Export]
    public Label? DebugLabel { get; private set; }

    private readonly List<KableConnection> _toSkipPeersCache = new();

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


    public uint NetworkTick { get; private set; }
    private double _networkTimeScaler = 1d;

    public override void _Ready()
    {
        base._Ready();

        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener)
        {
            AutoRecycle = true,
            IPv6Enabled = false,
            MaxConnectAttempts = 6
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
        _netManager?.Stop();
        _kablePeers.Clear();
        NetworkTick = 0;
        IsServer = false;
        MyConnectionId = KableConnectionId.Empty;
    }

    public void DisconnectNetwork(string? reason = "Lost connection to host.")
    {
        Reset();
        Core.World.DisplayedErrorMessages.Clear();

        if (_netManager is not null && _netManager.IsRunning)
        {
            GD.Print($"NetworkManager Disconnected: {reason}");
            Core.World.SetDisplayedErrorMessage($"NetworkManager: {reason}");
        }

        Core.World.GotoMainMenu();
    }

    public NetworkState GetState()
    {
        NetworkState state = new()
        {
            NetworkTick = NetworkTick,
            FinishedInitialSync = true
        };

        foreach (var actor in Core.World.SpawnedActors) state.ObjectStates.Add(actor.GetCurrentState());

        foreach (var controller in Core.World.SpawnedControllers) state.ObjectStates.Add(controller.GetCurrentState());

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
        if (_cachedNetworkState.HasValue && _cachedNetworkState.Value.NetworkTick == NetworkTick) return _cachedNetworkState.Value;

        _cachedNetworkState = GetState();
        return _cachedNetworkState.Value;
    }

    public void SetState(NetworkState state)
    {
        NetworkTick = state.NetworkTick;

        foreach (var objectState in state.ObjectStates)
        {
            var tmpKableObject = Core.World.GetKableObject(objectState.ObjectId);

            tmpKableObject?.IngestNetworkState(objectState);
        }

        SetStateCustom(ref state);
    }

    public void IngestNetworkState(NetworkState state)
    {
        SetState(state);
        _lastReceivedNetworkState = state;
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
        GD.PrintErr($"NetworkManager: Networking error: {socketError}");

        if (IsServer)
            return;

        Reset();

        Core.World.SetDisplayedErrorMessage("Unknown networking error.");
        Core.World.GotoMainMenu();
    }

    private void HandlePeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (IsClient)
        {
            Reset();
            GD.Print($"NetworkManager: lost connection to host.\nReason: {disconnectInfo.Reason}");
            Core.World.SetDisplayedErrorMessage("Lost connection to game host.");

            Core.World.GotoMainMenu();
            return;
        }

        var foundConnection = peer.GetKableConnection();
        if (foundConnection is null)
            return;

        _kablePeers.Remove(foundConnection.ConnectionId);

        var foundOwned = Core.World.GetAllOwnedBy(foundConnection);
        if (foundOwned is null)
            return;

        foreach (var kableObject in foundOwned) Core.World.DestroyObject(kableObject.KableId);
    }

    private void HandleNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        var connection = peer.GetKableConnection();
        if (connection is null)
            return;

        connection.LastLatency = latency;
    }

    private void HandlePeerConnected(NetPeer peer)
    {
        GD.Print("NetworkManager: New net peer connected.");
        var connection = new KableConnection(peer, Core.World.GenerateKableId());
        peer.SetKableConnection(connection);

        _kablePeers.Add(connection.ConnectionId, connection);


        Core.EventBus.Publish(new NetJoinedEvent
        {
            JoinedNetTick = NetworkTick,
            KableConnection = connection
        });

        if (IsServer)
            SendToClientReliableUnordered(new NetworkInitPacket
            {
                AssignedConnectionId = connection.ConnectionId!,
                ServerConnectionId = GetServerConnectionId()
            }, connection);
    }

    private void HandleObjectDespawnedEvent(ObjectDespawnedEvent e, KableConnection? source)
    {
        if (IsClient)
            return;

        SendToAllReliableUnordered(new DestroyObjectPacket
        {
            TargetObjectId = e.KableObject.KableId
        });
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
            NetworkTick++;
            Core.World.ProcessNetTick(NetworkTick);
        }
    }

    private void CalculateNetTimeScaler()
    {
        _networkTimeScaler = 1d;

        if (IsServer)
            return;
        if (!_lastReceivedNetworkState.HasValue)
            return;

        long currentOffset = NetworkTick - _lastReceivedNetworkState.Value.NetworkTick;

        var offsetError = (int)Math.Clamp(currentOffset - DESIRED_TICKS_BUFFER, int.MinValue, int.MaxValue);

        var adjustment = Math.Clamp(offsetError * 0.01d, -MAX_SLEW_ADJUST, MAX_SLEW_ADJUST);
        _networkTimeScaler = 1.0d - adjustment;

        if (Math.Abs(offsetError) > NETWORK_TICKS_PER_SECOND)
        {
            GD.PushWarning("Massive desync detected. Hard resetting clock.");
            NetworkTick = _lastReceivedNetworkState.Value.NetworkTick + DESIRED_TICKS_BUFFER;
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

        Core.World.ChangeMapTo(Core.Resources.LevelPrefabs.DevEnvMap);
        MyConnectionId = new KableConnectionId(Core.World.GenerateRawKableId());
        GD.Print($"Server KableId: {MyConnectionId}");

        var actorNode = Core.World.SpawnPrefab(Core.Resources.ActorPrefabs.PlayerActorPrefab);
        var controllerNode = Core.World.SpawnPrefab(Core.Resources.ControllerPrefabs.PlayerControllerPrefab);

        if (controllerNode is IController controller)
        {
            controller.SetKableAuthority(MyConnectionId);
            if (actorNode is IActor actor)
            {
                actor.SetKableAuthority(MyConnectionId);
                controller.Attach(actor);
                actor.Respawn();
            }
        }
    }


    public KableConnectionId GetServerConnectionId()
    {
        if (IsServer)
            return MyConnectionId;

        if (_lastReceivedNetworkState is null)
            throw new InvalidOperationException("NetworkManager not initialized");

        return _lastReceivedNetworkState.Value.ServerKableId;
    }

    public KableConnection? GetKableConnectionFromId(KableConnectionId? kableId)
    {
        if (kableId is null)
            return null;

        return _kablePeers.Values.FirstOrDefault(kablePeer => kablePeer.ConnectionId == kableId);
    }

    internal Dictionary<KableConnectionId, KableConnection> GetKablePeersReference()
    {
        return _kablePeers;
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

    private void SendToAllConnected<T>(T packet, DeliveryMethod deliveryMethod, List<KableConnection>? connectionsToSkip) where T : IGamePacket
    {
        _toSkipPeersCache.Clear();

        if (connectionsToSkip?.Count > 0)
            _toSkipPeersCache.AddRange(connectionsToSkip);

        _cachedNetWriter.Reset();
        PacketHandlersManager.SerializePacket(_cachedNetWriter, packet);
        _kablePeers.Values.Except(_toSkipPeersCache).ToList().ForEach(x => x.SendPacket(_cachedNetWriter, deliveryMethod));
    }

    public void SendToAllUnreliable<T>(T packet, List<KableConnection>? connectionsToSkip = null) where T : IGamePacket
    {
        SendToAllConnected<T>(packet, DeliveryMethod.Unreliable, connectionsToSkip);
    }

    public void SendToAllReliableOrdered<T>(T packet, List<KableConnection>? connectionsToSkip = null) where T : IGamePacket
    {
        SendToAllConnected<T>(packet, DeliveryMethod.ReliableOrdered, connectionsToSkip);
    }

    public void SendToAllReliableUnordered<T>(T packet, List<KableConnection>? connectionsToSkip = null) where T : IGamePacket
    {
        SendToAllConnected<T>(packet, DeliveryMethod.ReliableUnordered, connectionsToSkip);
    }
}