using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BotSendPrice.Services
{
    public static class FTPSender
    {
        private static string login;
        private static string pass;
        private static string ftpAddress;
        
        public async static Task<int> SendFileAsync(string path)
        {

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpAddress);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(login, pass);

            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    await fileStream.CopyToAsync(requestStream);
                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == FtpStatusCode.CommandOK ||
                            response.StatusCode == FtpStatusCode.FileActionOK)
                            return 0;
                        else
                            return -1;
                    }
                }
            }
        }


    }
}
