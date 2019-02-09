using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace Calc.Helpers
{
	public class RxValue<T> : IObservable<T>, IObserver<T>, INotifyPropertyChanged
	{
		private T _value;

		public T Value
		{
			get => _value;
			set => OnPropertyChanged(ref _value, value);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return this.RxProperty("Value")
				.Select(e => ((RxValue<T>) e.Sender).Value)
				.Merge(Observable.Return(Value)).Subscribe(observer);
		}

		public void OnNext(T value)
		{
			Value = value;
		}

		public void OnError(Exception error)
		{
		}

		public void OnCompleted()
		{
		}

		public virtual bool OnPropertyChanged(ref T field, T value, [CallerMemberName] string propertyName = "")
		{
			if (Equals(field, value))
				return false;
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}
}