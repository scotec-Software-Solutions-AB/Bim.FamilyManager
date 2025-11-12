# BIM Family Manager

**BIM Family Manager** is a powerful tool designed to streamline the management, organization, and deployment of Autodesk® Revit® family files within architectural and engineering workflows. Built on .NET 8, it offers a modern, user-friendly interface for handling Revit families from various sources, including local directories and Azure Storage.

## Features

- **Centralized Family Management:**  
Organize, browse, and manage your Revit family files seamlessly within the Revit environment.

- **Multiple Source Support:**  
Connect to local folders or cloud storage (Azure Storage) to access and synchronize family libraries.

- **Modern UI:**  
Offers both standard and modern user interfaces, with customizable themes and layouts.

- **Drag & Drop:**  
Easily add families using intuitive drag-and-drop functionality.

- **Settings Management:**  
Configure display, layout, and source settings to fit your workflow.

- **Extensible Architecture:**  
Designed for easy integration of additional storage providers or UI customizations.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Windows OS (recommended for Revit integration)

### Building the Project

**1. Clone the repository**
```
git clone https://github.com/scotec-Software-Solutions-AB/Bim.FamilyManager.git
cd family-manager
```

**2. Build the solution using Visual Studio 2022 or via command line**
```
dotnet build
```

**3. Deploy the Add-in**  
   Copy the built add-in files (DLLs and the corresponding `.addin` manifest file) to your Revit Add-ins folder.  
   The typical path for Revit 2025 is:
   ```
   C:\ProgramData\Autodesk\Revit\Addins\2025\
   ```
   or
  ```
   C:\Users\Olaf\AppData\Roaming\Autodesk\Revit\Addins\2025\
   ```  

**4. Start Revit**  
   Launch Autodesk Revit. The BIM Family Manager add-in will be available in the "scotec" tab.

**5. Using BIM Family Manager**  
   Access the Family Manager from the Add-Ins tab and configure your sources and settings as needed.

⚠️ Note: BIM Family Manager requires Revit 2025 or newer.

## Project Structure

- `Bim.FamilyManager`  
  Core logic and main entry point for the Family Manager add-in.

- `Bim.FamilyManager.Abstractions`  
  Contains shared interfaces and abstractions used across the Family Manager solution.

- `Bim.FamilyManager.Base`  
  Provides base implementations and common functionality for other modules.

- `Bim.FamilyManager.Ui`  
  Shared UI components, dialogs, and themes (including styles and resource dictionaries).

- `Bim.FamilyManager.Ui.Standard`  
  Standard/classic WPF user interface for Family Manager.

- `Bim.FamilyManager.Ui.Modern`  
  Modern WPF user interface for Family Manager, featuring updated layouts and controls.

- `Bim.FamilyManager.Source.Directory`  
  Integration for managing Revit family files from local or network directories.

- `Bim.FamilyManager.Source.AzureStorage`  
  Integration for managing Revit family files in Azure Storage.


## Screenshots

*(Add screenshots of the main UI, settings, and family management views here)*


## Professional Support & Custom Solutions

Need help getting started or customizing BIM Family Manager? Visit our [homepage](https://www.scotec-software.com/revitaddins) for professional support, tailored solutions, and consulting services.

We also offer custom Revit add-in development to match your specific workflows and integration needs.

## Contributing

We welcome and appreciate your contributions to BIM Family Manager! By contributing, you help improve a tool that supports the Revit and BIM community, making family management more efficient for everyone.

### Why Contribute?

- **Make a Difference:** Your improvements and ideas can help thousands of Revit users streamline their workflows.
- **Learn and Grow:** Collaborate with other developers, learn new skills, and gain experience working on a modern .NET 8 project.
- **Shape the Future:** Influence the direction of BIM Family Manager by suggesting features or enhancements that matter to you.

### How You Can Help

- **Report Issues:** Found a bug or have a suggestion? [Open an issue](https://github.com/your-org/family-manager/issues) to let us know.
- **Submit Pull Requests:** Fix bugs, add new features, or improve documentation. All contributions are reviewed and discussed.
- **Improve Documentation:** Help make our guides, examples, and explanations clearer for everyone.
- **Test and Review:** Try out new features, test on different Revit versions, and provide feedback.
- **Share Ideas:** Propose new features or enhancements that could benefit the community.

### Get Started

1. Fork the repository and create your branch from `main`.
2. Make your changes with clear, descriptive commit messages.
3. Ensure your code follows the existing style and passes all builds.
4. Submit a pull request and describe your changes.

We’re excited to collaborate with you! Whether you’re a seasoned developer or new to open source, your input is valuable. Join us in making BIM Family Manager the best it can be!

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

*BIM Family Manager is an independent open-source project and is not affiliated with, endorsed by, or sponsored by Autodesk, Inc. Revit is a registered trademark of Autodesk, Inc.*
  
    