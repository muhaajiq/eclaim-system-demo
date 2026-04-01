using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Entities.ViewModel.RunningNumber
{
    public class RunningNumberFormat
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public string Format { get; set; }

        public string Prefix { get; set; }

        public bool Autonumber { get; set; }
    }
}
