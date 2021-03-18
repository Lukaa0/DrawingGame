using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Drawing_Game
{
    class DrawingObject
    {
        public float X { get; set; }
        public float Y { get; set; }
        public MotionEventActions Action{ get; set; }
    }
}