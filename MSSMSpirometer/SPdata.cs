using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSMSpirometer
{
    class SPdata
    {
        private string subjectID;
        public SPdata()
        {
        }

        public SPdata(string subjectID, string subjectInfo, string sessionInfo, string predictedValues, string lLNValue, string uLNValue, string bestTestResults, string bestTestData, string precentageOfPredicted, string precentageOfPrePost, string zscore, string prePostChange, string rankedTestResult1, string rankedTestData1, string rankedTestResult2, string rankedTestData2, string rankedTestResult3, string rankedTestData3, string preBestTestResult, string preBestTestData, string preBestPercentageofPredicted, string preBestZscore, string interpretationInformation)
        {
            this.subjectID = subjectID;
            SubjectInfo = subjectInfo;
            SessionInfo = sessionInfo;
            PredictedValues = predictedValues;
            LLNValue = lLNValue;
            ULNValue = uLNValue;
            BestTestResults = bestTestResults;
            BestTestData = bestTestData;
            PrecentageOfPredicted = precentageOfPredicted;
            PrecentageOfPrePost = precentageOfPrePost;
            Zscore = zscore;
            PrePostChange = prePostChange;
            RankedTestResult1 = rankedTestResult1;
            RankedTestData1 = rankedTestData1;
            RankedTestResult2 = rankedTestResult2;
            RankedTestData2 = rankedTestData2;
            RankedTestResult3 = rankedTestResult3;
            RankedTestData3 = rankedTestData3;
            PreBestTestResult = preBestTestResult;
            PreBestTestData = preBestTestData;
            PreBestPercentageofPredicted = preBestPercentageofPredicted;
            PreBestZscore = preBestZscore;
            InterpretationInformation = interpretationInformation;
        }


        public string SubjectID
        {
            get => subjectID;
            set => subjectID = value;
        }

        private string SubjectInfo { get; set; }
        private string SessionInfo { get; set; }

        private string PredictedValues { get; set; }
        private string LLNValue { get; set; }
        private string ULNValue { get; set; }

        private string BestTestResults { get; set; }
        private string BestTestData { get; set; }

        private string PrecentageOfPredicted { get; set; }
        private string PrecentageOfPrePost { get; set; }
        private string Zscore { get; set; }
        private string PrePostChange { get; set; }

        private string RankedTestResult1 { get; set; }
        private string RankedTestData1 { get; set; }

        private string RankedTestResult2 { get; set; }
        private string RankedTestData2 { get; set; }

        private string RankedTestResult3 { get; set; }
        private string RankedTestData3 { get; set; }

        private string PreBestTestResult { get; set; }
        private string PreBestTestData { get; set; }

        private string PreBestPercentageofPredicted { get; set; }
        private string PreBestZscore { get; set; }
        private string InterpretationInformation { get; set; }

    }
}
