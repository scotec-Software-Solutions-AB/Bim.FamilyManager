using Autofac;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Source.Directory.Logic;
using Bim.FamilyManager.Source.Directory.Options;
using Bim.FamilyManager.Source.Directory.ViewModels.Settings;
using Bim.FamilyManager.Source.Directory.Views.Settings;
using Scotec.Wpf.ViewModels;
using Module = Autofac.Module;

namespace Bim.FamilyManager.Source.Directory.Modules;

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
    private const string DirectorySource = "DirectorySource";
    private const string DirectorySourceOptions = "DirectorySourceOptions";

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

        builder.RegisterType<DirectorySource>()
               .InstancePerDependency();

        builder.Register(context => typeof(DirectorySource))
               .Keyed<Type>(DirectorySource)
               .SingleInstance();

        builder.RegisterType<DirectorySourceOptions>()
               .Keyed<IFamilySourceOptions>(DirectorySource)
               .As<IFamilySourceOptions>()
               .InstancePerDependency();

        builder.Register(context => typeof(DirectorySourceOptions))
               .Keyed<Type>(DirectorySourceOptions)
               .SingleInstance();

        builder.RegisterType<DirectorySourceSettingsViewModel>()
               .Keyed<IFamilySourceSettingsViewModel>(DirectorySource)
               .InstancePerDependency();

        builder.RegisterType<ViewModelDescriptor<DirectorySourceSettingsViewModel, DirectorySourceSettingsView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();
    }
}
