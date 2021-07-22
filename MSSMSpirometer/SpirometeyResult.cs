using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSMSpirometer
{
    public class SpirometeyResult
    {
        public string subjectID { get; set; }

        public float PEF { get; set; }

        public float FEV1 { get; set; }

        public float FEV6 { get; set; }

        public float FEV1FEV6 { get; set; }

        public float FVC { get; set; }

        public float flowData { get; set; }

        public float volumeData { get; set; }


    }
}
