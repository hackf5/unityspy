namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System;
    using System.Reflection;
    using Autofac;
    using Autofac.Builder;
    using Autofac.Util;

    public static class AutofacExtensions
    {
        public static IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> AsSingleton<TLimit>(
            this IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> @this) =>
            @this.AsImplementedInterfaces().AsSelf().SingleInstance();

        public static IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> AsTransient<TLimit>(
            this IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, SingleRegistrationStyle> @this) =>
            @this.AsImplementedInterfaces().AsSelf().InstancePerDependency();

        public static void AutoRegisterAssemblyTypes(this ContainerBuilder builder, Assembly assembly)
        {
            foreach (var type in assembly.GetLoadableTypes())
            {
                if (type.GetCustomAttribute<RegisterAttribute>() is var attribute && attribute != null)
                {
                    switch (attribute.Type)
                    {
                        case RegistrationType.Transient:
                            builder.RegisterType(type).AsTransient();
                            break;
                        case RegistrationType.Singleton:
                            builder.RegisterType(type).AsSingleton();
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"Unknown registration type {attribute.Type} on type {type}.");
                    }
                }
            }
        }
    }
}