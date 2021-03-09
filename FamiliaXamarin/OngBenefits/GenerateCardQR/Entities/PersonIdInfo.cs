using System;
using System.Net.Http;
using Android.Provider;
using Android.Util;
using Java.IO;
using Org.Json;

namespace Familia.OngBenefits.GenerateCardQR.Entities
{
    public class PersonIdInfo : EventArgs
    {
        public string Cnp { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Nationality { get; set; }
        public string Birthplace { get; set; }
        public string HomeAddress { get; set; }
        public string Issued { get; set; }
        public string Series { get; set; }
        public string Number { get; set; }
        public string Validity { get; set; }


        protected bool CheckIntegrity()
        {
            return !string.IsNullOrEmpty(Cnp)
                   && !string.IsNullOrEmpty(LastName)
                   && !string.IsNullOrEmpty(FirstName)
                   && !string.IsNullOrEmpty(Nationality)
                   && !string.IsNullOrEmpty(Birthplace)
                   && !string.IsNullOrEmpty(HomeAddress)
                   && !string.IsNullOrEmpty(Issued)
                   && !string.IsNullOrEmpty(Series)
                   && !string.IsNullOrEmpty(Number)
                   && !string.IsNullOrEmpty(Validity);
        }

        public override string ToString()
        {
            return "Cnp: " + Cnp
                           + "\nNume: " + LastName
                           + "\nPrenume: " + FirstName
                           + "\nCetatenie: " + Nationality
                           + "\nLoc nastere: " + Birthplace
                           + "\nDomiciliu: " + HomeAddress
                           + "\nEmisa: " + Issued
                           + "\nSeria: " + Series
                           + "\nNr: " + Number
                           + "\nValabilitate: " + Validity;
        }

        public JSONObject ToJson()
        {
            JSONObject data = new JSONObject();

            try {
                
                data.Put("cnp",Cnp);
                data.Put("cnp",Cnp);
                data.Put("nume", LastName);
                data.Put("prenume", FirstName);
                data.Put("cetatenie", Nationality);
                data.Put("locNastere", Birthplace);
                data.Put("domiciliu", HomeAddress);
                data.Put("eliberat", Issued);
                data.Put("seria", Series);
                data.Put("nr", Number);
                data.Put("valabilitate", Validity);
            } catch (Exception e)
            {
                Log.Error("ToJsonProblem", e.Message);
            }

            return data;
        }

        public MultipartFormDataContent ToFormData()
        {
            return new MultipartFormDataContent
            {
                {new StringContent(Cnp), "cnp"},
                {new StringContent(LastName), "nume"},
                {new StringContent(FirstName), "prenume"},
                {new StringContent(Nationality), "cetatenie"},
                {new StringContent(Birthplace), "locNastere"},
                {new StringContent(HomeAddress), "domiciliu"},
                {new StringContent(Issued), "eliberat"},
                {new StringContent(Series), "seria"},
                {new StringContent(Number), "nr"},
                {new StringContent(Validity), "valabilitate"}
            };
        }

    }
}