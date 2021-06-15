
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

namespace BitfiWallet
{
 [Activity(Name = "com.rokits.noxadmin.BrightnessActivity", Label = "")]
 public class BrightnessActivity : Activity
 {
  protected override void OnCreate(Bundle savedInstanceState)
  {

   SetContentView(Resource.Layout.brightness);
   base.OnCreate(savedInstanceState);

  }

  bool BrightnessStarted = false;

  protected override void OnResume()
  {

   base.OnResume();

   OverridePendingTransition(0, 0);

   if (BrightnessStarted)
   {

    Finish();
   }
   else
   {
    ComponentName componentName = new ComponentName("com.android.systemui", "com.android.systemui.settings.BrightnessDialog");
    Intent intent = new Intent();
    intent.SetComponent(componentName);
    StartActivity(intent);

    BrightnessStarted = true;

   }
  }

 }
}
