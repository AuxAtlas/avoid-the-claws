#region

using System;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Data;

public record KableId(uint Id) : INetSerializable
{
    public uint Id { get; private set; } = Id;
    public static readonly KableId Empty = new(0);

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

    public void SetKableId(uint id)
    {
        Id = id;
    }
}