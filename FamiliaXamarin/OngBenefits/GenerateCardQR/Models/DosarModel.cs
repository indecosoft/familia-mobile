namespace Familia.OngBenefits.GenerateCardQR.Models
{
    public class DosarModel {

        private string nr;
        private string numeAj;
        private string relRudenie;
        private string titular;
        private string cnpTitular;

        public DosarModel(string nr, string numeAj, string relRudenie, string titular, string cnpTitular) {
            this.nr = nr;
            this.numeAj = numeAj;
            this.relRudenie = relRudenie;
            this.titular = titular;
            this.cnpTitular = cnpTitular;
        }

        public string getNr() {
            return nr;
        }

        public string getNumeAj() {
            return numeAj;
        }

        public string getRelRudenie() {
            return relRudenie;
        }

        public string getTitular() {
            return titular;
        }

        public string getCnpTitular() {
            return cnpTitular;
        }
    }

}