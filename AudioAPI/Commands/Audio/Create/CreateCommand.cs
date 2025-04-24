using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Parameters;
using System.Collections.Generic;

namespace AudioAPI.Commands.Audio;

public partial class AudioCommand : CommandBase
{
    [CommandOverload("create", "Creates a new audio handler.")]
    public void AudioCreate(
        [CommandParameter("Name", "Name of the handler.")] string name,
        [CommandParameter("SpeakerNames", "A list of speaker names.")] List<string> speakers = null
    ) {
        var handler = AudioHandler.GetOrAdd(name, x =>
        {
            foreach (var speaker in speakers)
            {
                x.AddSpeaker(speaker);
            }
        });
        
        Ok($"Handler '{name}' created, speaker ID: {handler.Id} ({handler.Speakers.Count} speaker(s))");
    }
}