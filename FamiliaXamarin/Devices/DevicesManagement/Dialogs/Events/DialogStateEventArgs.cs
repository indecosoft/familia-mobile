using System;
using Familia.Devices.DevicesManagement.Dialogs.Helpers;
using Familia.Devices.Helpers;

namespace Familia.Devices.DevicesManagement.Dialogs.Events {
	public class DialogStateEventArgs : EventArgs {
		public DialogStatuses Status { get; set; }
		public DeviceType DeviceType { get; set; }
	}
}