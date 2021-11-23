namespace HackF5.UnitySpy.Gui.ViewModel
{
    using System;

    public class AppendToTrailEventArgs : EventArgs
    {
        public AppendToTrailEventArgs(string value)
        {
            this.Value = value;
        }

        public string Value { get; }
    }
}