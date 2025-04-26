using LabExtended.Commands;
using LabExtended.Commands.Attributes;

namespace AudioAPI.Commands.Audio;

public partial class AudioCommand : CommandBase
{
    [CommandOverload("stop", "Stops an audio handler.")]
    public void AudioStop([CommandParameter("Name", "Name of the audio handler")] string name)
    {
        if (!AudioHandler.TryGetHandler(name, out var handler))
        {
            Fail($"Unknown audio handler");
            return;
        }

        if (!handler.Player.IsPlaying)
        {
            Fail("This audio handler is not playing.");
            return;
        }

        handler.Player.Stop(true);
        
        Ok($"Audio handler stopped.");
    }
}