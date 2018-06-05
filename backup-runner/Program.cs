using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Docker.DotNet;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace docker_backups
{
    static class Program
    {
        private static AppSettings _appSettings;
        
        private static void Main(string[] args)
        {
            var settings = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            if (!AppSettings.TryParse(settings, out _appSettings))
            {
                Console.Error.WriteLine("Unable to parse configuration. Please ensure that: \r\n" +
                                        " - appsettings.json file is present in the working directory ('Copy to output dir: always/when changed')\r\n" +
                                        " - all required fields are set. See appsettings.example.json for more an example.");
                return;
            }
            
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            // var certificate = new X509Certificate2 ("/Users/jorikvanderwerf/.docker/machine/machines/xxx/cert.pem");
            // var credentials = new CertificateCredentials(certificate);
            
            Console.WriteLine("==== Starting backup with configuration: ====");
            Console.WriteLine($" - docker endpoint: {_appSettings.DockerRemoteApiUri}");
            Console.WriteLine($" - AWS access key id: {_appSettings.AWSAccessKeyId.Substring(0, 4)}...");
            Console.WriteLine($" - AWS secret access key: {_appSettings.AWSSecretAccessKey.Substring(0, 4)}...");
            Console.WriteLine();
            
            var client = new DockerClientConfiguration(new Uri(_appSettings.DockerRemoteApiUri))
                .CreateClient();

            var containers = await client.Containers.ListContainersAsync(new ContainersListParameters());
            var containersRequiringBackups = containers.Where(x => x.Labels.ContainsKey("BACKUP")).ToList();
            
            Console.WriteLine($"Found {containers.Count} countainers of which {containersRequiringBackups.Count} require backups");
            
            foreach (var container in containersRequiringBackups)
            {
                await BackupContainerMounts(container, client);
            }
        }

        private static async Task BackupContainerMounts(ContainerListResponse container, DockerClient client)
        {
            Console.WriteLine($"┏━ Starting backup for: {container.Names.First()}");

            var fields = ContainerBackupFields.Create(container);
            if (!fields.IsValid())
            {
                Console.WriteLine($"Container {container.ID} is marked for backups, but does not have all required labels");
                return;
            }

            try
            {
                await client.Containers.PauseContainerAsync(container.ID);

                var mountBackupTasks = new List<Task>();

                foreach (var mount in container.Mounts)
                {
                    var task = BackupMount(mount, client, fields).ContinueWith(status =>
                    {
                        Console.WriteLine($"┗> {mount.Source}:{mount.Destination}");
                        return status;
                    });

                    mountBackupTasks.Add(task);
                }

                await Task.WhenAll(mountBackupTasks);

                Console.WriteLine(); // add an enter to create distance between containers
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error while running backup for container {container.Names.First()}. \n {e.Message}");
            }
            finally
            {
                await client.Containers.UnpauseContainerAsync(container.ID);
            }
        }

        private static async Task<Task> BackupMount(MountPoint mount, DockerClient client, ContainerBackupFields fields)
        {
            var backupContainer = await client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = "jorik/docker-mount-backup-tool",
                Env = new List<string>
                {
                    $"BACKUP_PROJECT={fields.Project}",
                    $"BACKUP_ROLE={fields.Role}",
                    $"BACKUP_VERSION={fields.Version}",
                    $"AWS_SECRET_ACCESS_KEY={_appSettings.AWSSecretAccessKey}",
                    $"AWS_ACCESS_KEY_ID={_appSettings.AWSAccessKeyId}"
                },
                HostConfig = new HostConfig
                {
                    AutoRemove = true,
                    Binds = new List<string>
                    {
                        $"{mount.Source}:/data:ro"
                    }
                }
            });

            var startResult =
                await client.Containers.StartContainerAsync(backupContainer.ID, new ContainerStartParameters());

            if (!startResult)
            {
                Console.Error.WriteLine($"Backup container created but unable to start: {backupContainer.ID}");
                return Task.CompletedTask;
            }

            return client.Containers.WaitContainerAsync(backupContainer.ID);
        }
    }
}
