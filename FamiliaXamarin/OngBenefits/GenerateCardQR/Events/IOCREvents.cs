using Familia.OngBenefits.GenerateCardQR.Entities;

namespace Familia.OngBenefits.GenerateCardQR.Events
{
    public interface IOCREvents
    {
        void OnScanningCompleted(PersonIdInfo args);
    }
}