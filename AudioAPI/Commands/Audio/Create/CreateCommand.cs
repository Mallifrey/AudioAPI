using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using System.Collections.Generic;

namespace AudioAPI.Commands.Audio.Create;

public class CreateCommand : CustomCommand
{
    public override string Command { get; } = "create";
    public override string Description { get; } = "Creates a new audio handler.";

    public override ArgumentDefinition[] BuildArgs()
    {
        return GetArgs(x =>
        {
            x.WithArg<string>("Name", "Name of the handler.");
            x.WithOptional<List<string>>("Speakers", "A list of speaker names.", new List<string>());
        });
    }

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);

        var name = args.GetString("Name");
        var speakers = args.GetList<string>("Speakers");
        
        var handler = AudioHandler.GetOrAdd(name, x =>
        {
            foreach (var speaker in speakers)
            {
                x.AddSpeaker(speaker);
            }
        });
        
        ctx.RespondOk($"Handler '{name}' created, speaker ID: {handler.Id} ({handler.Speakers.Count} speaker(s))");
    }
}