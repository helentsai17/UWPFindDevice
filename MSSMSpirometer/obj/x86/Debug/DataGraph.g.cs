﻿#pragma checksum "\\users9\users9$\tsait06\Data\Personal\DesktopFiles\UWPFindDevice\MSSMSpirometer\DataGraph.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6113791093BD1F881E034B146BCA58F0"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MSSMSpirometer
{
    partial class DataGraph : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1: // DataGraph.xaml line 1
                {
                    this.page = (global::Windows.UI.Xaml.Controls.Page)(target);
                }
                break;
            case 2: // DataGraph.xaml line 63
                {
                    global::Windows.UI.Xaml.Controls.Grid element2 = (global::Windows.UI.Xaml.Controls.Grid)(target);
                    ((global::Windows.UI.Xaml.Controls.Grid)element2).Loading += this.Page_Load;
                }
                break;
            case 3: // DataGraph.xaml line 64
                {
                    this.LineChart = (global::WinRTXamlToolkit.Controls.DataVisualization.Charting.Chart)(target);
                }
                break;
            case 4: // DataGraph.xaml line 72
                {
                    this.LineChartCurve = (global::WinRTXamlToolkit.Controls.DataVisualization.Charting.Chart)(target);
                }
                break;
            case 5: // DataGraph.xaml line 49
                {
                    this.dataoftest = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 6: // DataGraph.xaml line 50
                {
                    this.testindex = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 7: // DataGraph.xaml line 51
                {
                    this.testrank = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 8: // DataGraph.xaml line 52
                {
                    this.testqualityflags = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 9: // DataGraph.xaml line 53
                {
                    this.acceptabilityflags = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 10: // DataGraph.xaml line 54
                {
                    this.fetg = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 11: // DataGraph.xaml line 55
                {
                    this.fvcg = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 12: // DataGraph.xaml line 56
                {
                    this.expiratorycurve = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 13: // DataGraph.xaml line 57
                {
                    this.inspiratorytime = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 14: // DataGraph.xaml line 58
                {
                    this.fivcg = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 15: // DataGraph.xaml line 59
                {
                    this.inspiratorycurve = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

