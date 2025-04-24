using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace AudioAPI.Commands.List;

[Command("audiolist", "Parent command for listing audio related stuff.", [ "alist" ])]
public partial class ListCommand : CommandBase, IServerSideCommand { }
