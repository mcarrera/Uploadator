using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using Google.Apis.Upload;
using Uploaders.Model;
using System.Text.Json;



namespace Uploader
{

    public class Uploader
    {
        private async Task<string> UploadToDrive(string filePath)
        {
            try
            {
                // Initialize the Drive Service

                var credential = await GetCredentials();

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                });

                // Get the creation date of the file
                DateTime creationDate = File.GetCreationTime(filePath);

                // Get the original file extension
                string fileExtension = Path.GetExtension(filePath);

                // Generate the file name based on the creation date and original file extension
                string fileName = $"Scuola{creationDate.ToString("ddMMMMyyyy", new System.Globalization.CultureInfo("it-IT"))}{fileExtension}";

                // Infer MIME type from file extension
                string mimeType = MimeTypeMap.GetMimeType(fileExtension);

                // Upload file to drive.
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fileName,
                    Parents = [GetFolderId()]
                };

                FilesResource.CreateMediaUpload request;

                // Create a new file on drive.
                IUploadProgress result;

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    // Create a new file, with metadata and stream.
                    request = service.Files.Create(fileMetadata, stream, mimeType);
                    request.Fields = "id,webViewLink";

                    // Asynchronously await the upload operation
                    await Task.Run(() => request.Upload());
                }

                var file = request.ResponseBody;
                // Prints the uploaded file id.
                Console.WriteLine("File ID: " + file.Id);
                var shareUrl = $"https://drive.google.com/file/d/{file.Id}/view?usp=sharing";
                Console.WriteLine("File URL: " + shareUrl);

                // Print additional fields
                Console.WriteLine("Web View Link: " + file.WebViewLink);

                return file.Id;
            }
            catch (Exception e)
            {
                // TODO(developer) - handle error appropriately
                if (e is AggregateException)
                {
                    Console.WriteLine("Credential Not found");
                }
                else if (e is FileNotFoundException)
                {
                    Console.WriteLine("File not found");
                }
                else
                {
                    throw;
                }
            }
            return string.Empty;
        }

        public async Task Upload()
        {
            var folder = "ExternalData";
            // Get a list of files in the folder
            var files = Directory.GetFiles(folder);

            // Iterate over each file and upload to Google Drive
            foreach (string filePath in files)
            {
                await UploadToDrive(filePath);
                File.Delete(filePath);
            }
        }

        public static async Task<UserCredential> GetCredentials()
        {
            try
            {

                var credentials = ReadSecrets();

                if (credentials == null) throw new ArgumentNullException("credentials", "cannot read credential file");

                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = credentials.ClientId,

                            ClientSecret = credentials.ClientSecret
                        },
                        new[] { DriveService.Scope.Drive }, "user", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with authentication: {ex.Message}");
                throw;
            }
        }

        public static string GetFolderId()
        {
            var credentials = ReadSecrets();
            return credentials.FolderId;
        }

        private static Credentials ReadSecrets()
        {
            string credentialsFilePath = @"C:\secrets\gooogle-credentials-dev.json";

            string jsonString = File.ReadAllText(credentialsFilePath);
            
            if (!string.IsNullOrEmpty(jsonString))
            {
                return JsonSerializer.Deserialize<Credentials>(jsonString);
            }

            throw new ArgumentNullException();
        }
    }
}
