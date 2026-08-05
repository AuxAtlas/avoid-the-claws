using System.Collections.Generic;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using Godot;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Data.State;

public class ObjectState : INetSerializable
{
    public KableId ObjectId { get; set; }
    public KableConnectionId AuthorityConnectionId { get; set; }
    public uint NetworkTick { get; set; }

    public List<byte> CustomBytes { get; } = [];
    public List<uint> CustomUInts { get; } = [];
    public List<float> CustomFloats { get; } = [];
    public List<Vector3> CustomVectors { get; } = [];

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