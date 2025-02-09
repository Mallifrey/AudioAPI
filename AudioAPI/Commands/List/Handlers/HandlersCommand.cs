using LabExtended.API;
using LabExtended.Extensions;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using NorthwoodLib.Pools;

namespace AudioAPI.Commands.List.Handlers;

public class HandlersCommand : CustomCommand
{
    public override string Command { get; } = "handlers";
    public override string Description { get; } = "Lists all audio handlers.";

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        ctx.RespondOk(StringBuilderPool.Shared.BuildString(x =>
        {
            if (AudioHandler.Handlers.Count < 1)
            {
                x.AppendLine("No handlers found.");
                return;
            }

            x.Append(AudioHandler.Handlers.Count);
            x.Append(" audio handler(s):");
            x.AppendLine();
            
            foreach (var handler in AudioHandler.Handlers)
            {
                x.Append("- ");
                x.Append(handler.Key);
                x.AppendLine();
            }
        }));
    }
}