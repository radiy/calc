using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Calc.Helpers;

namespace Calc.ViewModels
{
	public enum Operations
	{
		None,
		Plus,
		Minus,
		Product,
		Division
	}

	public class StdCalc
	{
		private List<string> history = new List<string>();
		private decimal? input;
		private decimal lastInput;
		private Operations operation;

		private decimal output;
		private string rawInput;

		public StdCalc()
		{
			Memory.CollectionChanged += (sender, args) => { HaveMemory.Value = Memory.Count > 0; };
		}

		public RxValue<string> DisplayHistory { get; set; } = new RxValue<string>();
		public RxValue<string> DisplayValue { get; set; } = new RxValue<string>();
		public RxValue<bool> HaveMemory { get; set; } = new RxValue<bool>();
		public ObservableCollection<decimal> Memory { get; set; } = new ObservableCollection<decimal>();

		public void Num0() => AppendInput("0");

		public void Num1() => AppendInput("1");

		public void Num2() => AppendInput("2");

		public void Num3() => AppendInput("3");

		public void Num4() => AppendInput("4");

		public void Num5() => AppendInput("5");

		public void Num6() => AppendInput("6");

		public void Num7() => AppendInput("7");

		public void Num8() => AppendInput("8");

		public void Num9() => AppendInput("9");

		public void Dot() => AppendInput(CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);

		public void Product() => BinaryOperation(Operations.Product);

		public void Division() => BinaryOperation(Operations.Division);

		public void Plus() => BinaryOperation(Operations.Plus);

		public void Minus() => BinaryOperation(Operations.Minus);

		public void Sqrt() => UnaryOperation(x => (decimal) Math.Sqrt((double) x));

		public void Power() => UnaryOperation(x => (decimal) Math.Pow((double) x, 2));

		public void Cube() => UnaryOperation(x => (decimal) Math.Pow((double) x, 3));

		public void Fraction() => UnaryOperation(x => 1 / x);

		public void Percent() => UnaryOperation(x => lastInput / 100m * x);

		public void HandleKey(Key key)
		{
			switch (key)
			{
				case Key.Back:
					Backspace();
					break;
				case Key.Escape:
					Clear();
					break;
				case Key.Enter:
					Enter();
					break;
			}
		}

		public void HandleText(string text)
		{
			if (char.IsDigit(text[0]))
			{
				AppendInput(text);
				return;
			}

			if (text == CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator)
			{
				Dot();
				return;
			}

			switch (text)
			{
				case ".":
					Dot();
					break;
				case "/":
					Division();
					break;
				case "*":
					Product();
					break;
				case "-":
					Minus();
					break;
				case "+":
					Plus();
					break;
			}
		}

		public void Clear()
		{
			lastInput = 0;
			operation = Operations.None;
			output = 0;
			ClearHistory();
			ClearInput();
		}

		public void ClearInput()
		{
			SetInput(0, "");
		}

		private void BinaryOperation(Operations op)
		{
			if (input != null)
			{
				CommitHistory(input.Value);
				Operation();
				operation = op;
				CommitHistory(operation);
			}
			else
			{
				operation = op;
				//operation after enter
				if (history.Count == 0)
				{
					CommitHistory(output);
					CommitHistory(operation);
				}
				//operation change
				else
				{
					OverwriteHistory(operation);
				}
			}
		}

		private void ClearHistory()
		{
			DisplayHistory.Value = null;
			history = new List<string>();
		}

		private void CommitHistory(object value)
		{
			if (value is Operations operation)
				value = OpToString(operation);
			history.Add(value.ToString());
			DisplayHistory.Value = string.Join(" ", history);
		}
		
		private void OverwriteHistory(object value)
		{
			if (history.Count == 0)
				return;
			if (value is Operations operation)
				value = OpToString(operation);
			history[history.Count - 1] = value.ToString();
			DisplayHistory.Value = string.Join(" ", history);
		}

		private string OpToString(Operations op)
		{
			switch (op)
			{
				case Operations.Plus:
					return "+";
				case Operations.Minus:
					return "-";
				case Operations.Division:
					return "/";
				case Operations.Product:
					return "*";
			}

			return "";
		}

		private decimal Op(Operations op, decimal left, decimal right)
		{
			switch (op)
			{
				case Operations.Plus:
					return left + right;
				case Operations.Minus:
					return left - right;
				case Operations.Product:
					return left * right;
				case Operations.Division:
					return left / right;
				case Operations.None:
					return right;
			}

			return 0;
		}

		public void Enter()
		{
			Operation();
			ClearHistory();
		}

		private void Operation()
		{
			if (input != null)
				lastInput = input.Value;
			decimal value;
			try
			{
				value = Op(operation, output, input ?? lastInput);
			}
			catch (Exception e)
			{
				DisplayValue.Value = TranslateException(e);
				return;
			}
			SetOutput(value);
		}

		private void SetOutput(decimal value)
		{
			output = value;
			DisplayValue.Value = output.ToString();
			input = null;
			rawInput = null;
		}

		private void AppendInput(string num)
		{
			if (string.IsNullOrEmpty(rawInput) &&
			    num == CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator)
				num = "0" + num;

			TrySetInput(num);
		}

		private void TrySetInput(string num)
		{
			if (decimal.TryParse(rawInput + num, out var value))
				SetInput(value, rawInput + num);
		}

		private void SetInput(decimal value, string rawValue = null)
		{
			rawInput = rawValue ?? value.ToString();
			input = value;
			DisplayValue.Value = rawInput;
		}

		public void SwitchSign()
		{
			if (input == null)
				return;
			SetInput(decimal.Negate(input.Value));
		}

		public void Backspace()
		{
			if (rawInput == null)
				return;
			if (rawInput.Length == 0)
				return;
			if (rawInput.Length == 1)
				SetInput(0);
			TrySetInput(rawInput.Substring(0, rawInput.Length - 1));
		}

		public void MemoryPlus()
		{
			if (input == null)
				return;
			if (Memory.Count == 0)
				Memory.Add(0);
			Memory[Memory.Count - 1] += input.Value;
		}

		public void MemoryMinus()
		{
			if (input == null)
				return;
			if (Memory.Count == 0)
				Memory.Add(0);
			Memory[Memory.Count - 1] -= input.Value;
		}

		public void MemorySave()
		{
			if (input == null)
				return;
			Memory.Add(input.Value);
		}

		public void MemoryReset()
		{
			Memory.Clear();
		}

		public void MemoryRecovery()
		{
			if (Memory.Count == 0)
				return;
			SetInput(Memory[Memory.Count - 1]);
		}

		private void UnaryOperation(Func<decimal, decimal> func)
		{
			try
			{
				lastInput = func(input ?? lastInput);
			}
			catch (Exception e)
			{
				DisplayValue.Value = TranslateException(e);
				return;
			}

			if (input == null)
				OverwriteHistory(lastInput);
			else
				CommitHistory(lastInput);

			input = null;
			rawInput = null;
			DisplayValue.Value = lastInput.ToString();
		}

		private static string TranslateException(Exception exception)
		{
			if (exception is DivideByZeroException)
				return exception.Message;
			return "Введены неверные данные";
		}

		public void MemoryRecall(decimal item) => SetInput(item);
	}
}