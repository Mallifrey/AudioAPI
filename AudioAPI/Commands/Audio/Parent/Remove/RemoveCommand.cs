using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using Utils.NonAllocLINQ;

namespace AudioAPI.Commands.Audio.Parent.Remove;

public class RemoveCommand : CustomCommand<RemoveCommand.Arguments>
{
    public class Arguments
    {
        [CollectionParameter(Name = "Name", Description = "Name of the audio handler.")]
        public string Handler { get; set; }
        
        [CollectionParameter(Name = "Speaker", Description = "Name of the speaker of the audio handler.")]
        public string Speaker { get; set; }
    }

    public override string Command { get; } = "remove";
    public override string Description { get; } = "Removes parent of a specific speaker";

    public override Arguments Instantiate() => new Arguments();

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, Arguments collection)
    {
        base.OnCommand(sender, ctx, collection);

        if (!AudioHandler.TryGetHandler(collection.Handler, out var handler))
        {
            ctx.RespondFail($"Handler {collection.Handler} not found");
            return;
        }

        if (collection.Speaker == "*")
        {
            handler.Speakers.ForEachValue(x => x.transform.parent = handler.GameObject.transform);
            handler.ParentTransform = null;
            
            ctx.RespondOk($"Removed parent of {handler.Speakers.Count} speaker(s)");
            return;
        }

        if (!handler.HasSpeaker(collection.Speaker, out var speaker))
        {
            ctx.RespondFail($"Speaker {collection.Speaker} not found");
            return;
        }

        speaker.transform.parent = handler.GameObject.transform;
        
        ctx.RespondFail($"Removed parent of {collection.Speaker}");
    }
}