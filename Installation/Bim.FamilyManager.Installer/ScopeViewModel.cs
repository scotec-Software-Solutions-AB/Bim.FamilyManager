namespace Bim.FamilyManager.Installer
{
    public class ScopeViewModel : ObservableObject
    {
        private bool _isUserScope;
        private bool _isMachineScope;

        public bool IsUserScope
        {
            get => _isUserScope;
            set => SetProperty(ref _isUserScope, value);
        }

        public bool IsMachineScope
        {
            get => _isMachineScope;
            set => SetProperty(ref _isMachineScope, value);
        }
    }
}
