

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Drive.v3.Data;
using System.Windows.Forms;

namespace vsHelp.Classes
{
    public class GoogleDrive
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "vsHelp 4.0";
        public static DriveService GetService()
        {
            try
            {
                // O caminho para o seu arquivo JSON da conta de serviço.
                // Ele deve estar na mesma pasta do executável.
                string serviceAccountKeyPath = "service_account.json";

                if (!System.IO.File.Exists(serviceAccountKeyPath))
                {
                    throw new Exception($"O arquivo de chave da conta de serviço '{serviceAccountKeyPath}' não foi encontrado. Certifique-se de que a propriedade 'Copy to Output Directory' do arquivo está definida como 'Copy if newer' no Visual Studio.");
                }

                var credential = GoogleCredential.FromFile(serviceAccountKeyPath)
                    .CreateScoped(Scopes);

                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter serviço do Google Drive via Conta de Serviço: {ex.Message}",
                                "Erro de Autenticação", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return null;
            }
        }

        public static string UploadFileAndGetPublicLink(string filePath,
                                                        string folderName)

        {
            try

            {
                var service = GetService();

                // 1. USA DIRETAMENTE O ID DA PASTA COMPARTILHADA
                string folderId = "1o04wX-ZjImZPVNfQMBGzzm4sc_hkkA32";

                // 2. Preparar metadados do arquivo

                string originalFileName = Path.GetFileNameWithoutExtension(filePath);

                string fileExtension = Path.GetExtension(filePath);

                string expirationDate = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd");

                string newFileName =
                    $"{originalFileName} (expira {expirationDate}){fileExtension}";

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()

                {

                    Name = newFileName,

                    MimeType = "application/zip",

                    Parents = new List<string> { folderId }

                };

                // 3. Fazer upload com relatório de progresso

                FilesResource.CreateMediaUpload request;

                using (var stream = new FileStream(filePath, FileMode.Open))

                {
                    request =
                        service.Files.Create(fileMetadata, stream, "application/zip");

                    request.Fields = "id, webViewLink";

                    int lastReportedPercentage = -25;

                    request.ProgressChanged += (progress) =>

                    {
                        if (progress.Status == Google.Apis.Upload.UploadStatus.Uploading)

                        {
                            int currentPercentage =
                                (int)Math.Round(progress.BytesSent * 100.0 / stream.Length);

                            if (currentPercentage >= lastReportedPercentage + 25)

                            {
                                Utils.Notificacao("Progresso do Upload",
                                                  $"{currentPercentage}% concluído...");

                                lastReportedPercentage = currentPercentage;
                            }
                        }
                    };

                    Utils.Notificacao("Upload",
                                      "Iniciando upload para o Google Drive...");

                    var uploadResponse = request.Upload();

                    if (uploadResponse.Status !=
                        Google.Apis.Upload.UploadStatus.Completed)

                    {
                        throw uploadResponse.Exception;
                    }
                }

                var file = request.ResponseBody;

                var fileId = file.Id;

                // 4. Definir permissão pública

                var permission = new Permission() { Type = "anyone", Role = "reader" };

                service.Permissions.Create(permission, fileId).Execute();

                // 5. Obter link e copiar para a área de transferência

                var fileRequest = service.Files.Get(fileId);

                fileRequest.Fields = "webViewLink";

                var fileWithLink = fileRequest.Execute();

                var publicLink = fileWithLink.WebViewLink;

                // Usa uma thread dedicada em modo STA para definir o texto da área de
                // transferência

                Thread thread = new Thread(() => Clipboard.SetText(publicLink));

                thread.SetApartmentState(ApartmentState.STA);

                thread.Start();

                thread.Join();

                return publicLink;

            }

            catch (Exception e)

            {
                MessageBox.Show(
                    $"Erro ao fazer upload para o Google Drive: {e.Message}",
                    "Erro de Upload", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
        }

    }

}