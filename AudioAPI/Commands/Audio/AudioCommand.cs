using AudioAPI.Commands.Audio.Create;
using AudioAPI.Commands.Audio.Destroy;
using AudioAPI.Commands.Audio.Pause;
using AudioAPI.Commands.Audio.Play;
using AudioAPI.Commands.Audio.Stop;
using AudioAPI.Commands.Audio.Volume;

using CommandSystem;

using LabExtended.Commands;

namespace AudioAPI.Commands.Audio;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class AudioCommand : VanillaParentCommandBase
{
    public override string Command { get; } = "audio";
    public override string Description { get; } = "Audio management commands.";

    public override void LoadGeneratedCommands()
    {
        base.LoadGeneratedCommands();
        
        RegisterCommand(new CreateCommand());
        RegisterCommand(new DestroyCommand());
        RegisterCommand(new VolumeCommand());
        RegisterCommand(new PlayCommand());
        RegisterCommand(new PauseCommand());
        RegisterCommand(new StopCommand());
        
        RegisterCommand(new Parent.ParentCommand());
    }
}