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

namespace FamiliaXamarin.Medicatie.Entities
{
    public class Boala
    {
        public string Id { get; set; }
        public string NumeBoala { get; set; }
        public List<Medicament> MedicamentList { get; set; }

        public Boala()
        {
            MedicamentList = new List<Medicament>();
        }

        public Boala(string numeBoala)
        {
            this.NumeBoala = numeBoala;
            MedicamentList = new List<Medicament>();
        }

        public Boala(string numeBoala, List<Medicament> medicamentList)
        {
            this.NumeBoala = numeBoala;
            this.MedicamentList = medicamentList;
        }

       

        public void addMedicament(Medicament medicament)
        {
            medicament.IdMed = "id" + MedicamentList.Count + "";
            MedicamentList.Add(medicament);
        }

        public Medicament getMedicamentById(string idMed)
        {
            foreach (var item in MedicamentList)
            {
                if (item.IdMed.Equals(idMed))
                {
                    return item;
                }
            }
            return null;
        }

        public void removeMedicament(Medicament medicament)
        {
            MedicamentList.Remove(medicament);
        }

        public override bool Equals(Object obj)
        {
            if (obj == this)
            {
                return true;
            }
            Type type = typeof(Boala);
            if (obj != null && obj.GetType() != type) {
//            if (!(obj instanceof Boala)) {
                return false;
            }

            Boala m = (Boala)obj;

            return this.NumeBoala.Equals(m?.NumeBoala);
        }
    }
}