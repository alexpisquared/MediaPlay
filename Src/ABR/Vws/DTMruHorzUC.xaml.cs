using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace ABR.Vws
{
  public sealed partial class DTMruHorzUC : UserControl
  {
    const int halfSize = 28;
    public DTMruHorzUC() { InitializeComponent(); DataContextChanged += (s, e) => Bindings.Update(); }
    public VpxCmn.Model.MediaInfoDto Mid => DataContext as VpxCmn.Model.MediaInfoDto;

    void ProgressBar_ValueChanged(object s, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
      var pb = (ProgressBar)s;
      if (pb.Maximum == 0) return;
      DrawArc(arc_pathBig, new Point(halfSize, halfSize), halfSize - 1 - arc_pathBig.StrokeThickness / 2, 0, Math.PI * 2 * pb.Value / pb.Maximum);
      DrawArc(arc_pathSmr, new Point(halfSize, halfSize), halfSize - 1 - arc_pathSmr.StrokeThickness / 2, 0, Math.PI * 2 * pb.Value / pb.Maximum);
    }

    public void DrawArc(Path arc_path, Point center, double radius, double start_angle, double enndd_angle)
    {
      start_angle = ((start_angle % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
      enndd_angle = ((enndd_angle % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
      if (enndd_angle < start_angle)
      {
        var temp_angle = enndd_angle;
        enndd_angle = start_angle;
        start_angle = temp_angle;
      }
      var angle_diff = enndd_angle - start_angle;

      var pathGeometry = new PathGeometry();
      var pathFigure = new PathFigure();
      var arcSegment = new ArcSegment
      {
        IsLargeArc = angle_diff >= Math.PI
      };
      //Set start of arc
      pathFigure.StartPoint = new Point(center.X + radius * Math.Cos(start_angle), center.Y + radius * Math.Sin(start_angle));
      //set end point of arc.
      arcSegment.Point = new Point(center.X + radius * Math.Cos(enndd_angle), center.Y + radius * Math.Sin(enndd_angle));
      arcSegment.Size = new Size(radius, radius);
      arcSegment.SweepDirection = SweepDirection.Clockwise;

      pathFigure.Segments.Add(arcSegment);
      pathGeometry.Figures.Add(pathFigure);
      arc_path.Data = pathGeometry;
    }
  }
}