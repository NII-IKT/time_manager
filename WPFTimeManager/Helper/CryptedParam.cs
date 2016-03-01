using NLog;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace WPFTimeManager
{
    public static class CryptedParam
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const string mustContains = "I4DUfg,zn c S!HD dn8AFSD Hje13";
        private const string passForCrypt = " d@rA#D3!4%d^bc&na#d";
        
        private static string Encrypt(string plainText, string password, string salt = mustContains, string hashAlgorithm = "SHA1",
            int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY", int keySize = 256)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return "";

                byte[] initialVectorBytes = Encoding.ASCII.GetBytes(initialVector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(salt);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

                PasswordDeriveBytes derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, hashAlgorithm, passwordIterations);

                byte[] keyBytes = derivedPassword.GetBytes(keySize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;

                byte[] cipherTextBytes = null;

                using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initialVectorBytes))
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            cipherTextBytes = memStream.ToArray();
                            memStream.Close();
                            cryptoStream.Close();
                        }
                    }
                }

                symmetricKey.Clear();
                return Convert.ToBase64String(cipherTextBytes);
            }
            catch
            {
                logger.Error("Не удалось выполнить Encrypt.");
                return null;
            }
        }

        private static string Decrypt(string cipherText, string password, string salt = mustContains, string hashAlgorithm = "SHA1",
            int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY", int keySize = 256)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                    return "";

                byte[] initialVectorBytes = Encoding.ASCII.GetBytes(initialVector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(salt);
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

                PasswordDeriveBytes derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, hashAlgorithm, passwordIterations);
                byte[] keyBytes = derivedPassword.GetBytes(keySize / 8);

                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;

                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int byteCount = 0;

                using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes))
                {
                    using (MemoryStream memStream = new MemoryStream(cipherTextBytes))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
                        {
                            byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memStream.Close();
                            cryptoStream.Close();
                        }
                    }
                }

                symmetricKey.Clear();
                return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
            }
            catch
            {
                logger.Error("Не удалось выполнить Decrypt.");
                return null;
            }
        }

        public static string GetCryptPass(string pathToConfig)
        {
            try
            {
                return File.ReadLines(pathToConfig).First();
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:GET PASSWORD:{0}", ex.Message);
                return null;
            }
        }

        public static bool SetPassword(string pathToConfig, string newValue)
        {
            try
            {
                string temp = File.ReadLines(pathToConfig).Skip(1).First();
                using (StreamWriter sw = new StreamWriter(pathToConfig, false))
                {
                    sw.WriteLine(Encrypt(newValue + mustContains, passForCrypt));
                    sw.WriteLine(temp);
                    sw.Close();
                }
                return true;

            }
            catch
            {
                logger.Error("ERROR:SET PASS");
                return false;
            }
        }

        public static int GetIdle(string pathToConfig)
        {
            try
            {
                return int.Parse(Decrypt(File.ReadLines(pathToConfig).Skip(1).First(), passForCrypt));
            }
            catch (Exception ex)
            {
                logger.Error("ERROR:GET PASSWORD:{0}", ex.Message);
                MessageBox.Show("Страное время простоя.Оно точно не поправлен в конфигах?");
                Application.Current.Shutdown();
                return 0;
            }
        }

        public static bool SetIdle(string pathToConfig, string newValue)
        {
            try
            {
                string temp = GetCryptPass(pathToConfig);
                using (StreamWriter sw = new StreamWriter(pathToConfig, false))
                {
                    sw.WriteLine(temp);
                    sw.WriteLine(Encrypt(newValue, passForCrypt));
                    sw.Close();
                }
                return true;

            }
            catch
            {
                logger.Error("ERROR:SET IDLE");
                return false;
            }
        }

        public static bool VerifyPass(string pathToConfig, string pass)
        {
            if (Encrypt(pass + mustContains, passForCrypt) == GetCryptPass(pathToConfig))
                return true;
            else return false;
        }

        public static bool CheckPass(string pathToConfig)
        {
            try
            {
                if (Decrypt(GetCryptPass(pathToConfig), passForCrypt).Contains(mustContains))
                    return true;
                else return false;
            }
            catch (Exception ex)
            {
                logger.Error("PASSWORD READ ERROR:{0},{1}", ex.Message, Decrypt(GetCryptPass(pathToConfig), passForCrypt));
                return false;
            }
        }

        public static void CreateDefaultConfig(string pathToConfig)
        {
            using (StreamWriter sw = new StreamWriter(pathToConfig, false))
            {
                sw.WriteLine(Encrypt("1234" + mustContains, passForCrypt));//Pass=1234
                sw.WriteLine(Encrypt("10", passForCrypt));//Idle=10
                sw.Close();
            }
        }
    }
}