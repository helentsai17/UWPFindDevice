using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSMSpirometer
{
    class SpirometerData
    {
        public string MemoInfo { get; set; }

        //public string SessionNum { get; set; }
        public string SubjectInfo { get; set; }
        public string SessionInfo { get; set; }

        public string PredictedValues { get; set; }
        public string LLNValue { get; set; }
        public string ULNValue { get; set; }

        public string BestTestResults { get; set; }
        public string PercentageofPredicted { get; set; }
        public string PercentagePrePost { get; set; }
        public string Zscore { get; set; }
        public string PrePostChange { get; set; }
        public string PreBestTestResult { get; set; }
        public string PreBestPercentageofPredicted { get; set; }
        public string PreBestZscore { get; set; }


        public string RankResults_1 { get; set; }
        public string RankResults_2 { get; set; }
        public string RankResults_3 { get; set; }
    }
}
