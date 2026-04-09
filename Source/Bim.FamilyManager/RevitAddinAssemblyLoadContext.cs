namespace Bim.FamilyManager;

internal partial class RevitAddinAssemblyLoadContext
{
    partial void OnInitialize()
    {
        AddPreloadedAssemblies(["Scotec.Wpf.Controls"]);
    }
}
