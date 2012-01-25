namespace CQMTClient.Source.Utility
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    public class FtpConnection
    {
        private FtpWebRequest ftpWebConnection;

        public FtpConnection(string domainName, string username, string password)
        {
            this.DomainName = domainName;
            this.Username = username;
            this.Password = password;
            ServicePointManager.ServerCertificateValidationCallback += CertificateValidationCallBack;
        }

        private string Username { get; set; }

        private string Password { get; set; }

        public string DomainName { get; private set; }

        public bool UploadFile(byte[] fileContents, string fileName, string pathToLocation)
        {
            if (this.CreateDirectory(pathToLocation) && !this.FileExists(pathToLocation + fileName))
            {
                this.CreateConnection(pathToLocation + "/" + fileName, WebRequestMethods.Ftp.UploadFile);
                this.ftpWebConnection.ContentLength = fileContents.Length;
                using (var requestStream = this.ftpWebConnection.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }
                return this.RecieveFtpWebResponse();
            }
            return false;
        }

        private bool CreateDirectory(string path)
        {
            var partsOfPath = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var relativePath = "/";
            foreach (var directory in partsOfPath)
            {
                relativePath += directory + "/";
                if (!this.CheckIndividualDirectoryExists(relativePath))
                {
                    if (!this.CreateIndividualDirectory(relativePath))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CreateIndividualDirectory(string path)
        {
            this.CreateConnection(path, WebRequestMethods.Ftp.MakeDirectory);
            return this.RecieveFtpWebResponse();
        }

        private bool CheckIndividualDirectoryExists(string path)
        {
            this.CreateConnection(path, WebRequestMethods.Ftp.ListDirectory);
            return this.RecieveFtpWebResponse();
        }

        private bool FileExists(string path)
        {
            this.CreateConnection(path, WebRequestMethods.Ftp.GetFileSize);
            return this.RecieveFtpWebResponse();
        }

        private bool RecieveFtpWebResponse()
        {
            try
            {
                using (this.ftpWebConnection.GetResponse()) ;
            }
            catch (WebException)
            {
                return false;
            }
            return true;
        }

        private void CreateConnection(string path, string method)
        {
            this.ftpWebConnection = (FtpWebRequest)WebRequest.Create("ftp://" + this.DomainName + path);
            this.ftpWebConnection.Credentials = new NetworkCredential(this.Username, this.Password);
            this.ftpWebConnection.Method = method;
            this.ftpWebConnection.KeepAlive = true;
            this.ftpWebConnection.EnableSsl = true;
        }

        private static bool CertificateValidationCallBack(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null)
                {
                    return chain.ChainStatus
                        .Where(status => (certificate.Subject != certificate.Issuer) || (status.Status != X509ChainStatusFlags.UntrustedRoot))
                        .All(status => status.Status == X509ChainStatusFlags.NoError);
                }
                return true;
            }
            return false;
        }
    }
}
