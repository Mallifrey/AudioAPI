using LabExtended.Commands;
using LabExtended.Commands.Attributes;

namespace AudioAPI.Commands.Audio;

public partial class AudioCommand : CommandBase
{
    [CommandOverload("play", "Starts audio playback.")]
    public void AudioPlay(
        [CommandParameter("Name", "Name of the audio handler.")] string handlerName,
        [CommandParameter("Clip", "Name of the audio clip.")] string clipName
    ) {
        if (!AudioHandler.TryGetHandler(handlerName, out var handler))
        {
            Fail($"Unable to find handler for {handlerName}");
            return;
        }

        if (!AudioManager.TryGetClip(clipName, out var clip))
        {
            Fail($"Unable to find clip for {clipName}");
            return;
        }
        
        if (handler.Player.IsPlaying)
            handler.Player.Stop();
        
        handler.SetPosition(Sender.Position);
        handler.Player.Play(clip, true);
        
        Ok($"Started audio playback for {clip}");
    }
}