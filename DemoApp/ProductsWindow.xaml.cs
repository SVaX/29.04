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

		List<Product> productList = new List<Product>();

		List<Product> foundProducts = new List<Product>();

		/// <summary>
		/// Инициализация окна
		/// </summary>
		/// <param name="Authorized">Авторизован ли пользователь</param>
		/// <param name="user">Пользователь.</param>
		public ProductsWindow(bool Authorized, User? user)
		{
			InitializeComponent();

			productList = db.Products.ToList();

			if (Authorized)
			{
				_currentUser = user;
			}

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

			discountFilterComboBox.ItemsSource = new List<string>
			{
				"Все диапазоны", "0-10%", "10-15%", "15-∞%" 
			};

			costFilterComboBox.ItemsSource = new List<string>
			{
				"По возрастанию", "По убыванию"
			};

			productsList.ItemsSource = productList;

			updateRecordAmount();
		}

		/// <summary>
		/// Кнопка назад.
		/// </summary>
		private void backButton_Click(object sender, RoutedEventArgs e)
		{
			var logWIndow = new LoginWindow();
			logWIndow.Show();
			this.Close();
		}

		/// <summary>
		/// Событие изменения выбора фильтрации по скидке.
		/// </summary>
		private void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			productList = db.Products.ToList();
			switch (discountFilterComboBox.SelectedIndex)
			{
				case 1:
					{
						productList = db.Products.Where(p => p.ProductDiscountAmount < 10).ToList();
						productsList.ItemsSource = productList;
						updateRecordAmount();
						break;
					}
				case 2:
					{
						productList = db.Products.Where(p => p.ProductDiscountAmount > 10 && p.ProductDiscountAmount < 15).ToList();
						productsList.ItemsSource = productList;
						updateRecordAmount();
						break;
					}
				case 3:
					{
						productList = db.Products.Where(p => p.ProductDiscountAmount >= 15).ToList();
						productsList.ItemsSource = productList;
						updateRecordAmount();
						break;
					}
				case 0:
					{
						productList = db.Products.ToList();
						productsList.ItemsSource = productList;
						updateRecordAmount();
						break;
					}
			}
		}

		/// <summary>
		/// Событие изменения выбора фильтрации по цене.
		/// </summary>
		private void costFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (costFilterComboBox.SelectedIndex)
			{
				case 0:
					{
						productsList.ItemsSource = productList.OrderBy(p => p.ProductCost);
						updateRecordAmount();
						break;
					}
				case 1:
					{
						productsList.ItemsSource = productList.OrderByDescending(p => p.ProductCost);
						updateRecordAmount();
						break;
					}
			}
		}

		private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			foundProducts = productList.Where(p => p.ProductName.ToLower().Contains(searchTextBox.Text.ToLower())).ToList();
			productsList.ItemsSource = foundProducts;
			updateAfterSearch();
		}

		private void updateRecordAmount()
		{
			recordAmountLabel.Content = $"{productsList.Items.Count} из {productList.Count}";
		}

		private void updateAfterSearch()
		{
			recordAmountLabel.Content = $"{productsList.Items.Count} из {foundProducts.Count}";
		}

        private void productsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (_currentUser != null && _currentUser.RoleId == 2)
			{
				var window = new EditProductsWindow((Product)productsList.SelectedItem, _currentUser);
				window.Show();
				this.Close();
			}
			else
			{
				return;
			}
		}
    }
}
