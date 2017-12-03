using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Support.V7.App;

namespace FrogCroakPractice
{
    [Android.App.Activity(Label = "FrogCroakPractice", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : AppCompatActivity
    {
        TextView tv_RMessage;
        ImageView iv_Frog;
        TextView tv_LMessage;
        EditText et_Message;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            InitViews();
        }

        private void InitViews()
        {
            tv_RMessage = FindViewById<TextView>(Resource.Id.tv_RMessage);
            iv_Frog = FindViewById<ImageView>(Resource.Id.iv_Frog);
            tv_LMessage = FindViewById<TextView>(Resource.Id.tv_LMessage);
            et_Message = FindViewById<EditText>(Resource.Id.et_Message);
        }

        public void SelectImage(View v){
            
        }

        public void SendMessage(View v)
        {

        }

    }
}

