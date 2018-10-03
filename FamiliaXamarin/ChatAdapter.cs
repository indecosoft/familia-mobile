using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Refractored.Controls;
using Square.Picasso;

namespace FamiliaXamarin
{
    class ChatAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;

        private List<ChatModel> _messages;

        public ChatAdapter(List<ChatModel> messages)
        {
            _messages = messages;
            //_imageManager = new ImageManager(resources);
        }
        //Must override, just like regular Adapters
        public override int ItemCount
        {
            get
            {
                return _messages.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as SucHolder;

//            var currentCrewMember = _messages[position];
//
//            //Bind our data from our data source to our View References
//            viewHolder.UsernameView.Text = currentCrewMember.Username;
//            viewHolder.MessageView.Text = currentCrewMember.Message;
            //viewHolder._image.Text = currentCrewMember.Username;

            var message = _messages[position];
            viewHolder.UsernameView.Text = message.Username;
            viewHolder.MessageView.Text = message.Message;
            //viewHolder.setAvatar(message.getAvatar());
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            //Inflate our CrewMemberItem Layout
            //View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Srow, parent, false);

            //Create our ViewHolder to cache the layout view references and register

            //the OnClick event.
            int layout = -1;
            switch (viewType)
            {
                case 0:
                    layout = Resource.Layout.item_message;
                    break;
                case 1:
                    layout = Resource.Layout.item_log;
                    break;
                case 2:
                    layout = Resource.Layout.item_action;
                    break;
                case 3:
                    layout = Resource.Layout.item_my_message;
                    break;
            }
            View v = LayoutInflater
                .From(parent.Context)
                .Inflate(layout, parent, false);


            var viewHolder = new SucHolder(v);

            return viewHolder;
        }
        public void Clear()
        {
            _messages.Clear();
        }
        //private readonly ImageManager _imageManager;

        //This will fire any event handlers that are registered with our ItemClick
        //event.
        private void OnClick(int position)
        {
            if (ItemClick != null)
            {
                ItemClick(this, position);
            }
        }
        //Since this example uses a lot of Bitmaps, we want to do some house cleaning
        //and make them available for garbage collecting as soon as possible.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            //if (_imageManager != null)
            //{
            //    _imageManager.Dispose();
            //}
        }
        class SucHolder : RecyclerView.ViewHolder
        {
            public TextView UsernameView;
            public TextView MessageView;
            public CircleImageView Image;
            public SucHolder(View itemView)
                : base(itemView)
            {
                UsernameView = itemView.FindViewById<TextView>(Resource.Id.username);
                MessageView = itemView.FindViewById<TextView>(Resource.Id.message);
                Image = itemView.FindViewById<CircleImageView>(Resource.Id.profile_image);
                //sendEmail = itemView.FindViewById<Button>(Resource.Id.cbxStart);

                //itemView.Click += (sender, e) => listener(base.Position);
                //Add.Click += delegate
                // {
                //     var intent = new Intent(this.Activity, typeof(FormularSucursala));


                //     //intent.PutExtra("index", position);
                //     //crewProfileIntent.PutExtra("imageResourceId", SharedData.EmailData[position].PhotoResourceId);

                //     StartActivity(intent);
                // };
            }
        }
    }
}