using AdysTech.CredentialManager;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using U8Xml;

namespace PaloBackupAgent
{
    internal class Program
    {
        protected static string BackupFolder = "";
        protected static string LogFile = "";

        static void Main(string[] args)
        {
            // Set the default paths to be the current folder the exe is in
            BackupFolder = Assembly.GetEntryAssembly().CodeBase + @"\Backups";
            LogFile = Assembly.GetEntryAssembly().CodeBase + @"\log.txt";

            // Read the config file and backup the required areas
            ParseXML();

            // Write to a log file that this has completed
            File.AppendAllText(LogFile, $"\nJob Completed on: {DateTime.Now:dd-MM-yyyy HHmm}");
        }


        /// <summary>
        /// Reads the config.xml and builds a 'PaloDevice' to be used in the backup method
        /// </summary>
        private static void ParseXML()
        {
            try
            {
                using (XmlObject xml = XmlParser.ParseFile("config.xml"))
                {
                    // For each "Device"
                    foreach (XmlNode rnode in xml.Root.Children)
                    {
                        // Create an object to hold the parsed data
                        PaloDevice device = new PaloDevice();

                        // For each setting
                        foreach (XmlNode node in rnode.Children)
                        {
                            switch (node.Name.ToString().ToLower())
                            {
                                case "hostname":
                                    device.Address = node.InnerText.ToString();
                                    break;
                                case "phash":
                                    device.PHash = node.InnerText.ToString();
                                    break;
                                case "credentialmanager":
                                    device.PHash = CredentialManager.GetCredentials(node.InnerText.ToString()).Password;
                                    break;
                                case "backupconfig":
                                    device.BackupConfig = bool.Parse(node.InnerText.ToString());
                                    break;
                                case "backupstate":
                                    device.BackupState = bool.Parse(node.InnerText.ToString());
                                    break;
                                case "backupversion":
                                    device.BackupVersion = bool.Parse(node.InnerText.ToString());
                                    break;
                                case "tls":
                                    SetTLSVersion(node.InnerText.ToString(), device);
                                    break;
                                case "backuppath":
                                    BackupRewriter(node.InnerText.ToString());
                                    break;
                                case "logfile":
                                    LogFile = node.InnerText.ToString();
                                    break;
                            }
                        }

                        // Run the backup for this one device
                        BackupPalo(device);
                    }
                }
            }
            catch (Exception e)
            {
                // Write the error to log
                File.AppendAllText(LogFile, $"\n{DateTime.Now:dd-MM-yyyy HHmm}  -  Error encountered during backup:\n");
                File.AppendAllText(LogFile, e.Message+"\n\n");

                // Exit with a non-zero code
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Formats the user provided path
        /// </summary>
        public static void BackupRewriter(string newPath)
        {
            BackupFolder = newPath.Trim();

            if (BackupFolder.EndsWith(@"\"))
            {
                BackupFolder = BackupFolder.Substring(BackupFolder.Length - 1);
            }
        }

        /// <summary>
        /// Set what TLS version to be using when connecting to the palo device
        /// </summary>
        public static void SetTLSVersion(string version, PaloDevice device)
        {
            switch (version)
            {
                case "1.0":
                    device.TLSVersion = SecurityProtocolType.Tls;
                    break;
                case "1.1":
                    device.TLSVersion = SecurityProtocolType.Tls11;
                    break;
                case "1.2":
                    device.TLSVersion = SecurityProtocolType.Tls12;
                    break;
                case "1.3":
                    device.TLSVersion = SecurityProtocolType.Tls13;
                    break;
            }
        }

        private static void BackupPalo(PaloDevice device)
        {
            // Create the backup folder for this task
            string BackupPath = BackupFolder + $@"\{device.Address}\{DateTime.Now:dd-MM-yyyy HHmm}" ;
            Directory.CreateDirectory(BackupPath);

            // Set the required TLS version for this device
            ServicePointManager.SecurityProtocol = device.TLSVersion;

            // Backup each of the areas that where specified
            if (device.BackupConfig)
            {
                WriteResponce($"https://{device.Address}/api/?type=export&category=configuration&key={device.PHash}", BackupPath + @"\BackupConfig.xml");
            }
            if (device.BackupState)
            {
                WriteResponce($"https://{device.Address}/api/?type=export&category=device-state&key={device.PHash}", BackupPath + @"\DeviceState.tgz");
            }
            if (device.BackupVersion)
            {
                //WriteResponce($"https://{device.Address}/api/?type=version&key={device.PHash}", BackupPath + @"\VersionData.xml"); // basic info
                WriteResponce($"https://{device.Address}/api/?type=op&cmd=<show><system><info></info></system></show>&key={device.PHash}", BackupPath + @"\VersionData.xml"); // full info
            }

        }

        /// <summary>
        /// Writes the raw responce from a HttpWebRequest to file.
        /// </summary>
        public static void WriteResponce(string URL, string path)
        {
            HttpWebRequest request = WebRequest.Create(URL) as HttpWebRequest;
            var response = request.GetResponse();

            using (Stream receiveStream = response.GetResponseStream())
            {
                using (Stream fileStream = File.Create(path))
                {
                    receiveStream.CopyTo(fileStream);
                }
            }
        }
    }

    /// <summary>
    /// Data holder for a palo device
    /// </summary>
    public class PaloDevice
    {
        public string Address { get; set; }
        public string PHash { get; set; }
        public bool BackupConfig { get; set; } = false;
        public bool BackupState { get; set; } = false;
        public bool BackupVersion { get; set; } = false;

        public SecurityProtocolType TLSVersion { get; set; } = SecurityProtocolType.Tls12;
    }
}
