using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Calc.Controls;

namespace Calc
{
	internal enum AccentState
	{
		ACCENT_DISABLED = 0,
		ACCENT_ENABLE_GRADIENT = 1,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
		ACCENT_INVALID_STATE = 5
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct AccentPolicy
	{
		public AccentState AccentState;
		public uint AccentFlags;
		public uint GradientColor;
		public uint AnimationId;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WindowCompositionAttributeData
	{
		public WindowCompositionAttribute Attribute;
		public IntPtr Data;
		public int SizeOfData;
	}

	public enum WindowCompositionAttribute
	{
		// ...
		WCA_ACCENT_POLICY = 19
		// ...
	}

	public partial class MainWindow : Window
	{
		private readonly uint _blurBackgroundColor = 0xededed;
		private readonly uint _blurOpacity = 50;
		public Brush activeBrush = new SolidColorBrush(Color.FromArgb(0xc8, 0xed, 0xed, 0xed));
		private readonly Brush inactiveBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xed, 0xed, 0xed));

		public MainWindow()
		{
			InitializeComponent();
			UpdateState();
			Header.SetBinding(TextBlock.TextProperty, new Binding("Title")
			{
				Source = this,
				BindsDirectlyToSource = true
			});
			Background = activeBrush;
		}

		private void ClickMaximize(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Maximized;
		}

		private void ClickMinimize(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void ClickRestore(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Normal;
		}

		private void ClickClose(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void HeaderLeftClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 1)
			{
				e.Handled = true;
				DragMove();
			}
			else if (e.ClickCount == 2 && ResizeMode != ResizeMode.NoResize)
			{
				e.Handled = true;

				if (WindowState == WindowState.Normal)
					SystemCommands.MaximizeWindow(this);
				else
					SystemCommands.RestoreWindow(this);
			}
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			var windowHelper = new WindowInteropHelper(this);

			var accent = new AccentPolicy();
			accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
			accent.GradientColor = (_blurOpacity << 24) | (_blurBackgroundColor & 0xFFFFFF);
			var accentStructSize = Marshal.SizeOf(accent);
			var accentPtr = Marshal.AllocHGlobal(accentStructSize);
			try
			{
				Marshal.StructureToPtr(accent, accentPtr, false);
				var data = new WindowCompositionAttributeData();
				data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
				data.SizeOfData = accentStructSize;
				data.Data = accentPtr;
				WinApi.SetWindowCompositionAttribute(windowHelper.Handle, ref data);
			}
			finally
			{
				Marshal.FreeHGlobal(accentPtr);
			}

			var handleSource = HwndSource.FromHwnd(windowHelper.Handle);
			if (handleSource == null)
				return;
			handleSource.AddHook(WindowProc);
			var glow = new GlowWindow();
			glow.UpdateOwner(this);
		}

		private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case 0x0024: /* WM_GETMINMAXINFO */
					WmGetMinMaxInfo(hwnd, lParam);
					handled = true;
					break;
			}

			return (IntPtr) 0;
		}

		private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
		{
			var mmi = (MINMAXINFO) Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

			var monitor = WinApi.MonitorFromWindow(hwnd, WinApi.MONITOR_DEFAULTTONEAREST);
			if (monitor != IntPtr.Zero)
			{
				var monitorInfo = new MONITORINFO();
				WinApi.GetMonitorInfo(monitor, monitorInfo);
				var rcWorkArea = monitorInfo.rcWork;
				var rcMonitorArea = monitorInfo.rcMonitor;
				mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
				mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
				mmi.ptMaxSize.X = Math.Abs(rcWorkArea.right - rcWorkArea.left);
				mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
			}
			Marshal.StructureToPtr(mmi, lParam, true);
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			Background = activeBrush;
		}

		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);
			Background = inactiveBrush;
		}

		protected override void OnStateChanged(EventArgs e)
		{
			base.OnStateChanged(e);

			UpdateState();
		}

		private void UpdateState()
		{
			if (WindowState == WindowState.Maximized)
			{
				Maximize.Visibility = Visibility.Collapsed;
				Restore.Visibility = Visibility.Visible;
			}
			else
			{
				Maximize.Visibility = Visibility.Visible;
				Restore.Visibility = Visibility.Collapsed;
			}
		}

		private void ShowSystemMenu(object sender, MouseButtonEventArgs e)
		{
			ShowSystemMenu(e.GetPosition(this));
		}

		private void ShowSystemMenu(Point point)
		{
			var physicalScreenLocation = PointToScreen(point);
			var hwnd = new WindowInteropHelper(this).Handle;
			var hmenu = WinApi.GetSystemMenu(hwnd, false);
			var cmd = WinApi.TrackPopupMenuEx(hmenu, WinApi.TPM_LEFTBUTTON | WinApi.TPM_RETURNCMD,
				(int) physicalScreenLocation.X, (int) physicalScreenLocation.Y, hwnd, IntPtr.Zero);
			if (0 != cmd)
				WinApi.PostMessage(hwnd, WM.SYSCOMMAND, new IntPtr(cmd), IntPtr.Zero);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Space &&
				(e.KeyboardDevice.IsKeyDown(Key.LeftAlt) || e.KeyboardDevice.IsKeyDown(Key.RightAlt)))
			{
				e.Handled = true;
				ShowSystemMenu(new Point(0, 0));
			}

			base.OnKeyDown(e);
		}
	}
}