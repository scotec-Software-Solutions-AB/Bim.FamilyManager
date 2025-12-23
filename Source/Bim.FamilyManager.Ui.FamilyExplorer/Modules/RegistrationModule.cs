using Autofac;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Ui.FamilyExplorer.Options;
using Bim.FamilyManager.Ui.FamilyExplorer.ViewModels;
using Bim.FamilyManager.Ui.FamilyExplorer.Views;
using Bim.FamilyManager.Ui.Views;
using Scotec.Wpf.ViewModels;
using Module = Autofac.Module;

namespace Bim.FamilyManager.Ui.FamilyExplorer.Modules;

/// <summary>
///     Represents a module for registering dependencies and services related to the Family Manager functionality
///     in the Scotec Revit Family Manager application.
/// </summary>
/// <remarks>
///     This module is responsible for configuring and registering view models, views, and other services
///     required for the Family Manager feature. It utilizes Autofac for dependency injection and ensures
///     proper lifecycle management of registered components.
/// </remarks>
public class RegistrationModule : Module
{
    private const string LayoutName = "FamilyExplorerLayout";

    /// <summary>
    ///     Registers various components, view models, views, and services into the dependency injection container.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="Autofac.ContainerBuilder" /> used to register components and services.
    /// </param>
    /// <remarks>
    ///     This method overrides the base <see cref="Autofac.Module.Load" /> method to configure
    ///     the dependency injection container with specific registrations required for the application.
    ///     It includes registrations for view models, views, descriptors, and other services.
    /// </remarks>
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<FamilyManagerViewModel>()
               .Keyed<IFamilyManagerViewModel>(LayoutName)
               .InstancePerDependency();
        builder.RegisterType<ViewModelDescriptor<FamilyManagerViewModel, FamilyManagerView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();

        builder.RegisterType<FamilySourceViewModel>()
               .InstancePerDependency();

        builder.RegisterType<FolderViewModel>()
               .InstancePerDependency();
        builder.RegisterType<ViewModelDescriptor<FolderViewModel, FolderView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();

        builder.RegisterType<FamilyViewModel>()
               .InstancePerDependency();
        builder.RegisterType<ViewModelDescriptor<FamilyViewModel, FamilyView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();

        builder.RegisterType<FamilySymbolViewModel>()
               .InstancePerDependency();

        builder.RegisterType<LayoutOptionsViewModel>()
               .Keyed<ILayoutOptionsViewModel>(LayoutName)
               .InstancePerDependency();
        builder.RegisterType<ViewModelDescriptor<LayoutOptionsViewModel, LayoutSettingsView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();

        builder.Register(context => typeof(FamilyExplorerLayoutOptions))
               .Keyed<Type>(LayoutName)
               .SingleInstance();
    }
}
