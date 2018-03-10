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
        private readonly ITransferUtility transfer;

        public static readonly IList<string> UploadFileTypes = new[] { ".pdb", ".exe" };

        public SymbolUploader(ILogger<SymbolUploader> logger, ITransferUtility transfer)
        {
            this.logger = logger;
            this.transfer = transfer;
        }

        public async Task UploadSymbols(string extracted, string bucket, IEnumerable<IConfigurationSection> properties)
        {
            foreach (var file in Directory.EnumerateFiles(extracted, "*", SearchOption.AllDirectories))
            {
                if (!UploadFileTypes.Contains(Path.GetExtension(file)))
                {
                    logger.LogDebug("Skipping file {0} due to extension", file);
                    continue;
                }

                var objectKey = file.Replace(extracted, string.Empty).Replace('\\', '/').TrimStart('/');

                logger.LogInformation("Uploading {0} to {1}/{2}", file, bucket, objectKey);

                var request = new TransferUtilityUploadRequest
                {
                    FilePath = file,
                    Key = objectKey,
                    BucketName = bucket
                };

                foreach (var property in properties)
                {
                    request.Metadata.Add(property.Key, property.Value);
                }

                await transfer.UploadAsync(request);
                logger.LogInformation("Completed upload of {0}", objectKey);
            }
        }
    }
}
