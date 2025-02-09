using AudioAPI.Commands.List.Clips;
using AudioAPI.Commands.List.Handlers;

using CommandSystem;

using LabExtended.Commands;

namespace AudioAPI.Commands.List;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class ListCommand : VanillaParentCommandBase
{
    public override string Command { get; } = "audiolist";
    public override string[] Aliases { get; } = new string[] { "alist" };

    public override string Description { get; } = "Parent command for listing audio related stuff.";

    public override void LoadGeneratedCommands()
    {
        base.LoadGeneratedCommands();
        
        RegisterCommand(new ClipsCommand());
        RegisterCommand(new HandlersCommand());
    }
}