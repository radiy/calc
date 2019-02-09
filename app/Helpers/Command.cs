using System;
using System.Windows.Input;

namespace Calc.Helpers
{
	public class Command : ICommand
	{
		private readonly Action action;

		public Command(Action action)
		{
			this.action = action;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			action();
		}

		public event EventHandler CanExecuteChanged;
	}
}