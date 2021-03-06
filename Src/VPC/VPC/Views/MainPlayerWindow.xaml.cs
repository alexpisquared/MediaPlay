﻿using AsLink;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VPC.ViewModels;

namespace VPC
{
  public partial class MainPlayerWindow : AAV.WPF.Base.WindowBase
  {
    bool _isLocationChanged = false;

    public MainPlayerWindow(IVPViewModel vm) : this() => DataContext = vm; //DI - Dependency Injeciton (1):
    public MainPlayerWindow()
    {
      InitializeComponent();
      LocationChanged += (s, e) => _isLocationChanged = true;
      MouseLeftButtonUp += (s, e) => { base.OnMouseLeftButtonUp(e); if (!_isLocationChanged) (DataContext as VPViewModel).TglPlyPsCommand.Execute(this); }; //tu:
      MouseLeftButtonDown += (s, e) => { base.OnMouseLeftButtonDown(e); _isLocationChanged = false; };
      MouseMove += onMouseMove;
      NameScope.SetNameScope(cm, NameScope.GetNameScope(this)); //tu: mvvm menu & visual tree
    } // using AsLink.UI;

    void onMouseMove(object sender, MouseEventArgs e) => ((IVPViewModel)DataContext).FlashAllControlls();
    void wdw_Drop_1(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

      var files = e.Data.GetData(DataFormats.FileDrop) as string[];
      if (files.Length < 1) return;

      var csv = string.Join("|", files);      //if (ex.KeyStates == DragDropKeyStates.ControlKey)			//	m.LoadNewMedia(csv);//TODO: Add to the curent list			//else			//	m.LoadNewMedia(csv);

      (DataContext as VPViewModel).PlayNewFile(csv);
    }
    void showContextMenu(object sender, MouseButtonEventArgs e)
    {
      cm.PlacementTarget = this;
      cm.IsOpen = true;
      //todo: send pause cmd
    }

    void wmp_MediaOpened(object s, RoutedEventArgs e) => ChromeGird.Style = Application.Current.TryFindResource(MediaHelper.IsAudio(((MediaElement)s).Source.AbsolutePath) ? "DefaultStyle12345" : "FadeInOnMouseMove") as Style;
  }
}
