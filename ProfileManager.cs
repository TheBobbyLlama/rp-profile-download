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
                    ConditionalPrint(output, "image", curData.image);
                    output.AppendLine("\t\t},");
                }
                output.AppendLine("\t}");
                output.AppendLine("end");

                File.WriteAllText("RPProfileData.lua", output.ToString());
            }
            catch (HttpRequestException e)
            {
                MessageBox.Show(e.Message);
            }

            working = false;
        }

        private static void ConditionalPrint(StringBuilder output, string label, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                output.AppendLine("\t\t\t[\"" + label + "\"] = \"" + LuaFormat(value) + "\",");
            }
        }

        private static string LuaFormat(string input)
        {
            input = Regex.Replace(input, "/\\/g", "\\\\"); // Backslashes
            input = Regex.Replace(input, "\\|", "||"); // Pipes
            input = Regex.Replace(input, "\n", "\\n"); // New Lines
            input = Regex.Replace(input, "\"", "\\\""); // Quotes

            // MARKDOWN FORMATTING
            input = Regex.Replace(input, "#####\\s*(.+?\\\\n)", FormatHeaderText); // Header 5
            input = Regex.Replace(input, "####\\s*(.+?\\\\n)", FormatHeaderText); // Header 4
            input = Regex.Replace(input, "###\\s*(.+?\\\\n)", FormatHeaderText); // Header 3
            input = Regex.Replace(input, "##\\s*(.+?\\\\n)", FormatHeaderText); // Header 2
            input = Regex.Replace(input, "#\\s*(.+?\\\\n)", FormatHeaderText); // Header 1
            input = Regex.Replace(input, "{(.+?)}", "$1"); // Custom formatting - {Name} for profile link.
            input = Regex.Replace(input, "\\[(.*?)\\]\\(.*?\\)", "$1"); // Links
            input = Regex.Replace(input, "\\*\\*(.+?)\\*\\*", "|l0:1:1:0:1:C0C0C0|l$1|l"); // Bold (convert to underline)
            input = Regex.Replace(input, "~~(.+?)~~", "|l0:1:0:-25%:2:C0C0C0|l$1|l"); // Strikethrough
            return input;
        }

        private static string FormatHeaderText(Match input)
        {
            return input.Groups[1].ToString().ToUpper() + "\\n";
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
