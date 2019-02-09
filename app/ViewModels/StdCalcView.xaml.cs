using System;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Calc.Helpers;

namespace Calc.ViewModels
{
	public partial class StdCalcView : UserControl
	{
		private Subject<Size> size = new Subject<Size>();
		private Size switchSize = new Size(1000, 600);

		public StdCalcView()
		{
			InitializeComponent();

			NarrowView.MemoryList.Click += ShowMemoryMenu;
			WideView.MemoryList.Click += ShowMemoryMenu;
			var cancellation =new CancellationDisposable();
			var narrowMainGrid = (Grid)NarrowView.Content;
			narrowMainGrid.Children.Add(BuidlDecoration((Grid)narrowMainGrid.Children[0]));

			var wideMainGrid = (Grid)WideView.Content;
			wideMainGrid.Children.Add(BuidlDecoration((Grid)wideMainGrid.Children[0]));

			PreviewKeyDown += (sender, args) => Model.HandleKey(args.Key);
			PreviewTextInput += (sender, args) => Model.HandleText(args.Text);

			size.Throttle(TimeSpan.FromMilliseconds(300), DispatcherScheduler.Current)
				.Subscribe(UpdateView, cancellation.Token);
		}

		private void UpdateView(Size arg)
		{
			var isNarrow = arg.Height < switchSize.Height || arg.Width < switchSize.Width;
			if (isNarrow)
			{
				if (WideView.Visibility == Visibility.Visible)
				{
					WideView.Visibility = Visibility.Hidden;
					NarrowView.Visibility = Visibility.Visible;
				}
			}
			else
			{
				if (NarrowView.Visibility == Visibility.Visible)
				{
					WideView.Visibility = Visibility.Visible;
					NarrowView.Visibility = Visibility.Hidden;
				}
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			size.OnNext(sizeInfo.NewSize);
		}

		private Grid BuidlDecoration(Grid grid)
		{
			Panel.SetZIndex(grid, 0);
			Loaded += (sender, args) => { Focus(); };
			var decorationGrid = new Grid();
			var decorationBrush = (VisualBrush) Resources["EffectBrush"];
			var transform = new TranslateTransform
			{
				X = 1,
				Y = 1
			};
			decorationBrush.RelativeTransform = transform;
			decorationGrid.OpacityMask = decorationBrush;
			Panel.SetZIndex(decorationGrid, 1);
			foreach (var row in grid.RowDefinitions)
				decorationGrid.RowDefinitions.Add(new RowDefinition {Height = row.Height});
			foreach (var column in grid.ColumnDefinitions)
				decorationGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = column.Width});
			foreach (FrameworkElement child in grid.Children)
				if (child is StackPanel sourceStack)
				{
					var dstStack = new StackPanel();
					dstStack.Orientation = sourceStack.Orientation;
					CopyGridSettings(dstStack, sourceStack);

					foreach (FrameworkElement el in sourceStack.Children)
					{
						var border = GetBorder(child);
						border.SetBinding(HeightProperty, new Binding("ActualHeight")
						{
							BindsDirectlyToSource = true,
							Source = el,
							Mode = BindingMode.OneWay
						});
						border.SetBinding(WidthProperty, new Binding("ActualWidth")
						{
							BindsDirectlyToSource = true,
							Source = el,
							Mode = BindingMode.OneWay
						});
						border.SetBinding(IsEnabledProperty, new Binding("IsEnabled")
						{
							BindsDirectlyToSource = true,
							Source = el,
							Mode = BindingMode.OneWay
						});
						dstStack.Children.Add(border);
					}

					decorationGrid.Children.Add(dstStack);
				}
				else
				{
					var border = GetBorder(child);
					CopyGridSettings(border, child);
					decorationGrid.Children.Add(border);
				}

			MouseLeave += (sender, args) => { decorationGrid.Visibility = Visibility.Hidden; };
			MouseEnter += (sender, args) => { decorationGrid.Visibility = Visibility.Visible; };
			PreviewMouseMove += (sender, args) =>
			{
				var position = args.GetPosition(MainGrid);
				var height = MainGrid.ActualHeight;
				var width = MainGrid.ActualWidth;
				var transX = position.X / width;
				var transY = position.Y / height;
				transform.X = transX - 0.5;
				transform.Y = transY - 0.5;
			};
			return decorationGrid;
		}

		public StdCalc Model => DataContext as StdCalc;

		private Border GetBorder(FrameworkElement child)
		{
			var border = new Border();
			border.Style = (Style) Resources["BorderStyle"];
			border.HorizontalAlignment = child.HorizontalAlignment;
			border.VerticalAlignment = child.VerticalAlignment;
			return border;
		}

		private static void CopyGridSettings(FrameworkElement dst, FrameworkElement src)
		{
			Grid.SetRow(dst, Grid.GetRow(src));
			Grid.SetColumn(dst, Grid.GetColumn(src));
			Grid.SetColumnSpan(dst, Grid.GetColumnSpan(src));
		}

		public void ShowMemoryMenu(object sender, RoutedEventArgs e)
		{
			var contextMenu = new ContextMenu();
			foreach (var item in Model.Memory)
			{
				var menuItem = new MenuItem {Header = item};
				menuItem.Click += (o, args) => { Model.MemoryRecall(item); };
				contextMenu.Items.Add(menuItem);
			}

			((Button)sender).ContextMenu = contextMenu;
			contextMenu.IsOpen = true;
		}
	}
}