using System;

namespace Familia.Location {
	public interface ILocationEvents {
		void OnLocationRequested(EventArgs args);
	}
}