using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace AudioAPI.Commands.Audio.Play;

public class PlayCommand : CustomCommand<PlayCommand.Arguments>
{
    public class Arguments
    {
        [CollectionParameter(Name = "Name", Description = "Name of the audio handler.")]
        public string Handler { get; set; } = string.Empty;

        [CollectionParameter(Name = "Clip", Description = "Name of the audio clip.")]
        public string Clip { get; set; } = string.Empty;
    }

    public override string Command { get; } = "play";
    public override string Description { get; } = "Starts audio playback.";

    public override Arguments Instantiate() => new Arguments();

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, Arguments collection)
    {
        base.OnCommand(sender, ctx, collection);

        if (!AudioHandler.TryGetHandler(collection.Handler, out var handler))
        {
            ctx.RespondFail($"Unable to find handler for {collection.Handler}");
            return;
        }

        if (!AudioManager.TryGetClip(collection.Clip, out var clip))
        {
            ctx.RespondFail($"Unable to find clip for {collection.Clip}");
            return;
        }
        
        if (handler.Player.IsPlaying)
            handler.Player.Stop();
        
        handler.SetPosition(sender.Position);
        handler.Player.Play(clip, true);
        
        ctx.RespondOk($"Started audio playback for {collection.Clip}");
    }
}