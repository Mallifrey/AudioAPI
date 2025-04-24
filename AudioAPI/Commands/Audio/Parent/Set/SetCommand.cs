using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;

namespace AudioAPI.Commands.Audio;

public partial class AudioCommand : CommandBase
{
    [CommandOverload("parent set", "Sets the parent of a specific audio speaker.")]
    public void AudioParentSet(
        [CommandParameter("Name", "Name of the audio handler.")] string handlerName,
        [CommandParameter("Speaker", "Name of the speaker of the audio handler.")] string speakerName,
        [CommandParameter("Target", "Target of the audio handler. (or Sender)")] ExPlayer target = null
    ) {
        target ??= Sender;

        if (!AudioHandler.TryGetHandler(handlerName, out var handler))
        {
            Fail($"Handler {handlerName} not found.");
            return;
        }

        if (speakerName == "*")
        {
            handler.ParentTransform = target.CameraTransform;
            
            Ok($"Set {handler.Speakers.Count} speaker(s) to {target.Nickname}");
            return;
        }
        
        if (!handler.HasSpeaker(speakerName, out var speaker))
        {
            Fail($"Speaker '{speakerName}' not found.");
            return;
        }

        speaker.transform.parent = target.CameraTransform;
        
        Ok($"Set parent to {target.Nickname}");
    }
}