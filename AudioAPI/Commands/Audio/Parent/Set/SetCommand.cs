using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace AudioAPI.Commands.Audio.Parent.Set;

public class SetCommand : CustomCommand<SetCommand.Arguments>
{
    public class Arguments
    {
        [CollectionParameter(Name = "Name", Description = "Name of the audio handler.")]
        public string Handler { get; set; }
        
        [CollectionParameter(Name = "Speaker", Description = "Name of the speaker of the audio handler.")]
        public string Speaker { get; set; }

        [CollectionParameter(Name = "Target", Description = "Target of the audio handler.")]
        public ExPlayer Target { get; set; }
    }

    public override string Command { get; } = "set";
    public override string Description { get; } = "Sets the parent of a specific audio speaker.";
    
    public override Arguments Instantiate() => new Arguments();

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, Arguments collection)
    {
        base.OnCommand(sender, ctx, collection);

        if (!AudioHandler.TryGetHandler(collection.Handler, out var handler))
        {
            ctx.RespondFail($"Handler {collection.Handler} not found.");
            return;
        }

        if (collection.Speaker == "*")
        {
            handler.ParentTransform = collection.Target.CameraTransform;
            
            ctx.RespondOk($"Set {handler.Speakers.Count} speaker(s) to {collection.Target.Nickname}");
            return;
        }
        
        if (!handler.HasSpeaker(collection.Speaker, out var speaker))
        {
            ctx.RespondFail($"Speaker '{collection.Speaker}' not found.");
            return;
        }

        speaker.transform.parent = collection.Target.CameraTransform;
        
        ctx.RespondOk($"Set parent to {collection.Target.Nickname}");
    }
}