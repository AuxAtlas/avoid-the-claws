using System.Collections.Generic;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Data.State;
using Godot;

namespace AvoidClaws.code.dotnet.Components.Core;

[Tool]
public partial class AudioPlayerComponent : Node3D, IComponent
{
    [Export]
    protected AudioStreamPlayer3D? AudioPlayer
    {
        get => _audioPlayer;
        set
        {
            _audioPlayer = value;
            UpdateConfigurationWarnings();
        }
    }

    private AudioStreamPlayer3D? _audioPlayer;
    public IActor? ParentActor { get; private set; }
    public bool ReconciliationMode => ParentActor?.ReconciliationMode ?? false;

    public void SetupComponent()
    {
        ParentActor = GetParent()?.GetParentOrNull<IActor>();
        _audioPlayer?.Stop();
    }

    public override string[] _GetConfigurationWarnings()
    {
        List<string> warnings = new(base._GetConfigurationWarnings() ?? []);
        if (_audioPlayer == null) warnings.Add("Please set the 'AudioPlayer' on this component");

        return warnings.ToArray();
    }


    public void Play(AudioStream audio, float pitch = 1.0f, bool interrupt = false)
    {
        if (_audioPlayer == null)
            return;

        if (_audioPlayer.IsPlaying())
        {
            if (!interrupt)
                return;

            _audioPlayer.Stop();
        }

        _audioPlayer.PitchScale = pitch;
        _audioPlayer.Stream = audio;
        _audioPlayer.Play();
    }

    public void ReadStateFrom(ObjectState state)
    {
    }

    public void WriteStateTo(ObjectState state)
    {
    }
}