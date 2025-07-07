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
    }
}
