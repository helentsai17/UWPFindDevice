using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MSSMSpirometer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            InnerFrame.Navigate(typeof(DeviceVidPid));
        }

        private void findDevice(object sender, RoutedEventArgs e)
        {
            InnerFrame.Navigate(typeof(BlankPage));
        }
        private void findDevice2(object sender, RoutedEventArgs e)
        {
            InnerFrame.Navigate(typeof(BlankPage1));
        }

        private void findDevice3(object sender, RoutedEventArgs e)
        {
            InnerFrame.Navigate(typeof(DeviceVidPid));
            checkSesstion.IsEnabled = true;
        }
        private void datadisplay(object sender, RoutedEventArgs e)
        {
            InnerFrame.Navigate(typeof(USBDataDisplay));
            
            checkSesstion.IsEnabled = false;
        }

        private void dataStorageDispaly(object sender, RoutedEventArgs e)
        {
            InnerFrame.Navigate(typeof(DataStorage));
            checkSesstion.IsEnabled = true;
        }

        private void graphDisplay(object sender, RoutedEventArgs e)
        {
            InnerFrame.Navigate(typeof(DataGraph));
            checkSesstion.IsEnabled = true;
        }
    }
}
