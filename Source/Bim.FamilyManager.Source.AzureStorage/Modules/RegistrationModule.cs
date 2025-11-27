using Autofac;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Source.AzureStorage.Logic;
using Bim.FamilyManager.Source.AzureStorage.Options;
using Bim.FamilyManager.Source.AzureStorage.ViewModels;
using Bim.FamilyManager.Source.AzureStorage.ViewModels.Settings;
using Bim.FamilyManager.Source.AzureStorage.Views;
using Bim.FamilyManager.Source.AzureStorage.Views.Settings;
using Scotec.Identity.AzureActiveDirectory;
using Scotec.Wpf.ViewModels;
using Module = Autofac.Module;

namespace Bim.FamilyManager.Source.AzureStorage.Modules;

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
    private const string AzureStorageSource = "AzureStorageSource";
    private const string AzureStorageSourceOptions = "AzureStorageSourceOptions";

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

        builder.RegisterType<AadAuthService>()
               .As<IAadAuthService>()
               .SingleInstance();

        builder.RegisterType<AzureStorageSource>()
               .InstancePerDependency();

        builder.Register(context => typeof(AzureStorageSource))
               .Keyed<Type>(AzureStorageSource)
               .SingleInstance();

        builder.RegisterType<AzureStorageSourceOptions>()
               .Keyed<IFamilySourceOptions>(AzureStorageSource)
               .As<IFamilySourceOptions>()
               .InstancePerDependency();

        builder.Register(context => typeof(AzureStorageSourceOptions))
               .Keyed<Type>(AzureStorageSourceOptions)
               .SingleInstance();

        builder.RegisterType<AzureStorageSourceSettingsViewModel>()
               .Keyed<IFamilySourceSettingsViewModel>(AzureStorageSource)
               .InstancePerDependency();

        builder.RegisterType<ViewModelDescriptor<AzureStorageSourceSettingsViewModel, AzureStorageSourceSettingsView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();

        builder.RegisterType<AzureStorageSourcePanelViewModel>()
               .Keyed<IFamilySourcePanelViewModel>(AzureStorageSource)
               .InstancePerDependency();
        builder.RegisterType<ViewModelDescriptor<AzureStorageSourcePanelViewModel, AzureStorageSourcePanelView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();
    }
}
