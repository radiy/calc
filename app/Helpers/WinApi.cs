using System;
using System.Runtime.InteropServices;

namespace Calc
{
	public enum AccentState
	{
		ACCENT_DISABLED = 0,
		ACCENT_ENABLE_GRADIENT = 1,
		ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
		ACCENT_ENABLE_BLURBEHIND = 3,
		ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
		ACCENT_INVALID_STATE = 5
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct AccentPolicy
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
		WCA_ACCENT_POLICY = 19
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class MONITORINFO
	{
		public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

		public RECT rcMonitor = new RECT();

		public RECT rcWork = new RECT();

		public int dwFlags = 0;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 0)]
	public struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;

		public static readonly RECT Empty;
	}


	[StructLayout(LayoutKind.Sequential)]
	public struct MINMAXINFO
	{
		public POINT ptReserved;
		public POINT ptMaxSize;
		public POINT ptMaxPosition;
		public POINT ptMinTrackSize;
		public POINT ptMaxTrackSize;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;
	}

	internal enum WVR
	{
		ALIGNTOP = 0x0010,
		ALIGNLEFT = 0x0020,
		ALIGNBOTTOM = 0x0040,
		ALIGNRIGHT = 0x0080,
		HREDRAW = 0x0100,
		VREDRAW = 0x0200,
		VALIDRECTS = 0x0400,
		REDRAW = HREDRAW | VREDRAW
	}


	public enum IDC_SIZE_CURSORS
	{
		SIZENWSE = 32642,
		SIZENESW = 32643,
		SIZEWE = 32644,
		SIZENS = 32645
	}

	public enum WM
	{
		SETCURSOR = 0x0020,
		SYSCOMMAND = 0x112
	}

	public class WinApi
	{
		public const uint TPM_RETURNCMD = 0x0100;
		public const uint TPM_LEFTBUTTON = 0x0;
		public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

		public static int LOWORD(int i) => (short) (i & 0xFFFF);

		[DllImport("user32.dll")]
		public static extern IntPtr SetCursor(IntPtr cursor);

		[DllImport("user32.dll")]
		public static extern IntPtr LoadCursor(IntPtr hInstance, IDC_SIZE_CURSORS cursor);

		[DllImport("user32.dll")]
		public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

		[DllImport("user32.dll")]
		public static extern uint TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PostMessage(IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("User32")]
		public static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

		[DllImport("user32")]
		public static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);
	}
}