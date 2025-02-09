using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace AudioAPI.Commands.Audio.Stop;

public class StopCommand : CustomCommand
{ 
    public override string Command { get; } = "stop";
    public override string Description { get; } = "Stops an audio handler.";

    public override ArgumentDefinition[] BuildArgs() => GetArg<string>("Name", "Name of the audio handler");

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);

        if (!AudioHandler.TryGetHandler(args.GetString("Name"), out var handler))
        {
            ctx.RespondFail($"Unknown audio handler");
            return;
        }

        if (!handler.Player.IsPlaying)
        {
            ctx.RespondFail("This audio handler is not playing.");
            return;
        }

        handler.Player.Stop(true);
        
        ctx.RespondOk($"Audio handler stopped.");
    }
}