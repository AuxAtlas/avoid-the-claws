using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Handlers;
using AvoidClaws.code.dotnet.Networking.Packets;
using AvoidClaws.code.dotnet.Util;
using Godot;
using LiteNetLib;
using LiteNetLib.Utils;

namespace AvoidClaws.code.dotnet.Glue.Managers;

public class PacketHandlersManager
{
    private readonly Dictionary<ulong, PacketHandler<IGamePacket>> _registeredHandlers = new();

    public PacketHandlersManager()
    {
        DiscoverPacketHandlers(Assembly.GetExecutingAssembly());
    }

    public void DiscoverPacketHandlers(Assembly assembly)
    {
        var result = assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(PacketHandler<IGamePacket>)));

        IEnumerable<Type> typesResult = result as Type[] ?? result.ToArray();
        if (!typesResult.Any())
            return;

        foreach (var type in typesResult)
        {
            if (!type.IsGenericType)
                continue;

            foreach (var genericArgument in type.GetGenericArguments())
            {
                if (!genericArgument.IsSubclassOf(typeof(IGamePacket)))
                    continue;

                var hash = HashHelper.FastHash(genericArgument);

                if (_registeredHandlers.ContainsKey(hash))
                    throw new DuplicateNameException("Duplicate packet handler for " + genericArgument.Name);

                _registeredHandlers.Add(hash, (PacketHandler<IGamePacket>)Activator.CreateInstance(type)!);

                break;
            }
        }
    }

    internal void ProcessRawPacket(NetPacketReader reader, KableConnection source)
    {
        var hash = reader.GetULong();
        if (!_registeredHandlers.TryGetValue(hash, out var handler))
        {
            GD.PushWarning("Dropped incoming packet with an unknown packet type hash: " + hash);
            return;
        }

        handler.ProcessPacket(reader, source);
    }

    internal void SerializePacket(NetDataWriter writer, IGamePacket packet)
    {
        var hash = HashHelper.FastHash(packet.GetType());
        writer.Put(hash);
        packet.Serialize(writer);
    }
}