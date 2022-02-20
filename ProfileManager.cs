using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace RPProfileDownloader
{
    /// <summary>
    /// Used for pulling profile information from the ESO Rollplay system and converting it into a LUA file.
    /// </summary>
    public static class ProfileManager
    {
        public static bool working = false;
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Pulls profile info from Firebase and saves it to a file.
        /// </summary>
        public static async void UpdateProfiles()
        {
            Dictionary<string, ImageConverter.ImageData> imageLookup = new Dictionary<string, ImageConverter.ImageData>();
            working = true;

            try
            {
                string responseBody = await client.GetStringAsync("https://eso-roleplay.firebaseio.com/profiles.json");
                StringBuilder output = new StringBuilder();
                Dictionary<string, ProfileData> profileData = JsonConvert.DeserializeObject<Dictionary<string, ProfileData>>(responseBody);

                output.AppendLine("function RPProfileViewer: LoadProfileData()");
                output.AppendLine("\tself.ProfileData = {");
                foreach(string key in profileData.Keys)
                {
                    ProfileData curData = profileData[key];
                    output.AppendLine("\t\t[\"" + key + "\"] = {");
                    ConditionalPrint(output, "alignment", curData.alignment);
                    ConditionalPrint(output, "birthsign", curData.birthsign);
                    ConditionalPrint(output, "aliases", curData.aliases);
                    ConditionalPrint(output, "enemies", curData.enemies);
                    ConditionalPrint(output, "organizations", curData.organizations);
                    ConditionalPrint(output, "relationships", curData.relationships);
                    ConditionalPrint(output, "residence", curData.residence);
                    ConditionalPrint(output, "description", curData.description);
                    ConditionalPrint(output, "biography", curData.biography);

                    if (!String.IsNullOrEmpty(curData.image))
                    {
                        // Use a hash code for our image, to make it easier to track changes in the future.
                        string imageHash = Math.Abs(curData.image.GetHashCode()).ToString();
                        imageLookup.Add(key, new ImageConverter.ImageData(imageHash, curData.image));
                        ConditionalPrint(output, "image", imageHash);
                    }

                    output.AppendLine("\t\t},");
                }
                output.AppendLine("\t}");
                output.AppendLine("end");

                File.WriteAllText("RPProfileData.lua", output.ToString());
                ImageConverter.createProfileImages(imageLookup);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "An Error Occurred While Updating!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            working = false;
        }

        /// <summary>
        /// Helper function for spitting out LUA output.  Only adds to output if it gets a value.
        /// </summary>
        private static void ConditionalPrint(StringBuilder output, string label, string value)
        {
            if (!String.IsNullOrEmpty(value))
                output.AppendLine("\t\t\t[\"" + label + "\"] = \"" + LuaFormat(value) + "\",");
        }

        private static string LuaFormat(string input)
        {
            input = Regex.Replace(input, "/\\/g", "\\\\"); // Backslashes
            input = Regex.Replace(input, "\\|", "||"); // Pipes
            input = Regex.Replace(input, "(<br */?>|\n)", "\\n"); // New Lines
            input = Regex.Replace(input, "\"", "\\\""); // Quotes
            input = Regex.Replace(input, "&lt;", "<");
            input = Regex.Replace(input, "&gt;", ">");

            // MARKDOWN FORMATTING
            input = Regex.Replace(input, "(^|\\\\n)#{1,5} ?(.+?\\\\n)", FormatHeaderText);
            input = Regex.Replace(input, "{(.+?)}", "$1"); // Custom formatting - {Name} for profile link.
            input = Regex.Replace(input, "!(\\[.*?\\])\\(.*?\\)", "[$1]"); // Images
            input = Regex.Replace(input, "\\[(.*?)\\]\\(.*?\\)", "$1"); // Links
            input = Regex.Replace(input, "\\*\\*(.+?)\\*\\*", "*$1*"); // Bold (convert to single asterisk)
            input = Regex.Replace(input, "~~(.+?)~~", "-$1-"); // Strikethrough
            return input;
        }

        /// <summary>
        /// Helper function for converting "### Header Text" to "HEADER TEXT"
        /// </summary>
        private static string FormatHeaderText(Match input)
        {
            return input.Groups[2].ToString().ToUpper();
        }

        public class ProfileData
        {
            public string aliases { get; set; }
            public string alignment { get; set; }
            public string alliances { get; set; }
            public string birthsign { get; set; }
            public string enemies { get; set; }
            public string organizations { get; set; }
            public string relationships { get; set; }
            public string residence { get; set; }

            public string description { get; set; }
            public string biography { get; set; }
            public string image { get; set; }
        }
    }
}
