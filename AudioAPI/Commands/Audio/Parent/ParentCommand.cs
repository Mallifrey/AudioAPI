using AudioAPI.Commands.Audio.Parent.Remove;
using AudioAPI.Commands.Audio.Parent.Set;

using LabExtended.Commands;

namespace AudioAPI.Commands.Audio.Parent;

public class ParentCommand : VanillaParentCommandBase
{
    public override string Command { get; } = "parent";
    public override string Description { get; } = "Commands to manage primitive parents.";

    public override void LoadGeneratedCommands()
    {
        base.LoadGeneratedCommands();
        
        RegisterCommand(new RemoveCommand());
        RegisterCommand(new SetCommand());
    }
}