﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class DataStorage : Page
    {
        public DataStorage()
        {
            this.InitializeComponent();
        }

        string fileName = "SpirometerData.json";
        SpirometerData[] _data = Array.Empty<SpirometerData>();

        string bestTestResult;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.TryGetItemAsync(fileName) as IStorageFile;

            if (file != null)
            {
                var text = await FileIO.ReadTextAsync(file);
                _data = JsonConvert.DeserializeObject<SpirometerData[]>(text);
                ShowUser();
            }
        }

        private void ShowUser()
        {
            var getdatainfo = _data[0];
            
            OutputMemoInfo.Text = getdatainfo.MemoInfo;

            bestTestResult = getdatainfo.BestTestResults;
            BestTestResultText.Text = bestTestResult;
            
            testResultFormat(bestTestResult);
        }

        private void testResultFormat(string result)
        {
            if (result != null)
            {
                var resultArray = result.Split(",");
                VCtext.Text = resultArray[0];
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

    }



}
