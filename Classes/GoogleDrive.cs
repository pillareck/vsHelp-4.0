using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace vsHelp.Classes
{
    public class GoogleDrive
    {
        private const string GoogleDriveFolderId = "1YAesyzPQVbYqAc4a4JUHdtbUaPVIzxGZ";
        private static readonly string[] Scopes = { DriveService.Scope.Drive };
        private static readonly string ApplicationName = "vsHelp 4.0";

        public static DriveService GetService()
        {
            try
            {
                var clientSecretPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secret.json");
                if (!File.Exists(clientSecretPath))
                {
                    throw new FileNotFoundException("O arquivo client_secret.json não foi encontrado na pasta do aplicativo.", clientSecretPath);
                }

                UserCredential credential;
                using (var stream = new FileStream(clientSecretPath, FileMode.Open, FileAccess.Read))
                {
                    string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vsHelp", "drive-token.json");
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                }

                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter serviço do Google Drive: {ex.Message}", "Erro de Autenticação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public static string UploadFileAndGetPublicLink(string filePath)
        {
            try
            {
                var service = GetService();
                if (service == null) return null;

                string fileName = Path.GetFileName(filePath);
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fileName,
                    Parents = new List<string> { GoogleDriveFolderId }
                };

                FilesResource.CreateMediaUpload request;
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
                    request.Fields = "id, webViewLink";
                    request.SupportsAllDrives = true;

                    var uploadResponse = request.Upload();
                    if (uploadResponse.Status != Google.Apis.Upload.UploadStatus.Completed)
                    {
                        throw uploadResponse.Exception;
                    }
                }

                var uploadedFile = request.ResponseBody;
                if (uploadedFile == null)
                {
                    throw new Exception("Falha ao obter os detalhes do arquivo após o upload.");
                }

                var permission = new Google.Apis.Drive.v3.Data.Permission()
                {
                    Type = "anyone",
                    Role = "reader"
                };
                
                service.Permissions.Create(permission, uploadedFile.Id).Execute();

                var publicLink = uploadedFile.WebViewLink;

                if (string.IsNullOrEmpty(publicLink))
                {
                    throw new Exception("Não foi possível obter o link público do arquivo.");
                }

                return publicLink;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Erro ao fazer upload para o Google Drive: {e.Message}", "Erro de Upload", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}