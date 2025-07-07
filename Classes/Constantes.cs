using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vsHelp.Classes
{
    internal class Constantes
    {
        public static Dictionary<string, Action> UpdatePorCheckBox = new()
        {
            { "cbSenhaUsuario", Utils.AtualizarSenhasUsuarios },
            { "cbSenhaSupervisor", Utils.AtualizarSenhasSupervisores },
            { "cbEmail", Utils.AtualizarEmails },
            { "cbTelefone", Utils.AtualizarTelefones },
            { "cbAtualizaDB", Utils.AtualizarDB },
        };
    }
}
