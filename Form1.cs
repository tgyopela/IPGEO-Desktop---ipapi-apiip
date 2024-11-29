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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.IO;
namespace IPGEO
{
    public partial class Form1 : Form
    {
        string apiKeysFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        string apiSecret;
        public Form1()
        {
            InitializeComponent();
        }

        static Form1()
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
        }

        private const string ApiIpBaseUrl = "https://apiip.net/api/check?ip=";
        private const string IpApiBaseUrl = "https://ipapi.co/";
        private const string IpApiComBaseUrl = "http://ip-api.com/";
        private const string IpGeolocationBaseUrl = "https://api.ipgeolocation.io/ipgeo?apiKey=";
        private const string IpApiKeyQuery = "&output=json";
        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private async void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            string ipAddress = textBox2.Text;
            if (!IPAddress.TryParse(ipAddress, out _))
            {
                MessageBox.Show("Érvénytelen IP-cím formátum!");
                return;
            }
            string url = null;
            if (radioButton1.Checked)
            {
                url = $"{IpApiBaseUrl}{ipAddress}/json";
                await ProcessResponse(url);
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
                await ProcessResponse(url);
            }
            else if (radioButton3.Checked)
            {
                url = $"{IpApiComBaseUrl}/json/{ipAddress}";
                await ProcessResponse(url);
            }
            else if (radioButton4.Checked) 
            {
                if (string.IsNullOrWhiteSpace(textBox3.Text) ||
                        !Regex.IsMatch(textBox3.Text, @"^[a-zA-Z0-9-]+$"))
                {
                    MessageBox.Show("Érvénytelen vagy hiányzó API kulcs!");
                    textBox3.Focus();
                    return;
                }
                url = $"{IpGeolocationBaseUrl}{textBox3.Text}&ip={textBox2.Text}&output=json";
                await ProcessResponse(url);
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
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (!string.IsNullOrEmpty(error)) // Ha van hiba, dobjuk kivételként
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

        private async Task<string> PerformHttpRequestAsync(string url)
        {
            try
            {              
                string response = await httpClient.GetStringAsync(url); // Kérés elküldése
                return response; // Válasz visszaadása
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Hálózati hiba történt: " + ex.Message); // Hibaüzenet megjelenítése
                return null;
            }
        }

        private async Task ProcessResponse(string url)
        {
            try
            {
                Cursor = Cursors.WaitCursor; // Várakozási kurzor
                string response = await PerformHttpRequestAsync(url);
                if (response != null)
                {
                    string formattedJson = FormatJson(response);
                    richTextBox1.Text = formattedJson;
                }
            }
            finally
            {
                Cursor = Cursors.Default; // Alapértelmezett kurzor
            }
        }

        private string FormatJson(string json)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json); // JSON deszerializálása objektumként
                var options = new JsonSerializerOptions   // Újraformázás szépen behúzva
                {
                    WriteIndented = true // Behúzás engedélyezése
                };
                return JsonSerializer.Serialize(jsonElement, options);
            }
            catch (JsonException ex)
            {
                return $"Hiba a JSON formázásában: {ex.Message}\n\nEredeti JSON:\n{json}";// Hiba esetén visszaadjuk az eredeti JSON-t
            }
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1) {button2_Click(sender, e);}
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                apiSecret = apiKeysFile + "\\apiip.api";
                if (File.Exists(apiSecret))
                {
                    textBox3.Text = File.ReadAllText(apiSecret);
                }
                else
                {
                    textBox3.Text = "Hiányzó / Missing ApiKey";
                } 
            }
            else { textBox3.Text = ""; }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                apiSecret = apiKeysFile + "\\ipgeo.api";
                if (File.Exists(apiSecret))
                {
                    textBox3.Text = File.ReadAllText(apiSecret);
                }
                else
                {
                    textBox3.Text = "Hiányzó / Missing ApiKey";
                }
            }
            else { textBox3.Text = ""; }
        }
        private void button3_Click(object sender, EventArgs e)
        {//*tesztelos
         //*

            //*
        }
    }
}
