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
    public partial class EditMaterialWindows : Window
    {

        private int MaterialEditId;

        public EditMaterialWindows(DataBase.MaterialList editItemMaterial)
        {
            InitializeComponent();
            List<string> unitList = new List<string>() { "кг", "л", "м" };
            ComboBoxUnit.ItemsSource = unitList;

            ComboBoxMaterialType.ItemsSource = DataBase.DraftDataBaseEntity.GetContext().MaterialType.Where(i => i.ID != -1).ToList();
            ComboBoxSupplier.ItemsSource = DataBase.DraftDataBaseEntity.GetContext().Supplier.ToList();

            TextBoxNameMaterial.Text = editItemMaterial.Title;
            TextBoxCountInPack.Text = editItemMaterial.CountInPack.ToString();
            TextBoxCountInStock.Text = editItemMaterial.CountInStock.ToString();
            TextBoxMinCount.Text = editItemMaterial.MinCount.ToString();
            TextBoxDesc.Text = editItemMaterial.Description;
            TextBoxCoast.Text = editItemMaterial.Cost.ToString();

            TextBoxFotoName.Text = editItemMaterial.Image;
            FotoMaterial.Source = new BitmapImage(new Uri(TextBoxFotoName.Text,UriKind.Relative));

            ComboBoxMaterialType.SelectedValue = editItemMaterial.MaterialTypeID;
            ComboBoxUnit.SelectedItem = editItemMaterial.Unit;

            MaterialEditId = editItemMaterial.ID;

            DataGridSuppliersList.Items.Add(DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).Supplier);
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
            if (ComboBoxSupplier.SelectedIndex != -1)
                DataGridSuppliersList.Items.Add(ComboBoxSupplier.SelectedItem as DataBase.Supplier);
            else
                MessageBox.Show("Необходимо выбрать возможного поставщика из выпадающего списка.");
        }

        private void DeletedAt(object sender, RoutedEventArgs e)
        {
            DataGridSuppliersList.Items.Remove(DataGridSuppliersList.SelectedItem as DataBase.Supplier);
        }

        private void clickAddNewMaterial(object sender, RoutedEventArgs e)
        {
            string MessageError = "";

            if (TextBoxNameMaterial.Text.Length < 2)
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
            if (ComboBoxUnit.SelectedIndex == -1)
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
                    DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).Title = TextBoxNameMaterial.Text;
                    DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).CountInPack = int.Parse(TextBoxCountInPack.Text);
                    DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).Unit = ComboBoxUnit.SelectedValue.ToString();
                    DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).CountInStock = int.Parse(TextBoxCountInStock.Text);
                    DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).MinCount = int.Parse(TextBoxMinCount.Text);
                    DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).Description = TextBoxDesc.Text;
                    DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).Cost = int.Parse(TextBoxCoast.Text);
                    DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).Image = TextBoxFotoName.Text;
                    DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).MaterialTypeID = int.Parse(ComboBoxMaterialType.SelectedValue.ToString());

                    if (DataGridSuppliersList.Items.Count > 0)
                    {
                        DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).Supplier = DataGridSuppliersList.ItemsSource as List<DataBase.Supplier>;
                        //List < DataBase.Supplier > = new List<DataBase.Supplier>();
                    }
                    else
                    {
                        DataBase.DraftDataBaseEntity.GetContext().Material.Find(MaterialEditId).Supplier = null;
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
            float price;
            int minCount, countInPack, contInStock;
            if (int.TryParse(TextBoxCountInPack.Text, out countInPack)  &&  int.TryParse(TextBoxMinCount.Text, out minCount) && float.TryParse(TextBoxCoast.Text, out price) && int.TryParse(TextBoxCountInStock.Text, out contInStock))
            {
                if(contInStock < minCount)
                    InfoPriceMinCount.Text = "Минимально необходимая партия в " + (minCount - contInStock) +
                        " штук т.e( " + (countInPack + (minCount - contInStock))/countInPack + " упаковки) ободеться в: " 
                        + price * ((countInPack + (minCount - contInStock)) / countInPack) + " целковых";
            }
            else
            {
                InfoPriceMinCount.Text = "";
            }
        }


    }
}
