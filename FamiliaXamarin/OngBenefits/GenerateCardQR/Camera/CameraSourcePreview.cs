using System;
using System.IO;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace Familia.OngBenefits.GenerateCardQR.Camera
{
    public class CameraSourcePreview : ViewGroup, ISurfaceHolderCallback
    {
        private static readonly string TAG = "CameraSourcePreview";

        private Context mContext;
        private SurfaceView mSurfaceView;
        private bool mStartRequested;
        private bool mSurfaceAvailable;
        private Android.Gms.Vision.CameraSource mCameraSource;

        // private GraphicOverlay mOverlay;

        public CameraSourcePreview(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CameraSourcePreview(Context? context) : base(context)
        {
        }

        public CameraSourcePreview(Context? context, IAttributeSet? attrs) : base(context, attrs)
        {
            mContext = context;
            mStartRequested = false;
            mSurfaceAvailable = false;

            mSurfaceView = new SurfaceView(context);
            
            mSurfaceView.Holder.AddCallback(this);
            AddView(mSurfaceView);
        }

        public CameraSourcePreview(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public CameraSourcePreview(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context,
            attrs, defStyleAttr, defStyleRes)
        {
        }
        
        
        
        public  Android.Hardware.Camera GetCamera()
        {
            var javaHero = mCameraSource.JavaCast<Java.Lang.Object>();
            var fields = javaHero.Class.GetDeclaredFields();
            foreach (var field in fields)
            {
                if(field.Type.CanonicalName.Equals("android.hardware.Camera", StringComparison.OrdinalIgnoreCase))
                {
                    field.Accessible = true;
                    var camera = field.Get(javaHero);
                    var cCamera = (Android.Hardware.Camera)camera;
                    return cCamera;
                }
            }

            return null;
        }
        public void Start(Android.Gms.Vision.CameraSource cameraSource){
            if (cameraSource == null) {
                Stop();
            }

            mCameraSource = cameraSource;

            if (mCameraSource != null) {
                mStartRequested = true;
                StartIfReady();
            }
        }

        // public void Start(Android.Gms.Vision.CameraSource cameraSource, GraphicOverlay overlay) {
        //     // mOverlay = overlay;
        //     Start(cameraSource);
        // }

        public void Stop()
        {
            mCameraSource?.Stop();
        }

        public void Release() {
            if (mCameraSource != null) {
                mCameraSource.Release();
                mCameraSource = null;
            }
        }
        private void StartIfReady()
        {
            if (mStartRequested && mSurfaceAvailable)
            {
                mCameraSource.Start(mSurfaceView.Holder);
                // if (mOverlay != null) {
                //     Size size = mCameraSource.getPreviewSize();
                //     int min = Math.min(size.getWidth(), size.getHeight());
                //     int max = Math.max(size.getWidth(), size.getHeight());
                //     if (isPortraitMode()) {
                //         // Swap width and height sizes when in portrait, since it will be rotated by
                //         // 90 degrees
                //         mOverlay.setCameraInfo(min, max, mCameraSource.getCameraFacing());
                //     } else {
                //         mOverlay.setCameraInfo(max, min, mCameraSource.getCameraFacing());
                //     }
                //     mOverlay.clear();
                // }
                mStartRequested = false;
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            int previewWidth = 320;
            int previewHeight = 240;
            if (mCameraSource != null)
            {
                Android.Gms.Common.Images.Size size = mCameraSource.PreviewSize;
                if (size != null)
                {
                    previewWidth = size.Width;
                    previewHeight = size.Height;
                }
            }

            // Swap width and height sizes when in portrait, since it will be rotated 90 degrees
            if (IsPortraitMode())
            {
                int tmp = previewWidth;
                previewWidth = previewHeight;
                previewHeight = tmp;
            }

            int viewWidth = r - l;
            int viewHeight = b - t;

            int childWidth;
            int childHeight;
            int childXOffset = 0;
            int childYOffset = 0;
            float widthRatio = (float) viewWidth / (float) previewWidth;
            float heightRatio = (float) viewHeight / (float) previewHeight;

            // To fill the view with the camera preview, while also preserving the correct aspect ratio,
            // it is usually necessary to slightly oversize the child and to crop off portions along one
            // of the dimensions.  We scale up based on the dimension requiring the most correction, and
            // compute a crop offset for the other dimension.
            if (widthRatio > heightRatio)
            {
                childWidth = viewWidth;
                childHeight = (int) ((float) previewHeight * widthRatio);
                childYOffset = (childHeight - viewHeight) / 2;
            }
            else
            {
                childWidth = (int) ((float) previewWidth * heightRatio);
                childHeight = viewHeight;
                childXOffset = (childWidth - viewWidth) / 2;
            }

            for (int i = 0; i < ChildCount; ++i)
            {
                // One dimension will be cropped.  We shift child over or up by this offset and adjust
                // the size to maintain the proper aspect ratio.
                GetChildAt(i).Layout(
                    -1 * childXOffset, -1 * childYOffset,
                    childWidth - childXOffset, childHeight - childYOffset);
            }

            try
            {
                StartIfReady();
            }
            catch (SecurityException se)
            {
                Log.Error(TAG, "Do not have permission to start the camera", se);
            }
            catch (IOException e)
            {
                Log.Error(TAG, "Could not start camera source.", e);
            }
        }

        private bool IsPortraitMode()
        {
            Android.Content.Res.Orientation orientation = mContext.Resources.Configuration.Orientation;
            if (orientation == Android.Content.Res.Orientation.Landscape)
            {
                return false;
            }

            if (orientation == Android.Content.Res.Orientation.Portrait)
            {
                return true;
            }

            Log.Debug(TAG, "isPortraitMode returning false by default");
            return false;
        }

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            mSurfaceAvailable = true;
            try {
                StartIfReady();
            } catch (SecurityException se) {
                Log.Error(TAG,"Do not have permission to start the camera", se);
            } catch (IOException e) {
                Log.Error(TAG, "Could not start camera source.", e);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            mSurfaceAvailable = false;
        }
    }
}