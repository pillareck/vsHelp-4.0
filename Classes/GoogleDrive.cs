


using Google.Apis.Auth.OAuth2;
using System.Reflection;



using Google.Apis.Drive.v3;



using Google.Apis.Services;







using System;



using System.Collections.Generic;



using System.IO;



using System.Linq;



using System.Threading;



using Google.Apis.Drive.v3.Data;



using System.Windows.Forms;







namespace vsHelp.Classes



{



    public class GoogleDrive



    {



                static string[] Scopes = { DriveService.Scope.DriveFile };



                static string ApplicationName = "vsHelp 4.0";



        



                public static DriveService GetService()



                {



                    try



                    {



                        var assembly = Assembly.GetExecutingAssembly();



                        var credentialStream = assembly.GetManifestResourceStream("vsHelp.Resources.service_account.json");



        



                        if (credentialStream == null)



                        {



                            throw new Exception("O recurso 'service_account.json' não foi encontrado. Certifique-se de que ele está como 'Embedded Resource'.");



                        }



        



                        GoogleCredential credential;



                        using (var stream = new MemoryStream())



                        {



                            credentialStream.CopyTo(stream);



                            stream.Position = 0;



                            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);



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







        public static string UploadFileAndGetPublicLink(string filePath, string folderName)
        {
            try
            {
                var service = GetService();

                // 1. Usar o ID da pasta compartilhada diretamente
                string folderId = "1YAesyzPQVbYqAc4a4JUHdtbUaPVIzxGZ"; // ID da pasta compartilhada com a Service Account

                // 2. Preparar metadados do arquivo
                string originalFileName = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);
                string expirationDate = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd");
                string newFileName = $"{originalFileName} (expira {expirationDate}){fileExtension}";

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
                    request = service.Files.Create(fileMetadata, stream, "application/zip");
                    request.SupportsAllDrives = true; // Essencial para Drives Compartilhados/pastas compartilhadas
                    request.Fields = "id, webViewLink";

                    int lastReportedPercentage = -25;
                    request.ProgressChanged += (progress) =>
                    {
                        if (progress.Status == Google.Apis.Upload.UploadStatus.Uploading)
                        {
                            int currentPercentage = (int)Math.Round(progress.BytesSent * 100.0 / stream.Length);
                            if (currentPercentage >= lastReportedPercentage + 25)
                            {
                                Utils.Notificacao("Progresso do Upload", $"{currentPercentage}% concluído...");
                                lastReportedPercentage = currentPercentage;
                            }
                        }
                    };
                    
                    Utils.Notificacao("Upload", "Iniciando upload para o Google Drive...");
                    var uploadResponse = request.Upload();

                    if (uploadResponse.Status != Google.Apis.Upload.UploadStatus.Completed)
                    {
                        throw uploadResponse.Exception;
                    }
                }

                var file = request.ResponseBody;
                var fileId = file.Id;

                // 4. Permissão pública é herdada da pasta. Nenhuma ação é necessária.


                // 5. Obter link e copiar para a área de transferência
                var fileRequest = service.Files.Get(fileId);
                fileRequest.SupportsAllDrives = true; // Essencial para Drives Compartilhados/pastas compartilhadas
                fileRequest.Fields = "webViewLink";
                var fileWithLink = fileRequest.Execute();
                var publicLink = fileWithLink.WebViewLink;
                
                // Usa uma thread dedicada em modo STA para definir o texto da área de transferência
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