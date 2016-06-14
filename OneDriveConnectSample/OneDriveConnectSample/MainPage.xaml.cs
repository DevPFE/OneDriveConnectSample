using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OneDriveConnectSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Client Apication Id.
        private string clientId = "YourClientIDHere";

        // Return url.
        private string returnUrl = "https://login.live.com/oauth20_desktop.srf";

        // Define the permission scopes.
        private static readonly string[] scopes = new string[] { "onedrive.readwrite", "offline_access", "wl.signin", "wl.basic" };

        // Create the OneDriveClient interface.
        private IOneDriveClient oneDriveClient { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            if (this.oneDriveClient == null)
            {
                // Setting up the client here, passing in our Client Id, Return Url, Scopes that we want permission to, 
                // and building a Web Broker to go do our bidding. 
                this.oneDriveClient = await OneDriveClient.GetAuthenticatedMicrosoftAccountClient(
                    clientId, 
                    returnUrl, 
                    scopes, 
                    webAuthenticationUi: new WebAuthenticationBrokerWebAuthenticationUi());
            }

            try
            {
                // Now that we have our client built, lets get authenticated!
                if (!this.oneDriveClient.IsAuthenticated)
                {
                    await this.oneDriveClient.AuthenticateAsync();
                    // Show user we are connected.
                    txtBox_Response.Text = ("We are authenticated and connected! \r\n Now press the button to get the Drive ID from OneDrive!");
                }
                else
                {
                    // Show user we are already connected.
                    txtBox_Response.Text = ("You are already connected");
                }
                // Light up the button to allow attempt to get OneDrive Drive Id.

                // We are either just autheticated and connected or we already connected, either way we need the drive button now.
                btn_GetDriveId.Visibility = Visibility.Visible;
            }
            catch (OneDriveException exception)
            {
                // Eating the authentication cancelled exceptions and resetting our client. 
                if (!exception.IsMatch(OneDriveErrorCode.AuthenticationCancelled.ToString()))
                {
                    if (exception.IsMatch(OneDriveErrorCode.AuthenticationFailure.ToString()))
                    {
                        txtBox_Response.Text = "Authentication failed (did you cancel?), disposing of the client...";

                        ((OneDriveClient)this.oneDriveClient).Dispose();
                        this.oneDriveClient = null;
                    }
                    else
                    {
                        // Or we failed due to someother reason, let get that exception printed out.
                        txtBox_Response.Text = exception.Error.ToString();
                    }
                }
                else
                {
                    ((OneDriveClient)this.oneDriveClient).Dispose();
                    this.oneDriveClient = null;
                }
            }
        }

        private async void btn_GetDriveId_Click(object sender, RoutedEventArgs e)
        {
            if (this.oneDriveClient != null && this.oneDriveClient.IsAuthenticated == true)
            {
                var drive = await this.oneDriveClient.Drive.Request().GetAsync();
                txtBox_Response.Text = drive.Id.ToString();
            }
            else
            {
                txtBox_Response.Text = "We should never get here::: You are not logged in, login first and allow access before trying to get drive information.";
            }
            
        }
    }
}
