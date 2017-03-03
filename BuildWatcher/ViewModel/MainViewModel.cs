using GalaSoft.MvvmLight;

namespace BuildWatcher.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            BuildViewModel = new BuildViewModel();
            ConfigViewModel = new ConfigViewModel();
        }

        public BuildViewModel BuildViewModel { get; }
        public ConfigViewModel ConfigViewModel { get; }
    }
}