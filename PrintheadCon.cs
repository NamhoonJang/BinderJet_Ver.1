using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Xaar.Common.Utilities;
using Xaar.Core.Hardware;
using Xaar.Core.ImageProcessing;
using Xaar.Core.Model;
using Xaar.Framework.Utilities;
using Xaar.Framework.Configuration;
using Xaar.Framework.ImageProcessing;
using System.Windows.Forms;

namespace XAARWinform
{
    internal class PrintheadCon
    {
        static XpmInterconnectionFactory factory = new XpmInterconnectionFactory(Assembly.GetExecutingAssembly().GetName().Name);
        static XpmInterconnection xpmInterconnection = factory.Create(XpmInterconnectionMode.Xpm);
        PrintheadType phType;
        Printhead printhead;
        ProductDetect productDetect;
        TransportEncoder transportEncoder;
        Dictionary<string, IList<int>> phsPerXpmMap;
        //Bitmap image1;
        //BiDirectionalPrintSequence sequence;

        private AutoResetEvent _phPowered;
        private AutoResetEvent _xpmConnected;
        static private string _macAddress;
        static private int _phIndex = 0;
        static private bool _useExtPd = true;
        static private bool _useExtEnc = true;
        private List<string> _imageTags;

        bool FirstInit = false;

        public void Connect()
        {
            _phPowered = new AutoResetEvent(false); _xpmConnected = new AutoResetEvent(false);

            const int connectionTimeoutSeconds = 120;
            xpmInterconnection.XpmConnected += XpmConnected;
            xpmInterconnection.PrintheadPowerCompleted += PrintheadPowerCompleted;
            Console.WriteLine();
            Console.WriteLine(@"Initialising system... (Timeout {0} sec)", connectionTimeoutSeconds);
            xpmInterconnection.InitialiseSystem();
            if (FirstInit == false)
            {
                
                FirstInit = true;
            }
            if (!_xpmConnected.WaitOne(connectionTimeoutSeconds * 1000))
            {
                Console.WriteLine(@"XPM not detected. Press any key to exit..."); //Console.ReadKey();
                //return;
                MessageBox.Show("XPM not detected");
            }
            if (!_phPowered.WaitOne(35000))
            {
                Console.WriteLine(@"Printhead {0} has not been initialised. Press any key to exit...", _phIndex); //Console.ReadKey();
                //return;
                MessageBox.Show("Printhead has not been initialised");
            }
        }

        public void config()
        {
            phType = xpmInterconnection.GetAllPrintheadInformation().First(p => p.XspiPort.Equals(_phIndex)).PrintheadTag.PrintheadType;
            printhead = ExamplesUtilities.GetDefaultPrintead(phType, _phIndex);
            productDetect = ExamplesUtilities.GetProductDetect(!_useExtPd);
            transportEncoder = ExamplesUtilities.GetTransportEncoder(!_useExtEnc);
            phsPerXpmMap = new Dictionary<string, IList<int>> { { _macAddress, new[] { _phIndex } } };

            xpmInterconnection.SetGlobalPrintParameters(ExamplesUtilities.GetGlobalPrintParameters());

            const int tePdChainIndex = 0;
            var tePdChain = new TransportEncoderProductDetectChain(transportEncoder, productDetect, tePdChainIndex);
            var phChainMapping = new List<PrintheadTePdChainAssociation> {
                    new PrintheadTePdChainAssociation { PrinteadIndex = _phIndex, TransportEncoderProductDetectChain = tePdChainIndex }};
            if (!xpmInterconnection.SetupTransportEncoderProductDetectChains(_macAddress, phChainMapping, new[] { tePdChain })) return;

            xpmInterconnection.DisableIdleSpitting();
        }
        

        public void Disconnect()
        {
            if (xpmInterconnection != null)
            {
                xpmInterconnection.XpmConnected -= XpmConnected;
                xpmInterconnection.PrintheadPowerCompleted -= PrintheadPowerCompleted;
                xpmInterconnection.XpmDisconnected += XpmDisconnected;
                xpmInterconnection.Dispose();
                
            }
            
            //MessageBox.Show("Printhead disconnection");
        }

        public void Swathedown()
        {
            //var rows = printhead.PrintheadProperties.NumberOfRows;

            //var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), "360x360_dpi_mirrorON_Test_Wedge_8L_7DPD_8bit.bmp");

            //var uri = new System.Uri(imageFullName);
            //var image = new BitmapImage(uri);
            //var swatheMetadata = new SwatheMetadata { Tag = string.Format("Swathe#0"), PixelHeight = image.PixelHeight };
            //var swathe = new BitmapSwathe(image, phType) { Metadata = swatheMetadata };
            //if (!xpmInterconnection.DownloadSwathe(_macAddress, swathe)) return;
            string swathetag1, swathetag2;
            swathetag1 = Swathesdownload("abc_left.bmp", "Swathe#0");
            swathetag2 = Swathesdownload("abc_left_rot.bmp", "Swathe#1");
            //var uri_1 = new System.Uri("C:\\Program Files\\Xaar\\XPM Suite-3.0.7741.96\\Images\\8 bpp\\360x360_dpi_mirrorON_Test_Wedge_8L_7DPD_8bit - rotate180.bmp");
            //var image_1 = new BitmapImage(uri_1);
            //var swatheMetadata_1 = new SwatheMetadata { Tag = string.Format("Swathe#1"), PixelHeight = image.PixelHeight };
            //var swathe_1 = new BitmapSwathe(image_1, phType) { Metadata = swatheMetadata_1 };
            // var swathe = new BitmapSwathe(image, phType);
            _imageTags = new List<string>();
            _imageTags.Add(swathetag1);
            _imageTags.Add(swathetag2);

            //if (!xpmInterconnection.DownloadSwathe(_macAddress, swathe_1)) return;

            var sequence = xpmInterconnection.CreateBiDirectionalPrintSequence(_macAddress, "Sequence#1", printhead, BidirectionalMode.SingleProductDetect);
            sequence.AddPass(_imageTags[0]);
            sequence.AddPass(_imageTags[1], 0);
            sequence.Run();

            //Console.WriteLine(@"Downloading swathes");
        }

        public void Swathedown2()
        {
            //var rows = printhead.PrintheadProperties.NumberOfRows;

            //var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), "360x360_dpi_mirrorON_Test_Wedge_8L_7DPD_8bit.bmp");

            //var uri = new System.Uri(imageFullName);
            //var image = new BitmapImage(uri);
            //var swatheMetadata = new SwatheMetadata { Tag = string.Format("Swathe#0"), PixelHeight = image.PixelHeight };
            //var swathe = new BitmapSwathe(image, phType) { Metadata = swatheMetadata };
            //if (!xpmInterconnection.DownloadSwathe(_macAddress, swathe)) return;
            string swathetag1, swathetag2;
            swathetag1 = Swathesdownload("360x360_dpi_mirrorOFF_Test_Wedge_8L_7DPD_8bit.bmp", "Swathe#2");
            swathetag2 = Swathesdownload("360x360_dpi_mirrorOFF_Test_Wedge_8L_7DPD_8bit - rotate180.bmp", "Swathe#3");
            //var uri_1 = new System.Uri("C:\\Program Files\\Xaar\\XPM Suite-3.0.7741.96\\Images\\8 bpp\\360x360_dpi_mirrorON_Test_Wedge_8L_7DPD_8bit - rotate180.bmp");
            //var image_1 = new BitmapImage(uri_1);
            //var swatheMetadata_1 = new SwatheMetadata { Tag = string.Format("Swathe#1"), PixelHeight = image.PixelHeight };
            //var swathe_1 = new BitmapSwathe(image_1, phType) { Metadata = swatheMetadata_1 };
            // var swathe = new BitmapSwathe(image, phType);
            _imageTags = new List<string>();
            _imageTags.Add(swathetag1);
            _imageTags.Add(swathetag2);

            //if (!xpmInterconnection.DownloadSwathe(_macAddress, swathe_1)) return;

            var sequence = xpmInterconnection.CreateBiDirectionalPrintSequence(_macAddress, "Sequence#2", printhead, BidirectionalMode.SingleProductDetect);
            sequence.AddPass(_imageTags[0]);
            sequence.AddPass(_imageTags[1], 0);
            sequence.Run();

            //Console.WriteLine(@"Downloading swathes");
        }

        public void printstart()
        {
            //var sequence = xpmInterconnection.CreatePrintSequence(_macAddress, "Sequence#1", printhead);
            //sequence.Add(new PrintOperation(swathe.Metadata.Tag));
            

            Console.WriteLine(@"Entering print mode");
            if (!xpmInterconnection.EnterPrintMode(phsPerXpmMap)) return;
        }

        public void printstop()
        {
            Console.WriteLine(@"Exiting print mode");
            if (!xpmInterconnection.ExitPrintMode(phsPerXpmMap)) return;
        }

        private void XpmConnected(object sender, string macAddress)
        {
            Console.WriteLine(@"XPM {0} connected", macAddress);
            _macAddress = macAddress;
            _xpmConnected.Set();
        }
        private void XpmDisconnected(object sender, string macAddress)
        {
            Console.WriteLine(@"XPM {0} Disconnected", macAddress);
            _macAddress = macAddress;
            _xpmConnected.Set();
        }

        private void PrintheadPowerCompleted(object sender, XpmInterconnection.PrintheadPowerEventArgs phPowerEventArgs)
        {
            if (phPowerEventArgs.Outcome && _phIndex.Equals(phPowerEventArgs.PrintheadIndex))
            {
                Console.WriteLine(@"Printhead {0} on XPM {1} powered ok", phPowerEventArgs.PrintheadIndex, phPowerEventArgs.XpmMacAddress);
                //MessageBox.Show("Printhead connection");
                _phPowered.Set();
            }
        }

        public void swathesquencedel()
        {
            xpmInterconnection.DeleteAllPrintSequences();
            xpmInterconnection.DeleteAllSwathes();
        }

        public void squencedel()
        {
            xpmInterconnection.DeleteAllPrintSequences();
            //xpmInterconnection.DeleteAllSwathes();
        }


        public string Swathesdownload(string filename, string swathename)
        {
            var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), filename);
            var image = ImageUtilities.LoadImageFromFile(imageFullName);

            // var uri = new System.Uri(imageFullName);
            // var image = new BitmapImage(uri);
            var swatheMetadata = new SwatheMetadata { Tag = string.Format(swathename), PixelHeight = image.PixelHeight };
            var swathe = new BitmapSwathe(image, phType) { Metadata = swatheMetadata, GuardPaletteValue = 0x0F };
            xpmInterconnection.DownloadSwathe(_macAddress, swathe);

            //creatprintsquence();

            return swathe.Metadata.Tag;
        }
        public void ImageSwatheDown(string filename, string swathename)
        {
            var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), filename);
            var originalbitmap = ImageUtilities.LoadImageFromFile(imageFullName);

            var swatheMetadata = new SwatheMetadata { Tag = string.Format(swathename), PixelHeight = originalbitmap.PixelHeight };
            var swathe = new BitmapSwathe(originalbitmap, phType) { Metadata = swatheMetadata, GuardPaletteValue = 0x0F };
            //swathe.GuardPaletteValue = 15;
            xpmInterconnection.DownloadSwathe(_macAddress, swathe);
        }

        // 이미지 처리 함수
        Image swatheimage0, swatheimage1;
        public int dirrandnum, imagewidth;
        public void imageswathedown(int num)
        {
            string swathenum, swathenum2, swathenum3, swathenum4;

            if (imagewidth <= 957)
            {
                swathenum = "swathe#" + num.ToString();
                num += 1;
                swathenum2 = "swathe#" + num.ToString();

                //swathenum = "swathe" + num.ToString();
                ImageSwatheDown("randimg.bmp", swathenum);
                ImageSwatheDown("randimg-rotate.bmp", swathenum2);
                //File.Delete("images\\randimg.bmp");
            }
            else
            {
                swathenum = "swathe#" + num.ToString();
                num += 1;
                swathenum2 = "swathe#" + num.ToString();
                num += 1;
                swathenum3 = "swathe#" + num.ToString();
                num += 1;
                swathenum4 = "swathe#" + num.ToString();

                ImageSwatheDown("rnadleftimg.bmp", swathenum3);
                ImageSwatheDown("rnadleftimg-rotate.bmp", swathenum4);
                ImageSwatheDown("rnadrightimg.bmp", swathenum);
                ImageSwatheDown("rnadrightimg-rotate.bmp", swathenum2);
                //File.Delete("images\\rnadleftimg.bmp");
                //File.Delete("images\\rnadrightimg.bmp");
            }
            //creatprintsquence();
        }

        public void creatprintsquence(int num)
        {
            string sequencenum = "Sequence#" + num.ToString();
            string swathenum = "swathe#" + num.ToString();
            var sequence = xpmInterconnection.CreatePrintSequence(_macAddress, sequencenum, printhead);
            sequence.Add(new PrintOperation(swathenum));
            sequence.Run();
        }

        public void creatprintbisquence(int num)
        {
            string sequencenum = "Sequence#" + num.ToString();
            string swathenum = "swathe#" + num.ToString();
            num += 3; // 220617 라이트 -> 레프트 로테이트 이미지 순서로
            string swathenum2 = "swathe#" + num.ToString();
            var sequence = xpmInterconnection.CreateBiDirectionalPrintSequence(_macAddress, sequencenum, printhead, BidirectionalMode.SingleProductDetect);
            //var sequence = xpmInterconnection.CreatePrintSequence(_macAddress, sequencenum, printhead);
            sequence.AddPass(swathenum);
            sequence.AddPass(swathenum2, 0);
            sequence.Run();
        }

        public void ImageConProc(string filename)
        {
            var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), filename);
            Bitmap OriginalImage = new Bitmap(imageFullName);

            Image randimage, randimage2, randleftimage, randrightimage, randleftimage2, randrightimage2;
            Image leftcutimage, rightcutimage, leftcutimagerotate, rightcutimagerotate;

            Image resizecenterimage = ImageCenter(OriginalImage);
            //Image randimage;

            Random rand = new Random();
            int randnum = rand.Next();
            randnum = randnum % 2;

            // 220511 Randmization Off
            //randnum = 1;

            dirrandnum = randnum;
            imagewidth = resizecenterimage.Width;
            if (resizecenterimage.Width <= 957)
            {
                randimage = ImageRandAdd(resizecenterimage, randnum);
                randimage.Save("images\\randimg.bmp");
                randimage2 = ImageRandAddrotate(resizecenterimage, randnum);
                randimage2.Save("images\\randimg-rotate.bmp");
                //swatheimage0 = randimage;
            }
            else
            {
                leftcutimage = Imageleftcut(resizecenterimage);
                randleftimage = ImageRandAdd(leftcutimage, randnum);
                randleftimage.Save("images\\rnadleftimg.bmp");
                leftcutimagerotate = ImageRandAddrotate(leftcutimage, randnum);
                leftcutimagerotate.Save("images\\rnadleftimg-rotate.bmp");



                rightcutimage = Imagerightcut(resizecenterimage);
                randrightimage = ImageRandAdd(rightcutimage, randnum);
                randrightimage.Save("images\\rnadrightimg.bmp");
                leftcutimagerotate = ImageRandAddrotate(rightcutimage, randnum);
                leftcutimagerotate.Save("images\\rnadrightimg-rotate.bmp");
                //swatheimage0 = randleftimage;
                //swatheimage1 = randrightimage;
            }
        }

        public Image ImageCenter(Image image)
        {
            //var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), filename);
            Bitmap OriginalImage = new Bitmap(image);
            Image ResizeImage;
            if (OriginalImage.Width <= 957)
            {
                // 957 픽셀 이하
                // 백색 가로 957픽셀에 원본이미지 붙여넣기(가운데 정렬)
                ResizeImage = Resize_center(OriginalImage, 957, OriginalImage.Height);
            }
            else
            {
                // 958 픽셀 이상
                ResizeImage = Resize_center(OriginalImage, 1950, OriginalImage.Height);
            }
            return ResizeImage;
        }

        /*
        public void imagecon_left()
        {
            var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), "abc3.bmp");
            Bitmap image1 = new Bitmap(imageFullName);
            //image1.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Bitmap image2 = image1;
            //Bitmap image3 = image2.Clone(new Rectangle(150, 0, image2.Width - 150, image2.Height), System.Drawing.Imaging.PixelFormat.DontCare);
            Resize_left(image2, 1000, image2.Height);
            //System.Drawing.Size size = new System.Drawing.Size(image1.Width, image1.Height);
            //Bitmap image4 = new Bitmap(image3, size);
            //image3.image4("abc2.bmp");
        }

        public void imagecon_right()
        {
            var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), "abc3.bmp");
            Bitmap image1 = new Bitmap(imageFullName);
            //image1.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Bitmap image2 = image1;
            //Bitmap image3 = image2.Clone(new Rectangle(150, 0, image2.Width - 150, image2.Height), System.Drawing.Imaging.PixelFormat.DontCare);
            Resize_right(image2, 1000, image2.Height);
            //System.Drawing.Size size = new System.Drawing.Size(image1.Width, image1.Height);
            //Bitmap image4 = new Bitmap(image3, size);
            //image3.image4("abc2.bmp");
        }

        public void imagecut()
        {
            var imageFullName = Path.Combine(ExamplesUtilities.GetImagePath(), "abc.bmp");
            Bitmap image1 = new Bitmap(imageFullName);
            //image1.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Bitmap image2 = image1;
            Bitmap image3 = image2.Clone(new Rectangle(150, 0, image2.Width - 150, image2.Height), System.Drawing.Imaging.PixelFormat.DontCare);
            //System.Drawing.Size size = new System.Drawing.Size(image1.Width, image1.Height);
            //Bitmap image4 = new Bitmap(image3, size);
            image3.Save("abc1.bmp");
        }
        */

        // 이미지 가운데 정렬 작업 함수
        public static Bitmap Resize_center(Image originalImage, int targetX, int targetY)
        {
            //Image originalImage = Image.FromFile(importPath);

            double ratioX = targetX / (double)originalImage.Width;
            double ratioY = targetY / (double)originalImage.Height;

            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(originalImage.Width * ratio);
            int newHeight = (int)(originalImage.Height * ratio);

            Bitmap newImage = new Bitmap(targetX, targetY);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.FillRectangle(Brushes.Black, 0, 0, newImage.Width, newImage.Height);
                g.DrawImage(originalImage, (targetX - newWidth) / 2, (targetY - newHeight) / 2, newWidth, newHeight);
            }

            //Bitmap newImage2 = newImage.Clone(new Rectangle(0, 0, newImage.Width, newImage.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            newImage = Con32to24bpp(newImage);
            newImage = Convert24bppTo8bpp(newImage);
            
            //newImage.Save("abc3.bmp");

            //originalImage.Dispose();
            //newImage.Dispose();

            return newImage;
        }

        // 이미지를 왼쪽으로 Shift 함수
        public static Image Resize_left(Image originalImage, int targetX, int targetY)
        {
            //Image originalImage = Image.FromFile(importPath);

            double ratioX = targetX / (double)originalImage.Width;
            double ratioY = targetY / (double)originalImage.Height;

            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(originalImage.Width * ratio);
            int newHeight = (int)(originalImage.Height * ratio);

            Bitmap newImage = new Bitmap(targetX, targetY);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.FillRectangle(Brushes.Black, 0, 0, newImage.Width, newImage.Height);
                g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            //Bitmap newImage2 = newImage.Clone(new Rectangle(0, 0, newImage.Width, newImage.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            newImage = Con32to24bpp(newImage);
            newImage = Convert24bppTo8bpp(newImage);
            //newImage.Save("abc_left.bmp");

            //originalImage.Dispose();
            //newImage.Dispose();

            return newImage;
        }

        // 이미지를 오른쪽으로 Shift 함수
        public static Image Resize_right(Image originalImage, int targetX, int targetY)
        {
            //Image originalImage = Image.FromFile(importPath);

            double ratioX = targetX / (double)originalImage.Width;
            double ratioY = targetY / (double)originalImage.Height;

            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(originalImage.Width * ratio);
            int newHeight = (int)(originalImage.Height * ratio);

            Bitmap newImage = new Bitmap(targetX, targetY);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.FillRectangle(Brushes.Black, 0, 0, newImage.Width, newImage.Height);
                g.DrawImage(originalImage, targetX - newWidth, 0, newWidth, newHeight);
            }

            //Bitmap newImage2 = newImage.Clone(new Rectangle(0, 0, newImage.Width, newImage.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            newImage = Con32to24bpp(newImage);
            newImage = Convert24bppTo8bpp(newImage);
            //newImage.Save("abc_right.bmp");

            return newImage;

            //originalImage.Dispose();
            //newImage.Dispose();
        }

        public Image ImageRandAdd(Image originalimage, int randnum)
        {
            Image addimage;

            if (randnum == 1)
            {
                addimage = Resize_left(originalimage, 1000, originalimage.Height);
            }
            else
            {
                addimage = Resize_right(originalimage, 1000, originalimage.Height);
            }
            return addimage;
        }

        public Image ImageRandAddrotate(Image originalimage, int randnum)
        {
            Image addimage;
            originalimage.RotateFlip(RotateFlipType.Rotate180FlipNone);

            if (randnum == 1)
            {
                addimage = Resize_left(originalimage, 1000, originalimage.Height);
            }
            else
            {
                addimage = Resize_right(originalimage, 1000, originalimage.Height);
            }
            return addimage;
        }

        public Image Imageleftcut(Image origianal)
        {
            Bitmap originalimage = new Bitmap(origianal);
            Bitmap cutimage = originalimage.Clone(new Rectangle(0, 0, 978, originalimage.Height), System.Drawing.Imaging.PixelFormat.DontCare);
            System.Drawing.Size size = new System.Drawing.Size(cutimage.Width, cutimage.Height);
            Bitmap cutimage1 = new Bitmap(cutimage, size);
            //cutimage.Save("cutl.bmp");
            return cutimage1;
        }

        public Image Imagerightcut(Image origianal)
        {
            Bitmap originalimage = new Bitmap(origianal);
            Bitmap cutimage = originalimage.Clone(new Rectangle(972, 0, 1950-972, originalimage.Height), System.Drawing.Imaging.PixelFormat.DontCare);
            System.Drawing.Size size = new System.Drawing.Size(cutimage.Width, cutimage.Height);
            Bitmap cutimage1 = new Bitmap(cutimage, size);
            //cutimage.Save("cutr.bmp");
            return cutimage1;
        }

        public static Bitmap Con32to24bpp(Bitmap in32bppimage)
        {
            var bmp = new Bitmap(in32bppimage.Width, in32bppimage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp)) gr.DrawImage(in32bppimage, new Rectangle(0, 0, in32bppimage.Width, in32bppimage.Height));
            return bmp;
        }

        public static Bitmap Convert24bppTo8bpp(Bitmap in24bppImage)
        {
            if (in24bppImage != null)
            {
                //   
                Rectangle rect = new Rectangle(0, 0, in24bppImage.Width, in24bppImage.Height);
                BitmapData bmpData = in24bppImage.LockBits(rect, ImageLockMode.ReadOnly, in24bppImage.PixelFormat);

                int width = bmpData.Width;
                int height = bmpData.Height;
                int stride = bmpData.Stride;
                int offset = stride - width * 3;
                IntPtr ptr = bmpData.Scan0;
                int scanBytes = stride * height;

                int posScan = 0, posDst = 0;
                byte[] rgbValues = new byte[scanBytes];
                Marshal.Copy(ptr, rgbValues, 0, scanBytes);

                byte[] grayValues = new byte[width * height];

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        double temp = rgbValues[posScan++] * 0.11 +
                            rgbValues[posScan++] * 0.59 +
                            rgbValues[posScan++] * 0.3;
                        grayValues[posDst++] = (byte)temp;
                    }

                    // length = stride - width * bytePerPixel  
                    posScan += offset;
                }

                Marshal.Copy(rgbValues, 0, ptr, scanBytes);
                in24bppImage.UnlockBits(bmpData);

                // 8  
                Bitmap retBitmap = BuiltGrayBitmap(grayValues, width, height);
                return retBitmap;
            }
            else
            {
                return null;
            }
        }
        private static Bitmap BuiltGrayBitmap(byte[] rawValues, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                 ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            int offset = bmpData.Stride - bmpData.Width;
            IntPtr ptr = bmpData.Scan0;
            int scanBytes = bmpData.Stride * bmpData.Height;
            byte[] grayValues = new byte[scanBytes];


            int posSrc = 0, posScan = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    grayValues[posScan++] = rawValues[posSrc++];
                }

                posScan += offset;
            }


            Marshal.Copy(grayValues, 0, ptr, scanBytes);
            bitmap.UnlockBits(bmpData);  //   


            ColorPalette palette;

            // Format8bppIndexedPalette  
            using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                palette = bmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;

            return bitmap;
        }
        
        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        
        public void invertcolor()
        {
            var PaletteRemap = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
            xpmInterconnection.SetPaletteRemapTableFull(_macAddress, 0, PaletteRemap);
        }

        public void SetPaletteTable(byte[] binderValue)
        {
            //var PaletteRemap = new byte[] { 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x00, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
            //var PaletteRemap = new byte[] { 0x04, 0x03, 0x03, 0x02, 0x02, 0x02, 0x00, 0x00, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
            //var PaletteRemap = new byte[] { binderValue, binderValue, binderValue, binderValue, binderValue, binderValue, binderValue, 0x00, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
            var PaletteRemap = new byte[16];
            Array.Copy(binderValue, 0, PaletteRemap, 0, 8);
            Array.Copy(new byte[] {0,0,0,0,0,0,0,0}, 0, PaletteRemap, 8, 8);
            xpmInterconnection.SetPaletteRemapTableFull(_macAddress, 0, PaletteRemap);
            Console.WriteLine(String.Join(" ", PaletteRemap));
        }

        public void WriteWaveForm_d844()
        {
            xpmInterconnection.WriteWaveform("00-1e-c0-ad-28-0c", 0, 0, "C:\\Users\\Binder_Jet\\Desktop\\waveform\\D844-1001.6-8L-720.txt");
        }

        public void WriteWaveForm_h884()
        {
            xpmInterconnection.WriteWaveform("00-1e-c0-ad-28-0c", 0, 0, "C:\\Users\\Binder_Jet\\Desktop\\waveform\\H884-100312-7LCB-CONFIDENTIAL.txt");
        }
    }
}
