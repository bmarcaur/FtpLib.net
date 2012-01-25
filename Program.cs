using System;

namespace FtpTest
{
    using System.IO;

    static class Program
    {
        [STAThread]
        static void Main()
        {
            var testConnection = new FtpConnection("SERVER", "USER", "PASSWORD");
            var testFileLocation = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/testFile.txt";
            var testByteArray = File.ReadAllBytes(testFileLocation);
            testConnection.UploadFile(testByteArray, "testName", "/SOMEPATH");
        }
    }
}
