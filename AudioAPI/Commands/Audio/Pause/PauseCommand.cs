using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace AudioAPI.Commands.Audio.Pause;

public class PauseCommand : CustomCommand
{ 
    public override string Command { get; } = "pause";
    public override string Description { get; } = "Pauses/resumes an audio handler.";

    public override ArgumentDefinition[] BuildArgs() => GetArg<string>("Name", "Name of the audio handler");

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);

        if (!AudioHandler.TryGetHandler(args.GetString("Name"), out var handler))
        {
            ctx.RespondFail($"Unknown audio handler");
            return;
        }
        
        handler.Player.TogglePause();
        
        if (handler.Player.IsPaused)
            ctx.RespondOk("Handler paused.");
        else
            ctx.RespondOk("Handler resumed.");
    }
}