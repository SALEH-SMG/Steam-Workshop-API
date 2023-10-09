using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

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

                    // Create a new DataGridViewImageColumn for the "Preview URL" column
                    //DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
                    //imageColumn.HeaderText = "Preview";
                    //imageColumn.Name = "Preview";
                    //dataGridView1.Columns.Add(imageColumn);


                    // Define maximum width and height for the resized images
                    int maxWidth = 100; // Adjust to your desired width
                    int maxHeight = 100; // Adjust to your desired height

                    //dataGridView1.DataSource = workshops;
                    // Populate the DataGridView
                    dataGridView1.DataSource = workshops.Select(w => new
                    {
                        // Select the properties you want to display in the DataGridView
                        // Set the "Preview URL" as the value of the DataGridViewImageColumn
                        Preview = ResizeImage(GetImageFromUrl(w.Preview_url), maxWidth, maxHeight),
                        w.Title,
                        w.FileSize,
                        w.TimeCreated,
                        w.TimeUpdated
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

        private Image GetImageFromUrl(string imageUrl)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData(imageUrl);
                    using (MemoryStream memoryStream = new MemoryStream(data))
                    {
                        return Image.FromStream(memoryStream);
                    }
                }
            }
            catch
            {
                // Return a placeholder image or handle the error as needed
                return null;
            }
        }

        private Image ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            if (image == null)
            {
                // Handle the case where the image is null (e.g., error loading the image)
                return null;
            }

            int newWidth, newHeight;
            if (image.Width > image.Height)
            {
                newWidth = maxWidth;
                newHeight = (int)((float)image.Height / image.Width * maxWidth);
            }
            else
            {
                newHeight = maxHeight;
                newWidth = (int)((float)image.Width / image.Height * maxHeight);
            }

            Image resizedImage = new Bitmap(newWidth, newHeight);

            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
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
            //dataGridView1.Columns["Preview_url"].HeaderText = "Preview URL";
        }
    }
}
