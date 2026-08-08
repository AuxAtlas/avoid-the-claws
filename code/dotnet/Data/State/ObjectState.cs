#region

using System;
using System.Collections.Generic;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using Godot;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Data.State;

public class ObjectState : INetSerializable, IEquatable<ObjectState>
{
    public KableId ObjectId { get; set; } = new(0);
    public KableConnectionId AuthorityConnectionId { get; set; } = new(0);
    public uint NetworkTick { get; set; }

    private List<byte> CustomBytes { get; } = [];
    private List<uint> CustomUInts { get; } = [];
    private List<float> CustomFloats { get; } = [];
    private List<Vector3> CustomVectors { get; } = [];

    private int byteReadPos;
    private int uintReadPos;
    private int floatReadPos;
    private int vectorReadPos;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ObjectId);
        writer.Put(AuthorityConnectionId);
        writer.Put(NetworkTick);

        writer.PutBytesWithLength(CustomBytes.ToArray());
        writer.PutArray(CustomUInts.ToArray());
        writer.PutArray(CustomFloats.ToArray());
        writer.PutArray(CustomVectors.ToArray());
    }

    public void Deserialize(NetDataReader reader)
    {
        ObjectId.SetKableId(reader.GetUInt());
        AuthorityConnectionId.SetKableConnectionId(reader.GetUInt());
        NetworkTick = reader.GetUInt();

        ResetCustoms();

        CustomBytes.AddRange(reader.GetBytesWithLength());
        CustomUInts.AddRange(reader.GetUIntArray());
        CustomFloats.AddRange(reader.GetFloatArray());
        reader.ReadVector3ArrayInto(CustomVectors);
    }

    public void ResetCustoms()
    {
        CustomBytes.Clear();
        CustomUInts.Clear();
        CustomFloats.Clear();
        CustomVectors.Clear();

        byteReadPos = 0;
        uintReadPos = 0;
        floatReadPos = 0;
        vectorReadPos = 0;
    }

    public void Put(byte val)
    {
        CustomBytes.Add(val);
    }

    public void Put(uint val)
    {
        CustomUInts.Add(val);
    }

    public void Put(float val)
    {
        CustomFloats.Add(val);
    }

    public void Put(Vector3 val)
    {
        CustomVectors.Add(val);
    }

    public void Put(Vector2 val)
    {
        CustomFloats.Add(val.X);
        CustomFloats.Add(val.Y);
    }

    public byte ReadByte()
    {
        return CustomBytes[byteReadPos++];
    }

    public uint ReadUInt()
    {
        return CustomUInts[uintReadPos++];
    }

    public float ReadFloat()
    {
        return CustomFloats[floatReadPos++];
    }

    public Vector3 ReadVector3()
    {
        return CustomVectors[vectorReadPos++];
    }

    public Vector2 ReadVector2()
    {
        return new Vector2(ReadFloat(), ReadFloat());
    }

    public bool Equals(ObjectState? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ObjectId.Equals(other.ObjectId) && NetworkTick == other.NetworkTick && CustomBytes.Equals(other.CustomBytes) && CustomUInts.Equals(other.CustomUInts) && CustomFloats.Equals(other.CustomFloats) && CustomVectors.Equals(other.CustomVectors);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((ObjectState)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectId, NetworkTick, CustomBytes, CustomUInts, CustomFloats, CustomVectors);
    }
}