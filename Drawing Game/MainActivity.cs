using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AsyncNet.Tcp.Client;
using AsyncNet.Tcp.Remote;
using DrawingCL;

namespace Drawing_Game
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public static IRemoteTcpPeer peer;
        object lockObject = new object();
        private DoodleCanvas firstCanvas;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            firstCanvas = FindViewById<DoodleCanvas>(Resource.Id.first_canvas);
            firstCanvas.isActiveCanvas = true;
            firstCanvas.OnTouch += FirstCanvas_OnTouch;
            await SetupClient();


        }

        private void FirstCanvas_OnTouch(object sender, OnTouchEventArgs e)
        {
            CustObj custObj = new CustObj();
            custObj.Id = "test1";
            custObj.IsJoining = false;
            custObj.Message = e.MotionEvent.ToString();
            custObj.MotionEventBytes = e.MotionEvent;
            var bytes = ByteConverter.ObjectToByteArray(custObj);
            peer.SendAsync(bytes);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }
        private async Task SetupClient()
        {
            var client = new AsyncNetTcpClient("192.168.0.79", 30894);
            client.ConnectionEstablished += async (s, e) =>
            {
                peer = e.RemoteTcpPeer;
                Console.WriteLine($"Connected to [{peer.IPEndPoint}]");
                var id = "test1";
                var isJ = true;
                

                    var obj = new CustObj()
                    {
                        Message = "initial message",
                        Id = id,
                        IsJoining = isJ


                    };
                    isJ = false;
                    var bytes = ByteConverter.ObjectToByteArray(obj);
                    await peer.SendAsync(bytes);
                
            };
            client.FrameArrived += async (s, e) => {
                    var obj =  (CustObj) await ByteConverter.ByteArrayToObject(e.FrameData);

                    Parcel parcel = Parcel.Obtain();
                    parcel.Unmarshall(obj.MotionEventBytes, 0, obj.MotionEventBytes.Length);
                    parcel.SetDataPosition(0);
                    MotionEvent mEvent = (MotionEvent)MotionEvent.Creator.CreateFromParcel(parcel);
                   RunOnUiThread(()=>firstCanvas.Draww(mEvent));
                
            };

            await client.StartAsync();
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}

