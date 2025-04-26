using LabExtended.Commands;
using LabExtended.Commands.Attributes;

namespace AudioAPI.Commands.Audio;

public partial class AudioCommand : CommandBase
{
    [CommandOverload("volume", "Changes the volume of a specific audio handler.")]
    public void AduioVolume(
        [CommandParameter("Name", "Name of the audio handler")] string name,
        [CommandParameter("Volume", "New volume level.")] float volume
    ) {
        if (!AudioHandler.TryGetHandler(name, out var handler))
        {
            Fail($"Unknown audio handler: {name}");
            return;
        }
        
        handler.SetVolume(volume / 100f);
        handler.Player.Volume = volume;
        
        Ok($"Volume changed to {volume}%");
    }
}