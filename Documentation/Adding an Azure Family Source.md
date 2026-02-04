# Adding an Azure Family Source

An Azure family source connects Bim.FamilyManager to a Microsoft Azure Storage account, allowing access to cloud-hosted family libraries.

## Fields

- **Name**  
  *A user-friendly name for the Azure source (e.g., "Company Cloud Families").*  
  **Required**

- **Azure Storage Account Name**  
  *The name of your Azure Storage account.*  
  **Required**

- **Azure Container Name**  
  *The name of the blob container where family files are stored.*  
  **Required**

- **Access Key / SAS Token**  
  *The access key or Shared Access Signature (SAS) token for authentication.*  
  **Required**

- **Directory Path (within container)**  
  *The path inside the container where families are located (can be left blank for root).*  
  Optional

- **Include Subfolders**  
  *If enabled, Bim.FamilyManager will search all subfolders within the specified container path.*  
  Optional (default: enabled)

- **Active**  
  *Indicates whether this source is currently active and available in the manager.*  
  Optional (default: enabled)

- **Description**  
  *Optional notes or comments about the Azure source.*  
  Optional

## Steps

1. Enter a unique name for the source.
2. Fill in the Azure Storage account and container names.
3. Provide the access key or SAS token.
4. Specify the directory path if needed.
5. Enable or disable subfolder inclusion.
6. Optionally add a description.
7. Click "Save" or "Add" to register the source.

[Back to Managing Family Sources](Managing%20Family%20Sources.md)