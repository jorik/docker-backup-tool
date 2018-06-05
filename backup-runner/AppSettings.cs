using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.Configuration;

namespace docker_backups
{
    public class AppSettings
    {
        public readonly string DockerRemoteApiUri;
        public readonly string AWSSecretAccessKey;
        public readonly string AWSAccessKeyId;

        private AppSettings(string dockerRemoteApiUri, string awsSecretAccessKey, string awsAccessKeyId)
        {
            DockerRemoteApiUri = dockerRemoteApiUri;
            AWSSecretAccessKey = awsSecretAccessKey;
            AWSAccessKeyId = awsAccessKeyId;
        }

        public static bool TryParse(IConfigurationRoot config, out AppSettings appSettings)
        {
            var dockerRemoteApiUri = config[nameof(DockerRemoteApiUri)];
            var awsSecretAccessKey = config[nameof(AWSSecretAccessKey)];
            var awsAccessKeyId = config[nameof(AWSAccessKeyId)];

            if (string.IsNullOrEmpty(dockerRemoteApiUri) ||
                string.IsNullOrEmpty(awsSecretAccessKey) ||
                string.IsNullOrEmpty(awsAccessKeyId))
            {
                appSettings = null;
                return false;
            }
            
            appSettings = new AppSettings(dockerRemoteApiUri, awsSecretAccessKey, awsAccessKeyId);
            return true;
        }
    }
}