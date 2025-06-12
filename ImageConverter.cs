using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Drawing;
using System.Drawing.Imaging;

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
        /// We use the same DDS header for every image, so spit it out here.
        /// https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dds-header
        /// </summary>
        private static void WriteDDSHeader(BinaryWriter writer)
        {
            int i;

            // magic number
            writer.Write((uint)0x20534444);

            // DDS_HEADER
            writer.Write(124); // size
            writer.Write((uint)0x81007); // flags - DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT | DDSD_LINEARSIZE
            writer.Write(256); // height
            writer.Write(256); // width
            writer.Write(1036); // pitch/linearsize
            writer.Write(0); // depth
            writer.Write(1); // mipmapcount

            for (i = 0; i < 11; i++)
                writer.Write(0); // reserved

            writer.Write(32); // pixelformat - size
            writer.Write(0x05); // pixelformat - flags - DDPF_ALPHAPIXELS | DDPF_FOURCC
            writer.Write((uint)0x35545844); // pixelformat - fourcc
            writer.Write(0); // pixelformat - rgbbitcount
            writer.Write(0x00ff0000); // pixelformat - rbitmask
            writer.Write(0x0000ff00); // pixelformat - gbitmask
            writer.Write(0x000000ff); // pixelformat - bbitmask
            writer.Write(0xff000000); // pixelformat - abitmask
            writer.Write(0x1000); // caps - DDSCAPS_TEXTURE

            for (i = 0; i < 4; i++)
                writer.Write(0); // caps 2-4 + reserved2
        }

        /// <summary>
        /// Encode a 4x4 block (BC3 format)
        /// https://learn.microsoft.com/en-us/windows/win32/direct3d10/d3d10-graphics-programming-guide-resources-block-compression#bc3
        /// </summary>
        private static void EncodeTexelBlockBC3(BinaryWriter writer, List<Texel> texelData)
        {
            int i;

            byte alpha_0 =  texelData[0].alpha; // Max alpha
            byte alpha_1 =  texelData[0].alpha; // Min alpha
            int color_0 = 0; // Max color index
            double color_0_dist = -1;
            int color_1 = 0; // Min color index
            double color_1_dist = -1;

            // Collect min/max values for the block
            for (i = 0; i < texelData.Count; i++)
            {
                alpha_0 = Math.Max(alpha_0, texelData[i].alpha);
                alpha_1 = Math.Min(alpha_1, texelData[i].alpha);

                // Ignore color from transparent pixels
                if (texelData[i].alpha > 0)
                {
                    double tmpBdist = texelData[i].Distance(Texel.Black);
                    double tmpWdist = texelData[i].Distance(Texel.White);

                    if (tmpBdist > color_0_dist)
                    {
                        color_0 = i;
                        color_0_dist = tmpBdist;
                    }

                    if (tmpWdist > color_1_dist)
                    {
                        color_1 = i;
                        color_1_dist = tmpWdist;
                    }
                }
            }

            writer.Write(alpha_0);
            writer.Write(alpha_1);

            ulong alphaData = 0;

            // Pack alpha values if needed
            if (alpha_0 != alpha_1)
            {
                double alphaDiff = alpha_0 - alpha_1;

                for (i = 0; i < texelData.Count; i += 8)
                {
                    for (int j = i + 7; j >= i; j--)
                    {
                        alphaData <<= 3;

                        double interpolate = (alpha_0 - texelData[j].alpha) / alphaDiff;

                        if (interpolate < 0.125)
                            alphaData |= 0; // alpha_0
                        else if (interpolate < 0.25)
                            alphaData |= 2; // alpha_2
                        else if (interpolate < 0.375)
                            alphaData |= 3; // alpha_3
                        else if (interpolate < 0.5)
                            alphaData |= 4; // alpha_4
                        else if (interpolate < 0.625)
                            alphaData |= 5; // alpha_5
                        else if (interpolate < 0.75)
                            alphaData |= 6; // alpha_6
                        else if (interpolate < 0.875)
                            alphaData |= 7; // alpha_7
                        else
                            alphaData |= 1; // alpha_1
                    }
                }
            }

            // 3 byte chunks, need to be output in this order
            writer.Write((byte)(alphaData >> 24));
            writer.Write((byte)(alphaData >> 32));
            writer.Write((byte)(alphaData >> 40));

            writer.Write((byte)(alphaData));
            writer.Write((byte)(alphaData >> 8));
            writer.Write((byte)(alphaData >> 16));

            writer.Write(texelData[color_0].color);
            writer.Write(texelData[color_1].color);

            // Pack color values if needed
            if (texelData[color_0].color != texelData[color_1].color)
            {
                double colorDist = texelData[color_0].Distance(texelData[color_1]);

                for (i = 0; i < texelData.Count; i += 4)
                {
                    byte colorData = 0;

                    for (int j = i + 3; j >= i; j--)
                    {
                        colorData <<= 2;

                        double interpolate = texelData[color_0].Distance(texelData[j]) / colorDist;

                        if (interpolate < 0.25)
                            colorData |= 0; // color_0
                        else if (interpolate < 0.5)
                            colorData |= 2; // color_2
                        else if (interpolate < 0.75)
                            colorData |= 3; // color_3
                        else
                            colorData |= 1; // color_1
                    }

                    writer.Write(colorData);
                }
            }
            else
                writer.Write((uint)0);
        }

        private static void WriteImageToStream(Stream outStream, Bitmap curImage)
        {
            BinaryWriter writer = new BinaryWriter(outStream);

            WriteDDSHeader(writer);

            // BC3 encoding - break image into 4x4 blocks
            for (int y = 0; y < curImage.Height; y += 4)
            {
                for (int x = 0; x < curImage.Width; x += 4)
                {
                    List<Texel> texelData = new List<Texel>(16);

                    for (int v = y; v < y + 4; v++)
                    {
                        for (int u = x; u < x + 4; u++)
                        {
                            texelData.Add(new Texel(curImage.GetPixel(u, v)));
                        }
                    }

                    EncodeTexelBlockBC3(writer, texelData);
                }
            }

            // Finalize and we're done!
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Processes a single profile image asynchronously.
        /// </summary>
        public static async void ProcessImage(string key, string url, string hash)
        {
            try
            {
                Stream result = await client.GetStreamAsync(url);
                Bitmap curImage = new Bitmap(result);

                // Force to square.
                if (curImage.Height > curImage.Width)
                {
                    Rectangle cropArea = new Rectangle(0, (curImage.Height - curImage.Width) / 2, curImage.Width, curImage.Width);
                    curImage = curImage.Clone(cropArea, PixelFormat.Format32bppRgb);
                }
                else if (curImage.Height < curImage.Width)
                {
                    Rectangle cropArea = new Rectangle((curImage.Width - curImage.Height) / 2, 0, curImage.Height, curImage.Height);
                    curImage = curImage.Clone(cropArea, PixelFormat.Format32bppRgb);
                }

                // Addon will display 200x200, size to the next highest power of 2.
                curImage = new Bitmap(curImage, new Size(256, 256));

                // Save output file
                FileStream outStream = new FileStream(String.Format("../images/thumbs/{0}/{1}.dds", key, hash), FileMode.Create);

                WriteImageToStream(outStream, curImage);
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

        private class Texel
        {
            public static Texel Black = new Texel(Color.Black);
            public static Texel White = new Texel(Color.White);

            public ushort color { get; set; }
            public byte alpha { get; set; }
            protected double[] vector { get; set; }
            public Texel(Color input)
            {
                byte tmpR = (byte)(input.R / 8); // 5 bits
                byte tmpG = (byte)(input.G / 4); // 6 bits
                byte tmpB = (byte)(input.B / 8); // 5 bits

                color = (ushort)((tmpR << 11) + (tmpG << 5) + tmpB);
                vector = new[] { input.R / 255.0, input.G / 255.0, input.B / 255.0 };
                alpha = input.A;
            }

            public double Distance(Texel other)
            {
                return Math.Sqrt(Math.Pow(vector[0] - other.vector[0], 2) + Math.Pow(vector[1] - other.vector[1], 2) + Math.Pow(vector[2] - other.vector[2], 2));
            }
        }
    }
}
