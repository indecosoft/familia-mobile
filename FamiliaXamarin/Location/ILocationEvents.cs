using System;
namespace Familia.Helpers
{
    public interface ILocationEvents
    {
        void OnLocationRequested(object source, EventArgs args);
    }
}
