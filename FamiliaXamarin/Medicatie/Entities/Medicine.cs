using System;
using System.Collections.Generic;
using Java.Util;

namespace Familia.Medicatie.Entities
{
    public class Medicine
    {
        public string Name { get; set; }
        public string IdMed { get; set; }
        public List<Hour> Hours { get; set; }
        public int IntervalOfDay { get; set; }
        public int NumberOfDays { get; set; }
        public DateTime Date { get; set; }
        public List<int> Alarms { get; set; }

        public Calendar FinishCalendar { get; set; }

        public Medicine()
        {
            Hours = new List<Hour>();
        }

        public Medicine(string name)
        {
            Name = name;
            Hours = new List<Hour>();
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

        public Hour FindHourById(string mId)
        {
            foreach (Hour hour in Hours)
            {
                if (hour.Id.Equals(mId))
                {
                    return hour;
                }

            }

            return null;
        }

        public override bool Equals(Object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj?.GetType() != typeof(Medicine))
            {
                return false;
            }

            var m = (Medicine)obj;

            return Name.Equals(m.Name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}