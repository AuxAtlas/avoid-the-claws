using System;

namespace AvoidClaws.code.dotnet.Networking.Data;

public readonly struct KableConnectionId : IEquatable<KableConnectionId>
{
    public static readonly KableConnectionId Empty = new(0);

    public uint Id { get; } = 0;

    public KableConnectionId(uint connectionId)
    {
        Id = connectionId;
    }

    public KableConnectionId()
    {
    }

    public bool IsValid => Id != 0;

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(KableConnectionId a, KableConnectionId b)
    {
        return Equals(a, b) || a.Equals(b);
    }

    public static bool operator !=(KableConnectionId a, KableConnectionId b)
    {
        return !(a == b);
    }

    public bool Equals(KableConnectionId other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is KableConnectionId other && Equals(other);
    }

    public override string ToString()
    {
        return Id.ToString();
    }
}