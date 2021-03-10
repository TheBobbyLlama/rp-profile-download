using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using ImageMagick;

namespace RPProfileDownloader
{
    public static class ImageConverter
    {
        public static bool working = false;
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Helper function to choose DDS format, based on whether incoming image type supports transparency.
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        private static MagickFormat ChooseImageFormat(string imageName)
        {
            string extension = imageName.ToLower().Split(new char[] { '.' }).Last();

            switch (extension)
            {
                case "gif":
                case "png":
                    return MagickFormat.Dxt5;
                default:
                    return MagickFormat.Dxt1;
            }
        }
        public static async void createProfileImages(Dictionary<string, Data> input)
        {
            working = true;

            foreach (string key in input.Keys)
            {
                string thumbDirectory = String.Format("../images/thumbs/{0}", key);
                string thumbFile = String.Format("{0}.dds", input[key].hashString);

                if (!Directory.Exists(thumbDirectory))
                    Directory.CreateDirectory(thumbDirectory);

                if (!File.Exists(String.Format("{0}/{1}", thumbDirectory, thumbFile)))
                {
                    foreach (string curFile in Directory.GetFiles(thumbDirectory))
                        File.Delete(curFile);

                    Stream result = await client.GetStreamAsync(input[key].imageUrl);
                    MagickImage test = new MagickImage(result);

                    // Force to square.
                    if (test.Height > test.Width)
                        test.ChopVertical(test.Width, test.Width);
                    else if (test.Height < test.Width)
                        test.ChopHorizontal(test.Height, test.Height);

                    test.AdaptiveResize(256, 256);

                    test.Write(String.Format("../images/thumbs/{0}/{1}.dds", key, input[key].hashString), ChooseImageFormat(input[key].imageUrl));
                }
            }

            working = false;
        }
        public class Data
        {
            public string hashString { get; set; }
            public string imageUrl { get; set; }

            public Data(string myHash, string myUrl)
            {
                hashString = myHash;
                imageUrl = myUrl;
            }
        }
    }
}
