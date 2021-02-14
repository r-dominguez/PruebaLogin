using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.IO;


namespace PruebaLogin.ClasesAuxiliares
{
    public class CORREO
    {
        public static int enviarCorreo(string nombreCorreo, string asunto, string contenido, string rutaLog)
        {
            int respuesta = 0;
            try
            {
                string correo = ConfigurationManager.AppSettings["correo"];
                string clave = ConfigurationManager.AppSettings["clave"];
                string servidor = ConfigurationManager.AppSettings["servidor"];
                int puerto = int.Parse(ConfigurationManager.AppSettings["puerto"]);

                MailMessage mail = new MailMessage();
                mail.Subject = asunto;
                mail.IsBodyHtml = true;
                mail.Body = contenido;
                mail.From = new MailAddress(correo);
                mail.To.Add(new MailAddress(nombreCorreo));

                SmtpClient smtp = new SmtpClient();
                smtp.Host = servidor;
                smtp.EnableSsl = true;
                smtp.Port = puerto;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(correo, clave);
                smtp.Send(mail);
                respuesta = 1; 
            }
            catch(Exception ex)
            {
                respuesta = 0;
                string registroLog = "No se envió mail a la cuenta " + nombreCorreo;
                File.AppendAllText(rutaLog, registroLog);
            }
            return respuesta;
        }
    }
}