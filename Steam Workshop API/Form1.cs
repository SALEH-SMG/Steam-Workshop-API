using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Steam_Workshop_API
{
    public partial class Form1 : Form
    {
        private readonly HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the text from textBox2
                string inputText = textBox2.Text;

                // Split the input text by a comma (',') into an array of strings
                string[] Tvalues = inputText.Split(',');

                // Count the number of values in the array
                int count = Tvalues.Length;

                     var values = new Dictionary<string, string>
                     {
                         { "itemcount", count.ToString() }
                     };
                for (int t = 0; t < count;  t++) { 
                // Add the array to the dictionary if that's your intention
                values.Add("publishedfileids["+ t + "]", string.Join(",", Tvalues[t]));
                }
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    List<Workshop> workshops = ParseWorkshopsFromJson(responseString);
                    //dataGridView1.DataSource = workshops;
                    dataGridView1.DataSource = workshops.Select(w => new
                    {
                        // Select the properties you want to display in the DataGridView
                        w.Title,
                        w.FileSize,
                        w.TimeCreated,
                        w.TimeUpdated,
                        w.Preview_url// Use the formatted date property
                    }).ToList();
                SetDataGridViewHeaders();
                }
                else
                {
                    MessageBox.Show("Error: Unable to fetch data from the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<Workshop> ParseWorkshopsFromJson(string json)
        {
            JObject jsonObj = JObject.Parse(json);
            JObject responseObj = jsonObj["response"].ToObject<JObject>();
            List<Workshop> workshops = responseObj["publishedfiledetails"].ToObject<List<Workshop>>();
            return workshops;
        }

        private void SetDataGridViewHeaders()
        {
            dataGridView1.Columns["Title"].HeaderText = "Workshop Title";
            dataGridView1.Columns["FileSize"].HeaderText = "File Size (Bytes)";
            dataGridView1.Columns["TimeCreated"].HeaderText = "Time Created";
            dataGridView1.Columns["TimeUpdated"].HeaderText = "Time Updated";
            dataGridView1.Columns["Preview_url"].HeaderText = "Preview URL";
        }
    }
}
