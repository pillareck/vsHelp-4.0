using DevExpress.CodeParser;
using DevExpress.XtraEditors;
using MySqlConnector;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace vsHelp.Classes
{
    public class Conexao
    {
        private static IniFile config_ini = new(@"C:\Visual Software\MyCommerce\Config.ini");
        private static string Ip = config_ini.Read("SERVIDOR", "IPSERVIDOR");
        private static string Porta = config_ini.Read("SERVIDOR", "PORTASERVIDOR");
        private static string Usuario = config_ini.Read("SERVIDOR", "USUARIOSERVIDOR");
        private static string Senha = Properties.Conexao.Default.Senha;
        private static string BancoDeDados = config_ini.Read("SERVIDOR", "DATABASE");
        public static MySqlConnection connection;

        private static Conexao instancia = new();
        public static Conexao Instancia => instancia;

        private Conexao()
        {
            connection = new($"Server={Ip};Port={Porta};User Id={Usuario};Password={Senha};Database={BancoDeDados};Allow User Variables=true;");

            try
            {
                connection.Open();
            }
            catch (MySqlException e)
            {
                if (e.ErrorCode == MySqlErrorCode.UnknownDatabase)
                {
                    connection = new($"Server={Ip};Port={Porta};User Id={Usuario};Password={Senha};");
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = $"create database `{BancoDeDados}`";
                    cmd.ExecuteNonQuery();
                    connection.Close();
                    instancia = new();
                }
                else
                {
                    XtraMessageBox.Show(e.Message, "..::vsHelp::..");
                }
            }
        }

        public static void AtualizaConexao()
        {
            config_ini = new(@"C:\Visual Software\MyCommerce\Config.ini");

            Ip = config_ini.Read("SERVIDOR", "IPSERVIDOR");
            Porta = config_ini.Read("SERVIDOR", "PORTASERVIDOR");
            Usuario = config_ini.Read("SERVIDOR", "USUARIOSERVIDOR");
            Senha = Properties.Conexao.Default.Senha;
            BancoDeDados = config_ini.Read("SERVIDOR", "DATABASE");

            instancia = new();
        }

        // Adicione estes dois métodos à sua classe Conexao
        public static bool BancoExiste(string servidor, string porta, string usuario, string senha, string nomeBanco)
        {
            try
            {
                string connString = $"Server={servidor};Port={porta};User Id={usuario};Password={senha};";
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand($"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{nomeBanco}'", conn);
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool CriarBanco(string servidor, string porta, string usuario, string senha, string nomeBanco)
        {
            try
            {
                string connString = $"Server={servidor};Port={porta};User Id={usuario};Password={senha};";
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand($"CREATE DATABASE `{nomeBanco}`", conn);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void RestaurarBanco(string servidor, string porta, string usuario, string senha, string nomeBanco, string caminho, ProgressBarControl pb)
        {
            string connString = $"Server={servidor};Port={porta};User Id={usuario};Password={senha};Database={nomeBanco};Allow User Variables=true;";
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();

                pb.Invoke(() =>
                {
                    pb.EditValue = 0;
                    pb.Properties.Maximum = Utils.GetQtdComandos(caminho);
                    pb.Properties.Step = 1;
                });

                using (StreamReader reader = new StreamReader(caminho))
                {
                    string linha;
                    StringBuilder sqlCommand = new StringBuilder();

                    while ((linha = reader.ReadLine()) != null)
                    {
                        if (linha.StartsWith("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) || linha.StartsWith("USE", StringComparison.OrdinalIgnoreCase) || linha.StartsWith("DELIMITER", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        sqlCommand.AppendLine(linha);

                        if (linha.Trim().EndsWith(";"))
                        {
                            using (MySqlCommand cmd = new MySqlCommand(sqlCommand.ToString(), conn))
                            {
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                catch (Exception)
                                {
                                    //
                                }
                                finally
                                {
                                    pb.Invoke(() =>
                                    {
                                        pb.PerformStep();
                                        pb.Update();
                                    });
                                }
                            }
                            sqlCommand.Clear();
                        }
                    }

                    if (sqlCommand.Length > 0)
                    {
                        using (MySqlCommand cmd = new MySqlCommand(sqlCommand.ToString(), conn))
                        {
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception)
                            {
                                //
                            }
                            finally
                            {
                                pb.Invoke(() =>
                                {
                                    pb.PerformStep();
                                    pb.Update();
                                });
                            }
                        }
                    }
                }
            }

            Utils.Notificacao("Restauração", "Backup Restaurado");
        }
    }
}
