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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
namespace IPGEO
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private string PerformHttpRequest(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), UTF8Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private const string ApiIpBaseUrl = "https://apiip.net/api/check?ip=";
        private const string IpApiBaseUrl = "https://ipapi.co/";
        private const string IpApiComBaseUrl = "http://ip-api.com/";
        private const string IpApiKeyQuery = "&output=json";

        private async void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            string ipAddress = textBox2.Text;
            string kertcim ="";
            string url1 = "";
            if (!Regex.IsMatch(ipAddress, @"^(?:\d{1,3}\.){3}\d{1,3}$"))
            {
                MessageBox.Show("Érvénytelen IP-cím formátum!");
                return;
            }
            if (radioButton1.Checked)
            {//*ipapi.co
                kertcim = "https://ipapi.co/" + ipAddress + "/json";
                richTextBox1.Text = PerformHttpRequest(kertcim);
            }
            else if (radioButton2.Checked)
            { //apiip.net
                if (textBox3.Text.Length > 0)
                {
                    if (!Regex.IsMatch(textBox3.Text, @"^[a-zA-Z0-9-]+$"))
                    {
                        MessageBox.Show("Érvénytelen API kulcs!");
                        return;
                    }
                    kertcim = "https://apiip.net/api/check?ip=" + ipAddress + "&accessKey=" + textBox3.Text + "&output=json";
                    richTextBox1.Text = PerformHttpRequest(kertcim);
                }
                else
                {
                    MessageBox.Show("Nincs megadva a lekérési kulcs...");
                    textBox3.Focus();
                }
            }
            else if (radioButton3.Checked)
            {//ip-api.com
                string url = $"http://ip-api.com/json/{ipAddress}";
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetStringAsync(url);
                        richTextBox1.Text = response;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba történt az adatok lekérése során: " + ex.Message);
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
            listBox1.Items.Clear();
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
                MessageBox.Show("Hiba történt: " + ex.Message);
            }
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            textBox2.Text = listBox1.GetItemText(listBox1.SelectedItem);
        }

        //*
        private async Task<string> PerformHttpRequestAsync(string url)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // TLS protokoll konfiguráció (általában nem szükséges explicit)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    // User-Agent beállítása
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");

                    // Kérés elküldése
                    string response = await httpClient.GetStringAsync(url);

                    // Válasz visszaadása
                    return response;
                }
            }
            catch (HttpRequestException ex)
            {
                // Hibaüzenet megjelenítése
                MessageBox.Show("Hálózati hiba történt: " + ex.Message);
                return null;
            }
        }
        //*
        private async void button3_Click(object sender, EventArgs e)
        {//*tesztelos
         //*
            richTextBox1.Clear();
            string ipAddress = textBox2.Text;

            if (!Regex.IsMatch(ipAddress, @"^(?:\d{1,3}\.){3}\d{1,3}$"))
            {
                MessageBox.Show("Érvénytelen IP-cím formátum!");
                return;
            }
            string url = null;
            if (radioButton1.Checked)
            {
                url = $"{IpApiBaseUrl}{ipAddress}/json";
                string response = await PerformHttpRequestAsync(url);
                if (response != null)
                {
                    richTextBox1.Text = response;
                }
            }
            else if (radioButton2.Checked)
            {
                if (string.IsNullOrWhiteSpace(textBox3.Text) ||
                    !Regex.IsMatch(textBox3.Text, @"^[a-zA-Z0-9-]+$"))
                {
                    MessageBox.Show("Érvénytelen vagy hiányzó API kulcs!");
                    textBox3.Focus();
                    return;
                }
                url = $"{ApiIpBaseUrl}{ipAddress}&accessKey={textBox3.Text}&output=json";
                string response = await PerformHttpRequestAsync(url);
                if (response != null)
                {
                    richTextBox1.Text = response;
                }
            }
            else if (radioButton3.Checked)
            {
                url = $"{IpApiComBaseUrl}/json/{ipAddress}";
            }

            if (url != null)
            {
                try
                {
                    richTextBox1.Text = await PerformHttpRequestAsync(url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt: {ex.Message}");
                }
            }
            //*
        }
    }
}
