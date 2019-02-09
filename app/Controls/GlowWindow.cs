using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Calc.Controls
{
	public class GlowWindow : Window
	{
		private readonly Color activeColor = Color.FromArgb(80, 0, 0, 0);
		private readonly ShadowBorder border;
		private readonly Color inactiveColor = Color.FromArgb(50, 0, 0, 0);
		private ResizeDirection? resizeDirection;

		public GlowWindow()
		{
			WindowStyle = WindowStyle.None;
			AllowsTransparency = true;
			ShowInTaskbar = false;
			ResizeMode = ResizeMode.NoResize;
			Background = Brushes.Transparent;
			border = new ShadowBorder();
			SnapsToDevicePixels = true;
			Content = border;
		}

		public void UpdateOwner(Window host)
		{
			Owner = host;
			border.MouseLeave += ResetResizeCursor;
			border.MouseEnter += ShowResizeCursor;
			border.MouseMove += ShowResizeCursor;
			border.MouseLeftButtonDown += Resize;

			Owner.LocationChanged += (sender, args) => { Update(); };
			Owner.SizeChanged += (sender, args) => { Update(); };
			Owner.StateChanged += (sender, args) =>
			{
				switch (Owner.WindowState)
				{
					case WindowState.Maximized:
						Hide();
						break;
					default:
						Show();
						Owner.Activate();
						break;
				}
			};
			Owner.Activated += (sender, args) => { border.Color = activeColor; };
			Owner.Deactivated += (sender, args) => { border.Color = inactiveColor; };
			Update();
			Show();
		}

		public void Update()
		{
			var glowThickness = 10;
			Left = Owner.Left - glowThickness;
			Top = Owner.Top - glowThickness;
			Width = Owner.ActualWidth + glowThickness * 2;
			Height = Owner.ActualHeight + glowThickness * 2;
		}

		public void ResetResizeCursor(object sender, MouseEventArgs e)
		{
			Cursor = Cursors.Arrow;
			resizeDirection = null;
		}

		public void ShowResizeCursor(object sender, MouseEventArgs e)
		{
			resizeDirection = GetResizeDirection(sender, e);
			switch (resizeDirection.Value)
			{
				case ResizeDirection.BottomRight:
				case ResizeDirection.TopLeft:
					Cursor = Cursors.SizeNWSE;
					break;
				case ResizeDirection.BottomLeft:
				case ResizeDirection.TopRight:
					Cursor = Cursors.SizeNESW;
					break;
				case ResizeDirection.Right:
				case ResizeDirection.Left:
					Cursor = Cursors.SizeWE;
					break;
				case ResizeDirection.Top:
				case ResizeDirection.Bottom:
					Cursor = Cursors.SizeNS;
					break;
			}
		}

		private ResizeDirection GetResizeDirection(object sender, MouseEventArgs e)
		{
			var position = e.MouseDevice.GetPosition(this);
			var borderThickness = new Thickness(border.BorderThikness);
			var contentRect = new Rect(borderThickness.Left, borderThickness.Top,
				RenderSize.Width - borderThickness.Left - borderThickness.Right,
				RenderSize.Height - borderThickness.Bottom - borderThickness.Top);
			var onLeft = position.X <= contentRect.Left;
			var onRight = position.X >= contentRect.Right;
			var onTop = position.Y <= contentRect.Top;
			var onBottom = position.Y >= contentRect.Bottom;
			if (onBottom)
			{
				if (onLeft)
					return ResizeDirection.BottomLeft;
				if (onRight)
					return ResizeDirection.BottomRight;
				return ResizeDirection.Bottom;
			}

			if (onTop)
			{
				if (onLeft)
					return ResizeDirection.TopLeft;
				if (onRight)
					return ResizeDirection.TopRight;
				return ResizeDirection.Top;
			}

			if (onLeft) return ResizeDirection.Left;
			return ResizeDirection.Right;
		}

		public void Resize(object sender, MouseButtonEventArgs e)
		{
			var direction = resizeDirection ?? GetResizeDirection(sender, e);
			resizeDirection = null;
			var handle = new WindowInteropHelper(Owner).Handle;
			WinApi.SendMessage(handle,
				(uint) WM.SYSCOMMAND, (IntPtr) (61440 + direction), IntPtr.Zero);
			WinApi.SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
		}
	}
}