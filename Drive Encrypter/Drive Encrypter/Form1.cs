using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Configuration;
using System.ComponentModel;

namespace Drive_Encrypter
{
    public partial class Form1 : Form
    {
        string seldrive = "";
        string curfile = " ";
        BackgroundWorker bgwork = null;
        public Form1()
        {
            InitializeComponent();
            for (char i = 'A'; i <= 'Z'; i++)
            {
                if (Directory.Exists(i + ":\\"))
                {
                    if (i != 'C')
                        comboBox1.Items.Add(i+":\\");
                }
           
            }
            bgwork = backgroundWorker2;
        }

        //AES encryption algorithm
        static private byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }
        //Encrypts single file
        private void EncKarFile(string file, string password)
        {
            byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            File.WriteAllBytes(file, bytesEncrypted);
            System.IO.File.Move(file, file + ".acepradator");


        }
        //encrypts target directory
        private void encKarDict(string location, string password)
        {

            //extensions to be encrypt
            try
            {
                string[] files = Directory.GetFiles(location);
                string[] cDict = Directory.GetDirectories(location);
                for (int i = 0; i < files.Length; i++)
                {
                    string exts = Path.GetExtension(files[i]);
                    if (!exts.Contains(".acepradator"))
                    {
                        EncKarFile(files[i], password);
                    }

                }
                for (int i = 0; i < cDict.Length; i++)
                {
                    backgroundWorker2.ReportProgress((i / cDict.Length) * 100);
                    encKarDict(cDict[i], password);
                }
            }
            catch (Exception ex)
            {
                //lol we can not do anything
            }

        }
        private void encKarDictfull(string location, string password)
        {

            try
            {
                string[] files = Directory.GetFiles(location);
                string[] cDict = Directory.GetDirectories(location);
                for (int i = 0; i < files.Length; i++)
                {
                    EncKarFile(files[i], password);
                }
                for (int i = 0; i < cDict.Length; i++)
                {
                    int len = cDict.Length;
                    Decimal percant = ((Convert.ToDecimal(i) / Convert.ToDecimal(len)) * 100);
                    backgroundWorker2.ReportProgress(Convert.ToInt32(percant));
                    curfile = cDict[i];
                    if(!cDict[i].Contains(seldrive+ "System Volume Information"))
                        encKarDict(cDict[i], password);
                }
            }
            catch (Exception ex)
            {
                //lol we can not do anything
            }
            backgroundWorker2.ReportProgress(100);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            /*Encrypt*/
            button1.Enabled = false;
            button2.Enabled = false;
            try
            {
                seldrive = comboBox1.SelectedItem.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please Select Drive" + ex.ToString());
                return;
            }
            backgroundWorker2.RunWorkerAsync();
        }

        public byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {

                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();


                }
            }

            return decryptedBytes;
        }

        public void DecryptFile(string file, string password)
        {
            byte[] bytesToBeDecrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            File.WriteAllBytes(file, bytesDecrypted);
            string extension = System.IO.Path.GetExtension(file);
            string result = file.Substring(0, file.Length - extension.Length);
            System.IO.File.Move(file, result);

        }

        public void DecryptDirectory(string location, string password)
        {
            int err = 0;
            try
            {
                string[] files = Directory.GetFiles(location);
                string[] childDirectories = Directory.GetDirectories(location);
                for (int i = 0; i < files.Length; i++)
                {
                    string extension = Path.GetExtension(files[i]);
                    if (extension.Equals(".acepradator"))
                    {
                        DecryptFile(files[i], password);
                    }
                }
                for (int i = 0; i < childDirectories.Length; i++)
                {
                    int len = childDirectories.Length;
                    Decimal percant = ((Convert.ToDecimal(i)/Convert.ToDecimal(len)) * 100);
                    backgroundWorker1.ReportProgress(Convert.ToInt32(percant));
                    curfile = childDirectories[i];
                    if (!childDirectories[i].Contains(seldrive + "System Volume Information"))
                        DecryptDirectory(childDirectories[i], password);
                }
            }
            catch (Exception ex)
            {
                err = err + 1;
                MessageBox.Show(ex.Message);
            }
            if(err != 0)
            {
                MessageBox.Show("Error :" + err + " May Be PassWord is Wrong");
            }
            backgroundWorker1.ReportProgress(100);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //decrypt
            button1.Enabled = false;
            button2.Enabled = false;
            try
            {
                seldrive = comboBox1.SelectedItem.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please Select Drive " + ex.Message);
                return;
            }
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (key.Text.Equals("kingno1"))
            {
                DecryptDirectory(seldrive, textBox1.Text);
                MessageBox.Show("Decrypted", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Wrong Key", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (key.Text.Equals("kingno1"))
            {
                encKarDictfull(seldrive, textBox1.Text);
                MessageBox.Show("Encrypted", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Wrong Key", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            lblcurfile.Text = curfile;
        }

        private void backgroundWorker2_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            lblcurfile.Text = curfile;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = true;
            lblcurfile.Text = "Done :)";
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = true;
            lblcurfile.Text = "Done :)";
        }
    }
}
