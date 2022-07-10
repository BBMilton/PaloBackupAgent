using AdysTech.CredentialManager;
using System;
using System.IO;
using System.Net;
using System.Windows;
using U8Xml;

namespace PaloPHashGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Connect to the Palo device and generate the PHash
        /// </summary>
        private void ConnectButton(object sender, RoutedEventArgs e)
        {
            try
            {
                ParseXML(GetResponce());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error Parsing data");
            }
        }

        /// <summary>
        /// returns the raw responce from a HttpWebRequest
        /// </summary>
        private string GetResponce()
        {
            string URL = $"https://{AddressTextBox.Text}/api/?type=keygen&user={UsernameTextBox.Text}&password={PasswordTextBox.Password}";

            HttpWebRequest request = WebRequest.Create(URL) as HttpWebRequest;
            var response = request.GetResponse();

            using (Stream receiveStream = response.GetResponseStream())
            {
                using (StreamReader readStream = new StreamReader(receiveStream))
                {
                    return readStream.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Reads the XML data returned and extracts the key
        /// </summary>
        public void ParseXML(string data)
        {
            try
            {
                using (XmlObject xml = XmlParser.Parse(data))
                {
                    foreach (XmlNode rnode in xml.Root.Children) // each "result"
                    {
                        foreach (XmlNode node in rnode.Children) // each key
                        {
                            switch (node.Name.ToString().ToLower())
                            {
                                case "key":
                                    PHashTextBox.Text = node.InnerText.ToString();
                                    break;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error Parsing data");
            }
        }

        /// <summary>
        /// Copies the PHash to clipboard
        /// </summary>
        private void ClipboardButtonSet(object sender, RoutedEventArgs e)
        {
            if (PHashTextBox.Text != null && PHashTextBox.Text.Length > 0)
            {
                Clipboard.SetText(PHashTextBox.Text);
            }
        }

        private void SaveCMButton(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsernameTextBox.Text != null && UsernameTextBox.Text.Length > 0)
                {
                    if (PHashTextBox.Text != null && PHashTextBox.Text.Length > 0)
                    {
                        SaveToCM();
                    }
                    else
                    {
                        ParseXML(GetResponce());
                        SaveToCM();
                    }
                }
                else
                {
                    MessageBox.Show(this, "You will need to Create or Generate a PHash First!", "No data available to save");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error saving to Windows Credential Manager");
            }
        }

        private void SaveToCM()
        {
            NetworkCredential cred = new NetworkCredential(UsernameTextBox.Text, PHashTextBox.Text);
            CredentialManager.SaveCredentials("PBA-" + AddressTextBox.Text.ToLower(), cred);

            MessageBox.Show(this, "Successfully saved to Windows Credential Manager", "Save was successfully");
        }

        private void ClipboardButtonCM(object sender, RoutedEventArgs e)
        {
            try
            {
                //if there is a PHash present?
                if (PHashTextBox.Text != null && PHashTextBox.Text.Length > 0)
                {
                    Clipboard.SetText($"<CredentialManager>{"PBA-" + AddressTextBox.Text.ToLower()}</CredentialManager>");
                }
                //Otherwise are the other textboxes filled in?
                else if (AddressTextBox.Text != null && AddressTextBox.Text.Length > 0 && UsernameTextBox.Text != null && UsernameTextBox.Text.Length > 0 && PasswordTextBox.Password != null && PasswordTextBox.Password.Length > 0)
                {
                    ParseXML(GetResponce());
                    SaveToCM();
                }
                else
                {
                    MessageBox.Show(this, "You will need to generate a PHash First!", "No data available to copy");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error copying");
            }
        }
    }
}
