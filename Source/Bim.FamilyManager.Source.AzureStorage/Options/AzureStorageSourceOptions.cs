using Bim.FamilyManager.Base.Options;

namespace Bim.FamilyManager.Source.AzureStorage.Options;

/// <summary>
///     Represents the configuration options for an Azure-based family source in the Revit Family Manager.
/// </summary>
[FamilySourceOptions(OptionsName = "AzureStorageSource")]
public class AzureStorageSourceOptions : FamilySourceOptions
{
    /// <summary>
    ///     Gets or sets the Azure Active Directory client ID used for authentication.
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    ///     Gets or sets the Azure Active Directory tenant ID used for authentication.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    ///     Gets or sets the Azure Storage endpoint URL (e.g., https://account.blob.core.windows.net).
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the name of the Azure Blob container where families are stored.
    /// </summary>
    public string ContainerName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the virtual folder path within the container. This is optional and can be used to scope the source to
    ///     a subfolder.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the user name associated with the Azure storage source. This is optional and may be used for display
    ///     or auditing purposes.
    /// </summary>
    public string? UserName { get; set; }
}
