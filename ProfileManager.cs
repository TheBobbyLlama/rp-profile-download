using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace RPProfileDownloader
{
    public static class ProfileManager
    {
        public static bool working = false;
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Pulls profile info from Firebase and saves it to a file.
        /// </summary>
        public static async void UpdateProfiles()
        {
            working = true;

            try
            {
                string responseBody = await client.GetStringAsync("https://eso-roleplay.firebaseio.com/profiles.json");
                File.WriteAllText("profiles.json", responseBody);
                // TODO - Cache images???
                // JsonDocument profiles = JsonDocument.Parse(responseBody);
            }
            catch (HttpRequestException e)
            {
                MessageBox.Show(e.Message);
            }

            working = false;
        }
    }
}
