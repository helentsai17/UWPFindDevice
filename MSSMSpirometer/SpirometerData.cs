using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSMSpirometer
{
    class SpirometerData
    {
        // public string MemoInfo { get; set; }

        //public string SessionNum { get; set; }
        public string subjectID { get; set; }
        public string SessionInfo { get; set; }
        public string SubjectInfo { get; set; }
       

        public string PredictedValues { get; set; }
        public string LLNValue { get; set; }
        public string ULNValue { get; set; }

        public string BestTestResults { get; set; }

       
        public string PrecentageOfPredicted { get; set; }
        public string PrecentageOfPrePost { get; set; }
        public string Zscore { get; set; }
        public string PrePostChange { get; set; }
        public string PreBestTestResult { get; set; }
        public string PreBestTestData { get; set; }
        public string PreBestPercentageofPredicted { get; set; }
        public string PreBestZscore { get; set; }

        public string InterpretationInformation { get; set; }
        public string RankedTestResult1 { get; set; }
        public string RankedTestResult2 { get; set; }
        public string RankedTestResult3 { get; set; }

        public string BestTestData { get; set; }
        public string RankedTestData1 { get; set; }
        public string RankedTestData2 { get; set; }
        public string RankedTestData3 { get; set; }
    }


}
