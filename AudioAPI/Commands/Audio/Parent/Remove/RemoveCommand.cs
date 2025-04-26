using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using Utils.NonAllocLINQ;

namespace AudioAPI.Commands.Audio;

public partial class AudioCommand : CommandBase
{
    [CommandOverload("parent remove", "Removes parent of a specific speaker")]
    public void AudioParentRemove(
        [CommandParameter("Name", "Name of the audio handler")] string handlerName,
        [CommandParameter("Speaker", "Name of the speaker of the audio handler.")] string speakerName
    ) {
        if (!AudioHandler.TryGetHandler(handlerName, out var handler))
        {
            Fail($"Handler {handlerName} not found");
            return;
        }

        if (speakerName == "*")
        {
            handler.Speakers.ForEachValue(x => x.transform.parent = handler.GameObject.transform);
            handler.ParentTransform = null;
            
            Ok($"Removed parent of {handler.Speakers.Count} speaker(s)");
            return;
        }

        if (!handler.HasSpeaker(speakerName, out var speaker))
        {
            Fail($"Speaker {speakerName} not found");
            return;
        }

        speaker.transform.parent = handler.GameObject.transform;
        
        Fail($"Removed parent of {speakerName}");
    }
}