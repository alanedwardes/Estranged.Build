using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Estranged.Build.Symbols
{
    public class SymbolUploader
    {
        private readonly ILogger<SymbolUploader> logger;
        private readonly IAmazonS3 amazonS3;

        public static readonly IList<string> UploadFileTypes = new[] { ".pdb", ".exe" };

        public SymbolUploader(ILogger<SymbolUploader> logger, IAmazonS3 amazonS3)
        {
            this.logger = logger;
            this.amazonS3 = amazonS3;
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

                await VerifyUpload(file, bucket, key);
            }
        }

        private async Task UploadSymbolFile(string file, string bucket, string key, IEnumerable<IConfigurationSection> properties)
        {
            logger.LogInformation($"Uploading {file} to {bucket}/{key}");

            var request = new PutObjectRequest
            {
                FilePath = file,
                Key = key,
                BucketName = bucket
            };

            foreach (var property in properties)
            {
                request.Metadata.Add(property.Key, property.Value);
            }

            var retryPolicy = Policy
                .Handle<AmazonServiceException>()
                .RetryAsync(3);

            await retryPolicy.ExecuteAsync(() => amazonS3.PutObjectAsync(request));

            logger.LogInformation($"Completed upload of {key}");
        }

        private async Task VerifyUpload(string file, string bucket, string key)
        {
            string expectedHash = CalculateMD5(file);

            var response = await amazonS3.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = bucket,
                Key = key
            });

            logger.LogInformation($"Verifying hash of {key}");

            string actualHash = response.ETag.Trim('"');
            if (response.ETag != expectedHash)
            {
                throw new Exception($"Calculated hash does not match response from S3. Expected: {expectedHash}, actual: {actualHash}");
            }

            logger.LogInformation($"Hash of {key} verified: {actualHash}");
        }

        private string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
