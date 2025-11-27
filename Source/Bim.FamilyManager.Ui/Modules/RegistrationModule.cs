using Autofac;
using Autofac.Core;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Ui.ViewModels;
using Bim.FamilyManager.Ui.ViewModels.Settings;
using Bim.FamilyManager.Ui.Views;
using Bim.FamilyManager.Ui.Views.Settings;
using Scotec.Extensions.Utilities.Configuration;
using Scotec.Wpf.ViewModels;
using Module = Autofac.Module;

namespace Bim.FamilyManager.Ui.Modules;

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

        builder.RegisterType<FamilyDropViewModel>()
               .InstancePerDependency();

        builder.RegisterType<FamilyDropWindow>()
               .InstancePerDependency();

        builder.RegisterType<FamilyManagerPane>()
               .InstancePerDependency();

        builder.RegisterType<FamilyDropHandler>()
               .SingleInstance();

        builder.RegisterType<SettingsManagerWindow>()
               .InstancePerDependency();

        builder.RegisterType<SettingsManagerViewModel>()
               .InstancePerLifetimeScope();

        builder.RegisterType<FamilySourcesSettingsViewModel>()
               .As<ISettingsViewModel>()
               .InstancePerDependency();

        builder.RegisterType<ViewModelDescriptor<FamilySourcesSettingsViewModel, FamilySourcesSettingsView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();

        builder.RegisterType<DisplaySettingsViewModel>()
               .As<ISettingsViewModel>()
               .InstancePerDependency();

        builder.RegisterType<ViewModelDescriptor<DisplaySettingsViewModel, DisplaySettingsView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();

        builder.RegisterType<FamilySourceSettingsEditViewModel>()
               .InstancePerDependency();

        builder.RegisterType<ViewModelDescriptor<FamilySourceSettingsEditViewModel, FamilySourceSettingsEditView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();

        builder.RegisterType<FamilySourceSelectionViewModel>()
               .InstancePerDependency();

        builder.RegisterType<ViewModelDescriptor<FamilySourceSelectionViewModel, FamilySourceSelectionView>>()
               .As<IViewModelDescriptor>()
               .SingleInstance();

        builder.RegisterType<SettingsManager>()
               .SingleInstance();

        builder.Register<IFamilySourceSettingsViewModel.Factory>(context =>
        {
            var componentContext = context.Resolve<IComponentContext>();
            return options =>
            {
                var parameters = new Parameter[]
                {
                    new TypedParameter(options.GetType(), options)
                    // Autofac uses the type of the parameter to match it to the constructor arguments, and since both applyAction
                    // and cancelAction have the same type, Autofac cannot distinguish between them. As a result, it ends up passing
                    // the same value (applyAction) for both parameters. 
                    // To fix this, we need to use named parameters or parameter injection to explicitly specify which value corresponds
                    // to which constructor argument.
                };
                var service = componentContext.ResolveKeyed<IFamilySourceSettingsViewModel>(options.Type, parameters);

                return service;
            };
        });

        builder.Register<ILayoutOptionsViewModel.Factory>(context =>
        {
            var componentContext = context.Resolve<IComponentContext>();
            return (key, options) =>
            {
                var parameters = new Parameter[]
                {
                    new TypedParameter(options.GetType(), options)
                };

                var service = componentContext.ResolveKeyed<ILayoutOptionsViewModel>(key, parameters);

                return service;
            };
        });

        builder.Register<IFamilySourcePanelViewModel.Factory>(ctx =>
        {
            var c = ctx.Resolve<IComponentContext>();
            return familySource => c.ResolveOptionalKeyed<IFamilySourcePanelViewModel>(familySource.Type,
                new TypedParameter(familySource.GetType(), familySource));
        });
    }
}
