using System;
using System.Windows.Input;

using BuildWatcher.Properties;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Microsoft.VisualStudio.Services.Client;

namespace BuildWatcher.ViewModel
{
    public class ConfigViewModel : ViewModelBase
    {
        private string _projectUrl;
        private bool? _projectUrlValid;

        public ConfigViewModel()
        {
            TestCommand = new RelayCommand(Test);
            SaveCommand = new RelayCommand(Save);

            ProjectUrl = Settings.Default.ProjectUrl;
        }

        public ICommand TestCommand { get; }
        public ICommand SaveCommand { get; }

        public string ProjectUrl
        {
            get { return _projectUrl; }
            set { Set(ref _projectUrl, value); }
        }

        public bool? ProjectUrlValid
        {
            get { return _projectUrlValid; }
            set { Set(ref _projectUrlValid, value); }
        }

        private async void Test()
        {
            var vssClientCredentials = new VssClientCredentials();
            vssClientCredentials.Storage = new VssClientCredentialStorage();

            VssConnection connection = new VssConnection(new Uri(ProjectUrl), vssClientCredentials);
            
            try
            {
                await connection.ConnectAsync();
                ProjectUrlValid = connection.HasAuthenticated;
            }
            catch
            {
                ProjectUrlValid = false;
            }
        }

        private void Save()
        {
            Settings.Default.ProjectUrl = ProjectUrl;
            Settings.Default.Save();
        }
    }
}