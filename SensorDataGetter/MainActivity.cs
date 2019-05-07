using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Hardware;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Android.Support.V7.App;

namespace SensorDataGetter
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : Activity, ISensorEventListener
    {
        Thread trd_saving;
        bool isSaving;
        TextView a0, a1, a2, m0, m1, m2, o0, o1, o2, tv_count;

        Button btn_Save;
        SensorManager sensManager;
        Sensor aSensor, mSensor, oSensor;
        string dipdir;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.RequestWindowFeature(WindowFeatures.ActionBar);
            SetContentView(Resource.Layout.DataGetter);

            isSaving = false;

            a0 = FindViewById<TextView>(Resource.Id.tv_a0);
            a1 = FindViewById<TextView>(Resource.Id.tv_a1);
            a2 = FindViewById<TextView>(Resource.Id.tv_a2);
            m0 = FindViewById<TextView>(Resource.Id.tv_m0);
            m1 = FindViewById<TextView>(Resource.Id.tv_m1);
            m2 = FindViewById<TextView>(Resource.Id.tv_m2);
            o0 = FindViewById<TextView>(Resource.Id.tv_o0);
            o1 = FindViewById<TextView>(Resource.Id.tv_o1);
            o2 = FindViewById<TextView>(Resource.Id.tv_o2);
            tv_count = FindViewById<TextView>(Resource.Id.tv_count);
            btn_Save = FindViewById<Button>(Resource.Id.btn_Save);
            btn_Save.Click += Btn_Save_Click;

            String service_name = Context.SensorService;
            sensManager = (SensorManager)GetSystemService(service_name);

            oSensor = sensManager.GetDefaultSensor(SensorType.Orientation);
            sensManager.RegisterListener(this, oSensor, SensorDelay.Ui);

            aSensor = sensManager.GetDefaultSensor(SensorType.Accelerometer);
            sensManager.RegisterListener(this, aSensor, SensorDelay.Ui);
            mSensor = sensManager.GetDefaultSensor(SensorType.MagneticField);
            sensManager.RegisterListener(this, mSensor, SensorDelay.Ui);

        }

        private void Btn_Save_Click(object sender, EventArgs e)
        {
            if (isSaving)
            {
                btn_Save.Text = "Save";
                isSaving = false;
            }
            else
            {
                btn_Save.Text = "Stop";
                isSaving = true;

                EditText et = new EditText(this);
                Android.App.AlertDialog.Builder dlg = new Android.App.AlertDialog.Builder(this);

                dlg.SetTitle("Save").SetMessage("Please enter this filename").SetView(et).SetPositiveButton("OK", delegate {
                    dipdir = et.Text;
                    trd_saving = new Thread(new ThreadStart(SaveData));
                    Task.Factory.StartNew(() => { SaveData(); });
                }).SetNegativeButton("Cancel", delegate { }).Show();
            }
        }

        private void SaveData()
        {
            Java.IO.File dir = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory, "SensorData");
            if (!dir.Exists())
            {
                dir.Mkdirs();
            }


            string filename = dir + "/" + dipdir + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".csv";
            StreamWriter SW;
            SW = File.CreateText(filename);
            SW.WriteLine("Index,A0,A1,A2,M0,M1,M2,O0,O1,O2,Time");
            int i = 0;
            DateTime now = DateTime.Now;
            do
            {
                now = DateTime.Now;
                i++;
                SW.WriteLine(i.ToString() + "," + a0.Text + "," + a1.Text + "," + a2.Text + "," + m0.Text + "," + m1.Text + "," + m2.Text + "," + o0.Text + "," + o1.Text + "," + o2.Text + "," + now.ToString("yyyy-MM-dd hh:mm:ss"));
                RunOnUiThread(delegate {
                    tv_count.Text = i.ToString();
                });

                Thread.CurrentThread.Join(100);
            } while (isSaving);
            SW.Close();

            MediaScannerConnection.ScanFile(this, new String[] { filename }, null, null);
        }



        protected override void OnDestroy()
        {
            sensManager.UnregisterListener(this, oSensor);
            sensManager.UnregisterListener(this, aSensor);
            sensManager.UnregisterListener(this, mSensor);
            base.OnDestroy();
        }
        protected override void OnPause()
        {
            sensManager.UnregisterListener(this, oSensor);
            sensManager.UnregisterListener(this, aSensor);
            sensManager.UnregisterListener(this, mSensor);
            base.OnPause();
        }
        protected override void OnResume()
        {
            base.OnResume();
            sensManager.RegisterListener(this, oSensor, SensorDelay.Ui);
            sensManager.RegisterListener(this, aSensor, SensorDelay.Ui);
            sensManager.RegisterListener(this, mSensor, SensorDelay.Ui);
        }



        void ISensorEventListener.OnSensorChanged(SensorEvent e)
        {
            switch (e.Sensor.Type)
            {
                case SensorType.Accelerometer:
                    RunOnUiThread(delegate
                    {
                        a0.Text = e.Values[0].ToString();
                        a1.Text = e.Values[1].ToString();
                        a2.Text = e.Values[2].ToString();
                    });
                    break;
                case SensorType.MagneticField:
                    RunOnUiThread(delegate
                    {
                        m0.Text = e.Values[0].ToString();
                        m1.Text = e.Values[1].ToString();
                        m2.Text = e.Values[2].ToString();
                    });

                    break;
                case SensorType.Orientation:
                    RunOnUiThread(delegate
                    {
                        o0.Text = e.Values[0].ToString();
                        o1.Text = e.Values[1].ToString();
                        o2.Text = e.Values[2].ToString();
                    });
                    break;
            }

        }


        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            ;
        }
    }
}

