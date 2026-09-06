#region

using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets;
using AvoidClaws.code.dotnet.Services;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Handlers;

public abstract class PacketHandler<TPacketType> where TPacketType : IGamePacket
{
    [Inject]
    protected CoreGame Core { get; } = null!;

    protected bool IsServer => Core.Network.IsServer;
    protected bool IsClient => Core.Network.IsClient;


    public void ProcessPacket(dynamic packet, uint tick, KableConnection source)
    {
        Handle(packet, tick, source);
    }

    protected virtual void Handle(TPacketType packet, uint tick, KableConnection source)
    {
    }
}