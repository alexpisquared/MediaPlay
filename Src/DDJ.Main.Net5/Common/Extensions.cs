using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DDJ.Main.Cmn
{
  public static class ObservableCollectionEx
  {
    public static void ClearAddRange<T>(this ObservableCollection<T> trg, IEnumerable<T> src)
    {
      try
      {
        trg.Clear();
        src.ToList().ForEach(trg.Add);
      }
      catch (Exception) { if (Debugger.IsAttached) Debugger.Break(); else throw; }
    }
    public static void ClearAddRangeDisp<T>(this ObservableCollection<T> source, IEnumerable<T> range)
    {
      var dispatcher = Application.Current.Dispatcher;

      dispatcher.BeginInvoke(new Action(() =>
      {
        source.Clear();
        range.ToList().ForEach(source.Add);
      }));
    }
  }
}
