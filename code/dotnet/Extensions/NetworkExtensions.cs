using AvoidClaws.code.dotnet.Networking.Data;
using Godot;
using LiteNetLib;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Extensions;

public static class NetworkExtensions
{
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

            var xValues = new float[values.Length];
            var yValues = new float[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                xValues[i] = values[i].X;
                yValues[i] = values[i].Y;
            }

            writer.PutArray(xValues);
            writer.PutArray(yValues);
        }

        public void PutArray(Vector3[]? values)
        {
            values ??= [];

            var xValues = new float[values.Length];
            var yValues = new float[values.Length];
            var zValues = new float[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                xValues[i] = values[i].X;
                yValues[i] = values[i].Y;
                zValues[i] = values[i].Z;
            }

            writer.PutArray(xValues);
            writer.PutArray(yValues);
            writer.PutArray(zValues);
        }
    }

    extension(NetDataReader reader)
    {
        public Vector2 GetVector2()
        {
            Vector2 v;
            v.X = reader.GetFloat();
            v.Y = reader.GetFloat();
            return v;
        }

        public Vector2[] GetVector2Array()
        {
            var xValues = reader.GetFloatArray();
            var yValues = reader.GetFloatArray();

            var v = new Vector2[xValues.Length];
            for (var i = 0; i < v.Length; i++) v[i] = new Vector2(xValues[i], yValues[i]);

            return v;
        }

        public Vector3 GetVector3()
        {
            Vector3 v;
            v.X = reader.GetFloat();
            v.Y = reader.GetFloat();
            v.Z = reader.GetFloat();
            return v;
        }

        public Vector3[] GetVector3Array()
        {
            var xValues = reader.GetFloatArray();
            var yValues = reader.GetFloatArray();
            var zValues = reader.GetFloatArray();

            var v = new Vector3[xValues.Length];
            for (var i = 0; i < v.Length; i++) v[i] = new Vector3(xValues[i], yValues[i], zValues[i]);

            return v;
        }

        public KableId GetKableId()
        {
            var id = reader.GetUInt();
            return new KableId(id);
        }

        public KableConnectionId GetKableConnectionId()
        {
            var id = reader.GetUInt();
            return new KableConnectionId(id);
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