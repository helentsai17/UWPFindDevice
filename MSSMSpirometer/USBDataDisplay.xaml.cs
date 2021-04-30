using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MSSMSpirometer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class USBDataDisplay : Page
    {

        private MainPage rootPage = MainPage.Current;

        private CancellationTokenSource cancellationTokenSource;

        private Object cancelIoLock = new Object();

        private Boolean runningReadTask;
        private Boolean runningWriteTask;
        private Boolean runningReadWriteTask;

        // Indicate if we navigate away from this page or not.
        private Boolean navigatedAway;

        private UInt32 totalBytesWritten;
        private UInt32 totalBytesRead;
        public USBDataDisplay()
        {
            this.InitializeComponent();
            totalBytesRead = 0;
            totalBytesWritten = 0;
            runningReadTask = false;
            runningWriteTask = false;
            runningReadWriteTask = false;
        }

        public void Dispose()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            navigatedAway = false;
            navigatedAway = false;

            // Both the OSRFX2 and the SuperMutt use the same scenario
            // If no devices are connected, none of the scenarios will be shown and an error will be displayed
            Dictionary<DeviceType, UIElement> deviceScenarios = new Dictionary<DeviceType, UIElement>();
            deviceScenarios.Add(DeviceType.OsrFx2, GeneralScenario);


            Utilities.SetUpDeviceScenarios(deviceScenarios, DeviceScenarioContainer);

            // So we can reset future tasks
            ResetCancellationTokenSource();

            EventHandlerForDevice.Current.OnAppSuspendCallback = new SuspendingEventHandler(this.OnAppSuspension);

            // Reset the buttons if the app resumed and the device is reconnected
            EventHandlerForDevice.Current.OnDeviceConnected = new TypedEventHandler<EventHandlerForDevice, DeviceInformation>(this.OnDeviceConnected);

            UpdateButtonStates();
        }

        private void OnAppSuspension(object sender, SuspendingEventArgs e)
        {
            CancelAllIoTasks();
        }

        private void CancelAllIoTasks()
        {
            lock (cancelIoLock)
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();

                    // Existing IO already has a local copy of the old cancellation token so this reset won't affect it
                    ResetCancellationTokenSource();
                }
            }
        }

        private void OnDeviceConnected(EventHandlerForDevice sender, DeviceInformation onDeviceConnectedEventArgs)
        {
            UpdateButtonStates();
        }

        private void ResetCancellationTokenSource()
        {
            cancellationTokenSource = new CancellationTokenSource();

            // Hook the cancellation callback (called whenever Task.cancel is called)
            cancellationTokenSource.Token.Register(() => NotifyCancelingTask());
        }

        private async void NotifyCancelingTask()
        {
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                    ButtonBulkRead.IsEnabled = false;
                    ButtonBulkWrite.IsEnabled = false;

                    ButtonCancelAllIoTasks.IsEnabled = false;

                    if (!navigatedAway)
                    {
                        StatusBlock.Text = "Canceling task... Please wait...";
                    }
                }));
        }

        private void UpdateButtonStates()
        {

            ButtonBulkRead.IsEnabled = !runningReadWriteTask && !runningReadTask;
            ButtonBulkWrite.IsEnabled = !runningReadWriteTask && !runningWriteTask;
            ButtonCancelAllIoTasks.IsEnabled = IsPerformingIo();
        }
        private Boolean IsPerformingIo()
        {
            return (runningReadTask || runningWriteTask || runningReadWriteTask);
        }

        private async void NotifyTaskCanceled()
        {
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    if (!navigatedAway)
                    {
                        StatusBlock.Text = "The read or write operation has been cancelled";
                    }
                }));
        }

        //=======================================================================================

        private async void BulkRead_Click(object sender, RoutedEventArgs e)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Reading...";

                    // We need to set this to true so that the buttons can be updated to disable the read button. We will not be able to
                    // update the button states until after the read completes.
                    runningReadTask = true;
                    UpdateButtonStates();

                    // Both supported devices have the bulk in pipes on index 0
                    UInt32 bulkInPipeIndex = 0;

                    // Read as much data as possible in one packet
                    UInt32 bytesToRead = 512;

                    await BulkReadAsync(bulkInPipeIndex, bytesToRead, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException /*ex*/)
                {
                    NotifyTaskCanceled();
                }
                finally
                {
                    runningReadTask = false;

                    UpdateButtonStates();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }

        }

        private async Task BulkReadAsync(UInt32 bulkPipeIndex, UInt32 bytesToRead, CancellationToken cancellationToken)
        {
            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkInPipes[(int)bulkPipeIndex].InputStream;



            DataReader reader = new DataReader(stream);

            StatusBlock.Text = reader.ToString();

            Task<UInt32> loadAsyncTask;

            // Don't start any IO if we canceled the task
            lock (cancelIoLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Cancellation Token will be used so we can stop the task operation explicitly
                // The completion function should still be called so that we can properly handle a canceled task
                loadAsyncTask = reader.LoadAsync(bytesToRead).AsTask(cancellationToken);
            }

            UInt32 bytesRead = await loadAsyncTask;

            totalBytesRead += bytesRead;

            PrintTotalReadWriteBytes();

            // The data that is read is stored in the reader object
            // e.g. To read a string from the buffer:
            // reader.ReadString(bytesRead);   

            UsbBulkInPipe readPipe = EventHandlerForDevice.Current.Device.DefaultInterface.BulkInPipes[0];

            IBuffer buffer = reader.ReadBuffer(bytesRead);

            using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
            {
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                DataDisplay.Text = dataReader.ReadString(buffer.Length);
            }

            

        }


        private async void PrintTotalReadWriteBytes()
        {
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                new DispatchedHandler(() =>
                {
                    // If we navigated away from this page, do not print anything. The dispatch may be handled after
                    // we move to a different page.
                    if (!navigatedAway)
                    {
                        StatusBlock.Text = "Total bytes read: " + totalBytesRead.ToString("D", NumberFormatInfo.InvariantInfo) + "; Total bytes written: "
                            + totalBytesWritten.ToString("D", NumberFormatInfo.InvariantInfo);
                    }
                }));
        }


        //==============================Write data =================================================================

        private async void BulkWrite_Click(object sender, RoutedEventArgs e)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Writing...";

                    runningWriteTask = true;
                    UpdateButtonStates();

                    UInt32 bulkOutPipeIndex = 0;

                    UInt32 bytesToWrite = 512;

                    await BulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException /*ex*/)
                {
                    NotifyTaskCanceled();
                }
                finally
                {
                    runningWriteTask = false;

                    UpdateButtonStates();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }

        private async Task BulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            var Memoinfo = new byte[] { 0x02, 0x4d, 0x56, 0x4d, 0x49, 0x03, 0x01 };
            var DeviceName = new byte[] { 0x02, 0x56, 0x56, 0x44, 0x49, 0x03, 0xC };

            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(DeviceName);

            Task<UInt32> storeAsyncTask;

            // Don't start any IO if we canceled the task
            lock (cancelIoLock)
            {
                cancellationToken.ThrowIfCancellationRequested();
                storeAsyncTask = writer.StoreAsync().AsTask(cancellationToken);
            }

            UInt32 bytesWritten = await storeAsyncTask;

            totalBytesWritten += bytesWritten;

            PrintTotalReadWriteBytes();
        }



        //================================ force Cancel input ================================================
        private void CancelAllIoTasks_Click(object sender, RoutedEventArgs e)
        {

            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                CancelAllIoTasks();
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }

        }
    }
}