using System;

namespace Familia.Location
{
	public class LocationEventArgs : EventArgs
	{
		public Android.Locations.Location Location { get; set; }
	}
}
