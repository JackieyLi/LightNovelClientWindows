﻿

#pragma checksum "C:\Users\Yupeng\Source\Workspaces\LightNovelClientWindows\LightNovelClient\LightNovelClient.Shared\Controls\RichTextView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E19B556EF74B5F5FEBBC3F8C285503BF"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LightNovel.Controls
{
    partial class RichTextView : global::Windows.UI.Xaml.Controls.UserControl, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 13 "..\..\Controls\RichTextView.xaml"
                ((global::Windows.UI.Xaml.Controls.ScrollViewer)(target)).ViewChanged += this.ContentScrollViewer_ViewChanged;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

