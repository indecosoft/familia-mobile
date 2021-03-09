using System.Collections.Generic;
using Android.Gms.Vision.Texts;
using Android.Util;
using Familia.OngBenefits.GenerateCardQR.Entities;
using Familia.OngBenefits.GenerateCardQR.Events;

namespace Familia.OngBenefits.GenerateCardQR
{
    public class StringComputations : PersonIdInfo
    {
        private List<string> tags = new List<string>
            {"last na", "pre", "cet", "loc", "domi", "emis", "valid", "seria", "/"};

        public delegate void StringComputationsEventHandler(object source , PersonIdInfo args);
        public event StringComputationsEventHandler ScanningCompleted;

        public void RetrieveInfo(TextBlock item)
        {
            if (item.Value.ToLower().Contains("cnp ")) {
                GetCNP(item);
            }

            if (item.Value.ToLower().Contains(tags[0])) {
                GetNume(item);
            }
            
            if (item.Value.ToLower().Contains(tags[1])) {
                GetPrenume(item);
            }
            
            if (item.Value.ToLower().Contains(tags[2])) {
                GetCetatenie(item);
            }
            
            if (item.Value.ToLower().Contains(tags[3])) {
                GetLocNastere(item);
            }
            
            if (item.Value.ToLower().Contains(tags[4])) {
                GetDomiciliu(item);
            }
            
            if (item.Value.ToLower().Contains(tags[5])) {
                GetIssued(item);
            }
            
            if (item.Value.ToLower().Contains("seria")) {
                GetSeriesAndNumber(item);
            }
            
            if (item.Value.ToLower().Contains(tags[6])) {
                GetValidity(item);
            }

            if (CheckIntegrity())
            {
                // Log.Error("Scanned_Data", base.ToString());
                ScanningCompleted?.Invoke(this , this);
            }
        }
        
        
        public void GetCNP(TextBlock item)
        {
            string[] data = item.Value.Split("\n");
            foreach (string cnp in data)
            {
                if (cnp.ToLower().Contains("cnp ") && cnp.Length == 17)
                {
                    Cnp = cnp.Substring(4);
                }
            }

            // OcrGraphic graphic = new OcrGraphic(context, mGraphicOverlay, item);
            // mGraphicOverlay.Add(graphic);
        }

        //
        private void GetNume(TextBlock item)
        {
            string[] data = item.Value.Split("\n");

            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i].ToLower().Contains(tags[0])
                    && data[i + 1] != null
                    && !data[i + 1].ToLower().Contains(tags[1])
                    && !data[i + 1].ToLower().Contains(tags[2])
                    && !data[i + 1].ToLower().Contains(tags[3])
                    && !data[i + 1].ToLower().Contains(tags[4])
                    && !data[i + 1].ToLower().Contains(tags[5])
                    && !data[i + 1].ToLower().Contains(tags[8])
                    //&& !data[i + 1].contains(Storage.getInstance().getBuletin().getPrenume())
                )
                {
                    LastName = data[i + 1];
                }
            }
        }

        
        private void GetPrenume(TextBlock item)
        {
            string[] data = item.Value.Split("\n");
        
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i].ToLower().Contains(tags[1])
                    && data[i + 1] != null
                    && !data[i + 1].ToLower().Contains(tags[2])
                    && !data[i + 1].ToLower().Contains(tags[3])
                    && !data[i + 1].ToLower().Contains(tags[4])
                    && !data[i + 1].ToLower().Contains(tags[5])
                    && !data[i + 1].ToLower().Contains(tags[8])
                )
                {
                    FirstName = data[i + 1];
                }
            }
        }
        
        private void GetCetatenie(TextBlock item)
        {
            string[] data = item.Value.Split("\n");
        
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i].ToLower().Contains(tags[2])
                    && data[i + 1] != null
                    && !data[i + 1].ToLower().Contains(tags[3])
                    && !data[i + 1].ToLower().Contains(tags[4])
                    && !data[i + 1].ToLower().Contains(tags[5])
                    && !data[i + 1].ToLower().Contains(tags[8])
                )
                {
                    Nationality = data[i + 1];
                }
            }
        }
        
        public void GetLocNastere(TextBlock item)
        {
            string[] data = item.Value.Split("\n");
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i].ToLower().Contains(tags[3])
                    && data[i + 1] != null
                    && !data[i + 1].ToLower().Contains(tags[4])
                    && !data[i + 1].ToLower().Contains(tags[5])
                    && !data[i + 1].ToLower().Contains(tags[8])
                )
                {
                    Birthplace = data[i + 1];
                }
            }
        }

        
        private void GetDomiciliu(TextBlock item)
        {
            string[] data = item.Value.Split("\n");
            int length = data.Length;
            if (length > 2)
            {
                for (int j = 0; j < length - 2; j++)
                {
                    if (data[j].ToLower().Contains(tags[4])
                        && data[j + 1] != null
                        && data[j + 2] != null
                        && !data[j + 1].ToLower().Contains(tags[5])
                        && !data[j + 2].ToLower().Contains(tags[5])
                        && !data[j + 1].ToLower().Contains(tags[8]))
                    {
                        HomeAddress = data[j + 1] + " " + data[j + 2];
                    }
                }
            }
        
            //OcrGraphic graphic = new OcrGraphic(context, mGraphicOverlay, item);
            // mGraphicOverlay.Add(graphic);
        }
        
        private void GetIssued(TextBlock item)
        {
            string[] data = item.Value.Split("\n");
        
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i].ToLower().Contains(tags[5])
                    && !data[i + 1].ToLower().Contains(tags[8])
                    && data[i + 1] != null
                    && !data[i + 1].Contains("<")
                    && !data[i + 1].Contains("K")
                )
                {
                    Issued = data[i + 1];
                }
            }
        }
        
        private void GetSeriesAndNumber(TextBlock item)
        {
            string[] data = item.Value.Split("\n");
            string seria = null;
            string nr = null;
            foreach (string aData in data)
            {
                if (aData.ToLower().Contains("seria"))
                {
                    string[] a = aData.Split(" ");
        
                    for (int j = 0; j < a.Length - 1; j++)
                    {
                        if (a[j].ToLower().Equals("seria") && !a[j + 1].ToLower().Equals("nr") &&
                            a[j + 1] != null)
                        {
                            seria = a[j + 1];
                        }
        
                        if (a[j].ToLower().Equals("nr") && a[j + 1].Length >= 6 && a[j + 1] != null)
                        {
                            nr = a[j + 1];
                        }
                    }
                }
            }
        
            if (seria != null && nr != null)
            {
                Series = seria;
                Number = nr;
            }
        }
        
        private void GetValidity(TextBlock item)
        {
            string[] data = item.Value.Split("\n");
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i].ToLower().Contains(tags[6]) && data[i + 1] != null && data[i + 1].Length == 19)
                {
                    if (data[i + 1].Contains(","))
                    {
                        data[i + 1] = data[i + 1].Replace(",", ".");
                    }
                    Validity = data[i + 1];
                }
            }
        }
    }
}