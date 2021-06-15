using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using Android.Graphics;
using Android.Views;
using Android.Text;
using Android.Webkit;
using System.Text;
using Android.Util;
using NoxService.DEVWS;

namespace BitfiWallet
{

 [Activity(Name = "com.rokits.noxadmin.PageActivity", Label = "", Theme = "@style/FullscreenTheme")]
 class PageActivity : Activity
 {
  protected override void OnCreate(Bundle bundle)
  {

   SetContentView(Resource.Layout.htmlmodel);
   base.OnCreate(bundle);


   try
   {

    string task = Intent.GetStringExtra("pmtask");
    string title = Intent.GetStringExtra("title");



    if (!string.IsNullOrEmpty(task))
    {
     LoadPM(task, title);
     return;
    }


    // var ID = Intent.GetStringExtra("ID");
    // DEVWS dEVWS = new DEVWS();

    // var Content = dEVWS.GetArticle(ID);


    // if (string.IsNullOrEmpty(Content.Message))
    // {
    //   Finish();
    //   return;
    // }

    // byte[] body = Convert.FromBase64String(Content.Message);
    // string content = System.Text.Encoding.UTF8.GetString(body);

    // var htm = Html.FromHtml(content, FromHtmlOptions.ModeLegacy | FromHtmlOptions.OptionUseCssColors);

    // ((TextView)FindViewById(Resource.Id.tvhtmltitle)).Text = Content.Title;
    // ((TextView)FindViewById(Resource.Id.tvhtmltitle)).Typeface = Typeface.CreateFromAsset(Assets, "Rubik-Bold.ttf");

    // TextView textView = (TextView)FindViewById(Resource.Id.tvhtml);
    // textView.JustificationMode = JustificationMode.InterWord;
    // textView.SetText(htm, TextView.BufferType.Normal);

   }
   catch
   {
   }
  }

  private void LoadPM(string Content, string Title)
  {
   byte[] body = Convert.FromBase64String(Content);
   string content = System.Text.Encoding.UTF8.GetString(body);

   var htm = Html.FromHtml(content, FromHtmlOptions.ModeCompact | FromHtmlOptions.OptionUseCssColors);

   ((TextView)FindViewById(Resource.Id.tvhtmltitle)).Text = Title;
   ((TextView)FindViewById(Resource.Id.tvhtmltitle)).Typeface = Typeface.CreateFromAsset(Assets, "Rubik-Bold.ttf");

   TextView textView = (TextView)FindViewById(Resource.Id.tvhtml);
   textView.JustificationMode = JustificationMode.None;
   textView.SetText(htm, TextView.BufferType.Normal);

  }



 }


}