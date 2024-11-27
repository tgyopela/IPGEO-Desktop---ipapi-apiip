using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Security.Policy;
using System.Diagnostics;

namespace IPGEO
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string ipAddress;
            string command;
            string kulcs;
            //*ipapi.co
            if (radioButton1.Checked == true)
            {
                ipAddress = textBox2.Text;
                command = $"curl 'https://ipapi.co/{ipAddress}/json'";
                try
                {
                    string output = RunPowerShellCommand(command);
                    richTextBox1.Text = output;
                }
                catch (Exception ex)
                {
                    richTextBox1.Text += ex.Message;
                }
            }
            //apiip.net
            if (radioButton2.Checked == true)
            {
                ipAddress = textBox2.Text;
                if (textBox3.Text.Length > 0) 
                {
                    kulcs = textBox3.Text;
                    command = $"curl 'https://apiip.net/api/check?ip={ipAddress}&accessKey={kulcs}&output=json'";
                    try
                    {
                        string output = RunPowerShellCommand(command);
                        richTextBox1.Text = output;
                    }
                    catch (Exception ex)
                    {
                        richTextBox1.Text += ex.Message;
                    }
                }
                else
                {
                    MessageBox.Show("Nincs megadva a lekérési kulcs...");
                    textBox3.Focus(); 
                }

                
            }
        }
        private string RunPowerShellCommand(string command) //*IP lekérés
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-NoProfile -Command \"{command}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                // PowerShell futtatása
                process.Start();

                // Kimenet és hibaüzenet olvasása
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // Ha van hiba, dobjuk kivételként
                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }

                return output;
            }
        }

        private void button2_Click(object sender, EventArgs e) // A feloldandó domain
        {
            string domain = textBox1.Text; 
            string command = $"(Resolve-DnsName {domain}).IPAddress ";
            try
            {
                string result = RunPowerShellCommand(command);
                richTextBox2.Text = result;
                foreach (string line in richTextBox2.Lines)
                {
                    listBox1.Items.Add(line);
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            textBox2.Text = listBox1.GetItemText(listBox1.SelectedItem);
        }
    }
}
