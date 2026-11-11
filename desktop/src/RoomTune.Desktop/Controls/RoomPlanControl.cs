using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RoomTune.Core.Models;
using RoomTune.Core.Services;

namespace RoomTune.Desktop.Controls
{
    public class RoomPlanControl : FrameworkElement
    {
        private const double Pad = 30;
        private RoomProject _project;
        private object _dragTarget;
        private double _dpi = 1.0;

        public event Action ProjectChanged;

        public RoomProject Project
        {
            get { return _project; }
            set { _project = value; InvalidateVisual(); }
        }

        public RoomPlanControl()
        {
            Focusable = true;
            Cursor = Cursors.Hand;
            ClipToBounds = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            _dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));
            if (Project == null || ActualWidth < 100 || ActualHeight < 100) return;

            var room = Project.Room;
            double scale = Math.Min((ActualWidth - Pad * 2) / Math.Max(0.1, room.WidthM),
                                    (ActualHeight - Pad * 2) / Math.Max(0.1, room.LengthM));
            double offsetX = (ActualWidth - room.WidthM * scale) / 2.0;
            double offsetY = (ActualHeight - room.LengthM * scale) / 2.0;

            var roomRect = new Rect(offsetX, offsetY, room.WidthM * scale, room.LengthM * scale);
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(248, 250, 252)), new Pen(new SolidColorBrush(Color.FromRgb(203, 213, 225)), 1), roomRect);

            var gridPen = new Pen(new SolidColorBrush(Color.FromRgb(232, 237, 243)), 0.8);
            for (double x = 0.5; x < room.WidthM; x += 0.5)
            {
                double px = offsetX + x * scale;
                dc.DrawLine(gridPen, new Point(px, roomRect.Top), new Point(px, roomRect.Bottom));
            }
            for (double y = 0.5; y < room.LengthM; y += 0.5)
            {
                double py = offsetY + y * scale;
                dc.DrawLine(gridPen, new Point(roomRect.Left, py), new Point(roomRect.Right, py));
            }

            var reflectionPen = new Pen(new SolidColorBrush(Color.FromRgb(203, 213, 225)), 1);
            foreach (var reflection in AcousticsEngine.ComputeReflections(Project))
            {
                var p = Map(reflection.X, reflection.Y, offsetX, offsetY, scale);
                dc.DrawLine(reflectionPen, new Point(p.X - 4, p.Y - 4), new Point(p.X + 4, p.Y + 4));
                dc.DrawLine(reflectionPen, new Point(p.X - 4, p.Y + 4), new Point(p.X + 4, p.Y - 4));
            }

            var speakerBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            var subBrush = new SolidColorBrush(Color.FromRgb(217, 119, 6));
            var speakerPen = new Pen(new SolidColorBrush(Color.FromRgb(29, 78, 216)), 1.5);
            foreach (var speaker in Project.System.Speakers)
            {
                var p = Map(speaker.X, speaker.Y, offsetX, offsetY, scale);
                var brush = speaker.IsSubwoofer ? subBrush : speakerBrush;
                if (speaker.IsSubwoofer)
                {
                    dc.DrawRectangle(brush, new Pen(Brushes.White, 1.5), new Rect(p.X - 8, p.Y - 8, 16, 16));
                }
                else
                {
                    dc.DrawEllipse(brush, new Pen(Brushes.White, 1.5), p, 8, 8);
                }
                var listener = Map(room.ListenerX, room.ListenerY, offsetX, offsetY, scale);
                var angle = Math.Atan2(listener.Y - p.Y, listener.X - p.X);
                var end = new Point(p.X + Math.Cos(angle) * 24, p.Y + Math.Sin(angle) * 24);
                dc.DrawLine(new Pen(brush, 1.2), p, end);
                DrawText(dc, speaker.Role, new Point(p.X - 20, p.Y + 11), 11, new SolidColorBrush(Color.FromRgb(51, 65, 85)));
            }

            var listenerPoint = Map(room.ListenerX, room.ListenerY, offsetX, offsetY, scale);
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(14, 116, 144)), new Pen(Brushes.White, 2), listenerPoint, 10, 10);
            DrawText(dc, "听音位", new Point(listenerPoint.X - 22, listenerPoint.Y + 13), 11, new SolidColorBrush(Color.FromRgb(14, 116, 144)));

            DrawText(dc, "蓝 = 音箱 · 橙 = 低音炮 · 深青 = 听音位 · × = 第一反射点", new Point(12, ActualHeight - 20), 10, new SolidColorBrush(Color.FromRgb(100, 116, 139)));
            DrawText(dc, string.Format("{0:0.0} × {1:0.0} m", room.WidthM, room.LengthM), new Point(12, 10), 11, new SolidColorBrush(Color.FromRgb(71, 85, 105)));
        }

        private Point Map(double x, double y, double offsetX, double offsetY, double scale)
        {
            return new Point(offsetX + x * scale, offsetY + y * scale);
        }

        private void DrawText(DrawingContext dc, string text, Point origin, double size, Brush brush)
        {
            var formatted = new FormattedText(text ?? "", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), size, brush, _dpi);
            dc.DrawText(formatted, origin);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (Project == null) return;
            var pos = e.GetPosition(this);
            var room = Project.Room;
            double scale = Math.Min((ActualWidth - Pad * 2) / Math.Max(0.1, room.WidthM),
                                    (ActualHeight - Pad * 2) / Math.Max(0.1, room.LengthM));
            double offsetX = (ActualWidth - room.WidthM * scale) / 2.0;
            double offsetY = (ActualHeight - room.LengthM * scale) / 2.0;

            var listenerPoint = Map(room.ListenerX, room.ListenerY, offsetX, offsetY, scale);
            if ((listenerPoint - pos).Length < 20)
            {
                _dragTarget = "listener";
                CaptureMouse();
                return;
            }
            foreach (var speaker in Project.System.Speakers)
            {
                var p = Map(speaker.X, speaker.Y, offsetX, offsetY, scale);
                if ((p - pos).Length < 18)
                {
                    _dragTarget = speaker;
                    CaptureMouse();
                    return;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragTarget == null || Project == null) return;
            var pos = e.GetPosition(this);
            var room = Project.Room;
            double scale = Math.Min((ActualWidth - Pad * 2) / Math.Max(0.1, room.WidthM),
                                    (ActualHeight - Pad * 2) / Math.Max(0.1, room.LengthM));
            double offsetX = (ActualWidth - room.WidthM * scale) / 2.0;
            double offsetY = (ActualHeight - room.LengthM * scale) / 2.0;
            double x = Math.Max(0, Math.Min(room.WidthM, (pos.X - offsetX) / scale));
            double y = Math.Max(0, Math.Min(room.LengthM, (pos.Y - offsetY) / scale));
            if (_dragTarget is string && (string)_dragTarget == "listener")
            {
                room.ListenerX = x;
                room.ListenerY = y;
            }
            else
            {
                var speaker = _dragTarget as SpeakerPosition;
                if (speaker != null)
                {
                    speaker.X = x;
                    speaker.Y = y;
                }
            }
            InvalidateVisual();
            var handler = ProjectChanged;
            if (handler != null) handler();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            _dragTarget = null;
            ReleaseMouseCapture();
        }
    }
}
