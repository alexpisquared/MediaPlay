﻿using AsLink;
//using AsLink.UI;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using VPC.ViewModels;

namespace VPC
{
    public partial class MainPlayerWindow : AAV.WPF.Base.WindowBase
  {
        bool isLocationChanged = false;

        public MainPlayerWindow(IVPViewModel vm) : this() => DataContext = vm; //DI - Dependency Injeciton (1):
        public MainPlayerWindow()
        {
            InitializeComponent();
            LocationChanged += (s, e) => isLocationChanged = true;
            MouseLeftButtonUp += (s, e) => { base.OnMouseLeftButtonUp(e); if (!isLocationChanged) (this.DataContext as VPViewModel).TglPlyPsCommand.Execute(this); }; //tu:
            MouseLeftButtonDown += (s, e) => { base.OnMouseLeftButtonDown(e); isLocationChanged = false; };
            MouseMove += onMouseMove;
            NameScope.SetNameScope(cm, NameScope.GetNameScope(this)); //tu: mvvm menu & visual tree
        } // using AsLink.UI;

        void onMouseMove(object sender, MouseEventArgs e) => ((IVPViewModel)DataContext).FlashAllControlls();
        void wdw_Drop_1(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files.Length < 1) return;

            var csv = string.Join("|", files);      //if (ex.KeyStates == DragDropKeyStates.ControlKey)			//	m.LoadNewMedia(csv);//TODO: Add to the curent list			//else			//	m.LoadNewMedia(csv);

            (this.DataContext as VPViewModel).PlayNewFile(csv);
        }
        void showContextMenu(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            cm.PlacementTarget = this;
            cm.IsOpen = true;
            //todo: send pause cmd
        }
        //[Microsoft.Practices.Unity.Dependency]		//public IVPViewModel VM { set { DataContext = value; } } //DI - Dependency Injeciton (2)
    }
}
