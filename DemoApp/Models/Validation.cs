using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoApp.Models
{
	/// <summary>
	/// Валидация.
	/// </summary>
	public static class Validation
	{
		public static bool ValidateTextBoxes(TextBox[] textBoxes)
		{
			foreach (TextBox box in textBoxes)
			{
				if (String.IsNullOrEmpty(box.Text))
				{
					return false;
				}
			}

			return true;
		}
	}
}
