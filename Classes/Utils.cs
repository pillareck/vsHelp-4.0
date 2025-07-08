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
            System.Diagnostics.Debug.WriteLine("Iniciando CriarPacoteDeInstalacao...");
            string tempPath = Path.Combine(Path.GetTempPath(), "vsHelp_Installers");
            try
            {
                Directory.CreateDirectory(tempPath);
                System.Diagnostics.Debug.WriteLine($"Diretório temporário criado: {tempPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao criar diretório temporário {tempPath}: {ex.Message}");
                return null;
            }

            var instaladores = new Dictionary<string, string>
            {
                { "checkBox2", "Crystal10.exe" },
                { "checkBox6", "Edit Pad Pro 7.msi" },
                { "checkBox1", "Fontes" },
                { "checkBox5", "mariadb-11.4.4-winx64.msi" },
                { "checkBox7", "visualsoftware_suporteremoto.exe" }
            };

            string caminhoInstaladores = @"\\10.1.1.110\Temp_Didi\Instaladores Implantacao";

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
                    if (arquivo == "Fontes") // Special handling for "Fontes" folder
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
            string resultado = Winrar.CompactarPasta(tempPath, arquivoZip);
            System.Diagnostics.Debug.WriteLine($"Resultado da compactação: {resultado ?? "Falha"}");

            if (resultado == null)
            {
                System.Diagnostics.Debug.WriteLine("Compactação falhou. Deletando diretório temporário.");
                Directory.Delete(tempPath, true);
                return null;
            }

            System.Diagnostics.Debug.WriteLine("Compactação bem-sucedida. Deletando diretório temporário.");
            Directory.Delete(tempPath, true);
            return resultado;
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
