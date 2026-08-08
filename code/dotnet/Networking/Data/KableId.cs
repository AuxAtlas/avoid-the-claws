#region

using System;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Data;

public class KableId : INetSerializable, IEquatable<KableId>
{
    public uint Id { get; private set; }
    public static readonly KableId Empty = new(0);

    public bool IsValid => Id != 0;


    public KableId(uint kableId)
    {
        Id = kableId;
    }


    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(KableId a, KableId b)
    {
        return Equals(a, b) || a.Equals(b);
    }

    public static bool operator !=(KableId a, KableId b)
    {
        return !(a == b);
    }

    public bool Equals(KableId other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is KableId other && Equals(other);
    }

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