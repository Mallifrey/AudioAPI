using LabExtended.Extensions;

using LabExtended.Commands;

using NorthwoodLib.Pools;
using LabExtended.Commands.Attributes;

namespace AudioAPI.Commands.List;

public partial class ListCommand : CommandBase
{
    [CommandOverload("handlers", "Lists all audio handlers.")]
    public void ListHandlers ()
    {
        Ok(StringBuilderPool.Shared.BuildString(x =>
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