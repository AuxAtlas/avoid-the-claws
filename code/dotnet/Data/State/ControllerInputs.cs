#region

using System;
using AvoidClaws.code.dotnet.Extensions;
using Godot;
using LiteNetLib.Utils;

#endregion

namespace AvoidClaws.code.dotnet.Data.State;

public struct ControllerInputs : INetSerializable
{
    public uint NetworkTick;
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public byte AttackInputsPacked;
    public byte ActionInputsPacked;
    public void Serialize(NetDataWriter writer)
    {
        writer.Put(NetworkTick);
        writer.Put(MoveInput);
        writer.Put(LookInput);
        writer.Put(AttackInputsPacked);
        writer.Put(ActionInputsPacked);
    }
    public void Deserialize(NetDataReader reader)
    {
        NetworkTick = reader.GetUInt();
        MoveInput = reader.GetVector2();
        LookInput = reader.GetVector2();
        AttackInputsPacked = reader.GetByte();
        ActionInputsPacked = reader.GetByte();
    }

    /// <summary>
    /// Resets all inputs to zero
    /// </summary>
    public void Clear()
    {
        MoveInput = Vector2.Zero;
        LookInput = Vector2.Zero;
        AttackInputsPacked = 0;
        ActionInputsPacked = 0;
    }
}


public enum ActionName
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
    AttackPrimary,
    AttackSecondary,
    AbilityPrimary,
    AbilitySecondary,
}

public static class ActionNamesExtensions
{
    public static string ToActionString(this ActionName actionName) => actionName switch
    {
        ActionName.MoveForward => "move_forward",
        ActionName.MoveBackward => "move_backward",
        ActionName.MoveLeft => "move_left",
        ActionName.MoveRight => "move_right",
        ActionName.MoveUp => "move_up",
        ActionName.MoveDown => "move_down",
        ActionName.AttackPrimary => "attack_primary",
        ActionName.AttackSecondary => "attack_secondary",
        ActionName.AbilityPrimary => "ability_primary",
        ActionName.AbilitySecondary => "ability_secondary",
        
        _ => throw new NotImplementedException()
    };
}