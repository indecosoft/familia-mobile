using System;
using System.Collections.Generic;
namespace FamiliaXamarin.Medicatie.Entities
{
    public class Disease
    {
        public string Id { get; set; }
        public string DiseaseName { get; set; }
        public List<Medicine> ListOfMedicines { get; set; }

        public Disease()
        {
            ListOfMedicines = new List<Medicine>();
        }

        public Disease(string diseaseName)
        {
            DiseaseName = diseaseName;
            ListOfMedicines = new List<Medicine>();
        }

        public Disease(string diseaseName, List<Medicine> listOfMedicines)
        {
            DiseaseName = diseaseName;
            ListOfMedicines = listOfMedicines;
        }

        public void AddMedicine(Medicine medicament)
        {
            medicament.IdMed = "id" + ListOfMedicines.Count + "";
            ListOfMedicines.Add(medicament);
        }

        public Medicine GetMedicineById(string idMed)
        {
            foreach (var item in ListOfMedicines)
            {
                if (item.IdMed.Equals(idMed))
                {
                    return item;
                }
            }
            return null;
        }

        public void RemoveMedicine(Medicine medicament)
        {
            ListOfMedicines.Remove(medicament);
        }

        public override bool Equals(Object obj)
        {
            if (obj == this)
            {
                return true;
            }
            Type type = typeof(Disease);
            if (obj != null && obj.GetType() != type)
            {
                return false;
            }

            Disease m = (Disease)obj;

            return DiseaseName.Equals(m?.DiseaseName);
        }
    }
}