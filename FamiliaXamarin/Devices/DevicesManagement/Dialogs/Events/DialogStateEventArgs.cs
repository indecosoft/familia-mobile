using System;
using Familia.Devices.DevicesManagement.Dialogs.DialogHelpers;
using Familia.Devices.Helpers;

namespace Familia.Devices.DevicesManagement.Dialogs.DialogEvents {
	public class DialogStateEventArgs : EventArgs
	{
		public DialogStatuses Status { get; set; }
		public DeviceType DeviceType { get; set; }
	}
}
