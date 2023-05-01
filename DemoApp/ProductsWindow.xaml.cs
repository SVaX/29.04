using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using DemoApp.Models;

namespace DemoApp
{
	/// <summary>
	/// Interaction logic for ProductsWindow.xaml
	/// </summary>
	public partial class ProductsWindow : Window
	{
		/// <summary>
		/// Текущий пользователь.
		/// </summary>
		private User _currentUser = new User();

		/// <summary>
		/// Контекст БД.
		/// </summary>
		public DemoDbContext db = new DemoDbContext();
		
		/// <summary>
		/// Инициализация окна
		/// </summary>
		/// <param name="Authorized">Авторизован ли пользователь</param>
		/// <param name="user">Пользователь.</param>
		public ProductsWindow(bool Authorized, User? user)
		{
			InitializeComponent();

			string colorOfItem;

			if (Authorized)
			{
				_currentUser = user;
			}

			List<Product> productList = db.Products.ToList();

			foreach (Product product in productList)
			{
				product.ProductPhoto = $"/Resources/{product.ProductPhoto}";

				foreach (ProductManufacturer manufacturer in db.ProductManufacturers)
				{
					if (manufacturer.ProductManufacturerId == product.ProductManufacturerId)
					{
						product.ProductManufacturer = manufacturer;
					}
				}
			}

			filterComboBox.ItemsSource = new List<string>
			{
				"0-10%", "10-15%", "15-∞%", "All ranges"
			};

			productsList.ItemsSource = productList;
		}

		private void backButton_Click(object sender, RoutedEventArgs e)
		{
			var logWIndow = new LoginWindow();
			logWIndow.Show();
			this.Close();
		}

		private void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (filterComboBox.SelectedIndex)
			{
				case 0:
					{
						productsList.ItemsSource = db.Products.Where(p => p.ProductDiscountAmount < 10).ToList();
						break;
					}
				case 1:
					{
						productsList.ItemsSource = db.Products.Where(p => p.ProductDiscountAmount > 10 && p.ProductDiscountAmount < 15).ToList();
						break;
					}
				case 2:
					{
						productsList.ItemsSource = db.Products.Where(p => p.ProductDiscountAmount >= 15).ToList();
						break;
					}
				case 3:
					{
						productsList.ItemsSource = db.Products.ToList();
						break;
					}
			}
		}
	}
}
