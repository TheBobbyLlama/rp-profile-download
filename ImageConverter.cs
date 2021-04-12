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
        public static int workingCount = 0;
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Processes profile image info - only downloads images if we don't already have them.
        /// </summary>
        public static void createProfileImages(Dictionary<string, ImageData> input)
        {
            workingCount = input.Count;

            foreach (string key in input.Keys)
            {
                string thumbDirectory = String.Format("../images/thumbs/{0}", key);
                string thumbFile = String.Format("{0}.dds", input[key].hashString);

                if (!Directory.Exists(thumbDirectory))
                    Directory.CreateDirectory(thumbDirectory);

                if (!File.Exists(String.Format("{0}/{1}", thumbDirectory, thumbFile)))
                {
                    // Clean out any old images that are hanging around.
                    foreach (string curFile in Directory.GetFiles(thumbDirectory))
                        File.Delete(curFile);

                    ProcessImage(key, input[key].imageUrl, input[key].hashString);
                }
                else
                    workingCount--;
            }
        }

        /// <summary>
        /// Processes a single profile image asynchronously.
        /// </summary>
        public static async void ProcessImage(string key, string url, string hash)
        {
            try
            {
                Stream result = await client.GetStreamAsync(url);
                MagickImage curImage = new MagickImage(result);

                // Force to square.
                if (curImage.Height > curImage.Width)
                    curImage.ChopVertical(curImage.Width, curImage.Width);
                else if (curImage.Height < curImage.Width)
                    curImage.ChopHorizontal(curImage.Height, curImage.Height);

                // Addon will display 200x200, size to the next highest power of 2.
                curImage.AdaptiveResize(256, 256);

                curImage.Write(String.Format("../images/thumbs/{0}/{1}.dds", key, hash), curImage.HasAlpha ? MagickFormat.Dxt5 : MagickFormat.Dxt1);
            }
            catch { }

            workingCount--;
        }

        public class ImageData
        {
            public string hashString { get; set; }
            public string imageUrl { get; set; }

            public ImageData(string myHash, string myUrl)
            {
                hashString = myHash;
                imageUrl = myUrl;
            }
        }
    }
}
