using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
       

        // Indicate if we navigate away from this page or not.
        private Boolean navigatedAway;

        private UInt32 totalBytesWritten;
        private UInt32 totalBytesRead;

        int recordcount = 1;
        public USBDataDisplay()
        {
            this.InitializeComponent();
            totalBytesRead = 0;
            totalBytesWritten = 0;
            runningReadTask = false;
            runningWriteTask = false;
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

            Dictionary<DeviceType, UIElement> deviceScenarios = new Dictionary<DeviceType, UIElement>();
            deviceScenarios.Add(DeviceType.OsrFx2, GeneralScenario);


            Utilities.SetUpDeviceScenarios(deviceScenarios, DeviceScenarioContainer);

            // So we can reset future tasks
            ResetCancellationTokenSource();

            EventHandlerForDevice.Current.OnAppSuspendCallback = new SuspendingEventHandler(this.OnAppSuspension);

            // Reset the buttons if the app resumed and the device is reconnected
            EventHandlerForDevice.Current.OnDeviceConnected = new TypedEventHandler<EventHandlerForDevice, DeviceInformation>(this.OnDeviceConnected);

            WriteData();

            autoremotemode();

            StatusBlock.Text = "Please connect to the device first";

        }

        private async void autoremotemode()
        {
            DataRequest("RemoteMode");
            await ReadData();
            System.Threading.Thread.Sleep(500);
            autoReadMemoInfo();

        }

        private async void autoReadMemoInfo()
        {
           
            DataRequest("MemoryInfo");
            await ReadData();
            getNextBlock();
            StatusBlock.Text = "Device had successfully connected. you can now read the data.";

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ReadData();
            DataRequest("RemoteExit");
            
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
                    
                    if (!navigatedAway)
                    {
                        StatusBlock.Text = "Canceling task... Please wait...";
                    }
                }));
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

        int memoInfo = 1;
        // Read Record Message
        string recordNumber = "";

        #region store SPdata parameter
        string subjectID = "";
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

        #endregion

        #region Read data into format

        //========================= Read data==================================================

        private void storageString(string dataString)
        {
            if (dataString.Contains("VMMI"))
            {
                string submemoinfo = dataString.Substring(5, 3);
                memoInfo = Int32.Parse(submemoinfo);
                MemoInfordata.Text = submemoinfo;
                recordcount = memoInfo;
                RecordNum.Text = recordcount.ToString();
            }

            if (dataString.Contains("VMRR"))
            {
                recordNumber = dataString;
                if (recordNumber.Contains("VMRRDLT"))
                {
                    setdisplayBestResultToNull();
                    AllReadRecord.IsEnabled = true;
                    dataSave.IsEnabled = false;
                    StatusBlock.Text = recordNumber;
                }
            }

            if (dataString.Contains("A:"))
            {
                subjectInfo = dataString;
                subjectInfoText.Text = subjectInfo;
            }

            if (dataString.Contains("B:"))
            {
                sessionInfo = dataString;
                sessionInfoText.Text = sessionInfo;
                showSessionDateTime(sessionInfo);
            }

            if (dataString.Contains("C:"))
            {
                PredictedValues = dataString;
                PredictedValuesText.Text = PredictedValues;
            }

            if (dataString.Contains("D:"))
            {
                LLNValues = dataString;
                LLNValuesText.Text = LLNValues;
            }

            if (dataString.Contains("E:"))
            {
                ULNValues = dataString;
                ULNValuesText.Text = ULNValues;
            }
            
            if (dataString.Contains("F:"))
            {
                BestTestResults = dataString;
                BestTestResultsText.Text = BestTestResults;
                
                DisplayBestTestResault(BestTestResults);
            }

            if (dataString.Contains("G:"))
            {
                BestTestData = dataString;
                BestTestDataText.Text = BestTestData;
            }

            if (dataString.Contains("H:"))
            {
                PercentageofPredicted = dataString;
                PercentageofPredictedText.Text = PercentageofPredicted;
            }

            if (dataString.Contains("I:"))
            {
                PercentagePrePost = dataString;
                PercentagePrePostText.Text = PercentagePrePost;
            }

            if (dataString.Contains("J:"))
            {
                Z_score= dataString;
                Z_scoreText.Text = Z_score;
            }

            if (dataString.Contains("K:"))
            {
                PrePostChange = dataString;
                PrePostChangeText.Text = PrePostChange;
            }

            if (dataString.Contains("L:"))
            {
                RankedTestResult_1 = dataString;
                RankedTestResult_1Text.Text = RankedTestResult_1;
            }

            if (dataString.Contains("M:"))
            {
                RankedTestData_1 = dataString;
                RankedTestData_1Text.Text = RankedTestData_1;
            }

            if (dataString.Contains("N:"))
            {
                RankedTestResult_2 = dataString;
                RankedTestResult_2Text.Text = RankedTestResult_2;
            }

            if (dataString.Contains("O:"))
            {
                RankedTestData_2 = dataString;
                RankedTestData_2Text.Text = RankedTestData_2;
            }

            if (dataString.Contains("P:"))
            {
                RankedTestResult_3 = dataString;
                RankedTestResult_3Text.Text = RankedTestResult_3;
            }

            if (dataString.Contains("Q:"))
            {
                RankedTestData_3 = dataString;
                RankedTestData_3Text.Text = RankedTestData_3;
            }

            if (dataString.Contains("R:"))
            {
                PreBestTestResult = dataString;
                PreBestTestResultText.Text = PreBestTestResult;
            }

            if (dataString.Contains("S:"))
            {
                PreBestTestData = dataString;
                PreBestTestDataText.Text = PreBestTestData;
            }

            if (dataString.Contains("T:"))
            {
                PreBestPercentageofPredicted = dataString;
                PreBestPercentageofPredictedText.Text = PreBestPercentageofPredicted;
            }

            if (dataString.Contains("U:"))
            {
                PreBestZ_score = dataString;
                PreBestZ_scoreText.Text = PreBestZ_score;
            }

            if (dataString.Contains("V:"))
            {
                InterpretationInformation = dataString;
                InterpretationInformationText.Text = InterpretationInformation;
                AllReadRecord.IsEnabled = true;
                dataSave.IsEnabled = true;
                StatusBlock.Text = "Session data read completed";
            }
        }

        private void showSessionDateTime(string sessionInfo)
        {
            var sessionInfoArray = sessionInfo.Split(",");
            sessionDateTime.Text = sessionInfoArray[3];
        }

        private void setdisplayBestResultToNull()
        {
            VCtext.Text = "";
            EVC.Text = "";
            IVC.Text = "";
            FVC.Text = "";
            FIVC.Text = "";
            FIVC_FVC.Text = "";
            FEV05.Text = "";
            FEV05_FVC.Text = "";
            FEV075.Text = "";
            FEV075_FVC.Text = "";
            FEV1.Text = "";
            FEV1R.Text = "";
            FEV1_VC.Text = "";
            FEV1_EVC.Text = "";
            FEV1_IVC.Text = "";
            FEV1_FVC.Text = "";
            FEV1_FIVC.Text = "";
            FEV1_FEV6.Text = "";
            FEV1_PEF.Text = "";
            FEV3.Text = "";
            FEV3_VC.Text = "";
            FEV3_FVC.Text = "";
            FEV6.Text = "";
            PEV_l_s.Text = "";
            PEF_l_min.Text = "";
            FEF25.Text = "";
            FEF50.Text = "";
            FEF75.Text = "";
            FEF02_12.Text = "";
            FEF2575.Text = "";
            FEF2575_FVC.Text = "";
            FEF7586.Text = "";
            FIV1.Text = "";
            FIV1_FVC.Text = "";
            FIV1_FIVC.Text = "";
            PIF_l_S.Text = "";
            PIF_l_MIN.Text = "";
            FIF25.Text = "";
            FIF50.Text = "";
            FIF75.Text = "";
            FIF50FEF50.Text = "";
            FEF50FIF50.Text = "";
            MVVind.Text = "";
            FMFT.Text = "";
            FET.Text = "";
            FRC.Text = "";
            TV.Text = "";
            RV.Text = "";
            TLC.Text = "";
            IRV.Text = "";
            ERV.Text = "";
            IC.Text = "";
            Rind.Text = "";
            LungAge.Text = "";
            tPEF.Text = "";
            Tidal_PEF.Text = "";
            Text.Text = "";
            Vext.Text = "";
            Vext_FVC.Text = "";
        }

        private void DisplayBestTestResault(string result)
        {
            if (result != null && result != "")
            {
                var resultArray = result.Split(",");
                VCtext.Text = resultArray[0].Substring(3);
                EVC.Text = resultArray[1];
                IVC.Text = resultArray[2];
                FVC.Text = resultArray[3];
                FIVC.Text = resultArray[4];
                FIVC_FVC.Text = resultArray[5];
                FEV05.Text = resultArray[6];
                FEV05_FVC.Text = resultArray[7];
                FEV075.Text = resultArray[8];
                FEV075_FVC.Text = resultArray[9];
                FEV1.Text = resultArray[10];
                FEV1R.Text = resultArray[11];
                FEV1_VC.Text = resultArray[12];
                FEV1_EVC.Text = resultArray[13];
                FEV1_IVC.Text = resultArray[14];
                FEV1_FVC.Text = resultArray[15];
                FEV1_FIVC.Text = resultArray[16];
                FEV1_FEV6.Text = resultArray[17];
                FEV1_PEF.Text = resultArray[18];
                FEV3.Text = resultArray[19];
                FEV3_VC.Text = resultArray[20];
                FEV3_FVC.Text = resultArray[21];
                FEV6.Text = resultArray[22];
                PEV_l_s.Text = resultArray[23];
                PEF_l_min.Text = resultArray[24];
                FEF25.Text = resultArray[25];
                FEF50.Text = resultArray[26];
                FEF75.Text = resultArray[27];
                FEF02_12.Text = resultArray[28];
                FEF2575.Text = resultArray[29];
                FEF2575_FVC.Text = resultArray[30];
                FEF7586.Text = resultArray[31];
                FIV1.Text = resultArray[32];
                FIV1_FVC.Text = resultArray[33];
                FIV1_FIVC.Text = resultArray[34];
                PIF_l_S.Text = resultArray[35];
                PIF_l_MIN.Text = resultArray[36];
                FIF25.Text = resultArray[37];
                FIF50.Text = resultArray[38];
                FIF75.Text = resultArray[39];
                FIF50FEF50.Text = resultArray[40];
                FEF50FIF50.Text = resultArray[41];
                MVVind.Text = resultArray[42];
                FMFT.Text = resultArray[43];
                FET.Text = resultArray[44];
                FRC.Text = resultArray[45];
                TV.Text = resultArray[46];
                RV.Text = resultArray[47];
                TLC.Text = resultArray[48];
                IRV.Text = resultArray[49];
                ERV.Text = resultArray[50];
                IC.Text = resultArray[51];
                Rind.Text = resultArray[52];
                LungAge.Text = resultArray[53];
                tPEF.Text = resultArray[54];
                Tidal_PEF.Text = resultArray[55];
                Text.Text = resultArray[56];
                Vext.Text = resultArray[57];
                Vext_FVC.Text = resultArray[58];

            }
          
        }

        #endregion

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


        #region data collected and store 

        //==============================Data Collect================================================================

        private string getsubjectID()
        {

            var subjectArray = subjectInfo.Split(",");
            string subject = subjectArray[0];
            var subjectarray = subject.Split(":");

            var sessionArray = sessionInfo.Split(",");
            string session = sessionArray[3];

            string returnID = subjectarray[1] + session;

            return returnID;
        }


        //save data only if there is an data
        private void saveData_click(object sender, RoutedEventArgs e)
        {
            updatelocaldata();
           
        }

        
        string fileName = "SpirometerData.json";
        SpirometerData[] _data = Array.Empty<SpirometerData>();

        public async void updatelocaldata()
        {
            subjectID = getsubjectID();
       
            SpirometerData[] CreateData = new SpirometerData[]
            {
                new SpirometerData()
                {
                    //MemoInfo = this.memoInfo.ToString(),
                   // SessionNum = this.sessionInfo
                    //subjectID = getsubjectID();
                    subjectID = this.subjectID,
                    SubjectInfo = this.subjectInfo,
                    SessionInfo = this.sessionInfo,

                    PredictedValues = this.PredictedValues,
                    LLNValue = this.LLNValues,
                    ULNValue = this.ULNValues,
                    PrecentageOfPredicted = this.PercentageofPredicted,
                    PrecentageOfPrePost = this.PercentagePrePost,
                    Zscore = this.Z_score,
                    PrePostChange = this.PrePostChange,
                    PreBestTestResult = this.PreBestTestResult,
                    PreBestTestData = this.PreBestTestData,
                    PreBestPercentageofPredicted = this.PreBestPercentageofPredicted,
                    PreBestZscore = this.PreBestZ_score,

                    BestTestData = this.BestTestData,
                    RankedTestData1 = this.RankedTestData_1,
                    RankedTestData2 = this.RankedTestData_2,
                    RankedTestData3 = this.RankedTestData_3,
                    BestTestResults = this.BestTestResults,
                    RankedTestResult1 = this.RankedTestResult_1,
                    RankedTestResult2 = this.RankedTestResult_2,
                    RankedTestResult3 = this.RankedTestResult_3,
                    InterpretationInformation = this.InterpretationInformation
                    
                },
            };
            _data = CreateData;
            var folder = ApplicationData.Current.LocalFolder;
            var newfile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            var text = JsonConvert.SerializeObject(_data);
            await FileIO.WriteTextAsync(newfile, text);
        }

        public async void WriteData()
        {

            SpirometerData[] CreateData = new SpirometerData[]
            {
                new SpirometerData()
                {
                    subjectID = this.subjectID,
                    SubjectInfo = this.subjectInfo,
                    SessionInfo = this.sessionInfo,

                    PredictedValues = this.PredictedValues,
                    LLNValue = this.LLNValues,
                    ULNValue = this.ULNValues,
                    PrecentageOfPredicted = this.PercentageofPredicted,
                    PrecentageOfPrePost = this.PercentagePrePost,
                    Zscore = this.Z_score,
                    PrePostChange = this.PrePostChange,
                    PreBestTestResult = this.PreBestTestResult,
                    PreBestPercentageofPredicted = this.PreBestPercentageofPredicted,
                    PreBestZscore = this.PreBestZ_score,
                    PreBestTestData = this.PreBestTestData,
                    BestTestData = this.BestTestData,
                    RankedTestData1 = this.RankedTestData_1,
                    RankedTestData2 = this.RankedTestData_2,
                    RankedTestData3 = this.RankedTestData_3,
                    BestTestResults = this.BestTestResults,
                    RankedTestResult1 = this.RankedTestResult_1,
                    RankedTestResult2 = this.RankedTestResult_2,
                    RankedTestResult3 = this.RankedTestResult_3,
                    InterpretationInformation = this.InterpretationInformation
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
            else
            {
                var text = await FileIO.ReadTextAsync(file);
                _data = JsonConvert.DeserializeObject<SpirometerData[]>(text);
            }
        }

        #endregion

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

            //PrintTotalReadWriteBytes();
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

            //PrintTotalReadWriteBytes();
            await ReadData();
            
        }

        #endregion

        #region 6.3 Bluetooth Exit Remote Buttom Click

        private void RemoteExit_Click(object sender, RoutedEventArgs e)
        {
            DataRequest("RemoteExit");
            //ReadData();
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

            //PrintTotalReadWriteBytes();
             ReadData();
             
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

            //PrintTotalReadWriteBytes();

            //await ReadData();

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

           // PrintTotalReadWriteBytes();
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

        
           //PrintTotalReadWriteBytes();
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

            //PrintTotalReadWriteBytes();
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

            //PrintTotalReadWriteBytes();
        }
        #endregion



        #region 6.5. Read Record Message
        private async void ReadRecord_Click(object sender, RoutedEventArgs e)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Writing...";

                    runningWriteTask = true;

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


                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }

        #endregion


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
                    //StatusBlock.Text = "Reading...";

                    runningWriteTask = true;


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

                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }



        #region auto Read Record

        private async void ALLReadRecord_Click(object sender, RoutedEventArgs e)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Reading...";

                    AllReadRecord.IsEnabled = false;
                    dataSave.IsEnabled = false;

                    runningWriteTask = true;


                    UInt32 bulkOutPipeIndex = 0;

                    UInt32 bytesToWrite = 64;

                    await RRBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                    ReadData();
                }
                catch (OperationCanceledException /*ex*/)
                {
                    NotifyTaskCanceled();
                }
                finally
                {
                    runningWriteTask = false;
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
                    //StatusBlock.Text = "Writing...";

                    runningWriteTask = true;


                    UInt32 bulkOutPipeIndex = 0;
                    UInt32 bytesToWrite = 2;

                    await NEXTBulkWriteAsync(bulkOutPipeIndex, bytesToWrite, cancellationTokenSource.Token);
                    ReadData();
                }
                catch (OperationCanceledException /*ex*/)
                {
                    NotifyTaskCanceled();
                }
                finally
                {
                    runningWriteTask = false;


                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }

        private async Task ReadData()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {

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
            // TestData.Text = reader.ToString();
            Task<UInt32> loadAsyncTask;

            // Don't start any IO if we canceled the task
            lock (cancelIoLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                loadAsyncTask = reader.LoadAsync(bytesToRead).AsTask(cancellationToken);
            }

            UInt32 bytesRead = await loadAsyncTask;

            totalBytesRead += bytesRead;

            // PrintTotalReadWriteBytes();

            IBuffer buffer = reader.ReadBuffer(bytesRead);
            string dataString;
            byte[] data;
            byte endcode = 0x03;
            using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
            {
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

                //if datareader is equal to 0x03 then stop reading the data;



                dataString = dataReader.ReadString(buffer.Length);
                
                // DataDisplay.Text = dataString;


                CryptographicBuffer.CopyToByteArray(buffer, out data);
                
            }

            storageString(dataString);

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == endcode)
                {
                    getNextBlock();

                }
            }
        }


        #endregion



        private byte findbyte(int unit)
        {
            byte intToHexbyte = 0x30;

            switch (unit)
            {
                case 1:
                    intToHexbyte = 0x31;
                    break;
                case 2:
                    intToHexbyte = 0x32;
                    break;
                case 3:
                    intToHexbyte = 0x33;
                    break;
                case 4:
                    intToHexbyte = 0x34;
                    break;
                case 5:
                    intToHexbyte = 0x35;
                    break;
                case 6:
                    intToHexbyte = 0x36;
                    break;
                case 7:
                    intToHexbyte = 0x37;
                    break;
                case 8:
                    intToHexbyte = 0x38;
                    break;
                case 9:
                    intToHexbyte = 0x39;
                    break;
                default:
                    intToHexbyte = 0x30;
                    break;
            }

            return intToHexbyte;
        }

        private async Task RRBulkWriteAsync(UInt32 bulkPipeIndex, UInt32 bytesToWrite, CancellationToken cancellationToken)
        {
            byte[] value = new byte[] { 0x02, 0x4d, 0x56, 0x52, 0x52, 0x30, 0x33, 0x38, 0x03, 0x01 };

            // byte[] num = checkeachrecord();

            //int count = num.Length -1;

            int unit = recordcount % 10;
            int getten = recordcount / 10;
            int gethundred;
            if (recordcount / 100 > 1)
            {
                gethundred = recordcount / 100;
            }
            else
            {
                gethundred = 0;
            }
            
            byte unitbyte = findbyte(unit);
            byte tenbyte = findbyte(getten);
            byte hundredbyte = findbyte(gethundred);


            value[7] = unitbyte;
            value[6] = tenbyte;
            value[5] = hundredbyte;


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

           // PrintTotalReadWriteBytes();
        }

     



        #region Next Block

        private async void NextBlock_Click(object sender, RoutedEventArgs e)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    StatusBlock.Text = "Writing...";

                    runningWriteTask = true;
                

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

            //PrintTotalReadWriteBytes();
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

        private void NextRecord_click(object sender, RoutedEventArgs e)
        {
            if (recordcount < memoInfo)
            {
                recordcount += 1;
                RecordNum.Text = recordcount.ToString();
        
            }
            
        }

        private void PreRecord_click(object sender, RoutedEventArgs e)
        {
            if (recordcount > 1)
            {
                recordcount -= 1;
                RecordNum.Text = recordcount.ToString();
             
            }
                
        }

       
    }
}