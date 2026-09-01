#region

using System;
using System.Collections.Generic;
using System.Text.Json;
using AvoidClaws.code.dotnet.Extensions;
using AvoidClaws.code.dotnet.Networking.Data;
using Godot;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Data.State;

public record ObjectState : INetSerializable
{
    public KableId ObjectId { get; set; } = new(0);
    public KableConnectionId AuthorityConnectionId { get; set; } = new(0);
    public uint NetworkTick { get; set; }
    public static readonly ObjectState BlankStateRef = new ObjectState();

    private readonly List<byte> _customBytes = [];
    private readonly List<uint> _customUInts = [];
    private readonly List<float> _customFloats = [];
    private readonly List<Vector3> _customVectors = [];

    private int _byteReadPos;
    private int _uintReadPos;
    private int _floatReadPos;
    private int _vectorReadPos;

    private bool _isDirty = true;
    private int _lastHashCache = 0;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ObjectId);
        writer.Put(AuthorityConnectionId);
        writer.Put(NetworkTick);

        writer.PutBytesWithLength(_customBytes.ToArray());
        writer.PutArray(_customUInts.ToArray());
        writer.PutArray(_customFloats.ToArray());
        writer.PutArray(_customVectors.ToArray());
    }

    public void Deserialize(NetDataReader reader)
    {
        ObjectId.SetKableId(reader.GetUInt());
        AuthorityConnectionId.SetKableConnectionId(reader.GetUInt());
        NetworkTick = reader.GetUInt();

        ResetCustoms();

        _customBytes.AddRange(reader.GetBytesWithLength());
        _customUInts.AddRange(reader.GetUIntArray());
        _customFloats.AddRange(reader.GetFloatArray());
        reader.ReadVector3ArrayInto(_customVectors);
    }

    public void ResetCustoms()
    {
        _customBytes.Clear();
        _customUInts.Clear();
        _customFloats.Clear();
        _customVectors.Clear();

        _byteReadPos = 0;
        _uintReadPos = 0;
        _floatReadPos = 0;
        _vectorReadPos = 0;

        _isDirty = true;
    }

    public void Put(byte val)
    {
        _customBytes.Add(val);
        _isDirty = true;
    }

    public void Put(uint val)
    {
        _customUInts.Add(val);
        _isDirty = true;
    }

    public void Put(float val)
    {
        _customFloats.Add(val);
        _isDirty = true;
    }

    public void Put(Vector3 val)
    {
        _customVectors.Add(val);
        _isDirty = true;
    }

    public void Put(Vector2 val)
    {
        _customFloats.Add(val.X);
        _customFloats.Add(val.Y);
        _isDirty = true;
    }

    public byte ReadByte()
    {
        return _customBytes[_byteReadPos++];
    }

    public uint ReadUInt()
    {
        return _customUInts[_uintReadPos++];
    }

    public float ReadFloat()
    {
        return _customFloats[_floatReadPos++];
    }

    public Vector3 ReadVector3()
    {
        return _customVectors[_vectorReadPos++];
    }

    public Vector2 ReadVector2()
    {
        return new Vector2(ReadFloat(), ReadFloat());
    }
    public Vector3 GetVector3AtIndex(int index)
    {
        if(index >= 0 && index < _customVectors.Count)
            return _customVectors[index];
        throw new IndexOutOfRangeException();
    }

}