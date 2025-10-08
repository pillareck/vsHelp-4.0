using DevExpress.Office.Crypto;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vsHelp.Properties;
using vsHelp.Classes;
using Utils = vsHelp.Classes.Utils;
using System.Threading;
using Google.Apis.Drive.v3;

namespace vsHelp
{
    public partial class frmPrincipal : DevExpress.XtraEditors.DirectXForm
    {
        public static FileSystemWatcher configWatcher = new(@"C:\Visual Software\MyCommerce\");
        public static IniFile configFile = new(@"C:\Visual Software\MyCommerce\Config.ini");

        public frmPrincipal()
        {
            InitializeComponent();
            txtSenha.Properties.PasswordChar = '*';

            // Configura o ToolTip para o campo txtBanco usando o componente toolTip1 existente.
            toolTip1.SetToolTip(txtBanco, "Caso o banco não exista, ele será criado!");
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmNovo_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuracoes.Default.NomeSkin = skin.LookAndFeel.SkinName;
            Configuracoes.Default.PaletaSkin = skin.LookAndFeel.ActiveSvgPaletteName;
            Configuracoes.Default.Save();
        }

        private void frmNovo_Load(object sender, EventArgs e)
        {
            AtualizaConexao();
            CarregaOpcoesCQP();

            #region Configurando FileWatcher
            configWatcher.Filter = "Config.ini";
            configWatcher.NotifyFilter = NotifyFilters.LastWrite;
            configWatcher.Changed += Watcher_Changed;
            configWatcher.EnableRaisingEvents = true;
            #endregion

            #region Configurando Skin
            skin.LookAndFeel.SetSkinStyle(Configuracoes.Default.NomeSkin, Configuracoes.Default.PaletaSkin);
            skin.LookAndFeel.UseDefaultLookAndFeel = true;
            #endregion

            //TestGoogleDrive();
            EnsureInstallationsControlsVisibility();
        }

        private void EnsureInstallationsControlsVisibility()
        {
            button1.Visible = true;
            button1.Enabled = true;
            checkBox1.Visible = true;
            checkBox1.Enabled = true;
            checkBox2.Visible = true;
            checkBox2.Enabled = true;
            checkBox3.Visible = true;
            checkBox3.Enabled = true;
            checkBox4.Visible = true;
            checkBox4.Enabled = true;
            checkBox5.Visible = true;
            checkBox5.Enabled = true;
            checkBox6.Visible = true;
            checkBox6.Enabled = true;
            checkBox7.Visible = true;
            checkBox7.Enabled = true;
            lblVersaoFullIns.Visible = true;
            lblVersaoRealeseIns.Visible = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            AtualizaConexao();
        }

        private void txtCaminhoBackup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F1) return;

            txtCaminhoBackup.Text = Utils.ProcurarBackup() ?? txtCaminhoBackup.Text;
        }

        private void btnProcurar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                // Caminho padrão: pasta Downloads do usuário
                string pastaDownloads = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads"
                );

                ofd.InitialDirectory = pastaDownloads;
                ofd.Filter = "Arquivos de Backup (*.rar;*.sql)|*.rar;*.sql|Todos os arquivos (*.*)|*.*";
                ofd.Title = "Selecione o arquivo de backup";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtCaminhoBackup.Text = ofd.FileName;
                }
            }
        }

        private void txtBanco_Leave(object sender, EventArgs e)
        {
            string nomeBanco = txtBanco.Text.Trim();

            if (string.IsNullOrEmpty(nomeBanco))
                return;

            bool existe = Classes.Conexao.BancoExiste(txtServidor.Text, txtPorta.Text, txtUsuario.Text, txtSenha.Text, nomeBanco);

            if (!existe)
            {
                DialogResult result = XtraMessageBox.Show(
                    $"O banco '{nomeBanco}' não existe.\nDeseja criá-lo?",
                    "Banco não encontrado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    bool criado = Classes.Conexao.CriarBanco(txtServidor.Text, txtPorta.Text, txtUsuario.Text, txtSenha.Text, nomeBanco);

                    if (criado)
                    {
                        XtraMessageBox.Show($"Banco '{nomeBanco}' criado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        XtraMessageBox.Show($"Erro ao criar o banco '{nomeBanco}'.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtBanco.Clear();
                    }
                }
                else
                {
                    txtBanco.Clear();
                }
            }
        }


        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            if (tpRestaurarBanco.Controls.OfType<TextEdit>().Any(tb => tb.Text == ""))
            {
                XtraMessageBox.Show($"Algum campo obrigatório está vazio!\nPreencha todos os campos obrigatórios", "..::vsHelp::..");
                return;
            }

            string nomeBanco = txtBanco.Text.Trim();
            bool existe = Classes.Conexao.BancoExiste(txtServidor.Text, txtPorta.Text, txtUsuario.Text, txtSenha.Text, nomeBanco);

            if (!existe)
            {
                DialogResult result = XtraMessageBox.Show(
                    $"O banco '{nomeBanco}' não existe.\nDeseja criá-lo?",
                    "Banco não encontrado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    bool criado = Classes.Conexao.CriarBanco(txtServidor.Text, txtPorta.Text, txtUsuario.Text, txtSenha.Text, nomeBanco);

                    if (criado)
                    {
                        XtraMessageBox.Show($"Banco '{nomeBanco}' criado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        XtraMessageBox.Show($"Erro ao criar o banco '{nomeBanco}'.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtBanco.Clear();
                        return; // Impede a restauração se o banco não pôde ser criado
                    }
                }
                else
                {
                    txtBanco.Clear();
                    return; // Impede a restauração se o usuário não quis criar o banco
                }
            }

            if (Path.GetExtension(txtCaminhoBackup.Text) == ".rar")
            {
                txtCaminhoBackup.Text = Winrar.Descompacta(txtCaminhoBackup.Text);
            }

            //SalvarConexao();

            string caminhoSql = txtCaminhoBackup.Text;

            new Thread(() =>
            {
                Classes.Conexao.RestaurarBanco(txtServidor.Text, txtPorta.Text, txtUsuario.Text, txtSenha.Text, txtBanco.Text, caminhoSql, pbProgressoRestaura);

                try
                {
                    if (File.Exists(caminhoSql) && Path.GetExtension(caminhoSql).ToLower() == ".sql")
                    {
                        File.Delete(caminhoSql);
                    }
                }
                catch (Exception ex)
                {
                    // Apenas para log ou depuração
                    MessageBox.Show("Erro ao excluir o arquivo SQL: " + ex.Message);
                }

            }).Start();
        }


        //private void SalvarConexao()
        //{
        //    configFile.Write("Servidor", "IPSERVIDOR", txtServidor.Text);
        //    configFile.Write("Servidor", "PORTASERVIDOR", txtPorta.Text);
        //    configFile.Write("Servidor", "USUARIOSERVIDOR", txtUsuario.Text);
        //    Properties.Conexao.Default.Senha = txtSenha.Text;
        //    configFile.Write("Servidor", "DATABASE", txtBanco.Text);
        //    Properties.Conexao.Default.Save();
        //}

        private void AtualizaConexao()
        {
            txtServidor.Text = configFile.Read("Servidor", "IPSERVIDOR");
            txtPorta.Text = configFile.Read("Servidor", "PORTASERVIDOR");
            txtUsuario.Text = configFile.Read("Servidor", "USUARIOSERVIDOR");
            txtSenha.Text = Properties.Conexao.Default.Senha;
            txtBanco.Text = configFile.Read("Servidor", "DATABASE");
        }

        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            Classes.Conexao.AtualizaConexao(txtServidor.Text, txtPorta.Text, txtUsuario.Text, txtSenha.Text, txtBanco.Text);
            Utils.AtualizarCQP(gcHomologacao.Controls.OfType<CheckEdit>().Where(c => c.Checked).OrderBy(c => c.TabIndex).ToList());
            SalvarOpcoesCQP();
        }

        private void SalvarOpcoesCQP()
        {
            OpcoesCQP.Default.SenhaUsuario = cbSenhaUsuario.Checked;
            OpcoesCQP.Default.SenhaSupervisor = cbSenhaSupervisor.Checked;
            OpcoesCQP.Default.Telefones = cbTelefone.Checked;
            OpcoesCQP.Default.Email = cbEmail.Checked;
            OpcoesCQP.Default.AtualizaDB = cbAtualizaDB.Checked;
            OpcoesCQP.Default.Save();
        }

        private void CarregaOpcoesCQP()
        {
            cbSenhaUsuario.Checked = OpcoesCQP.Default.SenhaUsuario;
            cbSenhaSupervisor.Checked = OpcoesCQP.Default.SenhaSupervisor;
            cbTelefone.Checked = OpcoesCQP.Default.Telefones;
            cbEmail.Checked = OpcoesCQP.Default.Email;
            cbAtualizaDB.Checked = OpcoesCQP.Default.AtualizaDB;
        }

        private void btnVersaoFull_Click(object sender, EventArgs e)
        {
            new Thread(() => Utils.CopiarVersao(lblVersaoFull.Text)).Start();
        }

        private void btnVersaoRelease_Click(object sender, EventArgs e)
        {
            new Thread(() => Utils.CopiarVersao(lblVersaoRelease.Text)).Start();
        }

        private void btnVersaoBuild_Click(object sender, EventArgs e)
        {
            new Thread(() => Utils.CopiarVersao(lblVersaoBuild.Text)).Start();
        }

        private void tcPrincipal_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (e.Page == tcPrincipal.TabPages[2]) CarregaVersoes();
        }

        private void CarregaVersoes()
        {
            string caminhoRede = @"\\10.1.1.110\Arquivos\Atualizacoes\MyCommerce";

            if (!Directory.Exists(caminhoRede))
            {
                XtraMessageBox.Show(
                    "O caminho de rede não está acessível:\n" + caminhoRede,
                    "Atenção",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                // Desabilita botões relacionados
                btnVersaoFull.Enabled = false;
                btnVersaoRelease.Enabled = false;
                btnVersaoBuild.Enabled = false;

                return;
            }

            // Se o caminho existir, habilita os botões
            btnVersaoFull.Enabled = true;
            btnVersaoRelease.Enabled = true;
            btnVersaoBuild.Enabled = true;

            // Continua a lógica normal de carregar versões
            DirectoryInfo diretorio = new DirectoryInfo(caminhoRede);
            FileInfo[] arquivos = diretorio.GetFiles("*.exe");

            foreach (FileInfo fi in arquivos)
            {
                if (fi.ToString().EndsWith("Full.exe"))
                {
                    lblVersaoFull.Text = fi.Name;
                    lblVersaoFullIns.Text = fi.Name; // Update for Instalacoes tab
                }
                else if ((fi == arquivos.Where(file => file.ToString().EndsWith("0.exe")).OrderByDescending(file => file.LastWriteTime).FirstOrDefault()))
                {
                    lblVersaoRelease.Text = fi.Name;
                    lblVersaoRealeseIns.Text = fi.Name; // Update for Instalacoes tab
                }
                else if ((fi == arquivos.Where(file => !fi.ToString().EndsWith("0.exe")).OrderByDescending(file => file.LastWriteTime).FirstOrDefault()))
                {
                    lblVersaoBuild.Text = fi.Name;
                }
            }
        }

        private void tpInstalacoes_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void lblVersaoRealeseIns_Click(object sender, EventArgs e)
        {

        }

        private void lblVersaoFullIns_Click(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var checkBoxes = tpInstalacoes.Controls.OfType<CheckEdit>().ToList();
            bool copiarFull = checkBox3.Checked;
            bool copiarRelease = checkBox4.Checked;
            string versaoFull = lblVersaoFullIns.Text;
            string versaoRelease = lblVersaoRealeseIns.Text;

            string resultado = await Task.Run(() => Utils.CriarPacoteDeInstalacao(checkBoxes, copiarFull, copiarRelease, versaoFull, versaoRelease));

            if (resultado != null)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Clipboard.SetText(resultado);
                    XtraMessageBox.Show($"Pacote criado com sucesso!\nO caminho foi copiado para a área de transferência:\n{resultado}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    XtraMessageBox.Show("Erro ao criar o pacote de instalação. Verifique o log de depuração para mais detalhes.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        private void TestGoogleDrive()
        {
            var service = vsHelp.Classes.GoogleDrive.GetService();

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            var files = listRequest.Execute().Files;
            string fileNames = "";
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    fileNames += file.Name + "\n";
                }
                //XtraMessageBox.Show(fileNames, "Arquivos do Google Drive", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                //XtraMessageBox.Show("Nenhum arquivo encontrado.", "Arquivos do Google Drive", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}