using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using DrawingCL;
using Path = Android.Graphics.Path;

namespace Drawing_Game
{
    public class DoodleCanvas  : View
    {
        public event EventHandler<OnTouchEventArgs> OnTouch;
    private Paint mPaint;
    private Path mPath;
        private DrawingObject drawingObject;
        public bool isActiveCanvas;

        public DoodleCanvas(Context context,IAttributeSet attr): base(context,attr)
    {
        
        mPaint = new Paint();
        mPaint.Color=Color.Red;
        mPaint.SetStyle(Paint.Style.Stroke);
        mPaint.StrokeJoin = Paint.Join.Round;
        mPaint.StrokeCap = Paint.Cap.Round;
        mPaint.StrokeWidth = 10;
        mPath = new Path();
            drawingObject = new DrawingObject();
    }
        private void OnTouchHappend(OnTouchEventArgs e)
        {
            var handler = OnTouch;
            handler?.Invoke(this, e);
        }


        protected override void OnDraw(Canvas canvas)
    {
        canvas.DrawPath(mPath, mPaint);
        base.OnDraw(canvas);
    }
        public void Draww(MotionEvent mEvent)
        {
            switch (mEvent.Action)
            {

                case MotionEventActions.Down:
                    mPath.MoveTo(mEvent.GetX(), mEvent.GetY());
                    break;

                case MotionEventActions.Move:
                    mPath.LineTo(mEvent.GetX(), mEvent.GetY());
                    this.Invalidate();
                    break;

                case MotionEventActions.Up:
                    break;

            }
        }

    
    public override bool OnTouchEvent(MotionEvent mEvent)
    {
            var eventArgs = new OnTouchEventArgs();
            Parcel parcel = Parcel.Obtain();
            mEvent.WriteToParcel(parcel, 0);
            var bytes = parcel.Marshall();
            eventArgs.MotionEvent = bytes;
            if (this.Id == Resource.Id.first_canvas)
            {
                CustObj custObj = new CustObj();
                custObj.Id = "test1";
                custObj.IsJoining = false;
                custObj.Message = mEvent.Action.ToString();
                custObj.MotionEventBytes = eventArgs.MotionEvent;
                var bytess = ByteConverter.ObjectToByteArray(custObj);
                MainActivity.peer.SendAsync(bytess);
            }
            switch (mEvent.Action){

            case MotionEventActions.Down:
                mPath.MoveTo(mEvent.GetX(), mEvent.GetY());
            break;

            case MotionEventActions.Move:
                mPath.LineTo(mEvent.GetX(), mEvent.GetY());
            this.Invalidate();
            break;

            case MotionEventActions.Up:
                break;

        }
          

            return true;
    }
        byte[] ObjectToByteArray(MotionEvent obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                Parcel parcel = null;
                
                return ms.ToArray();
            }
        }


    }
    public class OnTouchEventArgs : EventArgs
    {
        public byte[] MotionEvent { get; set; }
    }
}