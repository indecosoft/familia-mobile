using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using FamiliaXamarin.Medicatie.Entities;

namespace FamiliaXamarin.Medicatie
{
    class MedicineAdapter : RecyclerView.Adapter
    {
        private List<Medicine> medicaments;
        private OnMedicamentClickListener listener;

        public MedicineAdapter()
        {
            medicaments = new List<Medicine>();
        }
        public void SetMedicaments(List<Medicine> medicamentsList)
        {
            medicaments.Clear();
            medicaments = medicamentsList;
        }
        public List<Medicine> GetMedicaments()
        {
            return medicaments;
        }

        public void SetListener(OnMedicamentClickListener listenerClick)
        {
            listener = listenerClick;
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            LayoutInflater inflater = LayoutInflater.From(parent.Context);
            View v = inflater.Inflate(Resource.Layout.item_med, parent, false);

            return new MedicamentAdapterViewHolder(v);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as MedicamentAdapterViewHolder;
            Medicine medicament = medicaments[position];
            if (holder == null) return;
            holder.tvTitle.Text = medicament.Name;
            holder.Medicament = medicament;
            holder.Listener = listener;
        }

        public override int ItemCount => medicaments.Count;
        public void AddMedicament(Medicine medicament)
        {
            
            if (!medicaments.Contains(medicament))
            {
                medicaments.Add(medicament);
            }
        }

        public void RemoveMedicament(Medicine medicament)
        {
            medicaments.Remove(medicament);
        }
        public void UpdateMedicament(Medicine medicament, string idMed)
        {
            Medicine m = GetMedicamentById(idMed);
            m.Name = medicament.Name;
        }
        public Medicine GetMedicamentById(string idMed)
        {
            foreach (Medicine item in medicaments)
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
        public TextView tvTitle;
        public RelativeLayout container;
        public Button btnDelete;
        public OnMedicamentClickListener Listener;
        public Medicine Medicament;
        public MedicamentAdapterViewHolder(View itemView) : base(itemView)
        {
            tvTitle = itemView.FindViewById<TextView>(Resource.Id.tv_title);
            //tvTime = itemView.FindViewById<TextView>(Resource.Id.tv_time);
            container = itemView.FindViewById<RelativeLayout>(Resource.Id.container);
            btnDelete = itemView.FindViewById<Button>(Resource.Id.btn_delete);
            container.Click += delegate
            {
                Listener?.OnMedicamentClick(Medicament);
            };
            btnDelete.Click += delegate
            {
                Listener?.OnMedicamentDeleteClick(Medicament);
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
        void OnMedicamentClick(Medicine medicament);

        void OnMedicamentDeleteClick(Medicine medicament);
    }
}