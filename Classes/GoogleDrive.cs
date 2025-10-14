






using Google.Apis.Auth.OAuth2;



using Google.Apis.Auth.OAuth2.Flows;



using Google.Apis.Auth.OAuth2.Responses;



using Google.Apis.Drive.v3;



using Google.Apis.Services;



using Newtonsoft.Json; // ** IMPORTANTE: Adicionar o pacote NuGet Newtonsoft.Json ao projeto **



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



        // ATENÇÃO: Coloque aqui o ID da pasta do Google Drive para onde os arquivos serão enviados.



        // O ID fica na URL da pasta. Ex: .../folders/AQUI_VAI_O_ID



        private const string GoogleDriveFolderId = "1YAesyzPQVbYqAc4a4JUHdtbUaPVIzxGZ";







        static readonly string[] Scopes = { DriveService.Scope.DriveFile };



        static readonly string ApplicationName = "vsHelp 4.0";







        public static DriveService GetService()



        {



            try



            {



                var assembly = Assembly.GetExecutingAssembly();



                var clientSecretsResourceName = "vsHelp.Resources.client_secrets.json";



                var tokenResourceName = "vsHelp.Resources.drive_token.json";







                // 1. Carregar o client_secrets.json



                ClientSecrets clientSecrets;



                using (var stream = assembly.GetManifestResourceStream(clientSecretsResourceName))



                {



                    if (stream == null) throw new Exception($"Recurso '{clientSecretsResourceName}' não encontrado. Verifique se a Build Action é 'Embedded Resource'.");



                    using (var reader = new StreamReader(stream))



                    {



                        clientSecrets = GoogleClientSecrets.FromStream(reader.BaseStream).Secrets;



                    }



                }







                // 2. Carregar o token (drive_token.json)



                TokenResponse token;



                using (var stream = assembly.GetManifestResourceStream(tokenResourceName))



                {



                    if (stream == null) throw new Exception($"Recurso '{tokenResourceName}' não encontrado. Siga as instruções para gerar e adicionar este arquivo.");



                    using (var reader = new StreamReader(stream))



                    {



                        // O FileDataStore armazena um dicionário de tokens. Extraímos o do nosso "user".



                        var allTokens = JsonConvert.DeserializeObject<Dictionary<string, TokenResponse>>(reader.ReadToEnd());



                        if (!allTokens.TryGetValue("user", out token))



                        {



                            throw new Exception("Token para o 'user' não encontrado no arquivo drive_token.json.");



                        }



                    }



                }







                // 3. Criar a credencial com o token carregado



                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer



                {



                    ClientSecrets = clientSecrets,



                    Scopes = Scopes



                });







                var credential = new UserCredential(flow, "user", token);







                // 4. O refresh do token é feito automaticamente pela biblioteca ao fazer uma chamada.



                // Se o token de acesso expirou, o token de refresh será usado para obter um novo.







                // 5. Criar e retornar o serviço do Drive



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







        public static string UploadFileAndGetPublicLink(string filePath) // Removido folderName, pois é fixo



        {



            try



            {



                var service = GetService();



                if (service == null) return null;







                // 1. Preparar metadados do arquivo



                string fileName = Path.GetFileName(filePath);



                var fileMetadata = new Google.Apis.Drive.v3.Data.File()



                {



                    Name = fileName,



                    MimeType = "application/zip", // Assumindo que sempre será zip



                    Parents = new List<string> { GoogleDriveFolderId } // Adiciona o arquivo na pasta específica



                };







                // 2. Fazer upload com relatório de progresso



                FilesResource.CreateMediaUpload request;



                using (var stream = new FileStream(filePath, FileMode.Open))



                {



                    request = service.Files.Create(fileMetadata, stream, fileMetadata.MimeType);



                    request.Fields = "id"; // Só precisamos do ID para o próximo passo



                    request.SupportsAllDrives = true;







                    int lastReportedPercentage = -25;



                    request.ProgressChanged += (progress) =>



                    {



                        if (progress.Status == Google.Apis.Upload.UploadStatus.Uploading)



                        {



                            long totalBytes = stream.Length;



                            if (totalBytes > 0)



                            {



                                int currentPercentage = (int)Math.Round(progress.BytesSent * 100.0 / totalBytes);



                                if (currentPercentage >= lastReportedPercentage + 25)



                                {



                                    Utils.Notificacao("Progresso do Upload", $"{currentPercentage}% concluído...");



                                    lastReportedPercentage = currentPercentage;



                                }



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



                if (file == null)



                {



                    throw new Exception("Falha ao obter o corpo da resposta do arquivo após o upload.");



                }







                // 3. Tornar o arquivo público (qualquer pessoa com o link pode ver)



                var permission = new Google.Apis.Drive.v3.Data.Permission()



                {



                    Type = "anyone",



                    Role = "reader"



                };



                var permRequest = service.Permissions.Create(permission, file.Id);



                permRequest.SupportsAllDrives = true;



                permRequest.Execute();







                // 4. Obter link web e copiar para a área de transferência



                var fileRequest = service.Files.Get(file.Id);



                fileRequest.Fields = "webViewLink";



                fileRequest.SupportsAllDrives = true;



                var fileWithLink = fileRequest.Execute();



                var publicLink = fileWithLink.WebViewLink;







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


