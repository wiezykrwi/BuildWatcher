using System;
using System.Linq;

using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var vssClientCredentials = new VssClientCredentials();
            vssClientCredentials.Storage = new VssClientCredentialStorage();
            
            VssConnection connection = new VssConnection(new Uri("https://reeferwatch.visualstudio.com"), vssClientCredentials);
            connection.ConnectAsync().Wait();

            var projectHttpClient = connection.GetClient<ProjectHttpClient>();
            var projects = projectHttpClient.GetProjects().Result;
            var project = projects.First();
            
            var buildHttpClient = connection.GetClient<BuildHttpClient>();
            var buildDefinitions = buildHttpClient.GetDefinitionsAsync(project.Id).Result;
            
            System.Console.WriteLine($"{buildDefinitions.Count} builds in {project.Name}");

            foreach (var definitionReference in buildDefinitions)
            {
                var builds = buildHttpClient.GetBuildsAsync(project.Id, new[] { definitionReference.Id }).Result;
                System.Console.WriteLine($"{definitionReference.Name}: {string.Join(",", builds.Take(5).Select(x => x.Result.ToString()))}");
            }

            System.Console.ReadLine();
        }
    }
}
