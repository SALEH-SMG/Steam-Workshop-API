using Newtonsoft.Json;
using System;

namespace Steam_Workshop_API
{
    internal class Workshop
    {
        public string Publishedfileid { get; set; }
        public string Filename { get; set; }
        [JsonProperty("file_size")]
        public long FileSizeBytes { get; set; }
        public string Preview_url { get; set; }
        public string Title { get; set; }
        [JsonProperty("time_created")]
        public long TimeCreatedUnix { get; set; }
        [JsonProperty("time_updated")]
        public long TimeupdatedUnix { get; set; }

        [JsonIgnore] // Ignore this property when serializing to JSON
        public DateTime TimeCreated
        {
            get
            {
                return UnixTimeStampToDateTime(TimeCreatedUnix);
            }
        }
        public DateTime TimeUpdated
        {
            get
            {
                return UnixTimeStampToDateTime(TimeupdatedUnix);
            }
        }

        public string FileSize
        {
            get
            {
                return FormatFileSize(FileSizeBytes);
            }
        }
        // Function to convert Unix timestamp to DateTime
        public static DateTime UnixTimeStampToDateTime(long unixTimestamp)
        {
            // The Unix timestamp is in seconds, so you need to multiply it by 1000 to get milliseconds
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixEpoch.AddSeconds(unixTimestamp).ToLocalTime();
        }
        // Function to format file size in a human-readable way
        public static string FormatFileSize(long fileSizeBytes)
        {
            const int byteConversion = 1024;
            double bytes = Convert.ToDouble(fileSizeBytes);

            if (bytes >= Math.Pow(byteConversion, 3)) // Gigabytes
            {
                return string.Format("{0:##.##} GB", bytes / Math.Pow(byteConversion, 3));
            }
            else if (bytes >= Math.Pow(byteConversion, 2)) // Megabytes
            {
                return string.Format("{0:##.##} MB", bytes / Math.Pow(byteConversion, 2));
            }
            else if (bytes >= byteConversion) // Kilobytes
            {
                return string.Format("{0:##.##} KB", bytes / byteConversion);
            }
            else // Bytes
            {
                return string.Format("{0} Bytes", bytes);
            }
        }
    }
}
