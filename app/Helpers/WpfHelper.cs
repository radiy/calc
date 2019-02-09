using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;

namespace Calc.Helpers
{
	public static class WpfHelper
	{
		public static IEnumerable<T> Flat<T>(this IEnumerable<T> collection,
			Func<T, IEnumerable<T>> flat)
		{
			foreach (var item in collection)
			{
				yield return item;
				foreach (var childItem in flat(item).Flat(flat))
					yield return childItem;
			}
		}

		public static IEnumerable<DependencyObject> LogicalChildren(this DependencyObject visual)
		{
			return LogicalTreeHelper.GetChildren(visual).OfType<DependencyObject>();
		}

		public static IEnumerable<DependencyObject> LogicalDescendants(this DependencyObject view)
		{
			return view.LogicalChildren().Flat(LogicalChildren);
		}

		public static IObservable<EventPattern<PropertyChangedEventArgs>> RxProperty(this INotifyPropertyChanged value,
			string name)
		{
			return Observable.FromEventPattern<PropertyChangedEventArgs>(value, "PropertyChanged")
				.Where(e => e.EventArgs.PropertyName == name);
		}
	}
}