using System;
using Android.App;
//using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
//using AlertDialog = Android.Support.V7.App.AlertDialog.Builder;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;


namespace Familia.Helpers {
	internal class ProgressBarDialog {
		private readonly AlertDialog _dialog;

		/// <summary>
		/// Retrieve the current Window for the activity
		/// </summary>
		public Window Window { get; }

		private View InitUi(Context ctx) {
			var inflater = (LayoutInflater) ctx.GetSystemService(Context.LayoutInflaterService);
			return inflater?.Inflate(Resource.Layout.progress_dialog_layout, null);
		}

		/// <summary>
		/// Create a new instance of class ProgressBarDialog
		/// </summary>
		/// <param name="title">Title of ProgressBar Dialog</param>
		/// <param name="content">Content of ProgressBar Dialog</param>
		/// <param name="cancelable">Set if ProgressBar Dialog can be canceled</param>
		/// <param name="neutralEventHandler">Neutral event action</param>
		/// <param name="cancelEventHandler">Cancel event action</param>
		/// <param name="ctx">Context of current Activity</param>
		/// <param name="okButtonText">OkButton text</param>
		/// <param name="neutralButtonText">NeutralButton text</param>
		/// <param name="cancelButtonText">CancelButton text</param>
		/// <param name="okEventHandler">Ok event action</param>
		public ProgressBarDialog(string title, string content, Context ctx, bool cancelable = true,
			string okButtonText = "Ok", EventHandler<DialogClickEventArgs> okEventHandler = null,
			string neutralButtonText = "Irresolute", EventHandler<DialogClickEventArgs> neutralEventHandler = null,
			string cancelButtonText = "Cancel", EventHandler<DialogClickEventArgs> cancelEventHandler = null) {
			View view = InitUi(ctx);

			var contentTv = view.FindViewById<TextView>(Resource.Id.loading_msg);
			contentTv.Text = content;
			_dialog = new AlertDialog.Builder(ctx , Resource.Style.AppTheme_Dialog).Create();

			_dialog.SetView(view);
			_dialog.SetTitle(title);

			if (okEventHandler != null)
				_dialog.SetButton((int)DialogButtonType.Positive,okButtonText , okEventHandler);
			if (neutralEventHandler != null)
				_dialog.SetButton((int)DialogButtonType.Neutral, neutralButtonText , neutralEventHandler);
			if (cancelEventHandler != null)
				_dialog.SetButton((int)DialogButtonType.Negative, cancelButtonText , cancelEventHandler);
			_dialog.SetCancelable(cancelable);
			Window = _dialog.Window;
			Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);
		}


		/// <summary>
		/// Display ProgressBar Dialog
		/// </summary>
		public void Show() {
			_dialog?.Show();
		}

		/// <summary>
		/// Close current ProgressBar Dialog
		/// </summary>
		public void Dismiss() {
			_dialog?.Dismiss();
		}
	}
}