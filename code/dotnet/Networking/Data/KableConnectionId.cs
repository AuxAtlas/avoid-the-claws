#region

using System;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Data;

public record KableConnectionId(uint Id) : INetSerializable
{
    public static readonly KableConnectionId Empty = new(0);
    public static readonly KableConnectionId Server = new(1);

    public uint Id { get; private set; } = Id;

    public bool IsValid => Id != 0;


    public override string ToString()
    {
        return Id.ToString();
    }
    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
    }
    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetUInt();
    }

    public void SetKableConnectionId(uint id)
    {
        Id = id;
    }
}