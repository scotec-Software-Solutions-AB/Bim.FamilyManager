using Autofac;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Base.Logic;
using Bim.FamilyManager.Base.Options;
using Bim.FamilyManager.Base.Settings;
using Microsoft.Extensions.Logging;
using Module = Autofac.Module;

namespace Bim.FamilyManager.Base.Modules;

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

        //builder.RegisterType<JsonFamilySourceConverter>()
        //       .InstancePerDependency();

        builder.RegisterType<RevitFamily>()
               .InstancePerDependency();
        builder.Register<IRevitFamily.Factory>(context =>
               {
                   var componentContext = context.Resolve<IComponentContext>();
                   return (name, familyInfo, saveAction) =>
                       new RevitFamily(name, familyInfo, saveAction, componentContext.Resolve<ILogger<RevitFamily>>());
               })
               .SingleInstance();

        builder.RegisterType<Logic.FamilyManager>()
               .As<IFamilyManager>()
               .SingleInstance();

        builder.Register(context => typeof(DisplayOptions))
               .Keyed<Type>("DisplayOptions")
               .SingleInstance();

        builder.RegisterType<SettingsManager>()
               .SingleInstance();

        builder.Register<IFamilySourceOptions.Factory>(context =>
        {
            var componentContext = context.Resolve<IComponentContext>();
            return key =>
            {
                var service = componentContext.ResolveKeyed<IFamilySourceOptions>(key);

                return service;
            };
        });

        builder.Register<ILayoutOptions.Factory>(context =>
        {
            var componentContext = context.Resolve<IComponentContext>();
            return key =>
            {
                var service = componentContext.ResolveKeyed<ILayoutOptions>(key);

                return service;
            };
        });
    }
}
