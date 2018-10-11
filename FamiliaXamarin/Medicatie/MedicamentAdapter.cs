using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using FamiliaXamarin.Medicatie.Entities;

namespace FamiliaXamarin.Medicatie
{
    class MedicamentAdapter : RecyclerView.Adapter
    {
        private List<Medicament> medicaments;
        private OnMedicamentClickListener listener;

        public MedicamentAdapter(List<Medicament> medicaments)
        {
            this.medicaments = new List<Medicament>();
            this.medicaments = medicaments;
        }

        public MedicamentAdapter()
        {
            this.medicaments = new List<Medicament>();
        }
        public void setMedicaments(List<Medicament> medicaments)
        {
            this.medicaments.Clear();
            this.medicaments = medicaments;
        }
        public List<Medicament> getMedicaments()
        {
            return medicaments;
        }

        public void setListener(OnMedicamentClickListener listener)
        {
            this.listener = listener;
        }
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            LayoutInflater inflater = LayoutInflater.From(parent.Context);
            View v = inflater.Inflate(Resource.Layout.item_med, parent, false);

            return new MedicamentAdapterViewHolder(v);
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as MedicamentAdapterViewHolder;
            Medicament medicament = medicaments[position];
            holder.tvTitle.Text =medicament.Name;
            holder.Medicament = medicament;
            holder.Listener = listener;
        }

        public override int ItemCount => medicaments.Count;
        public void addMedicament(Medicament medicament)
        {
            
            if (!medicaments.Contains(medicament))
            {
                medicaments.Add(medicament);
            }
        }

        public void removeMedicament(Medicament medicament)
        {
            medicaments.Remove(medicament);
        }
        public void updateMedicament(Medicament medicament, String idMed)
        {
            Medicament m = getMedicamentById(idMed);
            m.Name = medicament.Name;
        }
        public Medicament getMedicamentById(string idMed)
        {
            foreach (Medicament item in medicaments)
            {
                if (item.IdMed.Equals(idMed))
                {
                    return item;
                }
            }
            return null;
        }
    }




    public class MedicamentAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public TextView tvTitle;
        public TextView tvTime;
        public RelativeLayout container;
        public Button btnDelete;
        public OnMedicamentClickListener Listener;
        public Medicament Medicament;
        public MedicamentAdapterViewHolder(View itemView) : base(itemView)
        {
            //TextView = v;
            tvTitle = itemView.FindViewById<TextView>(Resource.Id.tv_title);
            tvTime = itemView.FindViewById<TextView>(Resource.Id.tv_time);
            container = itemView.FindViewById<RelativeLayout>(Resource.Id.container);
            btnDelete = itemView.FindViewById<Button>(Resource.Id.btn_delete);
            container.Click += delegate
            {
                if (Listener != null)
                {

                    Listener.OnMedicamentClick(Medicament);

                }
            };


            btnDelete.Click += delegate
            {
                if (Listener != null)
                {
                    Listener.OnMedicamentDeleteClick(Medicament);
                }
            };
        }
    }

    public class MedicamentAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
    public interface OnMedicamentClickListener
    {
        void OnMedicamentClick(Medicament medicament);

        void OnMedicamentDeleteClick(Medicament medicament);
    }
}