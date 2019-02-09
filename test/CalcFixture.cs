using Calc.ViewModels;
using NUnit.Framework;

namespace test
{
	public class CalcFixture
	{
		private StdCalc calc;

		[SetUp]
		public void Setup()
		{
			calc = new StdCalc();
		}

		[Test]
		public void Mul()
		{
			calc.Num5();
			calc.Product();
			Assert.AreEqual("5", calc.DisplayValue.Value);
			Assert.AreEqual("5 *", calc.DisplayHistory.Value);
			calc.Num3();
			calc.Enter();
			Assert.AreEqual("15", calc.DisplayValue.Value);
		}

		[Test]
		public void Simple()
		{
			calc.Num1();
			Assert.AreEqual("1", calc.DisplayValue.Value);
			Assert.IsNull(calc.DisplayHistory.Value);
			calc.Plus();
			Assert.AreEqual("1", calc.DisplayValue.Value);
			Assert.AreEqual("1 +", calc.DisplayHistory.Value);
			calc.Num2();
			Assert.AreEqual("2", calc.DisplayValue.Value);
			Assert.AreEqual("1 +", calc.DisplayHistory.Value);
			calc.Enter();
			Assert.AreEqual("3", calc.DisplayValue.Value);
			Assert.IsNull(calc.DisplayHistory.Value);
		}

		[Test]
		public void Chain()
		{
			calc.Num1();
			calc.Plus();
			calc.Num2();
			calc.Plus();
			Assert.AreEqual("3", calc.DisplayValue.Value);
			Assert.AreEqual("1 + 2 +", calc.DisplayHistory.Value);
			calc.Num3();
			calc.Plus();
			Assert.AreEqual("6", calc.DisplayValue.Value);
			Assert.AreEqual("1 + 2 + 3 +", calc.DisplayHistory.Value);
		}

		[Test]
		public void Repeat()
		{
			calc.Num1();
			calc.Plus();
			calc.Enter();
			Assert.AreEqual("2", calc.DisplayValue.Value);
			Assert.IsNull(calc.DisplayHistory.Value);
			calc.Enter();
			Assert.AreEqual("3", calc.DisplayValue.Value);
			Assert.IsNull(calc.DisplayHistory.Value);
		}

		[Test]
		public void Skip_input()
		{
			calc.Num1();
			calc.Plus();
			calc.Plus();
			calc.Plus();
			Assert.AreEqual("1", calc.DisplayValue.Value);
			Assert.AreEqual("1 +", calc.DisplayHistory.Value);
		}

		[Test]
		public void Update_history()
		{
			calc.Num1();
			calc.Plus();
			Assert.AreEqual("1", calc.DisplayValue.Value);
			Assert.AreEqual("1 +", calc.DisplayHistory.Value);
			calc.Minus();
			Assert.AreEqual("1", calc.DisplayValue.Value);
			Assert.AreEqual("1 -", calc.DisplayHistory.Value);
			calc.Num2();
			calc.Enter();
			Assert.AreEqual("-1", calc.DisplayValue.Value);
		}

		[Test]
		public void Composite()
		{
			calc.Num1();
			calc.Plus();
			calc.Num5();
			calc.Enter();
			calc.Plus();
			Assert.AreEqual("6", calc.DisplayValue.Value);
			Assert.AreEqual("6 +", calc.DisplayHistory.Value);
			calc.Enter();
		}

		[Test]
		public void Percent()
		{
			calc.Num1();
			calc.Plus();
			calc.Num5();
			calc.Percent();
			Assert.AreEqual("0,05", calc.DisplayValue.Value);
			Assert.AreEqual("1 + 0,05", calc.DisplayHistory.Value);
			calc.Enter();
			Assert.AreEqual("1,05", calc.DisplayValue.Value);
			Assert.AreEqual(null, calc.DisplayHistory.Value);
		}

		[Test]
		public void Dot()
		{
			calc.Num6();
			calc.Dot();
			Assert.AreEqual("6,", calc.DisplayValue.Value);
			calc.Num0();
			calc.Num9();
			Assert.AreEqual("6,09", calc.DisplayValue.Value);


			calc.Clear();
			calc.Dot();
			calc.Num7();
			Assert.AreEqual("0,7", calc.DisplayValue.Value);
		}

		[Test]
		public void Clear()
		{
			calc.Clear();
			calc.Num4();
			Assert.AreEqual("4", calc.DisplayValue.Value);
		}

		[Test]
		public void Unary_chain()
		{
			calc.Num2();
			calc.Power();
			Assert.AreEqual("4", calc.DisplayValue.Value);
			Assert.AreEqual("4", calc.DisplayHistory.Value);
			calc.Power();
			Assert.AreEqual("16", calc.DisplayValue.Value);
			Assert.AreEqual("16", calc.DisplayHistory.Value);
		}
	}
}