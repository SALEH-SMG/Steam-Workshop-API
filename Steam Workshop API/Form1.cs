using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Steam_Workshop_API
{
    public partial class Form1 : Form
    {
        private readonly HttpClient client = new HttpClient();
        private const int MaxImageWidth = 100;
        private const int MaxImageHeight = 100;

        public Form1()
        {
            InitializeComponent();
            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            // Configure the DataGridView
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;

            // Define a new DataGridViewTextBoxColumn for the sequence number
            DataGridViewTextBoxColumn sequenceNumberColumn = new DataGridViewTextBoxColumn();
            sequenceNumberColumn.HeaderText = "SNo";
            sequenceNumberColumn.Name = "SequenceNo";
            dataGridView1.Columns.Insert(0, sequenceNumberColumn); // Insert it as the first column

            // Define columns
            DataGridViewImageColumn previewColumn = new DataGridViewImageColumn();
            previewColumn.HeaderText = "Preview";
            previewColumn.Name = "Preview";
            dataGridView1.Columns.Add(previewColumn);

            DataGridViewLinkColumn modUrlColumn = new DataGridViewLinkColumn();
            modUrlColumn.HeaderText = "ID";
            modUrlColumn.Name = "ID";
            dataGridView1.Columns.Add(modUrlColumn);

            DataGridViewTextBoxColumn titleColumn = new DataGridViewTextBoxColumn();
            titleColumn.HeaderText = "Title";
            titleColumn.Name = "Title";
            dataGridView1.Columns.Add(titleColumn);

            DataGridViewTextBoxColumn fileSizeColumn = new DataGridViewTextBoxColumn();
            fileSizeColumn.HeaderText = "Size";
            fileSizeColumn.Name = "FileSize";
            dataGridView1.Columns.Add(fileSizeColumn);

            DataGridViewTextBoxColumn timeCreatedColumn = new DataGridViewTextBoxColumn();
            timeCreatedColumn.HeaderText = "Time Created";
            timeCreatedColumn.Name = "TimeCreated";
            dataGridView1.Columns.Add(timeCreatedColumn);

            DataGridViewTextBoxColumn timeUpdatedColumn = new DataGridViewTextBoxColumn();
            timeUpdatedColumn.HeaderText = "Time Updated";
            timeUpdatedColumn.Name = "TimeUpdated";
            dataGridView1.Columns.Add(timeUpdatedColumn);

        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                string inputText = textBox2.Text;
                string[] Tvalues = inputText.Split(',');

                var values = new Dictionary<string, string>
                {
                    { "itemcount", Tvalues.Length.ToString() }
                };

                for (int t = 0; t < Tvalues.Length; t++)
                {
                    values.Add($"publishedfileids[{t}]", Tvalues[t]);
                }

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    List<Workshop> workshops = ParseWorkshopsFromJson(responseString);

                    // Clear existing rows before adding new data
                    dataGridView1.Rows.Clear();

                    int sequenceNumber = 1; // Initialize the sequence number
                    foreach (var workshop in workshops)
                    {
                        // Create a row for each workshop
                        DataGridViewRow newRow = new DataGridViewRow();

                        // Create a cell for the sequence number
                        DataGridViewTextBoxCell sequenceNumberCell = new DataGridViewTextBoxCell();
                        sequenceNumberCell.Value = sequenceNumber;
                        newRow.Cells.Add(sequenceNumberCell);
                        sequenceNumber++; // Increment the sequence number

                        // Create cells for each column and add them to the row
                        DataGridViewImageCell imageCell = new DataGridViewImageCell();
                        imageCell.Value = ResizeImage(GetImageFromUrl(workshop.Preview_url), MaxImageWidth, MaxImageHeight);
                        newRow.Cells.Add(imageCell);

                        // Create a clickable link cell for the ID
                        DataGridViewLinkCell modUrlCell = new DataGridViewLinkCell();
                        modUrlCell.Value = workshop.Publishedfileid;
                        modUrlCell.LinkVisited = false; // To ensure the link appears clickable
                        modUrlCell.Tag = $"https://steamcommunity.com/sharedfiles/filedetails/?id={workshop.Publishedfileid}";
                        newRow.Cells.Add(modUrlCell);

                        DataGridViewTextBoxCell titleCell = new DataGridViewTextBoxCell();
                        titleCell.Value = workshop.Title;
                        newRow.Cells.Add(titleCell);

                        DataGridViewTextBoxCell fileSizeCell = new DataGridViewTextBoxCell();
                        fileSizeCell.Value = workshop.FileSize;
                        newRow.Cells.Add(fileSizeCell);

                        DataGridViewTextBoxCell timeCreatedCell = new DataGridViewTextBoxCell();
                        timeCreatedCell.Value = workshop.TimeCreated;
                        newRow.Cells.Add(timeCreatedCell);

                        DataGridViewTextBoxCell timeUpdatedCell = new DataGridViewTextBoxCell();
                        timeUpdatedCell.Value = workshop.TimeUpdated;
                        newRow.Cells.Add(timeUpdatedCell);

                        // Add the populated row to the DataGridView
                        dataGridView1.Rows.Add(newRow);
                    }
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = dataGridView1[e.ColumnIndex, e.RowIndex];
                if (cell is DataGridViewLinkCell linkCell)
                {
                    var url = cell.Tag as string;
                    if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                    {
                        System.Diagnostics.Process.Start(uri.ToString());
                    }
                }
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
    }
}
