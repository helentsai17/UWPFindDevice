using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

        string subjectid;
        string subjectInfomation;
        string sessionInformation;

        string PredictedValue;
        string LLN;
        string ULN;

        string bestTestResults;
        string precentageOfPredicted;
        string precentageOfPrePost;
        string zscore;
        string prePostChange;
        string preBestTestResult;
        string preBestTestData;
        string preBestPercentageofPredicted;
        string preBestZscore;
        string interpretationInformation;
        string bestTestData;

        string rankTestResult1;
        string rankTestResult2;
        string rankTestResult3;

        string RankedTestData1;
        string RankedTestData2; 
        string RankedTestData3;




        string dbsubjectID;
        float dbPEF;
        float dbFEV1;
        float dbFEV6;
        float dbFEV1FEV6;
        float dbFVC;
        float dbflowData;
        float dbvolumeData;


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

            subjectid = getdatainfo.subjectID;
    
            PredictedValue = getdatainfo.PredictedValues;
            bestTestResults = getdatainfo.BestTestResults;
            rankTestResult1 = getdatainfo.RankedTestResult1;
            rankTestResult2 = getdatainfo.RankedTestResult2;
            rankTestResult3 = getdatainfo.RankedTestResult3;
            subjectInfomation = getdatainfo.SubjectInfo;
            sessionInformation = getdatainfo.SessionInfo;

            precentageOfPredicted = getdatainfo.PrecentageOfPredicted;
            precentageOfPrePost = getdatainfo.PrecentageOfPrePost;
            zscore = getdatainfo.Zscore;
            prePostChange = getdatainfo.PrePostChange;
            preBestTestResult = getdatainfo.PreBestTestResult;
            preBestTestData = getdatainfo.PreBestTestData;
            preBestPercentageofPredicted = getdatainfo.PreBestPercentageofPredicted;
            preBestZscore = getdatainfo.PreBestZscore;
            interpretationInformation = getdatainfo.InterpretationInformation;
            bestTestData = getdatainfo.BestTestData;

            RankedTestData1 = getdatainfo.RankedTestData1;
            RankedTestData2 = getdatainfo.RankedTestData2;
            RankedTestData3 = getdatainfo.RankedTestData3;

            LLN = getdatainfo.LLNValue;
            ULN = getdatainfo.ULNValue;

            sessionInfoFormat(sessionInformation, subjectInfomation);

            subjectFormat(subjectInfomation);

            testResultFormat(bestTestResults);
        }

        string subjectinfo;

        private void subjectFormat(string subjectInfomation)
        {

            if(subjectInfomation != "" && subjectInfomation != null)
            {
                var subject = subjectInfomation.Split(",");

                if(subject[1] == "")
                {
                    subjectinfo = "no data";
                }

                DateofBirth.Text = subject[1];
                Gender.Text = subject[2];
                Age.Text = subject[3];
                HeightText.Text = subject[4];
                Weight.Text = subject[5];
                SomkingHistory.Text = subject[6];
                PopulationGroup.Text = subject[7];
                RegressionSet.Text = subject[8];
                CorrectionFactor.Text = subject[9];
            }
            
            
        }

        private void sessionInfoFormat(string sessionInformation, string subjectinfo)
        {
            if(sessionInformation!= null && sessionInformation != "")
            {
                var subject = subjectinfo.Split(",");

                SubjectID.Text = subject[0].Substring(3);



                var session = sessionInformation.Split(",");
                AccuracyDataTime.Text = session[0].Substring(3);
                CalibrationDateTime.Text = session[1];
                SessionDataTime.Text = session[3];
                SessionType.Text = session[8];
                SessionPosture.Text = session[9];
                SessionGrade.Text = session[10];
                PreSessionGrade.Text = session[11];
                NumberofBlowsPerformed.Text = session[12];
                FEV1Repeatability.Text = session[13];
                FEV1RepeatabilityP.Text = session[14] + " %";
                PEFRepeatability.Text = session[15];
                PEFRepeatabilityP.Text = session[16] + " %";
                FVCRepeatability.Text = session[17];
                FVCRepeatabilityP.Text = session[18] + " %";

                dbPEF = float.Parse(session[15]);

                dbsubjectID = subject[0].Substring(3) + session[0].Substring(3);

            }
        }

        private void testResultFormat(string result)
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

          
                 dbFEV1 = float.Parse(resultArray[10]);

                if(resultArray[22]!= "")
                {
                    dbFEV6 = float.Parse(resultArray[22]);
                }

                if (resultArray[17] != "")
                {
                    dbFEV1FEV6 = float.Parse(resultArray[17]);
                }

                if (resultArray[3] != "")
                {
                    dbFVC = float.Parse(resultArray[3]);
                }

                   
                 dbflowData = 0;
                 dbvolumeData = 0;


            }
        }

        private void setTozero()
        {    
            EVC.Text = 0.ToString();
            IVC.Text = 0.ToString();
            FVC.Text = 0.ToString();
            FIVC.Text = 0.ToString();
            FIVC_FVC.Text = 0.ToString();
            FEV05.Text = 0.ToString();
            FEV05_FVC.Text = 0.ToString();
            FEV075.Text = 0.ToString();
            FEV075_FVC.Text = 0.ToString();
            FEV1.Text = 0.ToString();
            FEV1R.Text = 0.ToString();
            FEV1_VC.Text = 0.ToString();
            FEV1_EVC.Text = 0.ToString();
            FEV1_IVC.Text = 0.ToString();
            FEV1_FVC.Text = 0.ToString();
            FEV1_FIVC.Text = 0.ToString();
            FEV1_FEV6.Text = 0.ToString();
            FEV1_PEF.Text = 0.ToString();
            FEV3.Text = 0.ToString();
            FEV3_VC.Text = 0.ToString();
            FEV3_FVC.Text = 0.ToString();
            FEV6.Text = 0.ToString();
            PEV_l_s.Text = 0.ToString();
            PEF_l_min.Text = 0.ToString();
            FEF25.Text = 0.ToString();
            FEF50.Text = 0.ToString();
            FEF75.Text = 0.ToString();
            FEF02_12.Text = 0.ToString();
            FEF2575.Text = 0.ToString();
            FEF2575_FVC.Text = 0.ToString();
            FEF7586.Text = 0.ToString();
            FIV1.Text = 0.ToString();
            FIV1_FVC.Text = 0.ToString();
            FIV1_FIVC.Text = 0.ToString();
            PIF_l_S.Text = 0.ToString();
            PIF_l_MIN.Text = 0.ToString();
            FIF25.Text = 0.ToString();
            FIF50.Text = 0.ToString();
            FIF75.Text = 0.ToString();
            FIF50FEF50.Text = 0.ToString();
            FEF50FIF50.Text = 0.ToString();
            MVVind.Text = 0.ToString();
            FMFT.Text = 0.ToString();
            FET.Text = 0.ToString();
            FRC.Text = 0.ToString();
            TV.Text = 0.ToString();
            RV.Text = 0.ToString();
            TLC.Text = 0.ToString();
            IRV.Text = 0.ToString();
            ERV.Text = 0.ToString();
            IC.Text = 0.ToString();
            Rind.Text = 0.ToString();
            LungAge.Text = 0.ToString();
            tPEF.Text = 0.ToString();
            Tidal_PEF.Text = 0.ToString();
            Text.Text = 0.ToString();
            Vext.Text = 0.ToString();
            Vext_FVC.Text = 0.ToString();
        }

        private void BestResult_click(object sender, RoutedEventArgs e)
        {
            testResultFormat(bestTestResults);
        }

        private void RankTest1_click(object sender, RoutedEventArgs e)
        {
            testResultFormat(rankTestResult1);
        }

        private void RankTest2_click(object sender, RoutedEventArgs e)
        {
            testResultFormat(rankTestResult2);
        }

        private void RankTest3_click(object sender, RoutedEventArgs e)
        {
            testResultFormat(rankTestResult3);
        }

        private void LLN_click(object sender, RoutedEventArgs e)
        {
            
            if (subjectinfo != "no data")
            {
                testResultFormat(LLN);
            }
            else
            {
                VCtext.Text = subjectinfo;
                setTozero();
            }
        }

        private void ULN_click(object sender, RoutedEventArgs e)
        {
            

            if (subjectinfo != "no data")
            {
                testResultFormat(ULN);
            }
            else
            {
                VCtext.Text = subjectinfo;
                setTozero();
            }
        }

        private void PredictedValue_click(object sender, RoutedEventArgs e)
        {

        if (subjectinfo != "no data")
            {
                testResultFormat(PredictedValue);
            }
        else
        {
            VCtext.Text = subjectinfo;
            setTozero();
        }
    }

        private void to_database(object sender, RoutedEventArgs e)
        {

            string M_str_sqlcon = "server=localhost;UID=root;password=Pupu1990;database=SpirometeyResult";
            MySqlConnection mysqlcon = new MySqlConnection(M_str_sqlcon);
            string lifeQuery = @"insert into SPdata values( @subjectid, @SubjectInfo, @SessionInfo, @PredictedValues, @LLNValues, @ULNValues, @BestTestResults,@BestTestData, @PrecentageOfPredicted,@PrecentageOfPrePost,@Zscore,@PrePostChange,@RankedTestResult1,@RankedTestData1,@RankedTestResult2,@RankedTestData2,@RankedTestResult3,@RankedTestData3,@PreBestTestResult,@PreBestTestData,@PreBestPercentageofPredicted, @PreBestZscore, @InterpretationInformation)";
            MySqlCommand mysqlcom = new MySqlCommand(lifeQuery, mysqlcon);
            mysqlcom.Parameters.AddWithValue("@subjectid", subjectid);
            mysqlcom.Parameters.AddWithValue("@SubjectInfo", subjectInfomation);
            mysqlcom.Parameters.AddWithValue("@SessionInfo", sessionInformation);
            mysqlcom.Parameters.AddWithValue("@PredictedValues", PredictedValue);
            mysqlcom.Parameters.AddWithValue("@LLNValues", LLN);
            mysqlcom.Parameters.AddWithValue("@ULNValues", ULN);
            mysqlcom.Parameters.AddWithValue("@BestTestResults", bestTestResults);
            mysqlcom.Parameters.AddWithValue("@BestTestData", bestTestData);
            mysqlcom.Parameters.AddWithValue("@PrecentageOfPredicted", precentageOfPredicted);
            mysqlcom.Parameters.AddWithValue("@PrecentageOfPrePost", precentageOfPrePost);

            mysqlcom.Parameters.AddWithValue("@Zscore", zscore);
            mysqlcom.Parameters.AddWithValue("@PrePostChange", prePostChange);
            mysqlcom.Parameters.AddWithValue("@RankedTestResult1", rankTestResult1);
            mysqlcom.Parameters.AddWithValue("@RankedTestData1", RankedTestData1);
            mysqlcom.Parameters.AddWithValue("@RankedTestResult2", rankTestResult2);
            mysqlcom.Parameters.AddWithValue("@RankedTestData2", RankedTestData2);
            mysqlcom.Parameters.AddWithValue("@RankedTestResult3", rankTestResult3);
            mysqlcom.Parameters.AddWithValue("@RankedTestData3", RankedTestData3);
            mysqlcom.Parameters.AddWithValue("@PreBestTestResult", preBestTestResult);
            mysqlcom.Parameters.AddWithValue("@PreBestTestData", preBestTestData);
            mysqlcom.Parameters.AddWithValue("@PreBestPercentageofPredicted", preBestPercentageofPredicted);
            mysqlcom.Parameters.AddWithValue("@PreBestZscore", preBestZscore);
            mysqlcom.Parameters.AddWithValue("@InterpretationInformation", interpretationInformation);

            mysqlcon.Open();
            mysqlcom.ExecuteNonQuery();
            //mysqlcon.Close();


            //string M_str_sqlcon = "server=localhost;UID=root;password=Pupu1990;database=SpirometeyResult";
            ////string M_str_sqlcon = "server=146.203.150.198;UID=helen;password=password;database=SpirometeyResult";
            //MySqlConnection mysqlcon = new MySqlConnection(M_str_sqlcon);

            //string lifeQuery = @"insert into ResultData values( @dbsubjectID, @dbPEF, @dbFEV1, @dbFEV6, @dbFEV1FEV6, @dbFVC, @dbflowData, @dbvolumeData)";
            //MySqlCommand mysqlcom = new MySqlCommand(lifeQuery, mysqlcon);
            //mysqlcom.Parameters.AddWithValue("@dbsubjectID", dbsubjectID);
            //mysqlcom.Parameters.AddWithValue("@dbPEF", dbPEF);
            //mysqlcom.Parameters.AddWithValue("@dbFEV1", dbFEV1);
            //mysqlcom.Parameters.AddWithValue("@dbFEV6", dbFEV6);
            //mysqlcom.Parameters.AddWithValue("@dbFEV1FEV6", dbFEV1FEV6);
            //mysqlcom.Parameters.AddWithValue("@dbFVC", dbFVC);
            //mysqlcom.Parameters.AddWithValue("@dbflowData", dbflowData);
            //mysqlcom.Parameters.AddWithValue("@dbvolumeData", dbvolumeData);

            //mysqlcon.Open();

            //mysqlcom.ExecuteNonQuery();

            //mysqlcon.Close();



        }
    }



}


