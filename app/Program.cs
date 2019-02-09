using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Calc.Helpers;
using Calc.ViewModels;

namespace Calc
{
	public class Program
	{
		[STAThread]
		public static void Main()
		{
			try
			{
#if CORE
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
				Window window;
				//we use undocumented function for window effect, tested only on windows 10
				if (Environment.OSVersion.Version < new Version(6, 2))
				{
					window = new Window
					{
						Content = new StdCalc()
					};
				}
				else
				{
					window = new MainWindow();
					((MainWindow) window).MainContent.Content = new StdCalc();
				}

				window.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag);
				window.Height = 520;
				window.Width = 350;
				window.MinHeight = 520;
				window.MinWidth = 350;
				window.Title = "Калькулятор";


				var app = new Application();
				app.Resources.MergedDictionaries.Add(RegisterMapping());
				app.Run(window);
			}
			catch (Exception e)
			{
#if !DEBUG
				MessageBox.Show(e.ToString(), "Ошибка при запуске приложения", MessageBoxButton.OK,
					MessageBoxImage.Error);
#endif
			}
		}

		/// <summary>
		///     simple mvvm implementation
		/// </summary>
		private static ResourceDictionary RegisterMapping()
		{
			var resourceDictionary = new ResourceDictionary();
			resourceDictionary.BeginInit();

			var types = typeof(Program).Assembly.GetTypes().Where(x => !x.IsAbstract
																		&& !x.IsNestedPrivate
																		&& !x.IsInterface
																		&& !x.IsEnum
																		&& x.Namespace?.StartsWith("Calc.ViewModels") ==
																		true)
				.ToArray();
			var userControlType = typeof(UserControl);
			var viewTypes = types.Where(x => userControlType.IsAssignableFrom(x));
			var modelTypes = types.Except(viewTypes).ToArray();
			foreach (var modelType in modelTypes)
			{
				var viewName = modelType.Name + "View";
				var viewType = viewTypes.FirstOrDefault(x => x.Name == viewName)
								?? throw new Exception(
									$"View {viewName} not found for ViewModel {modelType}, ViewModels namespace only for ViewModels");
				var template = new DataTemplate(modelType);
				var root = new FrameworkElementFactory(viewType);
				template.VisualTree = root;
				root.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(Loaded));
				resourceDictionary.Add(template.DataTemplateKey, template);
				template.Seal();
			}

			resourceDictionary.EndInit();
			return resourceDictionary;
		}

		public static void Loaded(object sender, RoutedEventArgs e)
		{
			var userControl = (UserControl) sender;
			ApplyConvention(userControl);
			userControl.DataContextChanged += (o, args) =>
			{
				if (userControl.DataContext != null)
					ApplyConvention(userControl);
			};
		}

		private static void ApplyConvention(UserControl view)
		{
			var els = view.LogicalDescendants().OfType<Button>();
			foreach (var el in els)
			{
				if (el.Name?.StartsWith("_") != true)
					continue;

				var methodName = el.Name.Trim('_');
				var methodInfo = view.DataContext.GetType().GetMethod(methodName)
								?? throw new Exception(
									$"Button name {el.Name} indicates it should by bound to method name in" +
									$" view model {view.DataContext.GetType()}, but method {methodName} not found");
				el.Command = new Command(() => methodInfo.Invoke(view.DataContext, null));
			}
		}
	}
}