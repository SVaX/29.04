using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemoApp
{
    /// <summary>
    /// Interaction logic for CaptchaWindow.xaml
    /// </summary>
    public partial class CaptchaWindow : Window
    {
        /// <summary>
        /// Текст капчи.
        /// </summary>
        private string captcha = "aaxue";

        /// <summary>
        /// Разрешено ли сейчас вводить данные.
        /// </summary>
        bool allowedtowrite = true;

        /// <summary>
        /// Инициализация.
        /// </summary>
        public CaptchaWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Нажатие на кнопку ввода капчи.
        /// </summary>
		private void captchaButton_Click(object sender, RoutedEventArgs e)
		{
            if (!allowedtowrite)
            {
                MessageBox.Show("Подождите!");
                return;
            }

            if (captchaTextBox.Text.ToLower().Trim(' ') == captcha)
            {
                MessageBox.Show("Правильно!");
				var logWindow = new LoginWindow();
				logWindow.Show();
				this.Close();
            }
            else
            {
                MessageBox.Show("Неверно!");

                var end = DateTime.UtcNow.AddSeconds(10);
                Timer timer = null;

				TimerCallback tmCallback = state => {
					if (DateTime.UtcNow > end)
					{
						timer?.Dispose();
						timer = null;
                        allowedtowrite = false;
                        return;
					}
				};

                timer = new Timer(tmCallback, null, 1000, 1000);
                
			}
		}

	}
}
