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
using System.Windows.Shapes;
using DemoApp.Models;
using Microsoft.Win32;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Build.Evaluation;
using System.Globalization;

namespace DemoApp
{
    /// <summary>
    /// Логика взаимодействия для EditProductsWindow.xaml
    /// </summary>
    public partial class EditProductsWindow : Window
    {
        DemoDbContext db = new DemoDbContext();
        Product _currentProduct;
        User _currentUser;
        public EditProductsWindow(Product product, User user)
        {
            InitializeComponent();
            
            _currentProduct = product;
            _currentUser = user;

            InitSupplierAndManufacturer();

            InitImage();

            InitComboBoxes();

            InitTextBoxes();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProductsWindow(true, _currentUser);
            window.Show();
            this.Close();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            OrderProduct orderProduct = db.OrderProducts.Where(op => op.ProductId == _currentProduct.ProductId).FirstOrDefault();
            
            if (orderProduct != null)
            {
				db.OrderProducts.Remove(orderProduct);
				db.SaveChanges();
			}

            db.Products.Remove(_currentProduct);
            db.SaveChanges();

            MessageBox.Show("Успешно удалено", "Внимание!");

            var window = new ProductsWindow(true, _currentUser);
            window.Show();
            this.Close();
        }

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
                descriptionTextBox
            };

            if (categoryComboBox.SelectedItem == null  || !DemoApp.Models.Validation.ValidateTextBoxes(textBoxes))
            {
                MessageBox.Show("Все поля должны быть заполнены!", "Внимание!");
                return;
            }

            if (Convert.ToInt16(discountTextBox.Text) > Convert.ToInt16(maxDiscountTextBox.Text))
            {
                MessageBox.Show("Скидка должна быть меньше максимальной!");
                return;
            }

            _currentProduct.ProductName = nameTextBox.Text;
            _currentProduct.UnitTypeId = db.UnitTypes.Where(ut => ut.UnitTypeName == unitTypeComboBox.SelectedItem.ToString()).Select(ut => ut.UnitTypeId).FirstOrDefault();
            _currentProduct.ProductCost =Convert.ToDecimal(costTextBox.Text, CultureInfo.InvariantCulture);
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

            db.Entry(_currentProduct).State = EntityState.Modified;
            db.SaveChanges();

            MessageBox.Show("Товар был изменен!");
            return;
        }

        private bool  ValidateManufacturerAndSupplier(ProductManufacturer man, ProductSupplier sup)
        {
            if (db.ProductManufacturers.Contains(man) && db.ProductSuppliers.Contains(sup))
            {
                return true;
            }
            return false;
        }

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

                System.IO.File.Copy(filename, parentDirName, true);

                _currentProduct.ProductPhoto = ofd.SafeFileName;

				db.Entry(_currentProduct).State = EntityState.Modified;
                db.SaveChanges();
                
                InitImage();
            }
        }
        private void InitImage()
        {
            BitmapImage imageSource = new BitmapImage();
            imageSource.BeginInit();
            try
            {
                imageSource.UriSource = new Uri(@"/DemoApp;component" + _currentProduct.ProductPhoto);
                BitmapImage nullImage = new BitmapImage();
                nullImage.BeginInit();
                nullImage.UriSource = new Uri(@"/DemoApp;component/Resources/", UriKind.Relative);
                if (imageSource.UriSource == nullImage.UriSource)
                {
                    imageSource.UriSource = new Uri(@"/DemoApp;component/Resources/picture.png", UriKind.Relative);
                }
            }
            catch
            {
                return;
            }
            imageSource.EndInit();
            productPhoto.Source = imageSource;
        }

        private void InitSupplierAndManufacturer()
        {
            foreach (ProductManufacturer man in db.ProductManufacturers)
            {
                if (man.ProductManufacturerId == _currentProduct.ProductManufacturerId)
                {
                    _currentProduct.ProductManufacturer = man;
                }
            }

            foreach (ProductSupplier sup in db.ProductSuppliers)
            {
                if (sup.ProductSupplierId == _currentProduct.ProductSupplierId)
                {
                    _currentProduct.ProductSupplier = sup;
                }
            }
        }

        private void InitTextBoxes()
        {
            articleTextBox.Text = _currentProduct.ProductArticleNumber.ToString();
            nameTextBox.Text = _currentProduct.ProductName;
            costTextBox.Text = _currentProduct.ProductCost.ToString();
            maxDiscountTextBox.Text = _currentProduct.ProductMaxDiscountAmount.ToString();
            manufacturerTextBox.Text = _currentProduct.ProductManufacturer.ProductManufacturerName;
            supplierTextBox.Text = _currentProduct.ProductSupplier.ProductSupplierName;
            discountTextBox.Text = _currentProduct.ProductDiscountAmount.ToString();
            quantityTextBox.Text = _currentProduct.ProductQuantityInStock.ToString();
            descriptionTextBox.Text = _currentProduct.ProductDescription.ToString();
        }

        private void InitComboBoxes()
        {
            foreach(ProductCategory category in db.ProductCategories)
            {
                categoryComboBox.Items.Add(category.ProductCategoryName);

                if (category.ProductCategoryId == _currentProduct.ProductCategoryId)
                {
                    categoryComboBox.SelectedItem = category.ProductCategoryName;
                }
            }
            foreach (UnitType unitType in db.UnitTypes)
            {
                unitTypeComboBox.Items.Add(unitType.UnitTypeName);

                if (unitType.UnitTypeId == _currentProduct.UnitTypeId)
                {
                    unitTypeComboBox.SelectedItem = unitType.UnitTypeName;
                }
            }
        }

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
