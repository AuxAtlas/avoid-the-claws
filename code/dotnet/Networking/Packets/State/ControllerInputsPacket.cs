#region

using AvoidClaws.code.dotnet.Data.State;
using AvoidClaws.code.dotnet.Networking.Data;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Packets.State;

public record ControllerInputsPacket : IGamePacket
{
    public KableId ControllerKableId { get; set; } = null!;
    public ControllerInputs Inputs { get; set; } = new();

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ControllerKableId);
        writer.Put(Inputs);
    }
    public void Deserialize(NetDataReader reader)
    {
        ControllerKableId.SetKableId(reader.GetUInt());
        Inputs = reader.Get<ControllerInputs>();
    }
}