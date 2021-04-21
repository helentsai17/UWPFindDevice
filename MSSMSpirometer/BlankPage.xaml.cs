using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;

using Windows.Devices.Enumeration;
using Windows.Devices.Enumeration.Pnp;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MSSMSpirometer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage : Page
    {
        public BlankPage()
        {
            this.InitializeComponent();
        }

        void InterfaceClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InterfaceClasses.SelectedItem == PrinterInterfaceClass)
            {
                InterfaceClassGuid.Text = "{0ECEF634-6EF0-472A-8085-5AD023ECBCCD}";
            }
            else if (InterfaceClasses.SelectedItem == WebcamInterfaceClass)
            {
                InterfaceClassGuid.Text = "{E5323777-F976-4F5B-9B55-B94699C46E44}";
            }
            else if (InterfaceClasses.SelectedItem == WpdInterfaceClass)
            {
                InterfaceClassGuid.Text = "{6AC27878-A6FA-4155-BA85-F98F491D4F33}";
            }
        }

        async void EnumerateDeviceInterfaces(object sender, RoutedEventArgs eventArgs)
        {
            EnumerateInterfacesButton.IsEnabled = false;
            DeviceInterfacesOutputList.Items.Clear();
            try
            {
                var selector = "System.Devices.InterfaceClassGuid:=\"" + InterfaceClassGuid.Text + "\"";
                //                 + " AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True";
                var interfaces = await DeviceInformation.FindAllAsync(selector, null);
                OutputText.Text = interfaces.Count + " device interface(s) found\n\n";
                foreach (DeviceInformation deviceInterface in interfaces)
                {
                    DisplayDeviceInterface(deviceInterface);
                }
            }
            catch (ArgumentException)
            {
                //The ArgumentException gets thrown by FindAllAsync when the GUID isn't formatted properly
                //The only reason we're catching it here is because the user is allowed to enter GUIDs without validation
                //In normal usage of the API, this exception handling probably wouldn't be necessary when using known-good GUIDs 
                OutputText.Text = "Caught ArgumentException. Verify that you've entered a valid interface class GUID.";
            }
            EnumerateInterfacesButton.IsEnabled = true;
        }

        async void DisplayDeviceInterface(DeviceInformation deviceInterface)
        {
            var id = "Id:" + deviceInterface.Id;
            var name = deviceInterface.Name;
            var isEnabled = "IsEnabled:" + deviceInterface.IsEnabled;
            var item = id + " is \n" + name + " and \n" + isEnabled;
            DeviceInterfacesOutputList.Items.Add(item);
        }
    }
}
