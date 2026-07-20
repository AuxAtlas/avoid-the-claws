using System;
using System.Text;

namespace AvoidClaws.code.dotnet.Util;

public static class HashHelper
{
    // FNV-1a hashing algorithm
    public static ulong FastHash(byte[] bytes)
    {
        const ulong fnv64Offset = 14695981039346656037;
        const ulong fnv64Prime = 0x100000001b3;
        var hash = fnv64Offset;

        for (var i = 0; i < bytes.Length; i++)
        {
            hash ^= bytes[i];
            hash *= fnv64Prime;
        }

        return hash;
    }

    public static ulong FastHash(string input)
    {
        return FastHash(Encoding.UTF8.GetBytes(input));
    }

    public static ulong FastHash(Type input)
    {
        return FastHash(input.Assembly.FullName + input.Assembly.GetName().Version + input.FullName);
    }

    public static ulong FastHash<T>()
    {
        return FastHash(typeof(T));
    }
}