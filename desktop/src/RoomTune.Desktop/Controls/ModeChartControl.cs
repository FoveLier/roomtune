using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using RoomTune.Core.Models;

namespace RoomTune.Desktop.Controls
{
    public class ModeChartControl : FrameworkElement
    {
        private IList<RoomMode> _modes = new List<RoomMode>();
        private double _dpi = 1.0;

        public IList<RoomMode> Modes
        {
            get { return _modes; }
            set { _modes = value ?? new List<RoomMode>(); InvalidateVisual(); }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            _dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
            if (ActualWidth < 100 || ActualHeight < 80) return;
            double left = 42, right = 14, top = 18, bottom = 30;
            double w = ActualWidth - left - right;
            double h = ActualHeight - top - bottom;
            var axisPen = new Pen(new SolidColorBrush(Color.FromRgb(203, 213, 225)), 1);
            var gridPen = new Pen(new SolidColorBrush(Color.FromRgb(235, 239, 244)), 1);
            dc.DrawLine(axisPen, new Point(left, top), new Point(left, top + h));
            dc.DrawLine(axisPen, new Point(left, top + h), new Point(left + w, top + h));

            double min = Math.Log10(16);
            double max = Math.Log10(300);
            double X(double freq) { return left + (Math.Log10(Math.Max(16, Math.Min(300, freq))) - min) / (max - min) * w; }

            foreach (var tick in new[] { 20.0, 50.0, 100.0, 200.0, 300.0 })
            {
                double x = X(tick);
                dc.DrawLine(gridPen, new Point(x, top), new Point(x, top + h));
                DrawText(dc, tick.ToString("0"), new Point(x - 8, top + h + 6), 10, new SolidColorBrush(Color.FromRgb(100, 116, 139)));
            }
            for (int db = 0; db <= 4; db++)
            {
                double y = top + h - db / 4.0 * h;
                dc.DrawLine(gridPen, new Point(left, y), new Point(left + w, y));
            }
            DrawText(dc, "模态密度", new Point(left, 3), 11, new SolidColorBrush(Color.FromRgb(71, 85, 105)));

            int maxCount = 1;
            var buckets = new List<Tuple<double, double, int>>();
            foreach (var group in _modes.GroupBy(item => Math.Round(item.FrequencyHz / 5.0) * 5.0).OrderBy(item => item.Key))
            {
                buckets.Add(Tuple.Create(group.Key, (double)group.Count(), group.Count()));
                maxCount = Math.Max(maxCount, group.Count());
            }
            var brushAxial = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            var brushTangential = new SolidColorBrush(Color.FromRgb(217, 119, 6));
            var brushOblique = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            foreach (var bucket in buckets)
            {
                double x = X(bucket.Item1);
                double barH = Math.Max(3, bucket.Item3 / (double)maxCount * (h - 10));
                var mode = _modes.FirstOrDefault(item => Math.Abs(item.FrequencyHz - bucket.Item1) < 2.6);
                var brush = mode != null && mode.Type == "轴向" ? brushAxial : mode != null && mode.Type == "切向" ? brushTangential : brushOblique;
                dc.DrawRectangle(brush, null, new Rect(x - 2, top + h - barH, 4, barH));
            }
            if (!_modes.Any())
                DrawText(dc, "暂无模态数据", new Point(left + 12, top + h / 2), 12, Brushes.Gray);
        }

        private void DrawText(DrawingContext dc, string text, Point point, double size, Brush brush)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), size, brush, _dpi);
            dc.DrawText(formatted, point);
        }
    }
}
