using LabExtended.Extensions;

using LabExtended.Commands;
using NorthwoodLib.Pools;
using LabExtended.Commands.Attributes;

namespace AudioAPI.Commands.List;

public partial class ListCommand : CommandBase
{
    [CommandOverload("clips", "Lists all loaded audio clips.")]
    public void ListClips()
    {
        Ok(StringBuilderPool.Shared.BuildString(x =>
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