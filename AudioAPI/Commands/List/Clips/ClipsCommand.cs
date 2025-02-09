using LabExtended.API;
using LabExtended.Extensions;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using NorthwoodLib.Pools;

namespace AudioAPI.Commands.List.Clips;

public class ClipsCommand : CustomCommand
{
    public override string Command { get; } = "clips";
    public override string Description { get; } = "Lists all loaded audio clips.";

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        ctx.RespondOk(StringBuilderPool.Shared.BuildString(x =>
        {
            AudioManager.LoadFiles();
            
            if (AudioManager.Clips.Count < 1)
            {
                x.AppendLine("No audio clips loaded.");
                return;
            }
            
            x.Append(AudioManager.Clips.Count);
            x.Append(" clip(s) loaded:");
            x.AppendLine();

            foreach (var clip in AudioManager.Clips)
            {
                x.Append("- ");
                x.Append(clip.Key);
                x.Append(" (");
                x.Append(clip.Value);
                x.Append(")");
                x.AppendLine();
            }
        }));
    }
}