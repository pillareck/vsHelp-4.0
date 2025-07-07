using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vsHelp.Classes
{ 
    public class Criptografia
    {
            private const int MIN_ASC = 32;
            private const int MAX_ASC = 126;
            private const int NUM_ASC = MAX_ASC - MIN_ASC + 1;
            private const int CHAVE = 2001;

            public static string CriptSenha(string Psenha)
            {
                StringBuilder toText = new StringBuilder();
                long offset = NumericPassword(CHAVE.ToString());
                Random random = new Random((int)offset);

                foreach (char ch in Psenha)
                {
                    int charCode = (int)ch;
                    if (charCode >= MIN_ASC && charCode <= MAX_ASC)
                    {
                        charCode -= MIN_ASC;
                        int randomOffset = random.Next(NUM_ASC + 1);
                        charCode = (charCode + randomOffset) % NUM_ASC;
                        charCode += MIN_ASC;
                        toText.Append((char)charCode);
                    }
                }

                return toText.ToString();
            }

        public static string DeCriptSenha(string Psenha)
        {
            // Dim v_sqlerrm As String
            // Dim SenhaCript As String
            // Dim var1 As String
            const int MIN_ASC = 32;
            const int MAX_ASC = 126;
            const int NUM_ASC = MAX_ASC - MIN_ASC + 1;

            var chave = "2001"; // qualquer nº para montar o algorítimo da criptografia

            float offset;
            int str_len;
            int I;
            float ch;
            string to_text;

            to_text = "";
            offset = NumericPassword(chave);
            VBMath.Rnd(-1);
            VBMath.Randomize(offset);
            str_len = Strings.Len(Psenha);

            for (I = 1; I <= str_len; I++)
            {
                ch = Strings.Asc(Strings.Mid(Psenha, I, 1));
                if (ch >= MIN_ASC & ch <= MAX_ASC)
                {
                    ch = ch - MIN_ASC;
                    offset = Conversion.Int((NUM_ASC + 1) * VBMath.Rnd());
                    ch = ((ch - offset) % NUM_ASC);

                    // Início do If
                    if (ch < 0)
                        ch = ch + NUM_ASC;
                    ch = ch + MIN_ASC;
                    to_text = to_text + Strings.Chr((int)ch);
                }
            }
            // Término do For

            return to_text;
        }

        private static int NumericPassword(string password)
        {
            // Início da Declaração das Variáveis
            int Value = 0;
            int ch;
            int shift1 = 0;
            int shift2 = 0;
            int I;
            int str_len;
            // Término da Declaração das Variáveis

            str_len = Strings.Len(password);

            // Início do For
            for (I = 1; I <= str_len; I++)
            {
                ch = Strings.Asc(Strings.Mid(password, I, 1));
                Value ^= (int)(ch * Math.Pow(2, shift1));
                Value ^= (int)(ch * Math.Pow(2, shift2));

                shift1 = (shift1 + 7) % 19;
                shift2 = (shift2 + 13) % 23;
            }
            // Término do For
            return (int)Value;
        }

    }



}

