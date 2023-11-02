using Blazored.Toast.Services;
using Docker.DotNet;
using Docker.DotNet.Models;
using S7_SecureContainer.Models;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;

namespace S7_SecureContainer.Services
{
    public class DockerService
    {
        private class Config
        {
            public string DockerHost { get; set; } = string.Empty;
        }
        public string ConnectionString { get; set; } = string.Empty;
        public bool ManualHostInput { get; set; } = false;
        public DockerClient? Client { get; set; } = null;
        public const string DefaultHost = "tcp:///dockerproxy:2375";
        public const string DefaultUnixHost = "unix:///var/run/docker.sock";
        private readonly IToastService ToastService;
        public DockerService (IToastService toastService)
        {
            ToastService = toastService;
        }

        public static string GetHostFromConfig()
        {
            const string configFile = "config.json";
            try
            {
                string configData = File.ReadAllText(configFile);
                Config? config = JsonSerializer.Deserialize<Config>(configData);
                if (config != null) return config.DockerHost;
            }
            catch (Exception) { }
            return string.Empty;
        }

        public void Clear()
        {
            Client = null;
            ConnectionString = string.Empty;
        }

        public async Task<bool> TryAutoConnect()
        {
            if (await TryConnect(DefaultUnixHost)) 
            {
                ToastService.ShowToast(ToastLevel.Error, 
                    string.Format("Please do not use a direct connection to docker host using: {0}",
                                    DefaultUnixHost));
                return false;
            }
            if (await TryConnect(DefaultHost)) { return true; }
            if(await TryConnect(GetHostFromConfig())) { return true; }
            ToastService.ShowToast(ToastLevel.Warning, "Could not auto-connect to a Docker host, please consult the docs.");
            ToastService.ShowToast(ToastLevel.Info, "Reverting to manual host input..");
            ManualHostInput = true;
            return false;
        }

        private async Task<bool> TryConnect(string connectionString)
        {
            if (connectionString == string.Empty) { return false; }
            List<ToastModel> toasts = await Connect(connectionString);
            if (!toasts.Any(t => t.Level == ToastLevel.Error))
            {
                toasts.ForEach(t => ToastService.ShowToast(t.Level, t.Message));
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<ToastModel>> Connect(string connectionString)
        {
            List<ToastModel> toasts = new();
            if (connectionString == string.Empty)
            {
                toasts.Add(new ToastModel(ToastLevel.Error, "Please enter a value.."));
                return toasts;
            }
            DockerClient client;
            try
            {
                client = new DockerClientConfiguration(
                new Uri(connectionString))
                    .CreateClient();
                IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    Limit = 1,
                });
                if (containers.Count == 0)
                {
                    toasts.Add(new ToastModel(ToastLevel.Error, "No containers found!"));
                    return toasts;
                }
            }
            catch (Exception)
            {
                toasts.Add(new ToastModel(ToastLevel.Error, "Could not connect to server url: " + connectionString));
                return toasts;
            }
            toasts.Add(new ToastModel(ToastLevel.Success, "Succesfully connected to: " + connectionString));
            ConnectionString = connectionString;
            Client = client;
            return toasts;
        }
    }
}
