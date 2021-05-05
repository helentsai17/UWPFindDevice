﻿using Newtonsoft.Json;
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
using Windows.Security.Cryptography;
using Windows.Storage;
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

        #region OnNavigatedTo 
        
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
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

        #endregion

        string memoInfo = "";

        // Read Record Message
        string recordNumber = "";
        string subjectInfo = "";
        string sessionInfo = "";
        string PredictedValues = "";
        string LLNValues = "";
        string ULNValues = "";
        string BestTestResults = "";
        string BestTestData = "";
        string PercentageofPredicted = "";
        string PercentagePrePost = "";
        string Z_score = "";
        string PrePostChange = "";
        string RankedTestResult_1 = "";
        string RankedTestData_1 = "";
        string RankedTestResult_2 = "";
        string RankedTestData_2 = "";
        string RankedTestResult_3 = "";
        string RankedTestData_3 = "";
        string PreBestTestResult = "";
        string PreBestTestData = "";
        string PreBestPercentageofPredicted = "";
        string PreBestZ_score = "";
        string InterpretationInformation = "";


        //=========================Bulk Read data==================================================

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
                    UInt32 bytesToRead = 2048;

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
            string dataString = "";

            using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
            {
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                dataString = dataReader.ReadString(buffer.Length);
                DataDisplay.Text = dataString;

                byte[] data;
                CryptographicBuffer.CopyToByteArray(buffer, out data);
               // TestData.Text = BitConverter.ToString(data);
            }

            storageString(dataString);

           

        }

        private void storageString(string dataString)
        {
            if (dataString.Contains("VMMI"))
            {
                memoInfo = dataString;
                MemoInfordata.Text = memoInfo;
            }

            if (dataString.Contains("VMRR"))
            {
                recordNumber = dataString;
                recordNum.Text = recordNumber;
            }

            if (dataString.Contains("A"))
            {
                subjectInfo = dataString;
                subjectInfoText.Text = subjectInfo;
            }

            if (dataString.Contains("B"))
            {
                sessionInfo = dataString;
                sessionInfoText.Text = sessionInfo;
            }

            if (dataString.Contains("C"))
            {
                PredictedValues = dataString;
                PredictedValuesText.Text = PredictedValues;
            }

            if (dataString.Contains("D"))
            {
                LLNValues = dataString;
                LLNValuesText.Text = LLNValues;
            }

            if (dataString.Contains("E"))
            {
                ULNValues = dataString;
                ULNValuesText.Text = ULNValues;
            }
            
            if (dataString.Contains("F"))
            {
                BestTestResults = dataString;
                BestTestResultsText.Text = BestTestResults;
            }

            if (dataString.Contains("G"))
            {
                BestTestData = dataString;
                BestTestDataText.Text = BestTestData;
            }

            if (dataString.Contains("H"))
            {
                PercentageofPredicted = dataString;
                PercentageofPredictedText.Text = PercentageofPredicted;
            }

            if (dataString.Contains("I"))
            {
                PercentagePrePost = dataString;
                PercentagePrePostText.Text = PercentagePrePost;
            }

            if (dataString.Contains("J"))
            {
                Z_score= dataString;
                Z_scoreText.Text = Z_score;
            }

            if (dataString.Contains("K"))
            {
                PrePostChange = dataString;
                PrePostChangeText.Text = PrePostChange;
            }

            if (dataString.Contains("L"))
            {
                RankedTestResult_1 = dataString;
                RankedTestResult_1Text.Text = RankedTestResult_1;
            }

            if (dataString.Contains("M"))
            {
                RankedTestData_1 = dataString;
                RankedTestData_1Text.Text = RankedTestData_1;
            }

            if (dataString.Contains("N"))
            {
                RankedTestResult_2 = dataString;
                RankedTestResult_2Text.Text = RankedTestResult_2;
            }

            if (dataString.Contains("O"))
            {
                RankedTestData_2 = dataString;
                RankedTestData_2Text.Text = RankedTestData_2;
            }

            if (dataString.Contains("P"))
            {
                RankedTestResult_3 = dataString;
                RankedTestResult_3Text.Text = RankedTestResult_3;
            }

            if (dataString.Contains("Q"))
            {
                RankedTestData_3 = dataString;
                RankedTestData_3Text.Text = RankedTestData_3;
            }

            if (dataString.Contains("R"))
            {
                PreBestTestResult = dataString;
                PreBestTestResultText.Text = PreBestTestResult;
            }

            if (dataString.Contains("S"))
            {
                PreBestTestData = dataString;
                PreBestTestDataText.Text = PreBestTestData;
            }

            if (dataString.Contains("T"))
            {
                PreBestPercentageofPredicted = dataString;
                PreBestPercentageofPredictedText.Text = PreBestPercentageofPredicted;
            }

            if (dataString.Contains("U"))
            {
                PreBestZ_score = dataString;
                PreBestZ_scoreText.Text = PreBestZ_score;
            }

            if (dataString.Contains("V"))
            {
                InterpretationInformation = dataString;
                InterpretationInformationText.Text = InterpretationInformation;
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


        //==============================Data Collect================================================================

       


        public async void WriteData()
        {
            string fileName = "SpirometerData.json";
            SpirometerData[] _data = Array.Empty<SpirometerData>();

            SpirometerData[] CreateData = new SpirometerData[]
            {
                new SpirometerData()
                {
                    MemoInfo = this.memoInfo
                },
            };

            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.TryGetItemAsync(fileName) as IStorageFile;

            if (file == null)
            {
                _data = CreateData;
                var newfile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                var text = JsonConvert.SerializeObject(_data);
                await FileIO.WriteTextAsync(newfile, text);
            }
        }


        #region all the data request button click 

        //==============================Write data =================================================================


        #region 6.1 Device Identification Message
        private void BulkWrite_Click(object sender, RoutedEventArgs e)
        {
            DataRequest("DeviceIdentification");
        }

        private async Task DIBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
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

        #endregion

        #region 6.2 Remote Mode buttom click 
        private void RemoteMode_Click(object sender, RoutedEventArgs e)
        {
            DataRequest("RemoteMode");
        }

        private async Task RMBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] RMvalue = new byte[] { 0x02, 0x4d, 0x56, 0x52, 0x4d, 0x03, 0x01 };

            byte bccValue = Getbcc(RMvalue);
            RMvalue[6] = bccValue;

            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(RMvalue);

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

        #endregion

        #region 6.3 Bluetooth Exit Remote Buttom Click

        private void RemoteExit_Click(object sender, RoutedEventArgs e)
        {
            DataRequest("RemoteExit");
        }

        private async Task RXBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] RXvalue = new byte[] { 0x02, 0x4d, 0x56, 0x58, 0x52, 0x03, 0x10 };

           
            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(RXvalue);

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
        #endregion

        #region 6.4 Memory Info Message 

        private void MemoryInfo_Click(object sender, RoutedEventArgs e)
        {
            DataRequest("MemoryInfo");
        }

        private async Task MIBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] MIvalue = new byte[] { 0x02, 0x4d, 0x56, 0x4d, 0x49, 0x03, 0x01 };

            byte bccValue = Getbcc(MIvalue);
            MIvalue[6] = bccValue;

            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(MIvalue);

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

        #endregion

        #region 6.7 Read Settings Message

        private void ReadSetting_Click(object sender, RoutedEventArgs e)
        {
            DataRequest("ReadSetting");
        }

        private async Task RSBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] value = new byte[] { 0x02, 0x4d, 0x56, 0x52, 0x53, 0x03, 0x01 };

            byte secbccValue = Getbcc(value);
            value[6] = secbccValue;

            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(value);

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
        #endregion

        #region 6.8 Accouracy Log Info

        private void AccouracyLog_Click(object sender, RoutedEventArgs e)
        {
            DataRequest("AccouracyLog");
        }

        private async Task ALBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] value = new byte[] { 0x02, 0x4d, 0x56, 0x41, 0x4c, 0x03, 0x01 };

            byte secbccValue = Getbcc(value);
            value[6] = secbccValue;

            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(value);

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



        #endregion

        #region 6.9 Audit Trail Message

        private void AuditTrail_Click(object sender, RoutedEventArgs e)
        {
            DataRequest("AuditTrail");
        }

        private async Task ATBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] value = new byte[] { 0x02, 0x4d, 0x56, 0x41, 0x54, 0x03, 0x01 };

            byte secbccValue = Getbcc(value);
            value[6] = secbccValue;

            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(value);

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


        #endregion

        #region 6.10. Current Time Message
        private void CurrentTime_Click(object sender, RoutedEventArgs e)
        {
            DataRequest("CurrentTime");
        }

        private async Task CTBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] value = new byte[] { 0x02, 0x4d, 0x56, 0x41, 0x54, 0x03, 0x01 };

            byte secbccValue = Getbcc(value);
            value[6] = secbccValue;

            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(value);

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
        #endregion


        /// <summary>
        /// Button click function selected
        /// </summary>
        /// <param name="ButtonClick"></param>
        public async void DataRequest(string ButtonClick)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Writing...";

                    runningWriteTask = true;
                    UpdateButtonStates();

                    UInt32 bulkOutPipeIndex = 0;

                    UInt32 bytesToWrite = 15;

                    String dataRequestNum = ButtonClick;
                    switch (dataRequestNum)
                    {
                        case "DeviceIdentification":
                            await DIBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                            break;
                        case "RemoteMode":
                            await RMBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                            break;
                        case "RemoteExit":
                             await RXBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                            break;
                        case "MemoryInfo":
                            await MIBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                            break;
                        case "ReadSetting":
                            await RSBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                            break;
                        case "AccouracyLog":
                            await ALBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                            break;
                        case "AuditTrail":
                            await ATBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                            break;
                        case "CurrentTime":
                            await CTBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                            break;
                        default:
                            await DIBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                            break;
                    }

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

        #region 6.5. Read Record Message

        private async void ReadRecord_Click(object sender, RoutedEventArgs e)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Writing...";

                    runningWriteTask = true;
                    UpdateButtonStates();

                    UInt32 bulkOutPipeIndex = 0;

                    UInt32 bytesToWrite = 64;

                    await RRBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
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

        private async Task RRBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] value = new byte[] { 0x02, 0x4d, 0x56, 0x52, 0x52, 0x30, 0x30, 0x33, 0x03, 0x01 };

            byte bccValue = Getbcc(value);
            value[9] = bccValue;

            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(value);

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

        #endregion

        #region Next Block

        private async void NextBlock_Click(object sender, RoutedEventArgs e)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Writing...";

                    runningWriteTask = true;
                    UpdateButtonStates();

                    UInt32 bulkOutPipeIndex = 0;

                    UInt32 bytesToWrite = 2;

                    await NEXTBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
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

        private async Task NEXTBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] value = new byte[] { 0x06 };

            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkOutPipes[(int)bulkPipeIndex].OutputStream;

            var writer = new DataWriter(stream);

            writer.WriteBytes(value);

            Task<UInt32> storeAsyncTask;

          
            lock (cancelIoLock)
            {
                cancellationToken.ThrowIfCancellationRequested();
                storeAsyncTask = writer.StoreAsync().AsTask(cancellationToken);
            }

            UInt32 bytesWritten = await storeAsyncTask;

            totalBytesWritten += bytesWritten;

            PrintTotalReadWriteBytes();
        }

        #endregion

        /// <summary>
        /// counting for bcc 
        /// </summary>
        public byte Getbcc(byte[] inputStream)
        {
            byte bcc = 1;
            if (inputStream != null && inputStream.Length > 0)
            {
                for (int i = 0; i < inputStream.Length; i++)
                {
                    bcc ^= inputStream[i];
                }

            }

            return bcc;
        }



#endregion


        #region auto Read Record

        

        private async void ALLReadRecord_Click(object sender, RoutedEventArgs e)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Writing...";

                    runningWriteTask = true;
                    UpdateButtonStates();

                    UInt32 bulkOutPipeIndex = 0;

                    UInt32 bytesToWrite = 16;

                    await RRBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);

                    ReadData();
                    getNextBlock();
                    ReadData();
                    getNextBlock();


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

        private async void getNextBlock()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Writing...";

                    runningWriteTask = true;
                    UpdateButtonStates();

                    UInt32 bulkOutPipeIndex = 0;
                    UInt32 bytesToWrite = 2;

                    await NEXTBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
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

        private async void ReadData()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    TestData.Text = "Reading...";

                    UInt32 bulkInPipeIndex = 0;
                    UInt32 bytesToRead = 2048;

                    await autoBulkReadAsync(bulkInPipeIndex, bytesToRead, cancellationTokenSource.Token);
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

        private async Task autoBulkReadAsync(UInt32 bulkPipeIndex, UInt32 bytesToRead, CancellationToken cancellationToken)
        {
            var stream = EventHandlerForDevice.Current.Device.DefaultInterface.BulkInPipes[(int)bulkPipeIndex].InputStream;

            DataReader reader = new DataReader(stream);

            TestData.Text = reader.ToString();

            Task<UInt32> loadAsyncTask;

            // Don't start any IO if we canceled the task
            lock (cancelIoLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                loadAsyncTask = reader.LoadAsync(bytesToRead).AsTask(cancellationToken);
            }

            UInt32 bytesRead = await loadAsyncTask;

            totalBytesRead += bytesRead;

            PrintTotalReadWriteBytes();

            IBuffer buffer = reader.ReadBuffer(bytesRead);
            string dataString;
            
           
            using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
            {
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                dataString = dataReader.ReadString(buffer.Length);
                DataDisplay.Text = dataString;

            }

            if (dataString.Contains("VMRR"))
            {
                recordNumber = dataString;
                recordNum.Text = recordNumber;
            }

            if (dataString.Contains("A"))
            {
                subjectInfo = dataString;
                subjectInfoText.Text = subjectInfo;
            }
        }


        #endregion

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