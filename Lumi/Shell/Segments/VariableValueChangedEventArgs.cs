namespace Lumi.Shell.Segments
{
    internal sealed class VariableValueChangedEventArgs
    {
        public bool Revert { get; set; }
        public string NewValue { get; }
        public string OldValue { get; }

        public VariableValueChangedEventArgs( string newValue, string oldValue )
        {
            this.NewValue = newValue;
            this.OldValue = oldValue;
        }
    }
}
