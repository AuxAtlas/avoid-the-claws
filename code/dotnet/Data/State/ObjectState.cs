using System.Collections.Generic;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using Godot;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Data.State;

public struct ObjectState : INetSerializable
{
    public KableId ObjectId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public uint NetworkTick { get; set; }

    public List<byte> CustomBytes { get; set; } = [];
    public List<uint> CustomUInts { get; set; } = [];
    public List<float> CustomFloats { get; set; } = [];
    public List<Vector3> CustomVectors { get; set; } = [];

    private int byteReadPos = 0;
    private int uintReadPos = 0;
    private int floatReadPos = 0;
    private int vectorReadPos = 0;

    public ObjectState()
    {
        Reset();
    }

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
        ObjectId = reader.GetKableId();
        AuthorityConnectionId = reader.GetKableConnectionId();
        NetworkTick = reader.GetUInt();

        ResetCustoms();

        CustomBytes.AddRange(reader.GetBytesWithLength());
        CustomUInts.AddRange(reader.GetUIntArray());
        CustomFloats.AddRange(reader.GetFloatArray());
        CustomVectors.AddRange(reader.GetVector3Array());
    }

    private void ResetCustoms()
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

    private void Reset()
    {
        ObjectId = KableId.Empty;
        AuthorityConnectionId = KableConnectionId.Empty;
        NetworkTick = 0;
        ResetCustoms();
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
}