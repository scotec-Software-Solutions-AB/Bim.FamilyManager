using System.Reflection;
using System.Runtime.Loader;
using Autofac;
using Bim.FamilyManager.Base.Options;
using Bim.FamilyManager.Base.Settings;
using Module = Autofac.Module;

namespace Bim.FamilyManager.Modules;

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

        builder.Register(context => typeof(DisplayOptions))
               .Keyed<Type>("DisplayOptions")
               .SingleInstance();

        builder.RegisterType<SettingsManager>()
               .SingleInstance();

        LoadOtherModules(builder);
    }

    /// <summary>
    ///     Loads additional modules into the dependency injection container.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="Autofac.ContainerBuilder" /> used to register additional modules.
    /// </param>
    /// <remarks>
    ///     This method identifies and registers all modules from assemblies loaded in the current
    ///     <see cref="System.Runtime.Loader.AssemblyLoadContext" />, excluding the executing assembly.
    ///     It ensures that dependencies from other modules are properly included in the container.
    /// </remarks>
    private void LoadOtherModules(ContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var context = AssemblyLoadContext.GetLoadContext(assembly);
        if (context is null)
        {
            return;
        }

        builder.RegisterAssemblyModules(context.Assemblies.Where(a => a != assembly).ToArray());
    }
}
