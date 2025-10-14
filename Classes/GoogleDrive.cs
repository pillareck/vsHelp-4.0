using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
                var assembly = Assembly.GetExecutingAssembly();
                var clientSecretResourceName = "vsHelp.Resources.client_secret.json";
                var tokenResourceName = "vsHelp.Resources.drive_token.json";

                // 1. Load client secrets
                ClientSecrets clientSecrets;
                using (var stream = assembly.GetManifestResourceStream(clientSecretResourceName))
                {
                    if (stream == null) throw new Exception($"Recurso '{clientSecretResourceName}' não encontrado.");
                    clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
                }

                // 2. Load token
                TokenResponse token;
                using (var stream = assembly.GetManifestResourceStream(tokenResourceName))
                {
                    if (stream == null) throw new Exception($"Recurso '{tokenResourceName}' não encontrado. Siga a Etapa 1 para gerá-lo e incorporá-lo.");
                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        token = JsonConvert.DeserializeObject<TokenResponse>(json);
                    }
                }

                // 3. Create credential
                var flow = new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = clientSecrets,
                        Scopes = Scopes
                    });

                var credential = new UserCredential(flow, "user", token);

                // 4. Create and return the Drive service
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

                Thread thread = new Thread(() => Clipboard.SetText(publicLink));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

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