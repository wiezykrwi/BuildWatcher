using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Windows.UI.Notifications;

using BuildWatcher.Properties;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;

namespace BuildWatcher.ViewModel
{
    public class BuildViewModel : ViewModelBase
    {
        private bool _isNotRefreshing;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public BuildViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                WatchedBuilds = new ObservableCollection<WatchedBuildViewModel>
                {
                    new WatchedBuildViewModel(1, "Test")
                    {
                        Result = BuildResult.Succeeded,
                        LastBuilds = { BuildResult.Succeeded, BuildResult.Succeeded, BuildResult.Failed }
                    }
                };
            }
            else
            {
                RefreshCommand = new RelayCommand(() =>
                {
                    Refresh();
                });
                
                FailedBuilds = new ObservableCollection<WatchedBuildViewModel>();
                WatchedBuilds = new ObservableCollection<WatchedBuildViewModel>();

                AutoRefresh();
            }
        }

        public ICommand RefreshCommand { get; }

        public ObservableCollection<WatchedBuildViewModel> FailedBuilds { get; }
        public ObservableCollection<WatchedBuildViewModel> WatchedBuilds { get; }

        public bool IsNotRefreshing
        {
            get { return _isNotRefreshing; }
            set { Set(ref _isNotRefreshing, value); }
        }

        private void AutoRefresh()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Refresh();
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            });
        }

        private async Task Refresh()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => IsNotRefreshing = false);

                if (string.IsNullOrEmpty(Settings.Default.ProjectUrl))
                {
                    return;
                }

                var vssClientCredentials = new VssClientCredentials();
                vssClientCredentials.Storage = new VssClientCredentialStorage();

                VssConnection connection = new VssConnection(new Uri(Settings.Default.ProjectUrl), vssClientCredentials);
                await connection.ConnectAsync();

                var projectHttpClient = connection.GetClient<ProjectHttpClient>();
                var projects = await projectHttpClient.GetProjects();
                var project = projects.First();

                var buildHttpClient = connection.GetClient<BuildHttpClient>();
                var buildDefinitions = await buildHttpClient.GetDefinitionsAsync(project.Id);
            
                foreach (var buildDefinition in buildDefinitions.OrderBy(b => b.Name))
                {
                    var builds = await buildHttpClient.GetBuildsAsync(project.Id, new[] { buildDefinition.Id });

                    UpdateWatchedBuild(buildDefinition, builds);
                }

            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => IsNotRefreshing = true);
            }
        }

        private void UpdateWatchedBuild(DefinitionReference buildDefinition, List<Build> builds)
        {
            bool wasFailedBuild = false;
            bool wasWatchedBuild = false;

            var watchedBuildViewModel = WatchedBuilds.FirstOrDefault(b => b.Id == buildDefinition.Id);
            if (watchedBuildViewModel != null)
            {
                wasWatchedBuild = true;
            }
            else
            {
                watchedBuildViewModel = FailedBuilds.FirstOrDefault(b => b.Id == buildDefinition.Id);
                if (watchedBuildViewModel != null)
                {
                    wasFailedBuild = true;
                }
                else
                {
                    watchedBuildViewModel = new WatchedBuildViewModel(buildDefinition.Id, buildDefinition.Name);
                }
            }

            watchedBuildViewModel.Result = builds.First().Result;
            watchedBuildViewModel.LastBuilds.Clear();
            foreach (var buildResult in builds.Take(5).Reverse().Select(b => b.Result).ToList())
            {
                watchedBuildViewModel.LastBuilds.Add(buildResult);
            }

            if (watchedBuildViewModel.Result == BuildResult.Failed)
            {
                if (!wasFailedBuild)
                {
                    Application.Current.Dispatcher.Invoke(() => FailedBuilds.Add(watchedBuildViewModel));

                    var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

                    var textElements = toastXml.GetElementsByTagName("text");
                    textElements[0].AppendChild(toastXml.CreateTextNode("Build failed")); // title
                    textElements[1].AppendChild(toastXml.CreateTextNode(buildDefinition.Name)); // line 1
                    textElements[2].AppendChild(toastXml.CreateTextNode("Build failed")); // line 2

                    var imageElements = toastXml.GetElementsByTagName("image");
                    imageElements[0].Attributes.GetNamedItem("src").NodeValue =
                        $"file:///{Path.GetFullPath("redcross.png")}";

                    var toast = new ToastNotification(toastXml);

                    ToastNotificationManager.CreateToastNotifier().Show(toast);
                }
                if (wasWatchedBuild)
                {
                    Application.Current.Dispatcher.Invoke(() => WatchedBuilds.Add(watchedBuildViewModel));
                }
            }
            else if (!wasWatchedBuild)
            {
                Application.Current.Dispatcher.Invoke(() => WatchedBuilds.Add(watchedBuildViewModel));
                if (wasFailedBuild)
                {
                    Application.Current.Dispatcher.Invoke(() => FailedBuilds.Remove(watchedBuildViewModel));
                }
            }
        }
    }
}