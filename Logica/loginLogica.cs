using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DTO;
using Persistencia;
using Persistencia.MODELS;

namespace Logica
{
    public class LoginLogica
    {
        public List<Menu_CLS> Login(Usuario_CLS oUsuario_CLS)
        {
            
            List<Menu_CLS> listaMenu = new List<Menu_CLS>();
            
            string nombreUsuario = oUsuario_CLS.nombreUsuario;
            string password = oUsuario_CLS.contra;

            SHA256Managed sha = new SHA256Managed();
            byte[] byteContra = Encoding.Default.GetBytes(password);
            byte[] byteContraCifrada = sha.ComputeHash(byteContra);
            string cadenaContraCifrada = BitConverter.ToString(byteContraCifrada).Replace("-", "");

            UsuarioP usuarioP = new UsuarioP();

            listaMenu = usuarioP.recuperarMenus(oUsuario_CLS, cadenaContraCifrada);

            return listaMenu;
        }
    }
}
