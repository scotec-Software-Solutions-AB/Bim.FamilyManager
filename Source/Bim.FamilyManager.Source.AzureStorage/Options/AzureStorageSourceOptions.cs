using Bim.FamilyManager.Base.Options;

namespace Bim.FamilyManager.Source.AzureStorage.Options;

/// <summary>
///     Represents the configuration options for an Azure-based family source in the Revit Family Manager.
/// </summary>
[FamilySourceOptions(OptionsName = "AzureStorageSource")]
public class AzureStorageSourceOptions : FamilySourceOptions
{
    /// <summary>
    ///     Gets or sets the Azure Storage client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the Azure Storage tenant ID.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    ///<summary>
    ///     Gets or sets the Azure Storage endpoint URL.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    ///     Gets or sets the name of the Azure Blob container.
    /// </summary>
    public string ContainerName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the virtual folder path within the container (optional).
    /// </summary>
    public string RootPath { get; set; } = string.Empty;
    
    public string? UserName { get; set; }
}
