using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Object = System.Object;

namespace FamiliaXamarin.Medicatie.Entities
{
    public class Medicament
    {
        public string Name { get; set; }
        public string IdMed { get; set; }
        public List<Hour> Hours { get; set; }
        public int IntervalZi { get; set; }
        public int NrZile { get; set; }
        public string Date { get; set; }
        public List<int> Alarms { get; set; }

        public Medicament()
        {
            this.Hours = new List<Hour>();
        }

        public Medicament(string name)
        {
            this.Name = name;
            this.Hours = new List<Hour>();
        }

        public int FindAlarmById(int id)
        {
            if (Alarms != null)
            {
                foreach (int alarm in Alarms)
                {
                    if (alarm == id)
                        return alarm;
                }
            }
             return 0;
        }

        public override bool Equals(Object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj.GetType() !=typeof(Medicament)) {
                return false;
            }

            Medicament m = (Medicament)obj;

            return this.Name.Equals(m.Name);
        }
    }
}