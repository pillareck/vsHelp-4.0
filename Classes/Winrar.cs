using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SharpCompress.Archives.Zip;
using SharpCompress.Writers;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace vsHelp.Classes
{
    public class Winrar
    {
        internal static string Descompacta(string caminho)
        {
            using (var arquivoLido = RarArchive.Open(caminho, new SharpCompress.Readers.ReaderOptions { Password = "vssql" })) // Verificar a lib SevenZipExtractor
            {
                foreach (var arquivo in arquivoLido.Entries.Where(entry => !entry.IsDirectory && entry.Key.EndsWith(".sql")))
                {
                    try
                    {
                        arquivo.WriteToDirectory(AppDomain.CurrentDomain.BaseDirectory, new ExtractionOptions()
                        {
                            ExtractFullPath = false,
                            Overwrite = true
                        });
                    }
                    catch (SharpCompress.Common.CryptographicException)
                    {
                        throw new OperationCanceledException("O arquivo tem senha, contate o suporte!");
                    }

                    return $"{AppDomain.CurrentDomain.BaseDirectory}{Path.GetFileName(arquivo.Key)}";
                }
            }

            return caminho;
        }

        internal static string CompactarPasta(string pastaParaCompactar, string arquivoDeSaida)
        {
            try
            {
                using (var archive = ZipArchive.Create())
                {
                    archive.AddAllFromDirectory(pastaParaCompactar);
                    var writerOptions = new WriterOptions(CompressionType.Deflate) { LeaveStreamOpen = false };
                    archive.SaveTo(arquivoDeSaida, writerOptions);
                }
                return arquivoDeSaida;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Erro ao compactar arquivos: {ex.Message}", "Erro de Compactação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
