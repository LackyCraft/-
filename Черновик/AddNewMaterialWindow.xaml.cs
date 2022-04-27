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
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace Черновик
{
    /// <summary>
    /// Логика взаимодействия для AddNewMaterialWindow.xaml
    /// </summary>
    public partial class AddNewMaterialWindow : Window
    {

        public AddNewMaterialWindow()
        {
            InitializeComponent();
            List<string> unitList = new List<string>() { "кг", "л", "м" };
            ComboBoxUnit.ItemsSource = unitList;

            ComboBoxMaterialType.ItemsSource = DataBase.DraftDataBaseEntity.GetContext().MaterialType.Where(i => i.ID != -1).ToList();
            ComboBoxSupplier.ItemsSource = DataBase.DraftDataBaseEntity.GetContext().Supplier.ToList();
        }

        private void selectedFoto(object sender, RoutedEventArgs e)
        {

            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                FotoMaterial.Source = new BitmapImage(new Uri(op.FileName));
                TextBoxFotoName.Text = op.FileName;
            }
        }

        private void cLickButtonAddSupplier(object sender, RoutedEventArgs e)
        {
            DataGridSuppliersList.Items.Add(ComboBoxSupplier.SelectedItem as DataBase.Supplier);
        }

        private void DeletedAt(object sender, RoutedEventArgs e)
        {
            DataGridSuppliersList.Items.Remove(DataGridSuppliersList.SelectedItem as DataBase.Supplier);
        }

        private void clickAddNewMaterial(object sender, RoutedEventArgs e)
        {
            string MessageError = "";

            if(TextBoxNameMaterial.Text.Length < 2)
            {
                MessageError += "Выберите наименование!";
            }

            int checkCount;
            if (!int.TryParse(TextBoxCountInPack.Text, out checkCount))
            {
                MessageError += "\nКоличество шт в упакове может быть только целым положительным числом!";
            }
            if (!int.TryParse(TextBoxCountInStock.Text, out checkCount))
            {
                MessageError += "\nОстаток на складе может быть только целым положительным числом!";
            }
            if (!int.TryParse(TextBoxMinCount.Text, out checkCount))
            {
                MessageError += "\nМинимальное количество может быть только целым положительным числом!";
            }
            if (TextBoxDesc.Text.Length < 2)
            {
                MessageError += "\nЗаполните описание!";
            }
            if (!int.TryParse(TextBoxCoast.Text, out checkCount))
            {
                MessageError += "\nСтоимость может быть только целым положительным числом!";
            }
            if(ComboBoxUnit.SelectedIndex == -1)
            {
                MessageError += "\nВыберите ед. измерения!";
            }
            if (ComboBoxMaterialType.SelectedIndex == -1)
            {
                MessageError += "\nВыберите тип материала!";
            }
            if (TextBoxFotoName.Text == "Выберите фотографию")
            {
                TextBoxFotoName.Text = "/materials/picture.png";
            }
            if (MessageError.Length > 2)
            {
                MessageBox.Show(MessageError);
            }
            else
            {
                try
                {
                    DataBase.Material newMaterials = new DataBase.Material();
                    newMaterials.Title = TextBoxNameMaterial.Text;
                    newMaterials.CountInPack = int.Parse(TextBoxCountInPack.Text);
                    newMaterials.Unit = ComboBoxUnit.SelectedValue.ToString();
                    newMaterials.CountInStock = int.Parse(TextBoxCountInStock.Text);
                    newMaterials.MinCount = int.Parse(TextBoxMinCount.Text);
                    newMaterials.Description = TextBoxDesc.Text;
                    newMaterials.Cost = int.Parse(TextBoxCoast.Text);
                    newMaterials.Image = TextBoxFotoName.Text;
                    newMaterials.MaterialTypeID = int.Parse(ComboBoxMaterialType.SelectedValue.ToString());
                    DataBase.DraftDataBaseEntity.GetContext().Material.Add(newMaterials);
                    DataBase.DraftDataBaseEntity.GetContext().SaveChanges();

                    if (DataGridSuppliersList.Items.Count > 0)
                    {
                        for (int i = 0; i < DataGridSuppliersList.Items.Count; i++)
                        {
                            //DataBase. newMaterials = new DataBase.Material();
                            newMaterials.Supplier.Add(DataGridSuppliersList.Items[i] as DataBase.Supplier);
                        }
                    }
                    DataBase.DraftDataBaseEntity.GetContext().SaveChanges();
                    MessageBox.Show("Запись была успешно добавлена в БД");
                    this.Close();

                    //Обновляем в главном меню Лист, в котором содержиться копия материалов из БД
                    (this.Owner as DraftMainWindow).updateAllMaterialList();

                }
                catch
                {
                    MessageBox.Show("Произошла непредвиденная ошибка");
                }
            }
        }

        private void changedTextBoxMinCount(object sender, TextChangedEventArgs e)
        {
            int minCount, price;
            if (int.TryParse(TextBoxMinCount.Text,out minCount) && int.TryParse(TextBoxCoast.Text, out price))
            {
                InfoPriceMinCount.Text = "Минимально партия будет обходится в: " + minCount*price + " целковых";
            }
            else
            {
                InfoPriceMinCount.Text = "";
            }
        }
    }
}
