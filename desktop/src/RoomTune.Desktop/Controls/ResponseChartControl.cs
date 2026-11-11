using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using RoomTune.Core.Models;

namespace RoomTune.Desktop.Controls
{
    public class ResponseChartControl : FrameworkElement
    {
        private MeasurementData _measurement;
        private double _dpi = 1.0;

        public MeasurementData Measurement
        {
            get { return _measurement; }
            set { _measurement = value; InvalidateVisual(); }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            _dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
            if (ActualWidth < 100 || ActualHeight < 80) return;
            double left = 46, right = 16, top = 22, bottom = 30;
            double w = ActualWidth - left - right;
            double h = ActualHeight - top - bottom;
            var axisPen = new Pen(new SolidColorBrush(Color.FromRgb(203, 213, 225)), 1);
            var gridPen = new Pen(new SolidColorBrush(Color.FromRgb(235, 239, 244)), 1);
            dc.DrawLine(axisPen, new Point(left, top), new Point(left, top + h));
            dc.DrawLine(axisPen, new Point(left, top + h), new Point(left + w, top + h));
            var points = _measurement == null || _measurement.Points == null
                ? new List<FrequencyPoint>()
                : _measurement.Points.Where(item => item.FrequencyHz >= 20 && item.FrequencyHz <= 20000).OrderBy(item => item.FrequencyHz).ToList();

            if (points.Count == 0)
            {
                DrawText(dc, "尚未导入 REW CSV", new Point(left + 16, top + h / 2 - 10), 14, new SolidColorBrush(Color.FromRgb(100, 116, 139)));
                DrawText(dc, "在“测量”页导入“频率, dB”格式的 REW 导出文件", new Point(left + 16, top + h / 2 + 12), 11, new SolidColorBrush(Color.FromRgb(148, 163, 184)));
                return;
            }

            double minDb = Math.Floor(points.Min(item => item.SplDb) / 5.0) * 5.0 - 5;
            double maxDb = Math.Ceiling(points.Max(item => item.SplDb) / 5.0) * 5.0 + 5;
            if (maxDb - minDb < 10) maxDb = minDb + 10;
            double minLog = Math.Log10(20), maxLog = Math.Log10(20000);
            double X(double f) { return left + (Math.Log10(Math.Max(20, Math.Min(20000, f))) - minLog) / (maxLog - minLog) * w; }
            double Y(double db) { return top + h - (db - minDb) / (maxDb - minDb) * h; }

            foreach (var f in new[] { 20.0, 50.0, 100.0, 200.0, 500.0, 1000.0, 2000.0, 5000.0, 10000.0, 20000.0 })
            {
                double x = X(f);
                dc.DrawLine(gridPen, new Point(x, top), new Point(x, top + h));
                DrawText(dc, f >= 1000 ? (f / 1000).ToString("0") + "k" : f.ToString("0"), new Point(x - 7, top + h + 6), 9, new SolidColorBrush(Color.FromRgb(100, 116, 139)));
            }
            for (double db = minDb; db <= maxDb; db += 10)
            {
                double y = Y(db);
                dc.DrawLine(gridPen, new Point(left, y), new Point(left + w, y));
                DrawText(dc, db.ToString("0"), new Point(8, y - 6), 9, new SolidColorBrush(Color.FromRgb(100, 116, 139)));
            }
            DrawText(dc, "dB", new Point(8, 4), 10, new SolidColorBrush(Color.FromRgb(100, 116, 139)));
            DrawText(dc, "Hz", new Point(left + w - 10, top + h + 6), 10, new SolidColorBrush(Color.FromRgb(100, 116, 139)));

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                bool first = true;
                foreach (var point in points)
                {
                    var p = new Point(X(point.FrequencyHz), Y(point.SplDb));
                    if (first) { ctx.BeginFigure(p, false, false); first = false; }
                    else ctx.LineTo(p, true, false);
                }
            }
            geometry.Freeze();
            dc.DrawGeometry(null, new Pen(new SolidColorBrush(Color.FromRgb(37, 99, 235)), 2), geometry);
        }

        private void DrawText(DrawingContext dc, string text, Point point, double size, Brush brush)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), size, brush, _dpi);
            dc.DrawText(formatted, point);
        }
    }
}
