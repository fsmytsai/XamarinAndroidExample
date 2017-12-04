using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Support.V7.App;
using Android.Content;
using Android.App;
using Android.Graphics;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Provider;
using System.Net;
using Newtonsoft.Json;
using FrogCroakPractice.Models;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Android.Views.InputMethods;
using System;

namespace FrogCroakPractice
{
    [Android.App.Activity(Label = "FrogCroakPractice", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : AppCompatActivity
    {
        LinearLayout Activity_Outer;
        TextView tv_RMessage;
        ImageView iv_Frog;
        TextView tv_LMessage;
        EditText et_Message;
        private readonly int CHOOSE_IMAGE_FILE = 58;
        private const int READ_EXTERNAL_STORAGE = 37;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            Window.SetSoftInputMode(SoftInput.StateHidden);
            InitViews();
        }

        private void InitViews()
        {
            Activity_Outer = FindViewById<LinearLayout>(Resource.Id.Activity_Outer);
            tv_RMessage = FindViewById<TextView>(Resource.Id.tv_RMessage);
            iv_Frog = FindViewById<ImageView>(Resource.Id.iv_Frog);
            tv_LMessage = FindViewById<TextView>(Resource.Id.tv_LMessage);
            et_Message = FindViewById<EditText>(Resource.Id.et_Message);

            FindViewById(Resource.Id.bt_SelectImage).Click += (sender, e) =>
            {
                SelectImage();
            };

            FindViewById(Resource.Id.bt_SendMessage).Click += (sender, e) =>
            {
                SendMessage();
            };
        }

        public void SelectImage()
        {
            if (ActivityCompat.CheckSelfPermission(this, Android.Manifest.Permission.ReadExternalStorage) == (int)Permission.Granted)
            {
                OpenImageChooser();
            }
            else
            {
                RequestPermissions(new string[] { Android.Manifest.Permission.ReadExternalStorage }, READ_EXTERNAL_STORAGE);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case READ_EXTERNAL_STORAGE:
                    if (grantResults.Length == 1 && grantResults[0] == (int)Permission.Granted)
                        OpenImageChooser();
                    else
                        Toast.MakeText(this, "您拒絕選取檔案", ToastLength.Short).Show();
                    return;
            }
        }

        void OpenImageChooser()
        {
            Intent picker = new Intent(Intent.ActionGetContent);
            picker.SetType("image/*");
            picker.PutExtra(Intent.ExtraAllowMultiple, false);
            picker.PutExtra(Intent.ExtraLocalOnly, true);
            Intent destIntent = Intent.CreateChooser(picker, "選青蛙圖片");
            StartActivityForResult(destIntent, CHOOSE_IMAGE_FILE);
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == CHOOSE_IMAGE_FILE && resultCode == Result.Ok)
            {
                tv_RMessage.Visibility = ViewStates.Gone;
                Android.Net.Uri uri = data.Data;

                //顯示圖片
                Bitmap bitmap = MediaStore.Images.Media.GetBitmap(this.ContentResolver, uri);
                iv_Frog.Visibility = ViewStates.Visible;
                iv_Frog.SetImageBitmap(bitmap);


                AllRequestResult result = null;

                tv_LMessage.Text = "辨識中...";

                await Task.Run(() =>
                {
                    string FileName = CreateFileAndGetFileNameFromUri(uri);
                    result = UploadImage(FilesDir.AbsolutePath + "/" + FileName);
                });

                if (result.IsSuccess)
                {
                    tv_LMessage.Text = (string)result.Result;
                }
                else
                {
                    tv_LMessage.Text = "辨識失敗";
                    WebExceptionHandler((WebException)result.Result);
                }
            }
        }

        public AllRequestResult UploadImage(string FilePath)
        {
            string ContentType = "";
            string[] Type = FilePath.Split('.');
            if (System.String.Compare(Type[Type.Length - 1], "jpg", true) == 0)
                ContentType = "jpeg";
            else
                ContentType = Type[Type.Length - 1].ToLower();

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "image/" + ContentType;
                    byte[] result = client.UploadFile("https://frogcroak.azurewebsites.net/api/ImageApi/UploadImage", FilePath);
                    string Frog = JsonConvert.DeserializeObject<string>(Encoding.Default.GetString(result));
                    return new AllRequestResult
                    {
                        IsSuccess = true,
                        Result = Frog
                    };
                }
            }
            catch (WebException ex)
            {
                return new AllRequestResult
                {
                    IsSuccess = false,
                    Result = ex
                };
            }
        }

        public async void SendMessage()
        {
            HideKeyboard();
            Activity_Outer.RequestFocus();
            string Content = et_Message.Text;
            et_Message.Text = "";
            if (Content.Trim() == "")
            {
                Toast.MakeText(this, "請輸入內容", ToastLength.Short).Show();
                return;
            }

            tv_RMessage.Text = Content;
            tv_RMessage.Visibility = ViewStates.Visible;
            iv_Frog.Visibility = ViewStates.Gone;

            tv_LMessage.Text = "辨識中...";

            AllRequestResult result = null;
            await Task.Run(() =>
            {
                result = CreateMessage(Content);
            });

            if (result.IsSuccess)
            {
                string ResMessage = (string)result.Result;
                ResMessage = ResMessage.Replace("\\n", "\n");
                tv_LMessage.Text = ResMessage;
            }
            else
            {
                tv_LMessage.Text = "辨識失敗";
                WebExceptionHandler((WebException)result.Result);
            }
        }

        public AllRequestResult CreateMessage(string Content)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string result = client.UploadString(
                        "https://frogcroak.azurewebsites.net/api/MessageApi/CreateMessage",
                        $"Content={Content}"
                    );

                    result = JsonConvert.DeserializeObject<string>(result);

                    return new AllRequestResult
                    {
                        IsSuccess = true,
                        Result = result
                    };
                }
            }
            catch (WebException ex)
            {
                return new AllRequestResult
                {
                    IsSuccess = false,
                    Result = ex
                };
            }
        }

        string CreateFileAndGetFileNameFromUri(Android.Net.Uri Uri)
        {
            string FileName = "";
            if (Uri.ToString().StartsWith("content://"))
            {
                using (var cursor = ContentResolver.Query(Uri, null, null, null, null))
                {
                    cursor.MoveToFirst();
                    FileName = cursor.GetString(cursor.GetColumnIndex(OpenableColumns.DisplayName));
                }
            }
            else
            {
                FileName = new Java.IO.File(Uri.ToString()).Name;
            }

            //將檔案寫進 App 內
            using (Stream input = ContentResolver.OpenInputStream(Uri))
            {
                using (var FileStream = File.Create(FilesDir.AbsolutePath + "/" + FileName))
                {
                    input.CopyTo(FileStream);
                }
            }

            return FileName;
        }

        void WebExceptionHandler(WebException exception)
        {
            if (exception.Status == WebExceptionStatus.ProtocolError && exception.Response != null)
            {
                var response = (HttpWebResponse)exception.Response;
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var content = reader.ReadToEnd();
                        Toast.MakeText(this, content, ToastLength.Short).Show();
                    }
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    new Android.Support.V7.App.AlertDialog.Builder(this)
                            .SetTitle("錯誤")
                            .SetMessage("伺服器錯誤，請聯絡開發人員")
                            .SetPositiveButton("好的", delegate
                            {
                            })
                            .Show();
                }
            }
            else
            {
                Toast.MakeText(this, "請檢察網路連線", ToastLength.Short).Show();
            }
        }

        void HideKeyboard()
        {
            InputMethodManager inputMethodManager = (InputMethodManager)GetSystemService(InputMethodService);

            try
            {
                inputMethodManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
        }

    }
}