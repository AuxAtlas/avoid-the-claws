#region

using System.Collections.Generic;
using AvoidClaws.code.dotnet.Data.State;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Components.Core;

[Tool]
public partial class AudioPlayerComponent : BaseComponent
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

    public override void Setup(uint tick)
    {
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
    

    protected override void GetCurrentStateCustom(in ObjectState stateBuffer) { }
    protected override void SetCurrentStateCustom(in ObjectState state) { }
}