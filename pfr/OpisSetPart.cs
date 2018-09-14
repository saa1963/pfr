using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace pfr
{
    public partial class OpisSet: INotifyPropertyChanged
    {
        public int KolObrab1
        {
            get => KolObrab;
            set
            {
                KolObrab = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("KolObrab1"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
