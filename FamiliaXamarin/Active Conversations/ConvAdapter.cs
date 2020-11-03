using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Familia.JsonModels;

namespace Familia.Active_Conversations {
	internal class ConvAdapter : RecyclerView.Adapter {
		public event EventHandler<ConvAdapterClickEventArgs> ItemClick;
		public event EventHandler<ConvAdapterClickEventArgs> ItemLongClick;
		private readonly List<ConverstionsModel> _listOfActiveConversations;

		public ConvAdapter(List<ConverstionsModel> data) {
			_listOfActiveConversations = data;
		}

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
			Context context = parent.Context;
			LayoutInflater inflater = LayoutInflater.From(context);

			// Inflate the custom layout
			View contactView = inflater.Inflate(Resource.Layout.item_conversations, parent, false);

			// Return a new holder instance
			var viewHolder = new ConvAdapterViewHolder(contactView, OnClick, OnLongClick);
			return viewHolder;
		}

		// Replace the contents of a view (invoked by the layout manager)
		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
			ConverstionsModel item = _listOfActiveConversations[position];

			if (holder is ConvAdapterViewHolder viewHolder)
				viewHolder.NameTextView.Text = item.Username;
		}

		public override int ItemCount => _listOfActiveConversations.Count;

		public void DeleteConversation(int position) {
			_listOfActiveConversations.RemoveAt(position);
		}

		private void OnClick(ConvAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		private void OnLongClick(ConvAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
	}

	public class ConvAdapterViewHolder : RecyclerView.ViewHolder {
		public TextView NameTextView { get; }

		public ConvAdapterViewHolder(View itemView, Action<ConvAdapterClickEventArgs> clickListener,
			Action<ConvAdapterClickEventArgs> longClickListener) : base(itemView) {
			NameTextView = itemView.FindViewById<TextView>(Resource.Id.contact_name);
			itemView.Click += (sender, e) =>
				clickListener(new ConvAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
			itemView.LongClick += (sender, e) =>
				longClickListener(new ConvAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
		}
	}

	public class ConvAdapterClickEventArgs : EventArgs {
		public View View { get; set; }
		public int Position { get; set; }
	}
}