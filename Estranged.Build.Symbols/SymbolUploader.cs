using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Estranged.Build.Symbols
{
    public class SymbolUploader
    {
        private readonly ILogger<SymbolUploader> logger;
        private readonly ITransferUtility transferUtility;

        public static readonly IList<string> UploadFileTypes = new[] { ".pdb", ".exe" };

        public SymbolUploader(ILogger<SymbolUploader> logger, ITransferUtility transferUtility)
        {
            this.logger = logger;
            this.transferUtility = transferUtility;
        }

        public async Task UploadSymbols(string extracted, string bucket, IEnumerable<IConfigurationSection> properties)
        {
            foreach (var file in Directory.EnumerateFiles(extracted, "*", SearchOption.AllDirectories))
            {
                if (!UploadFileTypes.Contains(Path.GetExtension(file)))
                {
                    logger.LogDebug($"Skipping file {file} due to extension");
                    continue;
                }

                string key = file.Replace(extracted, string.Empty).Replace('\\', '/').TrimStart('/');

                await UploadSymbolFile(file, bucket, key, properties);
            }
        }

        private async Task UploadSymbolFile(string file, string bucket, string key, IEnumerable<IConfigurationSection> properties)
        {
            logger.LogInformation($"Uploading {file} to {bucket}/{key}");

            var request = new TransferUtilityUploadRequest
            {
                FilePath = file,
                Key = key,
                BucketName = bucket
            };

            foreach (var property in properties)
            {
                request.Metadata.Add(property.Key, property.Value);
            }

            await transferUtility.UploadAsync(request);

            logger.LogInformation($"Completed upload of {key}");
        }
    }
}
