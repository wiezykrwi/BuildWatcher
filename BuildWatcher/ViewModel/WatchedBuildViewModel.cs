using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;

using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildWatcher.ViewModel
{
    public class WatchedBuildViewModel : ViewModelBase
    {
        private BuildResult? _result;

        public WatchedBuildViewModel(int id, string name)
        {
            Id = id;
            Name = name;

            LastBuilds = new ObservableCollection<BuildResult?>();
        }

        public int Id { get; }

        public string Name { get; }
        
        public BuildResult? Result
        {
            get { return _result; }
            set { Set(ref _result, value); }
        }

        public ObservableCollection<BuildResult?> LastBuilds { get; }
    }
}