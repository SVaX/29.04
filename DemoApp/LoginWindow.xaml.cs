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
using DemoApp.Models;

namespace DemoApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class LoginWindow : Window
	{
		/// <summary>
		/// Экземпляр БД.
		/// </summary>
		DemoDbContext db = new DemoDbContext();

		/// <summary>
		/// Кол-во попыток авторизации.
		/// </summary>
		private int _loginTries = 0;

		/// <summary>
		/// Инициализация окна.
		/// </summary>
		public LoginWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// События нажатия кнопки "Войти".
		/// </summary>
		private void loginButton_Click(object sender, RoutedEventArgs e)
		{
			if (!checkIfLoginTriesAreMore())
			{
				if (String.IsNullOrEmpty(loginTextBox.Text) || String.IsNullOrEmpty(passwordBox.Password))
				{
					MessageBox.Show("Все поля должны быть заполнены", "Внимание!");
					return;
				}

				/// Был ли найден пользователь.
				var userWasntFound = true;

				foreach (var user in db.Users)
				{
					if (user.UserLogin == loginTextBox.Text)
					{
						if (user.UserPassword == passwordBox.Password)
						{
							userWasntFound = false;

							MessageBox.Show("Авторизация прошла успешно!", "Внимание!");

							var prodWindow = new ProductsWindow(true, user);
							prodWindow.Show();
							this.Close();

						}
						else
						{
							MessageBox.Show("Пароль был неверным!", "Внимание!");
							_loginTries++;
							return;

						}
					}
				}

				if (userWasntFound)
				{
					_loginTries++;
					MessageBox.Show("Такого пользователя не существует.", "Внимание!");
					return;
				}
			}
			else
			{
				var captWindow = new CaptchaWindow();
				captWindow.Show();
				this.Close();
			}
		}

		/// <summary>
		/// Событие нажатия на кнопку "Войти без авторизации".
		/// </summary>
		private void nonAuthorizedButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Вход без авторизации прошел успешно!", "Внимание!");

			var prodWindow = new ProductsWindow(false, null);
			prodWindow.Show();
			this.Close();
		}

		private bool checkIfLoginTriesAreMore()
		{
			if (_loginTries == 2)
			{
				return true;
			}
			return false;
		}
	}
}
