namespace HackF5.UnitySpy.Gui.Wpf.Mvvm
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RegisterAttribute : Attribute
    {
        public RegisterAttribute(RegistrationType type)
        {
            this.Type = type;
        }

        public RegistrationType Type { get; }
    }
}