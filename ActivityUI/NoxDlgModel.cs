using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BitfiWallet
{
 public class NoxDlgModel
 {

  Activity _activity;

  public AlertDialog alertDialog1;
  public AlertDialog alertDialog2;

  public AlertDialog mdlDialog;
  public NoxDlgModel(Activity activity)
  {
   _activity = activity;
  }

  public void ShowSimplesDlg(string[] List, int Default, string Title, string PBtn, string NBtn, Action PAction, out SheetData data)
  {

   AlertDialog.Builder wfbuilder = new AlertDialog.Builder(_activity).SetTitle(Title)
     .SetSingleChoiceItems(List, Default, new PeerDlgActions(List, out data))
     .SetPositiveButton(PBtn, (EventHandler<DialogClickEventArgs>)null)
     .SetNegativeButton(NBtn, (EventHandler<DialogClickEventArgs>)null);

   AlertDialog xdalertDialog = wfbuilder.Create();

   try
   {
    xdalertDialog.Show();

    var wfnoBtn = xdalertDialog.GetButton((int)DialogButtonType.Negative);
    wfnoBtn.Click += (asender, args) =>
    {
     xdalertDialog.Dismiss();
    };


    var wfOKBtn = xdalertDialog.GetButton((int)DialogButtonType.Positive);
    wfOKBtn.Click += (asender, args) =>
    {

     PAction();
     xdalertDialog.Dismiss();
    };
   }
   catch { }

  }

  public void ShowMdlDlg(string[] List, string[] DisplayList, int Default, string Title, string PBtn, string NBtn, VibrationEffect vibeEffect, Vibrator v, Action NAction, Action PAction, out SheetData data)
  {

   try
   {
    if (mdlDialog != null && mdlDialog.IsShowing)
    {
     mdlDialog.Dismiss();
    }
   }
   catch { }

   v.Vibrate(vibeEffect);

   AlertDialog.Builder wfbuilder = new AlertDialog.Builder(_activity, Resource.Style.MyAlertDialogThemeNox).SetTitle(Title)
     .SetSingleChoiceItems(DisplayList, Default, new PeerDlgActions(List, out data))
     .SetPositiveButton(PBtn, (EventHandler<DialogClickEventArgs>)null)
     .SetNegativeButton(NBtn, (EventHandler<DialogClickEventArgs>)null);

   mdlDialog = wfbuilder.Create();

   try
   {
    mdlDialog.Show();

    var wfnoBtn = mdlDialog.GetButton((int)DialogButtonType.Negative);
    wfnoBtn.Click += (asender, args) =>
    {
     NAction();
     mdlDialog.Dismiss();
    };


    var wfOKBtn = mdlDialog.GetButton((int)DialogButtonType.Positive);
    wfOKBtn.Click += (asender, args) =>
    {

     PAction();
     mdlDialog.Dismiss();
    };
   }
   catch { }

  }

  public AlertDialog dlgNoxNotice;

  public void ShowNoxNotice(string title_notice, string message_notice, string btn_a_notice, string btn_b_notice, Action btn_a_action, Action btn_b_action)
  {

   if (dlgNoxNotice != null && dlgNoxNotice.IsShowing) dlgNoxNotice.Dismiss();

   AlertDialog.Builder wfbuilder = new AlertDialog.Builder(_activity, Resource.Style.MyAlertDialogThemeNox).SetTitle(title_notice).SetMessage(message_notice).SetCancelable(false)
     .SetPositiveButton(btn_a_notice, (EventHandler<DialogClickEventArgs>)null)
     .SetNegativeButton(btn_b_notice, (EventHandler<DialogClickEventArgs>)null);

   dlgNoxNotice = wfbuilder.Create();

   try
   {
    dlgNoxNotice.Show();

    TextView msgTxt = (TextView)dlgNoxNotice.FindViewById(Android.Resource.Id.Message);
    msgTxt.TextSize = 18;
    msgTxt.SetTextColor(new Color(200, 228, 230));
    msgTxt.SetLineSpacing(0, 1.2f);
    //   msgTxt.LetterSpacing = 0.08f;
    msgTxt.JustificationMode = Android.Text.JustificationMode.InterWord;
    msgTxt.SetPadding(msgTxt.PaddingLeft, msgTxt.PaddingTop + 20, msgTxt.PaddingRight, msgTxt.PaddingBottom + 40);
    msgTxt.SetTypeface(Typeface.Default, TypefaceStyle.Normal);

    var wfbtn_a_notice = dlgNoxNotice.GetButton((int)DialogButtonType.Positive);

    wfbtn_a_notice.Click += (asender, args) =>
    {
     btn_a_action();
         ///  dlg.Dismiss();
        };

    var wfbtn_b_notice = dlgNoxNotice.GetButton((int)DialogButtonType.Negative);

    wfbtn_b_notice.Click += (asender, args) =>
    {
     btn_b_action();
         /// dlgNoxNotice.Dismiss();
        };
   }
   catch { }

  }

  public void ShowNoxDlg(string[] List, int Default, string Title, string PBtn, string NBtn, Action PAction, Action NAction, out SheetData data)
  {

   AlertDialog.Builder wfbuilder = new AlertDialog.Builder(_activity, Resource.Style.MyAlertDialogThemeNox).SetTitle(Title).SetCancelable(false)
     .SetSingleChoiceItems(List, Default, new SheetActions(List, out data))
     .SetPositiveButton(PBtn, (EventHandler<DialogClickEventArgs>)null)
     .SetNegativeButton(NBtn, (EventHandler<DialogClickEventArgs>)null);


   alertDialog1 = wfbuilder.Create();

   try
   {
    alertDialog1.Show();

    var wfnoBtn = alertDialog1.GetButton((int)DialogButtonType.Negative);

    wfnoBtn.Click += (asender, args) =>
    {
     NAction();
    };


    var wfOKBtn = alertDialog1.GetButton((int)DialogButtonType.Positive);

    wfOKBtn.Click += (asender, args) =>
    {
     PAction();
    };
   }
   catch { }

  }

  public void ShowWaitMsg(string Msg, string Title)
  {

   AlertDialog.Builder builder = new AlertDialog.Builder(_activity, Resource.Style.MyAlertDialogThemeNox).SetMessage(Msg).SetTitle(Title)
     .SetCancelable(false);

   alertDialog2 = builder.Create();

   try
   {
    alertDialog2.Show();

    if (!string.IsNullOrEmpty(Msg))
    {
     TextView msgTxt = (TextView)alertDialog2.FindViewById(Android.Resource.Id.Message);
     msgTxt.TextSize = 16;
     msgTxt.SetTextColor(new Color(200, 228, 230));
     msgTxt.SetPadding(msgTxt.PaddingLeft, msgTxt.PaddingTop + 20, msgTxt.PaddingRight, msgTxt.PaddingBottom + 40);
     msgTxt.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
    }

   }
   catch { }
  }

  public void ShowErrorMsg(string Msg)
  {

   AlertDialog.Builder builder = new AlertDialog.Builder(_activity, Resource.Style.MyAlertDialogThemeNox).SetMessage(Msg)
     .SetCancelable(false)
     .SetPositiveButton("BACK", (EventHandler<DialogClickEventArgs>)null);

   var alertDialog = builder.Create();

   try
   {
    alertDialog.Show();



    if (!string.IsNullOrEmpty(Msg))
    {
     TextView msgTxt = (TextView)alertDialog.FindViewById(Android.Resource.Id.Message);
     msgTxt.TextSize = 16;
     msgTxt.SetTextColor(new Color(200, 228, 230));
     msgTxt.SetLineSpacing(0, 1.2f);
     msgTxt.LetterSpacing = 0.08f;
     msgTxt.JustificationMode = Android.Text.JustificationMode.InterWord;
     msgTxt.SetPadding(msgTxt.PaddingLeft, msgTxt.PaddingTop + 20, msgTxt.PaddingRight, msgTxt.PaddingBottom + 40);
     msgTxt.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
    }


    var wfOKBtn = alertDialog.GetButton((int)DialogButtonType.Positive);

    wfOKBtn.Click += (asender, args) =>
    {
     try
     {

      alertDialog2.Dismiss();
     }
     catch { }
     alertDialog.Dismiss();
    };

   }
   catch { }


  }
  public class SheetData
  {
   public string[] Items { get; set; }
   public int Selected { get; set; }
  }


  class PeerDlgActions : Java.Lang.Object, IDialogInterfaceOnClickListener
  {
   SheetData sheetData;
   public PeerDlgActions(string[] Items, out SheetData data)
   {
    sheetData = new SheetData();
    sheetData.Items = Items;
    sheetData.Selected = -1;
    data = sheetData;
   }
   public void OnClick(IDialogInterface dialog, int which)
   {
    if (which > -1)
    {
     sheetData.Selected = which;
    }
   }
  }
  class SheetActions : Java.Lang.Object, IDialogInterfaceOnClickListener
  {
   SheetData sheetData;
   public SheetActions(string[] Items, out SheetData data)
   {
    sheetData = new SheetData();
    sheetData.Items = Items;
    sheetData.Selected = 0;
    data = sheetData;
   }
   public void OnClick(IDialogInterface dialog, int which)
   {
    if (which > -1)
    {
     sheetData.Selected = which;
    }
   }
  }

 }

}