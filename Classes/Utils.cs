using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vsHelp.Properties;
using vsHelp.Classes;
using MySqlConnector;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using SharpCompress.Common;

namespace vsHelp.Classes
{
    internal static class Utils
    {
        internal static void AtualizarCQP(List<CheckEdit> checkEdits)
        {
            if (checkEdits.Count == 0)
            {
                XtraMessageBox.Show("Selecione alguma opção!", "..::vsHelp::..");
                return;
            }
            foreach (var cb in checkEdits)
            {
                Constantes.UpdatePorCheckBox[cb.Name]();
            }

            Notificacao("CQP Homologação", "Atualizações rodadas com sucesso!");
        }

        public static void Notificacao(string titulo, string msg)
        {
            new ToastContentBuilder()
                 .AddText(titulo)
                 .AddText(msg)
                 .Show();
        }

        internal static void AtualizarDB()
        {
            FileVersionInfo fviMyCommerce = FileVersionInfo.GetVersionInfo(@"C:\Visual Software\MyCommerce\myCommerce.exe");
            string versaoMyCommerce = fviMyCommerce.ProductMajorPart + "." + fviMyCommerce.ProductMinorPart + "." + fviMyCommerce.ProductBuildPart + "." + fviMyCommerce.ProductPrivatePart;
            string versaoUltimaAtualizacao = new MySqlCommand("select versao from atualizadb", Conexao.connection).ExecuteScalar()?.ToString();

            if (versaoMyCommerce == Regex.Replace(versaoUltimaAtualizacao, @"\.(00+)", "."))
            {
                MessageBox.Show($"O Banco já encontra-se atualizado para a versão {versaoMyCommerce}!", "..::vsHelp::..");
                return;
            }

            string caminhoAtualizaDB = @"C:\Visual Software\MyCommerce\AtualizarDB.exe";

            Process process = new();

            process.StartInfo.FileName = caminhoAtualizaDB;

            process.Start();

            Thread.Sleep(250);
            SendKeys.Send("{TAB}");
            SendKeys.Send("{ENTER}");

            process.WaitForExit();
        }

        public static void AtualizarSenhasUsuarios()
        {
            MySqlCommand sql = new("Update usuarios set password = 'W'", Conexao.connection);
            sql.ExecuteNonQuery();
        }

        public static void AtualizarSenhasSupervisores()
        {
            MySqlCommand sql = new("Update usuarios_supervisores set password = '4'", Conexao.connection);
            sql.ExecuteNonQuery();
        }

        public static void AtualizarEmails()
        {
            MySqlCommand sql = new("Update clientes set email = 'vscqp.visualsoftware@gmail.com'", Conexao.connection);
            sql.ExecuteNonQuery();
        }

        public static void AtualizarTelefones()
        {
            MySqlCommand sql = new("Update clientes set telefone1 = '(99)999999999', telefone2 = '(99)999999999'", Conexao.connection);
            sql.ExecuteNonQuery();
        }

        internal static string ProcurarBackup()
        {
            OpenFileDialog ofd = new()
            {
                InitialDirectory = Configuracoes.Default.UltimaPasta,
                Filter = "Arquivos SQL e RAR (*.sql;*.rar)|*.sql;*.rar"
            };

            if (DialogResult.OK == ofd.ShowDialog())
            {
                Configuracoes.Default.UltimaPasta = Path.GetDirectoryName(ofd.FileName);
                Configuracoes.Default.Save();
                return ofd.FileName;
            }

            return null;
        }

        public static void CopiarVersao(string arquivo)
        {
            string servidor = @"\\10.1.1.110\Arquivos\Atualizacoes\MyCommerce\" + arquivo;
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string destino = Path.Combine(desktop, arquivo);

            File.Copy(servidor, destino, true);
            Notificacao("Versão copiada", $"Versão \"{arquivo}\" enviada para Área de Trabalho.");
        }

        internal static int GetQtdComandos(string caminho)
        {
            int commandCount = 0;

            using (StreamReader reader = new StreamReader(caminho))
            {
                string linha;
                while ((linha = reader.ReadLine()) != null)
                {
                    if (linha.StartsWith("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) || linha.StartsWith("USE", StringComparison.OrdinalIgnoreCase) || linha.StartsWith("DELIMITER", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (linha.Trim().EndsWith(";"))
                    {
                        commandCount++;
                    }
                }
            }

            return commandCount;
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, copy them and their contents to new location
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static string CriarPacoteDeInstalacao(List<DevExpress.XtraEditors.CheckEdit> checkBoxes, bool copiarFull, bool copiarRelease, string versaoFull, string versaoRelease)
        {
            Notificacao("Criação de Pacote", "Iniciando a criação do pacote de instalação...");
            System.Diagnostics.Debug.WriteLine("Iniciando CriarPacoteDeInstalacao...");
            string tempPath = Path.Combine(Path.GetTempPath(), "vsHelp_Installers");
            string resultado = null;

            try
            {
                Directory.CreateDirectory(tempPath);
                System.Diagnostics.Debug.WriteLine($"Diretório temporário criado: {tempPath}");

                var instaladores = new Dictionary<string, string>
                {
                    { "checkBox2", "Crystal10.rar" },
                    { "checkBox6", "Edit Pad Pro 7.rar" },
                    { "checkBox1", "Fontes.rar" },
                    { "checkBox5", "mariadb-11.4.4-winx64.rar" },
                    { "checkBox7", "visualsoftware_suporteremoto.rar" }
                };

                string caminhoInstaladores = @"\\10.1.1.110\Arquivos\Temp_Didi\Instaladores Implantacao";

                if (!Directory.Exists(caminhoInstaladores))
                {
                    System.Diagnostics.Debug.WriteLine($"Erro: Caminho de instaladores não acessível: {caminhoInstaladores}");
                    return null;
                }
                System.Diagnostics.Debug.WriteLine($"Caminho de instaladores acessível: {caminhoInstaladores}");

                foreach (var cb in checkBoxes.Where(c => c.Checked && instaladores.ContainsKey(c.Name)))
                {
                    string arquivo = instaladores[cb.Name];
                    string origem = Path.Combine(caminhoInstaladores, arquivo);
                    string destino = Path.Combine(tempPath, arquivo);
                    System.Diagnostics.Debug.WriteLine($"Tentando copiar: {origem} para {destino}");
                    try
                    {
                        if (arquivo == "Fontes")
                        {
                            CopyDirectory(origem, destino, true);
                        }
                        else
                        {
                            File.Copy(origem, destino, true);
                        }
                        System.Diagnostics.Debug.WriteLine($"Copiado com sucesso: {arquivo}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao copiar {arquivo}: {ex.Message}");
                        return null;
                    }
                }

                if (copiarFull)
                {
                    System.Diagnostics.Debug.WriteLine("Copiando versão Full...");
                    CopiarVersaoParaTemp(versaoFull, tempPath);
                }
                if (copiarRelease)
                {
                    System.Diagnostics.Debug.WriteLine("Copiando versão Release...");
                    CopiarVersaoParaTemp(versaoRelease, tempPath);
                }

                string arquivoZip = Path.Combine(Path.GetTempPath(), $"PacoteInstalacao_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
                System.Diagnostics.Debug.WriteLine($"Tentando compactar pasta: {tempPath} para {arquivoZip}");
                resultado = Winrar.CompactarPasta(tempPath, arquivoZip);
                System.Diagnostics.Debug.WriteLine($"Resultado da compactação: {resultado ?? "Falha"}");

                if (resultado == null)
                {
                    System.Diagnostics.Debug.WriteLine("Compactação falhou.");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine("Compactação bem-sucedida. Iniciando upload...");
                string link = GoogleDrive.UploadFileAndGetPublicLink(resultado, "Pacote de Instaladores");

                if (link != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Upload concluído. Link: {link}");
                    Notificacao("Upload Concluído", "Link copiado para a área de transferência!");
                    try
                    {
                        File.Delete(resultado); // Exclui o arquivo .zip local
                        System.Diagnostics.Debug.WriteLine($"Arquivo local {resultado} deletado.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao deletar arquivo local {resultado}: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Upload para o Google Drive falhou.");
                }

                return link; // Retorna o link do Google Drive
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Um erro inesperado ocorreu em CriarPacoteDeInstalacao: {ex.Message}");
                return null;
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("Iniciando limpeza do diretório temporário.");
                DeleteDirectoryWithRetry(tempPath);
            }
        }

        private static void DeleteDirectoryWithRetry(string path, int retries = 5, int delayMilliseconds = 500)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                        System.Diagnostics.Debug.WriteLine($"Diretório {path} deletado com sucesso.");
                        return; 
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Diretório {path} não existe. Nada a deletar.");
                        return; 
                    }
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Tentativa {i + 1} falhou ao deletar {path}: {ex.Message}");
                    if (i < retries - 1)
                    {
                        Thread.Sleep(delayMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Um erro inesperado ocorreu ao deletar {path}: {ex.Message}");
                    return; 
                }
            }
            System.Diagnostics.Debug.WriteLine($"Não foi possível deletar o diretório {path} após {retries} tentativas.");
        }


        private static void CopiarVersaoParaTemp(string arquivo, string tempPath)
        {
            if (string.IsNullOrEmpty(arquivo) || arquivo.Contains("não encontrada")) return;

            string servidor = "\\\\10.1.1.110\\Arquivos\\Atualizacoes\\MyCommerce\\" + arquivo;
            string destino = Path.Combine(tempPath, arquivo);
            try
            {
                File.Copy(servidor, destino, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao copiar a versão {arquivo}: {ex.Message}");
            }
        }
    }
}
