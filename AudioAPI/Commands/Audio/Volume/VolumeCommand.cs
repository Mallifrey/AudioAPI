using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace AudioAPI.Commands.Audio.Volume;

public class VolumeCommand : CustomCommand
{ 
    public override string Command { get; } = "volume";
    public override string Description { get; } = "Changes the volume of a specific audio handler.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArgs(x =>
        {
            x.WithArg<string>("Name", "Name of the audio handler");
            x.WithArg<float>("Volume", "New volume level.");
        });
    }

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);

        var name = args.GetString("Name");
        var volume = args.Get<float>("Volume");

        if (!AudioHandler.TryGetHandler(name, out var handler))
        {
            ctx.RespondFail($"Unknown audio handler: {name}");
            return;
        }
        
        handler.SetVolume(volume / 100f);
        handler.Player.Volume = volume;
        
        ctx.RespondOk($"Volume changed to {volume}%");
    }
}