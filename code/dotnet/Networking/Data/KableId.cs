using System;

namespace AvoidClaws.code.dotnet.Networking.Data;

public readonly struct KableId : IEquatable<KableId>
{
    public uint Id { get; } = 0;

    public KableId(uint kableId)
    {
        Id = kableId;
    }

    public KableId()
    {
    }

    public bool IsValid => Id != 0;

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
}