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

    }

    public class Inspiratory
    {
        public int Time { get; set; }
        public int Icurve { get; set; }
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

            //OutputMemoInfo.Text = getdatainfo.MemoInfo;
            BestTestData = getdatainfo.BestTestData;
            

            dataFormat(BestTestData);
        }

        private void dataFormat(string bestTestData)
        {
            if(bestTestData != null && bestTestData != "")
            {
                var dataformat = bestTestData.Split(",");
                //dataoftest.Text = dataformat[0];
                //testindex.Text = dataformat[1];
                //testrank.Text = dataformat[2];
                //testqualityflags.Text = dataformat[3];
                //acceptabilityflags.Text = dataformat[4];
                //fetg.Text = dataformat[5];
                //fvcg.Text = dataformat[6];
                expiratorycurve.Text = dataformat[7];
                inspiratorytime.Text = dataformat[8];
                fivcg.Text = dataformat[9];
                inspiratorycurve.Text = dataformat[10];

                ECurveData = dataformat[7];
                
                ICurveData = dataformat[10];

                LoadChartContents();
            }
           
           
        }



        private void LoadChartContents()
        {
            List<Expiratory> exiratory = new List<Expiratory>();
            string[] ecurveArry;
            if (ECurveData != null && ECurveData != "")
            {
                ecurveArry = ECurveData.Split(":");
                
                for (int i = 0; i < ecurveArry.Length - 1; i++)
                {
                    exiratory.Add(new Expiratory() { Time = i, Ecurve = int.Parse(ecurveArry[i]) });
                }
            }

            List<Inspiratory> inspiratory = new List<Inspiratory>();
            string[] IcurveArry;
            if (ICurveData != null && ICurveData != "")
            {
                IcurveArry = ICurveData.Split(":");
                //fvcg.Text = IcurveArry[2].ToString();
                for (int i = 0; i < IcurveArry.Length - 1; i++)
                {
                    inspiratory.Add(new Inspiratory() { Time = i, Icurve = int.Parse(IcurveArry[i]) });
                }
            }

            (LineChart.Series[0] as LineSeries).ItemsSource = exiratory;
            (LineChartCurve.Series[0] as LineSeries).ItemsSource = inspiratory; 
        }

        private void Page_Load(FrameworkElement sender, object args)
        {
            LoadChartContents();
        }
    }
}
