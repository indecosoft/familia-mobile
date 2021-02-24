using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Familia.OngBenefits
{
    class OngBenefitModel
    {
        public int idBeneficiu { get; set; }
        public int idBeneficiuAsisoc { get; set; }
        public int idClient{ get; set; }

        public string nume { get; set; }

        public string detalii { get; set; }


        public OngBenefitModel(int idBeneficiu, int idBeneficiuAsisoc, int idClient, string nume, string detalii = null) {
            this.idBeneficiu = idBeneficiu;
            this.idBeneficiuAsisoc = idBeneficiuAsisoc;
            this.idClient = idClient;
            this.nume = nume;
            this.detalii = detalii;
        }
    }
}