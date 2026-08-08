#region

using System.Collections.Generic;
using System.Runtime.InteropServices;
using AvoidClaws.code.dotnet.Networking.Data;
using Godot;
using LiteNetLib;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Extensions;

public static class NetworkExtensions
{
    static private readonly List<float> _xValuesBuffer = new();
    static private readonly List<float> _yValuesBuffer = new();
    static private readonly List<float> _zValuesBuffer = new();

    extension(byte b)
    {
        /// <summary>
        ///     Returns
        ///     a
        ///     copy
        ///     of
        ///     given
        ///     byte
        ///     with
        ///     a
        ///     specific
        ///     bit
        ///     set
        ///     based
        ///     on
        ///     a
        ///     boolean
        ///     value.
        /// </summary>
        public byte WithBitSet(int bitIndex, bool value)
        {
            // Create a mask with a 1 at the target bit index.
            var mask = (byte)(1 << bitIndex);

            if (value)
                // Use bitwise OR (|) to set the bit to 1.
                return (byte)(b | mask);

            // Use bitwise AND (&) with a negated mask (~) to set the bit to 0.
            return (byte)(b & ~mask);
        }

        /// <summary>
        ///     Retrieves
        ///     a
        ///     boolean
        ///     value
        ///     from
        ///     a
        ///     specific
        ///     bit
        ///     of
        ///     a
        ///     byte.
        /// </summary>
        public bool GetBit(int bitIndex)
        {
            // Create a mask with a 1 at the target bit index.
            var mask = (byte)(1 << bitIndex);

            // Use bitwise AND (&) to isolate the target bit.
            // Compare the result to the mask to see if the bit was 1.
            return (b & mask) == mask;
        }
    }

    extension(NetDataWriter writer)
    {
        public void Put(KableId kableId)
        {
            writer.Put(kableId.Id);
        }

        public void Put(KableConnectionId connectionId)
        {
            writer.Put(connectionId.Id);
        }

        public void Put(Vector2 vector)
        {
            writer.Put(vector.X);
            writer.Put(vector.Y);
        }

        public void Put(Vector3 vector)
        {
            writer.Put(vector.X);
            writer.Put(vector.Y);
            writer.Put(vector.Z);
        }

        public void PutArray(Vector2[]? values)
        {
            values ??= [];

            _xValuesBuffer.Clear();
            _yValuesBuffer.Clear();
            for (var i = 0; i < values.Length; i++)
            {
                _xValuesBuffer[i] = values[i].X;
                _yValuesBuffer[i] = values[i].Y;
            }

            writer.PutSpan(CollectionsMarshal.AsSpan(_xValuesBuffer));
            writer.PutSpan(CollectionsMarshal.AsSpan(_yValuesBuffer));
        }

        public void PutArray(Vector3[]? values)
        {
            values ??= [];

            _xValuesBuffer.Clear();
            _yValuesBuffer.Clear();
            _zValuesBuffer.Clear();
            for (var i = 0; i < values.Length; i++)
            {
                _xValuesBuffer[i] = values[i].X;
                _yValuesBuffer[i] = values[i].Y;
                _zValuesBuffer[i] = values[i].Z;
            }

            writer.PutSpan(CollectionsMarshal.AsSpan(_xValuesBuffer));
            writer.PutSpan(CollectionsMarshal.AsSpan(_yValuesBuffer));
            writer.PutSpan(CollectionsMarshal.AsSpan(_zValuesBuffer));
        }
    }

    extension(NetDataReader reader)
    {
        public Vector2 GetVector2()
        {
            var x = reader.GetFloat();
            var y = reader.GetFloat();
            return new Vector2(x, y);
        }

        /// <summary>
        /// Reads an array of Vector2 into the given List 'v'
        /// </summary>
        public void ReadVector2ArrayInto(List<Vector2> v)
        {
            var xValues = reader.GetFloatArray();
            var yValues = reader.GetFloatArray();

            for (var i = 0; i < xValues.Length; i++)
            {
                v.Add(new Vector2(xValues[i], yValues[i]));
            }
        }

        public Vector3 GetVector3()
        {
            var x = reader.GetFloat();
            var y = reader.GetFloat();
            var z = reader.GetFloat();
            return new Vector3(x, y, z);
        }

        public void ReadVector3ArrayInto(List<Vector3> v)
        {
            var xValues = reader.GetFloatArray();
            var yValues = reader.GetFloatArray();
            var zValues = reader.GetFloatArray();

            for (var i = 0; i < xValues.Length; i++)
            {
                v.Add(new Vector3(xValues[i], yValues[i], zValues[i]));
            }
        }
    }


    extension(NetPeer peer)
    {
        public KableConnectionId? GetKableId()
        {
            return peer.GetKableConnection()?.ConnectionId;
        }

        public KableConnection? GetKableConnection()
        {
            return (KableConnection?)peer.Tag;
        }

        public void SetKableConnection(KableConnection? kableConnection)
        {
            peer.Tag = kableConnection;
        }
    }
}