# BIM Family Manager

**BIM Family Manager** is a powerful tool designed to streamline the management, organization, and deployment of Autodesk® Revit® family files within architectural and engineering workflows. Built on .NET 8, it offers a modern, user-friendly interface for handling Revit families from various sources, including local directories and Azure Storage.

## Features

- **Centralized Family Management:**  
Organize, browse, and manage your Revit family files seamlessly within the Revit environment.

- **Multiple Source Support:**  
Connect to local folders or cloud storage (Azure Storage) to access and synchronize family libraries.

- **Modern UI:**  
Offers a modern user interface, with customizable themes and layouts.

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
   C:\Users\YourUser\AppData\Roaming\Autodesk\Revit\Addins\2025\
   ```  

**4. Launch Revit**  
Start Autodesk Revit. The BIM Family Manager add-in will appear in the "scotec" tab.

**5. Configure & Use**  
Access BIM Family Manager from the Add-Ins tab and set up your sources and preferences.

> **Note:** BIM Family Manager requires Revit 2025 or newer.

## Project Structure

- `Bim.FamilyManager`  
Main logic and entry point for the add-in.

- `Bim.FamilyManager.Abstractions`  
Shared interfaces and abstractions.

- `Bim.FamilyManager.Base`  
Base implementations and common utilities.

- `Bim.FamilyManager.Ui`  
Shared UI components, dialogs, and themes.

- `Bim.FamilyManager.Ui.Standard`  
Classic WPF user interface.

- `Bim.FamilyManager.Ui.Modern`  
Modern WPF user interface with updated layouts.

- `Bim.FamilyManager.Source.Directory`  
Integration for local/network directory management.

- `Bim.FamilyManager.Source.AzureStorage`  
Integration for Azure Storage management.

## Screenshots


![Revit with Bim.FamilyManager](./Documentation/Images/family_manager_revit.png "Revit with Bim.FamilyManager")

![Bim.FamilyManager](./Documentation/Images/family_manager_modern_view.png "Bim.FamilyManager")

![Bim.FamilyManager Family Sources](./Documentation/Images/settings_family_sources.png "Bim.FamilyManager Family Sources")


## Professional Support & Custom Solutions

Need assistance or custom features? Visit our [homepage](https://www.scotec-software.com/revitaddins) for professional support, tailored solutions, and consulting services.

We also offer custom Revit add-in development to fit your specific workflows and integration needs.

## Contributing

We welcome contributions to BIM Family Manager! Your input helps improve a tool that benefits the Revit and BIM community.

### Why Contribute?

- **Make an Impact:** Help thousands of Revit users optimize their workflows.
- **Grow Your Skills:** Collaborate, learn, and gain experience with a modern Revit add-in.
- **Shape the Project:** Suggest features and enhancements that matter to you.

### How to Contribute

- **Report Issues:** Found a bug or have a suggestion? [Open an issue](https://github.com/scotec-Software-Solutions-AB/Bim.FamilyManager/issues).
- **Submit Pull Requests:** Fix bugs, add features, or improve documentation.
- **Enhance Documentation:** Make guides and examples clearer for everyone.
- **Test & Review:** Try new features, test on different Revit versions, and provide feedback.
- **Share Ideas:** Propose new features or improvements.

### Getting Started

1. Fork the repository and create your branch from `develop`.
2. Make changes with clear, descriptive commit messages.
3. Ensure your code follows the existing style and passes all builds.
4. Submit a pull request with a description of your changes.

We’re excited to collaborate! Whether you’re experienced or new to open source, your input is valuable. Help us make BIM Family Manager the best it can be!

## License

This project is licensed under the MIT License. See the [LICENSE](license.txt) file for details.

*Autodesk, the Autodesk logo, Revit, and other Autodesk marks are registered trademarks or trademarks of Autodesk, Inc. and/or its subsidiaries and/or affiliates in the USA and/or other countries. BIM Family Manager is an independent open-source project and is not affiliated with, sponsored, authorized, or endorsed by Autodesk, Inc. or any of its subsidiaries.*