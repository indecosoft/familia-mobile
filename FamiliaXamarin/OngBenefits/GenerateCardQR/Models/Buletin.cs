using Java.Text;

namespace Familia.OngBenefits.GenerateCardQR.Models
{
 public class Buletin {
    private string cnp;
    private string nume;
    private string prenume;
    private string cetatenie;
    private string locNastere;
    private string domiciliu;
    private string emisa;
    private string seria;
    private string nr;
    private string valabilitate;

    public Buletin() {
        // this(null, null, null, null, null, null, null, null, null, null);
    }

    private Buletin(string cnp, string nume, string prenume, string cetatenie, string locNastere, string domiciliu, string emisa, string seria, string nr, string valabilitate) {
        this.cnp = cnp;
        this.nume = nume;
        this.prenume = prenume;
        this.cetatenie = cetatenie;
        this.locNastere = locNastere;
        this.domiciliu = domiciliu;
        this.emisa = emisa;
        this.seria = seria;
        this.nr = nr;
        this.valabilitate = valabilitate;
    }

    private string normalize(string data) {
        data = Normalizer.Normalize(data, Normalizer.Form.Nfkd);
        return data.Replace("[^\\p{ASCII}]", "");
    }

    public string printData() {
        return "Cnp: " + this.cnp
                + "Nume: " + this.nume
                + "Prenume: " + this.prenume
                + "Cetatenie: " + this.cetatenie
                + "Loc nastere: " + this.locNastere
                + "Domiciliu: " + this.domiciliu
                + "Emisa: " + this.emisa
                + "Seria: " + this.seria
                + "Nr: " + this.nr
                + "Valabilitate: " + this.valabilitate;
    }

    public bool verify() {
        return this.cnp != null
                && this.nume != null
                && this.prenume != null
                && this.cetatenie != null
                && this.locNastere != null
                && this.domiciliu != null
                && this.emisa != null
                && this.seria != null
                && this.nr != null
                && this.valabilitate != null
                && !this.nume.Equals(this.prenume)
                && !this.domiciliu.Contains(this.emisa.Substring(0, 4));
    }

    public void clear() {
        this.cnp = null;
        this.nume = null;
        this.prenume = null;
        this.cetatenie = null;
        this.locNastere = null;
        this.domiciliu = null;
        this.emisa = null;
        this.seria = null;
        this.nr = null;
        this.valabilitate = null;
    }

    public string getCnp() {
        return cnp;
    }

    public void setCnp(string cnp) {
        this.cnp = cnp;
    }

    public string getNume() {
        return nume;
    }

    public void setNume(string nume) {
        this.nume = normalize(nume);
    }

    public string getPrenume() {
        return prenume;
    }

    public void setPrenume(string prenume) {
        this.prenume = normalize(prenume);
    }

    public string getCetatenie() {
        return cetatenie;
    }

    public void setCetatenie(string cetatenie) {
        this.cetatenie = normalize(cetatenie);
    }

    public string getLocNastere() {
        return locNastere;
    }

    public void setLocNastere(string locNastere) {
        this.locNastere = normalize(locNastere);
    }

    public string getDomiciliu() {
        return domiciliu;
    }

    public void setDomiciliu(string domiciliu) {
        this.domiciliu = normalize(domiciliu);
    }

    public string getEmisa() {
        return emisa;
    }

    public void setEmisa(string emisa) {
        this.emisa = normalize(emisa);
    }

    public string getSeria() {
        return seria;
    }

    public void setSeria(string seria) {
        this.seria = seria;
    }

    public string getNr() {
        return nr;
    }

    public void setNr(string nr) {
        this.nr = nr;
    }

    public string getValabilitate() {
        return valabilitate;
    }

    public void setValabilitate(string valabilitate) {
        this.valabilitate = valabilitate;
    }
}
}