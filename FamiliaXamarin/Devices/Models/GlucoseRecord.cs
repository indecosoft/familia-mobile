using System;
namespace Familia.Devices.Models {
    public class GlucoseRecord {
		public int flag_context = 0;
		public int flag_cs = 0;
		public int flag_fasting = 0;
		public int flag_hilow = 0;
		public int flag_ketone = 0;
		public int flag_meal = 0;
		public int flag_nomark = 0;
		public float glucoseData = 0.0f;
		public int sequenceNumber = 0;
		public long time = 0;
        public DateTime DateTimeRecord;
		public string time_iso8601 = "";
		public int timeoffset = 0;
	}
}
