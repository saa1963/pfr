using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pfr
{
    public class clsProtokol: IComparable<clsProtokol>
    {
        public string Doffice { get; set; }
        public string Acc { get; set; }
        public decimal Sm { get; set; }

        public int CompareTo(clsProtokol other)
        {
            return (Doffice + Acc).CompareTo(other.Doffice + other.Acc);
        }
    }
}
