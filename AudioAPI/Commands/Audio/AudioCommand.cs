using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace AudioAPI.Commands.Audio;

[Command("audio", "Audio management commands.")]
public partial class AudioCommand : CommandBase, IServerSideCommand { }
