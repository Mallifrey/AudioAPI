using LabExtended.Commands;
using LabExtended.Commands.Attributes;

namespace AudioAPI.Commands.Audio;

public partial class AudioCommand : CommandBase
{
    [CommandOverload("pause", "Pauses/resumes an audio handler.")]
    public void AudioPause([CommandParameter("Name", "Name of the audio handler")] string name)
    {
        if (!AudioHandler.TryGetHandler(name, out var handler))
        {
            Fail($"Unknown audio handler");
            return;
        }
        
        handler.Player.TogglePause();
        
        if (handler.Player.IsPaused)
            Ok("Handler paused.");
        else
            Ok("Handler resumed.");
    }
}