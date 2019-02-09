using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calc.Controls
{
	public class ShadowBorder : Decorator
	{
		public static DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color),
			typeof(Color), typeof(ShadowBorder),
			new FrameworkPropertyMetadata(Color.FromArgb(76, 0, 0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

		public static DependencyProperty BorderThiknessProperty = DependencyProperty.Register(nameof(BorderThikness),
			typeof(int), typeof(ShadowBorder),
			new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender));

		public Color Color
		{
			get => (Color) GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		public int BorderThikness
		{
			get => (int) GetValue(BorderThiknessProperty);
			set => SetValue(BorderThiknessProperty, value);
		}

		protected override Size ArrangeOverride(Size arrangeSize)
		{
			var thickness = BorderThikness;
			var childRect = new Rect(thickness,
				thickness,
				Math.Max(0.0, arrangeSize.Width - thickness * 2),
				Math.Max(0.0, arrangeSize.Height - thickness * 2));
			Child?.Arrange(childRect);
			return arrangeSize;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			var thickness = BorderThikness;
			var fix = thickness * 2;
			var childConstraint = new Size(Math.Max(0.0, constraint.Width - fix),
				Math.Max(0.0, constraint.Height - fix));
			Child?.Measure(childConstraint);

			var size = Child?.DesiredSize ?? new Size();
			return new Size(size.Width + fix, size.Height + fix);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			var thickness = BorderThikness;
			var color = Color;
			var lefttop = new Point(thickness, thickness);
			var size = new Size(ActualWidth - thickness * 2, ActualHeight - thickness * 2);

			//right
			var stops = new GradientStopCollection(2);
			stops.Add(new GradientStop(color, 0));
			stops.Add(new GradientStop(Colors.Transparent, 1));
			var brush = new LinearGradientBrush(stops, new Point(0, 0.5), new Point(1, 0.5));
			var rightRect = new Rect(lefttop.X + size.Width, lefttop.Y, thickness, size.Height);
			drawingContext.DrawRectangle(brush, null, rightRect);

			//left
			stops = new GradientStopCollection(2);
			stops.Add(new GradientStop(Colors.Transparent, 0));
			stops.Add(new GradientStop(color, 1));
			brush = new LinearGradientBrush(stops, new Point(0, 0.5), new Point(1, 0.5));
			var leftRect = new Rect(lefttop.X - thickness, lefttop.Y, thickness, size.Height);
			drawingContext.DrawRectangle(brush, null, leftRect);

			//top
			stops = new GradientStopCollection(2);
			stops.Add(new GradientStop(Colors.Transparent, 0));
			stops.Add(new GradientStop(color, 1));
			brush = new LinearGradientBrush(stops, new Point(0.5, 0), new Point(0.5, 1));
			drawingContext.DrawRectangle(brush, null,
				new Rect(lefttop.X, lefttop.Y - thickness, size.Width, thickness));

			//bottom
			brush = new LinearGradientBrush(new GradientStopCollection(2)
			{
				new GradientStop(color, 0),
				new GradientStop(Colors.Transparent, 1)
			}, new Point(0.5, 0), new Point(0.5, 1));
			drawingContext.DrawRectangle(brush, null,
				new Rect(lefttop.X, lefttop.Y + size.Height, size.Width, thickness));

			//top-right
			brush = new LinearGradientBrush(new GradientStopCollection(2)
			{
				new GradientStop(color, 0),
				new GradientStop(Colors.Transparent, 0.5)
			}, new Point(0, 1), new Point(1, 0));
			drawingContext.DrawGeometry(brush, null, DrawSegment(rightRect.TopLeft, thickness, 0, 90));
			//bottom-right
			brush = new LinearGradientBrush(new GradientStopCollection(2)
			{
				new GradientStop(color, 0),
				new GradientStop(Colors.Transparent, 0.5)
			}, new Point(0, 0), new Point(1, 1));
			drawingContext.DrawGeometry(brush, null, DrawSegment(rightRect.BottomLeft, thickness, 270, 360));

			//top-left
			brush = new LinearGradientBrush(new GradientStopCollection(2)
			{
				new GradientStop(color, 0),
				new GradientStop(Colors.Transparent, 0.5)
			}, new Point(1, 1), new Point(0, 0));
			drawingContext.DrawGeometry(brush, null, DrawSegment(leftRect.TopRight, thickness, 90, 180));
			//bottom-left
			brush = new LinearGradientBrush(new GradientStopCollection(2)
			{
				new GradientStop(color, 0),
				new GradientStop(Colors.Transparent, 0.5)
			}, new Point(1, 0), new Point(0, 1));
			drawingContext.DrawGeometry(brush, null, DrawSegment(leftRect.BottomRight, thickness, 180, 270));
		}

		public static Geometry DrawSegment(Point center, int radius, int beginAngle, int endAngle)
		{
			var line = new LineSegment();
			var arcSegment = new ArcSegment();
			var sectorFigure = new PathFigure();
			var geometry = new PathGeometry();
			sectorFigure.StartPoint = center;
			sectorFigure.IsClosed = true;
			arcSegment.IsStroked = true;
			arcSegment.SweepDirection = SweepDirection.Counterclockwise;
			arcSegment.Size = new Size(radius, radius);

			line.IsStroked = true;
			sectorFigure.Segments.Add(line);
			sectorFigure.Segments.Add(arcSegment);
			geometry.Figures.Add(sectorFigure);

			sectorFigure.StartPoint = center;
			var beginRadian = beginAngle * 2d * Math.PI / 360;
			line.Point = new Point(center.X + radius * Math.Cos(beginRadian),
				center.Y - radius * Math.Sin(beginRadian));
			var endRadian = endAngle * 2d * Math.PI / 360;
			arcSegment.Point = new Point(center.X + radius * Math.Cos(endRadian),
				center.Y - radius * Math.Sin(endRadian));
			return geometry;
		}
	}
}