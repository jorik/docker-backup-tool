using System.Linq;
using Docker.DotNet.Models;

namespace docker_backups
{
    public class ContainerBackupFields
    {
        public readonly string Project;
        public readonly string Role;
        public readonly string Version;

        public bool IsValid() => !string.IsNullOrEmpty(Project) &&
                               !string.IsNullOrEmpty(Role) &&
                               !string.IsNullOrEmpty(Version);

        private ContainerBackupFields(string project, string role, string version)
        {
            Project = project;
            Role = role;
            Version = version;
        }

        public static ContainerBackupFields Create(ContainerListResponse container)
        {
            return new ContainerBackupFields(
                project: container.Labels.FirstOrDefault(x => x.Key.ToLower() == "project").Value,
                role: container.Labels.FirstOrDefault(x => x.Key.ToLower() == "role").Value,
                version: container.Labels.FirstOrDefault(x => x.Key.ToLower() == "version").Value);
        }

    }
}