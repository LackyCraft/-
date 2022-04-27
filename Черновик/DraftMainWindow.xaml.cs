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

namespace Черновик
{
    public partial class DraftMainWindow : Window
    {
        //ЛИСТ (копия) БД - будет хранится в памяти
        public List<DataBase.MaterialList> AllMaterialList;

        //Лист, который будет для сортировки датагрида в реальном времени
        private List<DataBase.MaterialList> gridMaterialList;

        //Размер одной страницы 15 - строк
        private int PageSize = 15;

        public DraftMainWindow()
        {
            InitializeComponent();

            try
            {
                //заполняем комбобок фильтрации типами материалов
                ComboBoxFilter.ItemsSource = DataBase.DraftDataBaseEntity.GetContext().MaterialType.ToList();

                //заполняем комбобок сортировки типами для сортировки
                List<string> sortList = new List<string>() { "Нимаенование", "Остаток на складе", "Стоимость" };
                ComboBoxSort.ItemsSource = sortList;

                //заполняем листы, пока что полными слепками БД
                AllMaterialList = DataBase.DraftDataBaseEntity.GetContext().MaterialList.ToList();

                ComboBoxFilter.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("Warning x0\nПроизошла непредвиденная ошибка\nПотеряно соединение с базой данных");
            }

        }

        //Функция обновляет лист, который является эталонной копией БД. Вызывается исключительно в дочерних страницах
        public void updateAllMaterialList()
        {
            AllMaterialList = DataBase.DraftDataBaseEntity.GetContext().MaterialList.ToList();
            drawDataGrid();
        }

        //Функция для перестройки dataGrid в реальном времени
        public void drawDataGrid()
        {
            gridMaterialList = AllMaterialList;
            int selectedTypeMAterialId = int.Parse(ComboBoxFilter.SelectedValue.ToString());

            if (selectedTypeMAterialId != -1)
            {
                gridMaterialList = gridMaterialList.Where(i => i.MaterialTypeID == selectedTypeMAterialId).ToList();
            }
            if (ComboBoxSort.SelectedIndex !=-1)
            {
                if (ComboBoxSort.SelectedIndex == 0)
                {
                    gridMaterialList = gridMaterialList.OrderBy(i => i.Title).ToList();
                }else if(ComboBoxSort.SelectedIndex == 1)
                {
                    gridMaterialList = gridMaterialList.OrderBy(i => i.CountInStock).ToList();
                }else if (ComboBoxSort.SelectedIndex == 2)
                {
                    gridMaterialList = gridMaterialList.OrderBy(i => i.Cost).ToList();
                }
            }
            if (TextBoxSearch.Text.Length > 1)
            {
                gridMaterialList = gridMaterialList.Where(i => i.Title.Contains(TextBoxSearch.Text) || i.Description.Contains(TextBoxSearch.Text)).ToList();
            }

            TextBoxStatusLine.Text = "Найдено: " + gridMaterialList.Count.ToString() + " из " + AllMaterialList.Count.ToString() + " записей в БД";

            //Получаем количество страниц
            TextBoxCountPage.Text = ((gridMaterialList.Count + PageSize - 1) / PageSize).ToString();

            //Считаем сколько, строк надо пропустить до выбранной страницы.
            int skipSize = (int.Parse(TextBoxPageNumber.Text)-1)*PageSize;

            //Заполняем грид 15-ю записями с нужной нам страницы
            DataGridMaterialList.ItemsSource = gridMaterialList.Skip(skipSize).Take(PageSize);
            DataGridMaterialList.CanUserAddRows = false;
        }

        //Изменение состояний комбобоксов вызывает обновление dataGrid
        private void selectionChangedComboBox(object sender, SelectionChangedEventArgs e)
        {
            TextBoxPageNumber.Text = "1";
            drawDataGrid();
        }

        //Реализация пагинации
        private void clickButtonPage(object sender, RoutedEventArgs e)
        {
            if(sender == ButtonNextPage && int.Parse(TextBoxPageNumber.Text)<int.Parse(TextBoxCountPage.Text))
            {
                TextBoxPageNumber.Text = (int.Parse(TextBoxPageNumber.Text) + 1).ToString();
                drawDataGrid();
            }
            if (sender == ButtonBackPage && int.Parse(TextBoxPageNumber.Text) > 1)
            {
                TextBoxPageNumber.Text = (int.Parse(TextBoxPageNumber.Text) - 1).ToString();
                drawDataGrid();
            }
        }

        //Перенаправление при поиске на обновление DataGrid
        private void textChangedTextBoxSearch(object sender, TextChangedEventArgs e)
        {
            TextBoxPageNumber.Text = "1";
            drawDataGrid();
        }

        //Обработка отчистки фильтра, поиска, и сортирвоки
        private void clickButtonClear(object sender, RoutedEventArgs e)
        {
            ComboBoxSort.SelectedIndex = -1;
            ComboBoxFilter.SelectedValue = -1;
            TextBoxSearch.Text = "";
        }

        //Обработка выбора материалов для редактирования минимального количества
        private void selectionChangedDataGridMaterialList(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridMaterialList.SelectedItems.Count > 0)
            {
                TextBoxEditMinCount.IsEnabled = true;
                ButtonEditMinCount.IsEnabled = true;
                try
                {
                    double minCount = (DataGridMaterialList.SelectedItems[0] as DataBase.MaterialList).MinCount;
                    for (int i = 1; i < DataGridMaterialList.SelectedItems.Count; i++)
                    {
                        if ((DataGridMaterialList.SelectedItems[i] as DataBase.MaterialList).MinCount < minCount)
                            minCount = (DataGridMaterialList.SelectedItems[i] as DataBase.MaterialList).MinCount;
                    }
                    TextBoxEditMinCount.Text = minCount.ToString();
                }
                catch
                {
                    MessageBox.Show("Warning x0\nПроизошла непредвиденная ошибка\nПотеряно соединение с базой данных");
                }
            }
            else
            {
                TextBoxEditMinCount.IsEnabled = false;
                ButtonEditMinCount.IsEnabled = false;
            }
        }

        // Обработка нажатия ОК при быстром редактирвоании
        private void clickButtonEditMinCount(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show("Вы правда хотите изменить у всех выделенных материалов минимальное количесвто на:" + TextBoxEditMinCount.Text + "?", "Измененить минимальное количесвто материала", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    for (int i = 0; i < DataGridMaterialList.SelectedItems.Count; i++)
                    {
                        int idEditMeterial = (DataGridMaterialList.SelectedItems[i] as DataBase.MaterialList).ID;
                        DataBase.Material editMaterial = DataBase.DraftDataBaseEntity.GetContext().Material.Where(x => x.ID == idEditMeterial).First();
                        editMaterial.MinCount = double.Parse(TextBoxEditMinCount.Text);
                        DataBase.DraftDataBaseEntity.GetContext().SaveChanges();
                    }
                    AllMaterialList = DataBase.DraftDataBaseEntity.GetContext().MaterialList.ToList();
                    drawDataGrid();
                }
                catch
                {
                    MessageBox.Show("Warning x0\nПроизошла непредвиденная ошибка\nПотеряно соединение с базой данных");
                }
            }
        }

        // Контроль ввода минимального кол-ва шт материала при реадактировании
        private void textChangedTextBoxEditMinCount(object sender, TextChangedEventArgs e)
        {
            int editMinCount=0;
            if(!int.TryParse(TextBoxEditMinCount.Text, out editMinCount) || editMinCount<=0)
            {
                MessageBox.Show("Мнимальное количество шт, иожет быть только положительным целым цислом!");
                TextBoxEditMinCount.Text = "1";
            }
        }

        //Добавление нового метариала
        private void clickButtonAddNewMaterial(object sender, RoutedEventArgs e)
        {
            AddNewMaterialWindow ownedWindow = new AddNewMaterialWindow();
            ownedWindow.Owner = this;
            ownedWindow.Show();
        }

        //Редактирование выделенного материала
        private void clickButtonEditNewMaterial(object sender, RoutedEventArgs e)
        {
            if (DataGridMaterialList.SelectedItems.Count > 0)
            {
                EditMaterialWindows ownedWindow = new EditMaterialWindows(DataGridMaterialList.SelectedItems[0] as DataBase.MaterialList);
                ownedWindow.Owner = this;
                ownedWindow.Show();
            }
            else
            {
                MessageBox.Show("Не выбран материал, для редактирования!\nВыберите необходимый материал и нажмите кнопку редактировать");
            }
        }
    }
}
