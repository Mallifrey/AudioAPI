using LabExtended.Commands;
using LabExtended.Commands.Attributes;

namespace AudioAPI.Commands.Audio;

public partial class AudioCommand : CommandBase
{
    [CommandOverload("destroy", "Destroys an audio handler.")]
    public void AudioDestroy([CommandParameter("Name", "Name of the audio handler.")] string name)
    {

        if (!AudioHandler.TryGetHandler(name, out AudioHandler audioHandler))
        {
            Fail($"Audio handler not found: {name}");
            return;
        }
        
        audioHandler.Dispose();
        
        Ok($"Audio handler has been destroyed.");
    }
}