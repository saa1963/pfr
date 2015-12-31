using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pfr
{
    public partial class TrnSet
    {
        public string Fio 
        {
            get { return Fam + " " + Imya + " " + Otch; }
        }
    }
}
