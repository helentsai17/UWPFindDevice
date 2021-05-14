using Newtonsoft.Json;
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
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MSSMSpirometer
{
    public class Expiratory
    {
        public int Time { get; set; }
        public int Ecurve { get; set; }
        public int Ecurve1 { get; set; }
        public int Ecurve2 { get; set; }
        public int Ecurve3 { get; set; }
    }

    public class Inspiratory
    {
        public int Time { get; set; }
        public int Icurve { get; set; }
        public int Icurve1 { get; set; }
        public int Icurve2 { get; set; }
        public int Icurve3{ get; set; }
    }

    public sealed partial class DataGraph : Page
    {
        public DataGraph()
        {
            this.InitializeComponent();
        }

        string fileName = "SpirometerData.json";
        SpirometerData[] _data = Array.Empty<SpirometerData>();

        string BestTestData;
        string ECurveData;
        string ICurveData;

        string RankData1;
        string ECurveData1;
        string ICurveData1;
        string RankData2;
        string ECurveData2;
        string ICurveData2;
        string RankData3;
        string ECurveData3;
        string ICurveData3;
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

            BestTestData = getdatainfo.BestTestData;
            RankData1 = getdatainfo.RankData_1;
            RankData2 = getdatainfo.RankData_2;
            RankData3 = getdatainfo.RankData_3;

            dataFormat(BestTestData,RankData1,RankData2,RankData3);
        }

        private void dataFormat(string bestTestData, string rankData1, string rankData2, string rankData3)
        {
            if(bestTestData != null && bestTestData != "")
            {
                var dataformat = bestTestData.Split(",");
               // dataoftest.Text = dataformat[0];
                //testindex.Text = dataformat[1];
                //testrank.Text = dataformat[2];
                //testqualityflags.Text = dataformat[3];
                //acceptabilityflags.Text = dataformat[4];
                //fetg.Text = dataformat[5];
                //fvcg.Text = dataformat[6];
               // expiratorycurve.Text = dataformat[7];
                //inspiratorytime.Text = dataformat[8];
                //fivcg.Text = dataformat[9];
                inspiratorycurve.Text = dataformat[10];
                ECurveData = dataformat[7];
                ICurveData = dataformat[10];
                
            }

            if(rankData1 != null && rankData1 != "")
            {
                var rankdata1Array = rankData1.Split(",");
                
                ECurveData1 = rankdata1Array[7];
                ICurveData1 = rankdata1Array[10];
                
                
            }

            if (rankData2 != null && rankData2 != "")
            {
                var rankdata2Array = rankData2.Split(",");
                dataoftest.Text = rankdata2Array[0];
                testindex.Text = rankdata2Array[1];
                testrank.Text = rankdata2Array[2];
                testqualityflags.Text = rankdata2Array[3];
                acceptabilityflags.Text = rankdata2Array[4];
                fetg.Text = rankdata2Array[5];
                fvcg.Text = rankdata2Array[6];
                expiratorycurve.Text = rankdata2Array[7];
                inspiratorytime.Text = rankdata2Array[8];
                fivcg.Text = rankdata2Array[9];
                inspiratorycurve.Text = rankdata2Array[10];
                ECurveData2 = rankdata2Array[7];
                ICurveData2 = rankdata2Array[10];
            }

            if (rankData3 != null && rankData3 != "")
            {
                var rankdata3Array = rankData3.Split(",");
                ECurveData3 = rankdata3Array[7];
                ICurveData3 = rankdata3Array[10];
            }
            LoadChartContents();

        }



        private void LoadChartContents()
        {
            List<Expiratory> exiratory = new List<Expiratory>();
            string[] ecurveArry;
            string[] ecurveArry1;
            string[] ecurveArry2;
            string[] ecurveArry3;
            if (ECurveData != null && ECurveData != "")
            {
                ecurveArry = ECurveData.Split(":");
                ecurveArry1 = ECurveData1.Split(":");
                ecurveArry2 = ECurveData2.Split(":");
                ecurveArry3 = ECurveData3.Split(":");

                for (int i = 0; i < ecurveArry.Length - 1; i++)
                {
                    exiratory.Add(new Expiratory() { Time = i, Ecurve = int.Parse(ecurveArry[i]), Ecurve1 = int.Parse(ecurveArry1[i]) , Ecurve2 = int.Parse(ecurveArry2[i]) , Ecurve3 = int.Parse(ecurveArry3[i]) });
                }
            }

            List<Inspiratory> inspiratory = new List<Inspiratory>();
            string[] IcurveArry;
            string[] IcurveArry1;
            string[] IcurveArry2;
            string[] IcurveArry3;

            if (ICurveData != null && ICurveData != "")
            {
                IcurveArry = ICurveData.Split(":");
                IcurveArry1 = ICurveData1.Split(":");
                IcurveArry2 = ICurveData2.Split(":");
                IcurveArry3 = ICurveData3.Split(":");

                for (int i = 0; i < IcurveArry.Length - 1; i++)
                {
                    inspiratory.Add(new Inspiratory() { Time = i, Icurve = int.Parse(IcurveArry[i]), Icurve1 = int.Parse(IcurveArry1[i]), Icurve2 = int.Parse(IcurveArry2[i]), Icurve3 = int.Parse(IcurveArry3[i]) });
                }
            }

            (LineChart.Series[1] as LineSeries).ItemsSource = exiratory;
            (LineChart.Series[2] as LineSeries).ItemsSource = exiratory;
            (LineChart.Series[3] as LineSeries).ItemsSource = exiratory;
            (LineChart.Series[0] as LineSeries).ItemsSource = exiratory;
           


            (LineChartCurve.Series[0] as LineSeries).ItemsSource = inspiratory;
            (LineChartCurve.Series[1] as LineSeries).ItemsSource = inspiratory;
            (LineChartCurve.Series[2] as LineSeries).ItemsSource = inspiratory;
            (LineChartCurve.Series[3] as LineSeries).ItemsSource = inspiratory;
        }

        private void Page_Load(FrameworkElement sender, object args)
        {
            LoadChartContents();
        }
    }
}
