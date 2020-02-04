using System;

namespace Familia.Devices.Models {
    public class GlucoseRecord {
	    public int FlagHilow { get; set; }
	    public int? GlucoseData { get; set; }
	    public DateTime DateTimeRecord { get; set; }
    }
}
