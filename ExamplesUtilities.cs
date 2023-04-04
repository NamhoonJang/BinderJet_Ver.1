using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xaar.Common.Interfaces;
using Xaar.Common.Logging;
using Xaar.Common.Utilities;
using Xaar.Core.Hardware;
using Xaar.Framework.Hardware;
using Xaar.Core.ImageProcessing;
using Xaar.Core.Interfaces;
using Xaar.Core.Model;
using Xaar.Framework.Configuration;
using Xaar.Framework.Utilities;
using Timer = System.Timers.Timer;

namespace XAARWinform
{
    /// <summary>
    /// This class contains examples of how to use some of the features of the XPM using the libraries developed by Xaar.
    /// </summary>
    public static class ExamplesUtilities
    {
        private const int InitialisationTimeout = 120000;
        private const int PhInitialisationTimeout = 50000;
        private static object _phPowerLock = new object();

        /// <summary>
        /// How to initialise the system
        /// </summary>
        /// <param name="initialiser">The service to initialise the system</param>
        /// <param name="xpmInterconnection">The <see cref="XpmInterconnection"/> to interface to the XPM</param>
        /// <returns></returns>
        public static int? InitialiseSystem(HardwareInitialiser initialiser, XpmInterconnection xpmInterconnection)
        {
            var numberOfBoxes = InitialiseSystemWithUserMessage(initialiser);

            // Try one more time, but give it sufficient time for booting
            if (numberOfBoxes == null || numberOfBoxes == 0)
            {
                Console.WriteLine(@"System might be booting, give it some time to settle");

                var autoresetEvent = new AutoResetEvent(false);
                const int maxBootTimeInSeconds = 30;

                var timer = new System.Timers.Timer(1000);
                var countDownTimer = maxBootTimeInSeconds;
                timer.Elapsed += delegate
                {
                    --countDownTimer;
                    Console.Write(@"\r{0} seconds", countDownTimer.ToString("D2"));
                    
                    if (countDownTimer == 0)
                    {
                        timer.Stop();
                        autoresetEvent.Set();
                    }
                };
                timer.Start();

                autoresetEvent.WaitOne();
                
                numberOfBoxes = InitialiseSystemWithUserMessage(initialiser);
            }

            return numberOfBoxes;
        }
        
        /// <summary>
        /// Turns on a specific printhead on a specific box with additional debug messages
        /// </summary>
        /// <param name="macAddress">The MAC address of the XPM</param>
        /// <param name="port">The port where the printhead is connected (0-based)</param>
        /// <param name="xpmInterconnection">The <see cref="XpmInterconnection"/> to interface to the XPM</param>
        /// <returns>True if the printhead was succesfully powered, otherwise false</returns>
        public static bool TurnOnPrinthead(string macAddress, int port, XpmInterconnection xpmInterconnection)
        {
            Console.WriteLine();
            Console.WriteLine(@"\nTurning printhead {0} on.", port);
            Console.WriteLine();
            
            return xpmInterconnection.TurnOnPrinthead(macAddress, port);
        }
        
        /// <summary>
        /// Get certain global parameters
        /// </summary>
        /// <returns>The common print parameters to use</returns>
        public static GlobalPrintParameters GetGlobalPrintParameters()
        {
            var globalPrintParameters = new GlobalPrintParameters
            {
                GreyScalePalette = GreyScalePalette.GetInvertedPalette()
            };
            
            return globalPrintParameters;
        }

        ///// <summary>
        ///// Set up a timer for generating a software PD on a regular basis, generating the first PD
        ///// immediately. The event is set when all PDs have been generated.
        ///// </summary>
        ///// <returns>The <see cref="System.Timers.Timer"/> object to use</returns>

        /// <summary>
        /// Set up a timer for generating a software PD on a regular basis
        /// </summary>
        /// <param name="printingService">The service used for printing</param>
        /// <param name="macAddress">The MAC address of the XPM</param>
        /// <param name="port">The port where the printhead is connected (0-based)</param>
        /// <param name="swPdIntervalMilliseconds">The time (ms) between two software PDs</param>
        /// <param name="swPdToGenerate">The number of sowftare PDs to generate</param>
        /// <param name="evt">The event set when all required software PDs have been generated</param>
        /// <returns>The <see cref="System.Timers.Timer"/> to control (start/stop) the generation of software PDs</returns>
        public static Timer SetupSoftwarePdTimer(IPrintingService printingService,
            string macAddress, int port,
            int swPdIntervalMilliseconds, int swPdToGenerate,
            AutoResetEvent evt)
        {
            var swPdTimer = new Timer { Interval = 10, AutoReset = false };
            var swPdCount = 0;

            swPdTimer.Elapsed += delegate
            {
                swPdTimer.Interval = swPdIntervalMilliseconds;

                if (swPdCount++ < swPdToGenerate)
                {
                    Console.WriteLine(@"Firing PD {0}/{1}", swPdCount, swPdToGenerate);
                    printingService.GenerateProductDetect(macAddress, port);
                    swPdTimer.Start();
                }
                else
                {
                    evt.Set();
                }
            };

            return swPdTimer;
        }

        /// <summary>
        /// Start the swathe count monitoring task, which displays the swathe count when values change
        /// </summary>
        public static void StartSwatheCountMonitoring(XpmInterconnection xpmInterconnection, string firstXpmMacAddress, 
            bool printInitialValues, CancellationToken token, out Task task, Logger logger = null, int loopTime = 1000)
        {
            loopTime = Math.Min(50, loopTime);

            var lastSwatheCounts = new uint[,] { };

            task = Task.Factory.StartNew(() =>
            {
                var isFirstTime = true;
                while (!token.IsCancellationRequested)
                {
                    var currentSwatheCounts = xpmInterconnection.GetSwatheCounts(firstXpmMacAddress);
                    if (isFirstTime)
                    {
                        lastSwatheCounts = currentSwatheCounts;
                    }

                    // TODO: modify when correct data is returned (the supported number of rows by the FPGA)
                    var sb = new StringBuilder();
                    var maxPorts = currentSwatheCounts.GetLength(0);
                    int maxRows = currentSwatheCounts.GetLength(1);
                    for (var port = 0; port < maxPorts; port++)
                    {
                        for (var row = 0; row < maxRows; row++)
                        {
                            if (currentSwatheCounts[port, row] != lastSwatheCounts[port, row] || (printInitialValues && isFirstTime))
                            {
                                var trace = string.Format("PH[{0:D2}], Row[{1:D2}] = {2:D3}", port, row, currentSwatheCounts[port, row]);
                                trace += Environment.NewLine;
                                sb.Append(trace);
                            }
                        }
                    }

                    if (sb.Length > 0)
                    {
                        var msg1 = @"*** Completed swathe counts ***";
                        Console.WriteLine(msg1);
                        var msg2 = sb.ToString();
                        Console.WriteLine(msg2);
                        if (logger != null)
                        {
                            logger.LogInformation(msg1 + Environment.NewLine + msg2);
                        }
                    }
                    lastSwatheCounts = currentSwatheCounts;
                    isFirstTime = false;
                    Thread.Sleep(loopTime); // Was 50 but this is a 'basic' example!
                }

                Console.WriteLine();
                Console.WriteLine(@"Exiting swathe monitoring");
            }, token);
        }

        public static ProductDetect GetProductDetect(bool useSoftwareProductDetect = false)
        {
            ProductDetect productDetect;
            if (useSoftwareProductDetect)
            {
                productDetect = new ProductDetect(
                    datumOffset: 0.0,
                    triggerSource: ProductDetectTriggerSource.Software,
                    externalTriggerIgnoreCount: 0,
                    ignoreReverseProductDetect: false,
                    triggersToGenerate: 5,
                    triggerInterval: 1000,
                    absoluteTriggerType: AbsoluteProductDetectTrigger.Forward,
                    forwardTriggerPosition: 0.0,
                    reverseTriggerPosition: 0.0,
                    invertInputs: false,
                    disableInputs: false,
                    externalTriggerType: ExternalProductDetectTrigger.UpDownCount);
            }
            else
            {
                productDetect = new ProductDetect(
                    datumOffset: 0.0,
                    triggerSource: ProductDetectTriggerSource.External,
                    externalTriggerIgnoreCount: 0,
                    ignoreReverseProductDetect: false,
                    triggersToGenerate: 0,
                    triggerInterval: 0,
                    absoluteTriggerType: AbsoluteProductDetectTrigger.Forward,
                    forwardTriggerPosition: 0.0,
                    reverseTriggerPosition: 0.0,
                    invertInputs: false,
                    disableInputs: false,
                    externalTriggerType: ExternalProductDetectTrigger.UpDownCount);
                    productDetect.InputStablePeriod = 8; // 1000 = 1000 x 10ns = 10,000 ns = 10μs
            }
            return productDetect;
        }

        public static TransportEncoder GetTransportEncoder(bool useInternalTransportEncoder, int dpi = 360)
        {
            TransportEncoder transportEncoder;
            if (useInternalTransportEncoder)
            {
                transportEncoder = new TransportEncoder(
                    type: EncoderType.Internal,
                    internalFrequency: 144000.0,
                    datum: 0.0,
                    resolution: 2.0,
                    requiredDotsPerInch: dpi,
                    maximumTransportSpeed: 400.0,
                    encoderDivide: 76,
                    encoderPulseMultiply: 256,
                    fractionalMultiplier: 0.990907972446,
                    subPixels: 20,
                    invertInputs: false,
                    followSequence: true,
                    edgeControl: EncoderEdgeControl.RisingAndFallingE0E1,
                    useManualEncoding: true,
                    loadDatumOnExternalProductDetect: false,
                    generateProductDetect: false);
            }
            else
            {
                transportEncoder = new TransportEncoder(
                     type: EncoderType.External,
                    internalFrequency: 0.0,
                    datum: 0.0,
                    resolution: 4.01,
                    requiredDotsPerInch: dpi,
                    maximumTransportSpeed: 360.0,
                    encoderDivide: 60,
                    encoderPulseMultiply: 256,
                    fractionalMultiplier: 0.999046505905512,
                    subPixels: 100,
                    invertInputs: false,
                    followSequence: true,
                    edgeControl: EncoderEdgeControl.RisingAndFallingE0E1,
                    useManualEncoding: false,
                    loadDatumOnExternalProductDetect: false,
                    generateProductDetect: false);
                /*
                type: EncoderType.External,
                internalFrequency: 0.0,
                datum: 0.0,
                resolution: 2.0,
                requiredDotsPerInch: dpi,
                maximumTransportSpeed: 600.0,
                encoderDivide: 76,
                encoderPulseMultiply: 256,
                fractionalMultiplier: 0.990907972446,
                subPixels: 100,
                invertInputs: false,
                followSequence: true,
                edgeControl: EncoderEdgeControl.RisingAndFallingE0E1,
                useManualEncoding: false,
                loadDatumOnExternalProductDetect: false,
                generateProductDetect: false);
                */
            }
            return transportEncoder;
        }

        /// <summary>
        /// Support function for system initialisation with user message
        /// </summary>
        /// <param name="initialiser"></param>
        /// <returns></returns>
        public static int? InitialiseSystemWithUserMessage(HardwareInitialiser initialiser)
        {
            Console.Write(@"\nInitialising system...");

            initialiser.Initialise();

            Console.WriteLine(@"{0}", initialiser.Initialised ? "Ok" : "Not Ok");

            var numberOfBoxes = initialiser.XpmBoxCount;
            Console.WriteLine(@"Found {0} XPM boxes", numberOfBoxes ?? 0);

            return numberOfBoxes;
        }

        /// <summary>
        /// Helper function to connect and wait for a printhead to be ready
        /// </summary>
        public static bool ConnectAndWaitForPrintheadReady(XpmInterconnection xpmInterconnection, int xpmPort)
        {
            return _ConnectAndWaitForPrintheadsReady(xpmInterconnection, new[] {xpmPort});
        }

        /// <summary>
        /// Helper function to connect and wait for a collection of printheads to be ready
        /// </summary>
        public static bool ConnectAndWaitForPrintheadReady(XpmInterconnection xpmInterconnection, IEnumerable<int> xpmPorts)
        {
            return _ConnectAndWaitForPrintheadsReady(xpmInterconnection, xpmPorts);
        }

        /// <summary>
        /// Helper function to connect and wait for a collection of printheads to be ready for specific mac addresses
        /// </summary>
        public static bool ConnectAndWaitForPrintheadReady(XpmInterconnection xpmInterconnection, IEnumerable<Tuple<string, int>> xpmPorts)
        {
            return _ConnectAndWaitForPrintheadsReady(xpmInterconnection, xpmPorts);
        }

        /// <summary>
        /// Setup the encoder and product detect
        /// </summary>
        public static void SetupEncoderAndProductDetect(XpmInterconnection xpmInterconnection, string macAddress, 
            int xpmPort, bool useInternalProductDetect = false, bool useInternalEncoder = false, 
            int tePdChainIndex = 0)
        {
            _SetupEncoderAndProductDetect(xpmInterconnection, macAddress, new[] {xpmPort}, useInternalProductDetect,
                useInternalEncoder, tePdChainIndex);
        }

        /// <summary>
        /// Setup the encoder and product detect for a collection of ports
        /// </summary>
        public static void SetupEncoderAndProductDetect(XpmInterconnection xpmInterconnection, string macAddress,
                IEnumerable<int> xpmPorts, bool useInternalProductDetect = false, bool useInternalEncoder = false, 
                int tePdChainIndex = 0, int dpi = 360)
        {
            _SetupEncoderAndProductDetect(xpmInterconnection, macAddress, xpmPorts, useInternalProductDetect,
                useInternalEncoder, tePdChainIndex, dpi);
        }

        /// <summary>
        /// Get the path to the images to print
        /// </summary>
        /// <returns>The path to the images</returns>
        public static string GetImagePath()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().CodeBase;
            var assemblyPath = assemblyName.Replace("file:///", string.Empty);
            var assemblyRoot = FileUtilities.GetDirectoryPathFromFileOrDirectoryPath(assemblyPath);

            var imagesPath = Path.Combine(assemblyRoot, "Images");

            return imagesPath;
        }

        /// <summary>
        /// Get the path to the test waveforms
        /// </summary>
        /// <returns>The path to the test waveforms</returns>
        public static string GetWaveformPath()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().CodeBase;
            var assemblyPath = assemblyName.Replace("file:///", string.Empty);
            var assemblyRoot = FileUtilities.GetDirectoryPathFromFileOrDirectoryPath(assemblyPath);

            var imagesPath = Path.Combine(assemblyRoot, "Waveforms");

            return imagesPath;
        }

        /// <summary>
        /// Download two 1000 pixel wide swathes. One of which is the high laydown encoded version of the other
        /// </summary>
        /// <param name="xpmInterconnection">The connection to the system</param>
        /// <param name="macAddress">The MAC address of the XPM to which to send images</param>
        /// <param name="swathesRequired">The number of swathes required to be downloaded</param>
        /// <returns>A list of swathe tags identifying the swathes downloaded</returns>
        /// <param name="swatheStartIndex">The swathe index to use for tagging purposes, allowing subsequent calls to use unique tags</param>
        /// <returns>A list of swathe tags identifying the swathes downloaded</returns>
        public static List<string> DownloadHighLaydown1000PixelWideSwathes(XpmInterconnection xpmInterconnection, string macAddress, int swathesRequired, int swatheStartIndex = 0)
        {
            var tagList = new List<string>();
            var path = GetImagePath();
            var swatheDownloaded = swatheStartIndex;
            try
            {
                var images = Directory.GetFiles(path, "Nozzle Chec" +
                    "k Pattern" + "*.bmp");
                foreach (var image in images)
                {
                    Console.WriteLine(@"Downloading " + image);

                    var bitmap = ImageUtilities.LoadImageFromFile(image);

                    var swathe = new BitmapSwathe(ImageUtilities.LoadImageFromFile(image), PrintheadType.Xaar1003)
                    {
                        Metadata = new SwatheMetadata
                        {
                            Tag = string.Format("Swathe#{0}", ++swatheDownloaded),
                            PixelHeight = bitmap.PixelHeight
                        }
                    };

                    tagList.Add(swathe.Metadata.Tag);

                    xpmInterconnection.DownloadSwathe(macAddress, swathe);

                    if (swatheDownloaded >= swathesRequired)
                        break;
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine(@"Directory not found: " + path);
            }

            return tagList;
        }

        /// <summary>
        /// Download all 1000 pixel wide swathes (for all rows) from the image directory, up to the given number
        /// </summary>
        /// <param name="xpmInterconnection">The connection to the system</param>
        /// <param name="macAddress">The MAC address of the XPM to which to send images</param>
        /// <param name="swathesRequired">The number of swathes required to be downloaded</param>
        /// <returns>A list of swathe tags identifying the swathes downloaded</returns>
        /// <param name="swatheStartIndex">The swathe index to use for tagging purposes, allowing subsequent calls to use unique tags</param>
        /// <returns>A list of swathe tags identifying the swathes downloaded</returns>
        public static List<string> Download1000PixelWideSwathes(XpmInterconnection xpmInterconnection, string macAddress, int swathesRequired, int swatheStartIndex = 0)
        {
            var tagList = new List<string>();
            var path = GetImagePath();
            var swatheDownloaded = swatheStartIndex;
            try
            {
                var images = Directory.GetFiles(path, "1000" + "*.bmp");
                foreach (var image in images)
                {
                    Console.WriteLine(@"Downloading " + image);

                    var bitmap = ImageUtilities.LoadImageFromFile(image);

                    var swathe = new BitmapSwathe(ImageUtilities.LoadImageFromFile(image), PrintheadType.Xaar1003)
                    {
                        Metadata = new SwatheMetadata
                        {
                            Tag = string.Format("Swathe#{0}", ++swatheDownloaded),
                            PixelHeight = bitmap.PixelHeight
                        }
                    };

                    tagList.Add(swathe.Metadata.Tag);

                    xpmInterconnection.DownloadSwathe(macAddress, swathe);

                    if (swatheDownloaded >= swathesRequired)
                        break;
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine(@"Directory not found: " + path);
            }

            return tagList;
        }

        /// <summary>
        /// Download all 2000 pixel wide swathes (for all rows) from the image directory, up to the given number
        /// </summary>
        /// <param name="xpmInterconnection">The connection to the system</param>
        /// <param name="macAddress">The MAC address of the XPM to which to send images</param>
        /// <param name="numberOfSwathesRequired">The number of swathes required to be downloaded</param>
        /// <param name="swatheStartIndex">The swathe index to use for tagging purposes, allowing subsequent calls to use unique tags</param>
        /// <returns>A list of swathe tags identifying the swathes downloaded</returns>
        public static List<string> Download2000PixelWideSwathes(XpmInterconnection xpmInterconnection, string macAddress, int numberOfSwathesRequired, int swatheStartIndex = 0)
        {
            var tagList = new List<string>();
            var imagePath = GetImagePath();
            var swatheDownloaded = swatheStartIndex;

            try
            {
                // For all rows the images must be 2000 pixels wide so we look for images which start "2000 ..."
                var availableImages = Directory.GetFiles(imagePath, "2000" + "*.bmp");

                foreach (var image in availableImages)
                {
                    Console.WriteLine(@"Downloading " + image);

                    var bitmap = ImageUtilities.LoadImageFromFile(image);

                    var swathe = new BitmapSwathe(ImageUtilities.LoadImageFromFile(image), PrintheadType.Xaar2001Plus, RowSelection.AllRows)
                    {
                        Metadata = new SwatheMetadata
                        {
                            Tag = string.Format("Swathe#{0}", ++swatheDownloaded),
                            PixelHeight = bitmap.PixelHeight
                        }
                    };

                    tagList.Add(swathe.Metadata.Tag);

                    xpmInterconnection.DownloadSwathe(macAddress, swathe);

                    if (swatheDownloaded >= numberOfSwathesRequired)
                    {
                        break;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine(@"Unable to find the given image path : " + imagePath);
            }
            
            return tagList;
        }

        /// <summary>
        /// Download a given number of swathes (for each pair of rows) from the image directory
        /// </summary>
        /// <param name="xpmInterconnection">The connection to the system</param>
        /// <param name="macAddress">Download images to the XPM with this MAC address</param>
        /// <param name="numberOfSwathesRequired">The number of swathes required to be downloaded</param>
        /// <returns>A list of swathe tags identifying the swathes downloaded</returns>
        public static List<string> DownloadSwathesForXaar2001PairRows(XpmInterconnection xpmInterconnection, string macAddress, int numberOfSwathesRequired)
        {
            var tagList = new List<string>();
            var imagePath = GetImagePath();
            var swatheCount = 0;

            try
            {
                // For a single rows the images must be 1000 pixels wide so we look for images which start "1000 ..."
                var availableImages = Directory.GetFiles(imagePath, "1000" + "*.bmp");
                
                foreach (var image in availableImages)
                {
                    Console.WriteLine(@"Downloading " + image);

                    var bitmap = ImageUtilities.LoadImageFromFile(image);

                    var rowSelection = RowSelection.None;
                    if (swatheCount % 2 == 0)
                    {
                        rowSelection = RowSelection.Rows1And2;
                    }
                    else if (swatheCount % 2 == 1)
                    {
                        rowSelection = RowSelection.Rows3And4;
                    }

                    var swathe = new BitmapSwathe(ImageUtilities.LoadImageFromFile(image), PrintheadType.Xaar2001Plus, rowSelection)
                    {
                        Metadata = new SwatheMetadata
                        {
                            Tag = string.Format("Swathe#{0}", ++swatheCount),
                            PixelHeight = bitmap.PixelHeight
                        }
                    };

                    tagList.Add(swathe.Metadata.Tag);

                    xpmInterconnection.DownloadSwathe(macAddress, swathe);

                    if (swatheCount >= numberOfSwathesRequired)
                    {
                        break;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine(@"Unable to find the given image path : " + imagePath);
            }
            
            return tagList;
        }

        /// <summary>
        /// Download a given number of swathes (for a single row) from the image directory
        /// </summary>
        /// <param name="xpmInterconnection">The connection to the system</param>
        /// <param name="macAddress">Download images to the XPM with this MAC address</param>
        /// <param name="numberOfSwathesRequired">The number of swathes required to be downloaded</param>
        /// <returns>A list of swathe tags identifying the swathes downloaded</returns>
        public static List<string> DownloadSwathesForXaar2001SingleRows(XpmInterconnection xpmInterconnection, string macAddress, int numberOfSwathesRequired)
        {
            var tagList = new List<string>();
            var imagePath = GetImagePath();
            var swatheCount = 0;

            try
            {
                // For a single rows the images must be 500 pixels wide so we look for images which start "500 ..."
                var availableImages = Directory.GetFiles(imagePath, "500" + "*.bmp");

                foreach (var image in availableImages)
                {
                    Console.WriteLine(@"Downloading " + image);

                    var bitmap = ImageUtilities.LoadImageFromFile(image);

                    var rowSelection = RowSelection.None;
                    if (swatheCount % 4 == 0)
                    {
                        rowSelection = RowSelection.Row1;
                    }
                    else if (swatheCount % 4 == 1)
                    {
                        rowSelection = RowSelection.Row2;
                    }
                    else if (swatheCount % 4 == 2)
                    {
                        rowSelection = RowSelection.Row3;
                    }
                    else if (swatheCount % 4 == 3)
                    {
                        rowSelection = RowSelection.Row4;
                    }

                    var swathe = new BitmapSwathe(ImageUtilities.LoadImageFromFile(image), PrintheadType.Xaar2001Plus, rowSelection)
                    {
                        Metadata = new SwatheMetadata
                        {
                            Tag = string.Format("Swathe#{0}", ++swatheCount),
                            PixelHeight = bitmap.PixelHeight
                        }
                    };

                    tagList.Add(swathe.Metadata.Tag);

                    xpmInterconnection.DownloadSwathe(macAddress, swathe);

                    if (swatheCount >= numberOfSwathesRequired)
                    {
                        break;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine(@"Unable to find the given image path : " + imagePath);
            }

            return tagList;
        }

        /// <summary>
        /// Get a <see cref="Printhead"/> with default row offsets for the given port
        /// </summary>
        /// <param name="phType">The supported printhead type</param>
        /// <param name="port">The XPM port (0-based)</param>
        /// <returns>The printhead with some default settings</returns>
        public static Printhead GetDefaultPrintead(PrintheadType phType, int port)
        {
            var printheadFactory = new PrintheadFactory();
            PrintheadProperties phProperties = printheadFactory.GetPrintheadProperties(phType); ;

            double[] arr = new double[2];
            arr[0] = 68.0;
            arr[1] = 0.0;

            var phf = new PrintheadFactory();
            var php = phf.GetPrintheadProperties(phType);
           /* return new Printhead(phProperties, xpmPort: port, offset: 0.0,
                rowOffsets: new List<double>(php.DefaultRowOffsets), useDir: false, mirrorImage: false);*/
            return new Printhead(phProperties, xpmPort: port, offset: 0.0,
                rowOffsets: new List<double>(arr), useDir: false, mirrorImage: false);
        }

        public static SystemConfiguration LoadSystemConfiguration(string systemConfigurationFile, Logger logger)
        {
            var repository = new SystemRepository(logger);
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
                .Replace("file:\\", string.Empty);

            var systemConfigurationPath = Path.Combine(path, systemConfigurationFile);

            Console.WriteLine("\n\nLoading configuration:\n{0}", systemConfigurationPath);
            var systemConfiguration = new SystemConfiguration() { LoggingService = logger };

            if (!repository.ReadFromXml(systemConfigurationPath, ref systemConfiguration))
            {
                Console.WriteLine(string.Format("\nUnable to read configuration {0}.", systemConfigurationPath));
                return null;
            }

            if (!systemConfiguration.IsValid())
            {
                Console.WriteLine("\nThe system configuration {0} is not valid", systemConfiguration.Name);
                //WaitForKeyPressedAndCleanup();
                return null;
            }
            return systemConfiguration;
        }

        public static void SetupEncodersAndProductDetects(XpmInterconnection xpmInterconnection, SystemConfiguration sc,
            Dictionary<string, IList<int>> phMapSubset, int inputStablePeriod = 8)
        {
            foreach (var macAddress in phMapSubset.Keys)
            {
                var tec = sc.TransportEncoders.First();
                var te = tec.ToTransportEncoder();
                var pd = sc.ProductDetects.First().ToProductDetect();
                pd.InputStablePeriod = (uint) inputStablePeriod;
                var tePdChainIndex = sc.TransportEncoderProductDetectChains.First().ChainId - 1;

                var xpmPorts = phMapSubset[macAddress];

                var printheadChainMapping = new List<PrintheadTePdChainAssociation>();
                foreach (var xpmPort in xpmPorts)
                {
                    printheadChainMapping.Add(
                        new PrintheadTePdChainAssociation
                        {
                            PrinteadIndex = xpmPort,
                            TransportEncoderProductDetectChain = tePdChainIndex
                        });
                }

                var tePdChain = new TransportEncoderProductDetectChain(te, pd, tePdChainIndex);

                xpmInterconnection.SetupTransportEncoderProductDetectChains(
                    macAddress, printheadChainMapping, new[] {tePdChain});
            }
        }

        private static bool _ConnectAndWaitForPrintheadsReady(XpmInterconnection xpmInterconnection, IEnumerable<int> xpmPorts)
        {
            var connected = new AutoResetEvent(false);
            var ready = new AutoResetEvent(false);
            var phReady = new List<int>();

            xpmInterconnection.XpmConnected += (sender, macAddress) =>
            {
                Console.WriteLine(@"XPM Connected: " + macAddress);
                connected.Set();
            };

            xpmInterconnection.PrintheadPowerCompleted += (sender, args) =>
            {
                lock (_phPowerLock)
                {
                    if (args.Outcome)
                    {
                        phReady.Add(args.PrintheadIndex);
                        Console.WriteLine(@"Printhead {0,2:#0} ready", +args.PrintheadIndex);

                        if (phReady.Intersect(xpmPorts).Count() == xpmPorts.Count())
                        {
                            ready.Set();
                        }
                    }
                    else
                    {
                        Console.WriteLine(@"Printhead {0,2:#0} NOT ready", +args.PrintheadIndex);
                    }
                }
            };

            Console.WriteLine(@"Initialising system");
            xpmInterconnection.InitialiseSystem();

            if (!connected.WaitOne(InitialisationTimeout))
            {
                Console.WriteLine(@"No XPM boxes detected");
                return false;
            }

            if (!ready.WaitOne(PhInitialisationTimeout))
            {
                Console.WriteLine(@"Failed to power on printheads " + xpmPorts.Except(phReady).FormatSequence());
                return false;
            }

            return true;
        }

        private static bool _ConnectAndWaitForPrintheadsReady(XpmInterconnection xpmInterconnection, IEnumerable<Tuple<string, int>> expectedPrintheads)
        {
            var connected = new AutoResetEvent(false);
            var ready = new AutoResetEvent(false);
            var phReady = new List<Tuple<string, int>>();
            var expectedXpms = expectedPrintheads.Select(p => p.Item1).Distinct();
            var detectedXpms = new HashSet<string>();
            var xpmLock = new object();
            var phLock = new object();

            xpmInterconnection.XpmConnected += (sender, macAddress) =>
            {
                lock (xpmLock)
                {
                    Console.WriteLine(@"XPM Connected: " + macAddress);
                    detectedXpms.Add(macAddress);

                    if (detectedXpms.ToArray().Intersect(expectedXpms).Count() == detectedXpms.Count)
                    {
                        connected.Set();
                    }
                }
            };

            xpmInterconnection.PrintheadPowerCompleted += (sender, args) =>
            {
                if (args.Outcome)
                {
                    lock (phLock)
                    {
                        phReady.Add(new Tuple<string, int>(args.XpmMacAddress, args.PrintheadIndex));
                        Console.WriteLine(@"Printhead {0,2:#0} on XPM {1} ready", +args.PrintheadIndex,
                            args.XpmMacAddress);

                        if (phReady.Intersect(expectedPrintheads).Count() == expectedPrintheads.Count())
                        {
                            ready.Set();
                        }
                    }
                }
                else
                {
                    Console.WriteLine(@"Printhead {0,2:#0} NOT ready", +args.PrintheadIndex);
                }
            };

            Console.WriteLine(@"Initialising system");
            xpmInterconnection.InitialiseSystem();

            if (!connected.WaitOne(InitialisationTimeout))
            {
                Console.WriteLine(@"No expected XPM boxes detected");
                return false;
            }

            if (!ready.WaitOne(PhInitialisationTimeout))
            {
                Console.WriteLine(@"Failed to power on printheads");// + expectedPrintheads.Except(phReady).FormatSequence());
                return false;
            }

            return true;
        }

        private static void _SetupEncoderAndProductDetect(XpmInterconnection xpmInterconnection, string macAddress,
                    IEnumerable<int> xpmPorts, bool useInternalProductDetect = false, bool useInternalEncoder = false, 
                    int tePdChainIndex = 0, int dpi = 360)
        {
            var productDetect = GetProductDetect(useInternalProductDetect);
            var transportEncoder = GetTransportEncoder(useInternalEncoder, dpi);

            var globalPrintParameters = GetGlobalPrintParameters();
            xpmInterconnection.SetGlobalPrintParameters(globalPrintParameters);

            var tePdChain = new TransportEncoderProductDetectChain(transportEncoder, productDetect, tePdChainIndex);

            var printheadChainMapping = new List<PrintheadTePdChainAssociation>();
            foreach (var xpmPort in xpmPorts)
            {
                printheadChainMapping.Add(
                    new PrintheadTePdChainAssociation
                    {
                        PrinteadIndex = xpmPort,
                        TransportEncoderProductDetectChain = tePdChainIndex
                    });
            }

            xpmInterconnection.SetupTransportEncoderProductDetectChains(
                macAddress, printheadChainMapping, new[] { tePdChain });
        }
    }
}
