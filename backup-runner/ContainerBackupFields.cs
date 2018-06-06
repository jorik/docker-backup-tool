using System.Linq;
using Docker.DotNet.Models;

namespace docker_backups
{
    public class ContainerBackupFields
    {
        public readonly string Project;
        public readonly string Role;

        public bool IsValid() => !string.IsNullOrEmpty(Project) &&
                                 !string.IsNullOrEmpty(Role);

        private ContainerBackupFields(string project, string role)
        {
            Project = project;
            Role = role;
        }

        public static ContainerBackupFields Create(ContainerListResponse container)
        {
            return new ContainerBackupFields(
                project: container.Labels.FirstOrDefault(x => x.Key.ToLower() == "project").Value,
                role: container.Labels.FirstOrDefault(x => x.Key.ToLower() == "role").Value
            );
        }

    }
}