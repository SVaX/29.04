using DemoApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

namespace DemoApp
{
	/// <summary>
	/// Interaction logic for AddProductsWindow.xaml
	/// </summary>
	public partial class AddProductsWindow : Window
	{
		DemoDbContext db = new DemoDbContext();
		Product _currentProduct = new Product();
		User _currentUser;
		public AddProductsWindow(User user)
		{
			InitializeComponent();

			_currentUser = user;

			InitComboBoxes();

		}

		/// <summary>
		/// Инициализация выпадающих списков.
		/// </summary>
		private void InitComboBoxes()
		{
			foreach (ProductCategory category in db.ProductCategories)
			{
				categoryComboBox.Items.Add(category.ProductCategoryName);
				categoryComboBox.SelectedItem = category.ProductCategoryName;
				_currentProduct.ProductCategory = category;
			}
			foreach (UnitType unitType in db.UnitTypes)
			{
				unitTypeComboBox.Items.Add(unitType.UnitTypeName);

				unitTypeComboBox.SelectedItem = unitType.UnitTypeName;
				_currentProduct.UnitType = unitType;
			}
		}

		/// <summary>
		/// Проверка на валидность поставщиков и производителей.
		/// </summary>
		/// <param name="man">Производитель</param>
		/// <param name="sup">Поставщик</param>
		/// <returns></returns>
		private bool ValidateManufacturerAndSupplier(ProductManufacturer man, ProductSupplier sup)
		{
			if (db.ProductManufacturers.Contains(man) && db.ProductSuppliers.Contains(sup))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Кнопка "Назад"
		/// </summary>
		private void backButton_Click(object sender, RoutedEventArgs e)
		{
			var window = new ProductsWindow(true, _currentUser);
			window.Show();
			this.Close();
		}

		/// <summary>
		/// Кнопка "Сохранить"
		/// </summary>
		private void saveButton_Click(object sender, RoutedEventArgs e)
		{
			TextBox[] textBoxes = new TextBox[]
			{
				nameTextBox,
				costTextBox,
				manufacturerTextBox,
				maxDiscountTextBox,
				supplierTextBox,
				discountTextBox,
				quantityTextBox,
				descriptionTextBox,
				articleTextBox,
			};

			if (categoryComboBox.SelectedItem == null || !DemoApp.Models.Validation.ValidateTextBoxes(textBoxes))
			{
				MessageBox.Show("Все поля должны быть заполнены!", "Внимание!");
				return;
			}

			if (!(int.TryParse(discountTextBox.Text, out int value) && int.TryParse(maxDiscountTextBox.Text, out int valueAgain)))
			{
				MessageBox.Show("Поля со скидкой нужно заполнить числами!");
				return;
			}

			if (!int.TryParse(quantityTextBox.Text, out int valueHere))
			{
				MessageBox.Show("Кол-во товара должно быть указано числом!");
				return;
			}

			if (Convert.ToInt16(discountTextBox.Text) > Convert.ToInt16(maxDiscountTextBox.Text))
			{
				MessageBox.Show("Скидка должна быть меньше максимальной!");
				return;
			}

			if (Convert.ToDecimal(costTextBox.Text, CultureInfo.InvariantCulture) <= 0)
			{
				MessageBox.Show("Цена должна быть больше нуля!");
				return;
			}

			_currentProduct.ProductArticleNumber = articleTextBox.Text;
			_currentProduct.ProductName = nameTextBox.Text;
			_currentProduct.UnitTypeId = db.UnitTypes.Where(ut => ut.UnitTypeName == unitTypeComboBox.SelectedItem.ToString()).Select(ut => ut.UnitTypeId).FirstOrDefault();
			_currentProduct.ProductCost = Convert.ToDecimal(costTextBox.Text, CultureInfo.InvariantCulture);
			_currentProduct.ProductMaxDiscountAmount = Convert.ToByte(Convert.ToInt32(maxDiscountTextBox.Text));

			var manufacturer = db.ProductManufacturers.Where(m => m.ProductManufacturerName == manufacturerTextBox.Text).FirstOrDefault();
			var supplier = db.ProductSuppliers.Where(s => s.ProductSupplierName == supplierTextBox.Text).FirstOrDefault();

			if (!ValidateManufacturerAndSupplier(manufacturer, supplier))
			{
				MessageBox.Show("Производитель или поставщик указаны неверно");
				return;
			}
			_currentProduct.ProductManufacturerId = manufacturer.ProductManufacturerId;
			_currentProduct.ProductSupplierId = supplier.ProductSupplierId;
			_currentProduct.ProductDiscountAmount = Convert.ToByte(Convert.ToInt32(discountTextBox.Text));
			_currentProduct.ProductQuantityInStock = Convert.ToInt32(quantityTextBox.Text);
			_currentProduct.ProductDescription = descriptionTextBox.Text;

			if (_currentProduct.ProductPhoto == null)
			{
				_currentProduct.ProductPhoto = "";
			}

			db.Products.Add(_currentProduct);
			db.SaveChanges();

			MessageBox.Show("Товар был добавлен");
			backButton_Click(sender, e);
			return;
		}

		/// <summary>
		/// Выбор картинки.
		/// </summary>
		private void selectImage_Click(object sender, RoutedEventArgs e)
		{
			var ofd = new OpenFileDialog();
			ofd.DefaultExt = ".jpg";
			ofd.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

			Nullable<bool> result = ofd.ShowDialog();

			if (result == true)
			{
				string filename = ofd.FileName;
				string currentDir = AppDomain.CurrentDomain.BaseDirectory;
				FileInfo fileInfo = new FileInfo(currentDir);
				DirectoryInfo dirInfo = fileInfo.Directory.Parent;
				string parentDirName = dirInfo.Name;

				fileInfo = new FileInfo(parentDirName);
				dirInfo = fileInfo.Directory.Parent;
				parentDirName = dirInfo.Name;

				fileInfo = new FileInfo(dirInfo.ToString());
				dirInfo = fileInfo.Directory.Parent;
				parentDirName = dirInfo.ToString() + "\\Resources\\" + ofd.SafeFileName;

				System.IO.File.Copy(filename, parentDirName);

				_currentProduct.ProductPhoto = ofd.SafeFileName;

			}
		}

		/// <summary>
		/// Выбор типа исчисления товара.
		/// </summary>
		private void unitTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				foreach (var ut in db.UnitTypes)
				{
					if (ut.UnitTypeName == unitTypeComboBox.SelectedValue)
					{
						_currentProduct.UnitType = ut;
					}
				}
			}
			catch (InvalidOperationException)
			{
				return;
			}
		}

		/// <summary>
		/// Выбор категории товара.
		/// </summary>
		private void categoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				foreach (var cat in db.ProductCategories)
				{
					if (cat.ProductCategoryName == categoryComboBox.SelectedValue)
					{
						_currentProduct.ProductCategory = cat;
					}
				}
			}
			catch (InvalidOperationException)
			{
				return;
			}
		}
	}
}
