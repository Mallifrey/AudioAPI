using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace AudioAPI.Commands.Audio.Destroy;

public class DestroyCommand : CustomCommand<DestroyCommand.Arguments>
{
    public class Arguments
    {
        [CollectionParameter(Name = "Name", Description = "Name of the audio handler.")]
        public string Name { get; set; } = string.Empty;
    }
    
    public override string Command { get; } = "destroy";
    public override string Description { get; } = "Destroys an audio handler.";

    public override Arguments Instantiate() => new Arguments();

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, Arguments collection)
    {
        base.OnCommand(sender, ctx, collection);

        if (!AudioHandler.TryGetHandler(collection.Name, out AudioHandler audioHandler))
        {
            ctx.RespondFail($"Audio handler not found: {collection.Name}");
            return;
        }
        
        audioHandler.Dispose();
        
        ctx.RespondOk($"Audio handler has been destroyed.");
    }
}