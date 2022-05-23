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

namespace Pleasant_Rustle
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class PleasantRustleMainWindowMainWindow : Window
    {


        public List<DataBase.AgentList> AllAgentList;

        //Лист, который будет для сортировки датагрида в реальном времени
        private List<DataBase.AgentList> gridAgentslList;

        //Размер одной страницы 15 - строк
        private int PageSize = 15;

        public PleasantRustleMainWindowMainWindow()
        {
            InitializeComponent();

            try
            {
                //заполняем комбобок фильтрации типами материалов
                DataBase.AgentType agentsType = new DataBase.AgentType();
                agentsType.Title = "Все типы";
                agentsType.Image = null;
                ComboBoxFilter.Items.Add(agentsType);
                foreach (DataBase.AgentType agentItemsType in DataBase.Entities.GetContext().AgentType.ToList())
                {
                    ComboBoxFilter.Items.Add(agentItemsType);
                }

                //заполняем комбобок сортировки типами для сортировки
                List<string> sortList = new List<string>() { "Нимаенование (по возрастанию)", "Размер скидки (по возрастанию)", "Стоимость (по возрастанию)",
                "Нимаенование (по убыванию)", "Размер скидки (по убыванию)", "Стоимость (по убыванию)"};
                ComboBoxSort.ItemsSource = sortList;

                //заполняем листы, пока что полными слепками БД
                AllAgentList = DataBase.Entities.GetContext().AgentList.ToList();

                ComboBoxFilter.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("Warning x0\nПроизошла непредвиденная ошибка\nПотеряно соединение с базой данных");
            }

        }

        void butonClickPage(object sender, EventArgs e)
        {
            Button pageButton = sender as Button;
            TextBoxPageNumber.Text = pageButton.Content.ToString();
            drawDataGrid();
        }

        //Функция обновляет лист, который является эталонной копией БД. Вызывается исключительно в дочерних страницах
        public void updateAllAgentlList()
        {
            AllAgentList = DataBase.Entities.GetContext().AgentList.ToList();
            drawDataGrid();
        }

        //Функция для перестройки dataGrid в реальном времени
        public void drawDataGrid()
        {
            gridAgentslList = AllAgentList;

            if (ComboBoxFilter.SelectedIndex != 0)
            {
                gridAgentslList = gridAgentslList.Where(i => i.AgentTypeID == ComboBoxFilter.SelectedIndex).ToList();
            }
            if (ComboBoxSort.SelectedIndex != -1)
            {
                if (ComboBoxSort.SelectedIndex == 0)
                {
                    gridAgentslList = gridAgentslList.OrderBy(i => i.Title).ToList();
                }
                else if (ComboBoxSort.SelectedIndex == 1)
                {
                    gridAgentslList = gridAgentslList.OrderBy(i => i.Procent).ToList();
                }
                else if (ComboBoxSort.SelectedIndex == 2)
                {
                    gridAgentslList = gridAgentslList.OrderBy(i => i.countCoast).ToList();
                }
                else if (ComboBoxSort.SelectedIndex == 3)
                {
                    gridAgentslList = gridAgentslList.OrderByDescending(i => i.Title).ToList();
                }
                else if (ComboBoxSort.SelectedIndex == 4)
                {
                    gridAgentslList = gridAgentslList.OrderByDescending(i => i.Procent).ToList();
                }
                else if (ComboBoxSort.SelectedIndex == 5)
                {
                    gridAgentslList = gridAgentslList.OrderByDescending(i => i.countCoast).ToList();
                }
            }
            if (TextBoxSearch.Text.Length > 1)
            {
                gridAgentslList = gridAgentslList.Where(i => i.Title.Contains(TextBoxSearch.Text) || i.Phone.Contains(TextBoxSearch.Text) || i.Email.Contains(TextBoxSearch.Text)).ToList();
            }

            TextBoxStatusLine.Text = "Найдено: " + gridAgentslList.Count.ToString() + " из " + AllAgentList.Count.ToString() + " записей в БД";

            //Получаем количество страниц
            TextBoxCountPage.Text = ((gridAgentslList.Count + PageSize - 1) / PageSize).ToString();

            //Считаем сколько, строк надо пропустить до выбранной страницы.
            int skipSize = (int.Parse(TextBoxPageNumber.Text) - 1) * PageSize;

            StackPanelPageList.Children.Clear();
            for (int i = 1; i <= ((gridAgentslList.Count + PageSize - 1) / PageSize); i++)
            {
                Button pageButton = new Button();
                pageButton.Content = i.ToString();
                pageButton.Background = null;
                pageButton.Foreground = (Brush)Application.Current.MainWindow.FindResource("dop");
                pageButton.BorderBrush = null;
                if (i.ToString() == TextBoxPageNumber.Text)
                {
                    pageButton.FontWeight = FontWeights.Bold;
                    pageButton.Foreground = Brushes.Red;
                }
                pageButton.Click += butonClickPage;

                StackPanelPageList.Children.Add(pageButton);
            }

            //Заполняем грид 15-ю записями с нужной нам страницы
            DataGridAgentList.ItemsSource = gridAgentslList.Skip(skipSize).Take(PageSize);
            DataGridAgentList.CanUserAddRows = false;
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
            if (sender == ButtonNextPage && int.Parse(TextBoxPageNumber.Text) < int.Parse(TextBoxCountPage.Text))
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
            ComboBoxFilter.SelectedValue = 0;
            TextBoxSearch.Text = "";
        }

        //Обработка выбора материалов для редактирования минимального количества
        private void selectionChangedDataGridAgentList(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridAgentList.SelectedItems.Count > 0)
            {
                TextBoxEditPrioritet.IsEnabled = true;
                ButtonEditMinCount.IsEnabled = true;
                try
                {
                    double maxPriority = (DataGridAgentList.SelectedItems[0] as DataBase.AgentList).Priority;
                    for (int i = 1; i < DataGridAgentList.SelectedItems.Count; i++)
                    {
                        if ((DataGridAgentList.SelectedItems[i] as DataBase.AgentList).Priority > maxPriority)
                            maxPriority = (DataGridAgentList.SelectedItems[i] as DataBase.AgentList).Priority;
                    }
                    TextBoxEditPrioritet.Text = maxPriority.ToString();
                }
                catch
                {
                    MessageBox.Show("Warning x0\nПроизошла непредвиденная ошибка\nПотеряно соединение с базой данных");
                }
            }
            else
            {
                TextBoxEditPrioritet.IsEnabled = false;
                ButtonEditMinCount.IsEnabled = false;
            }
        }

        // Обработка нажатия ОК при быстром редактирвоании приоритета
        private void clickButtonEditMinCount(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show("Вы правда хотите изменить у всех выделенных агентов приоритет на:" + TextBoxEditPrioritet.Text + "?", "Измененить минимальное количесвто материала", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    for (int i = 0; i < DataGridAgentList.SelectedItems.Count; i++)
                    {
                        int idEditAgent = (DataGridAgentList.SelectedItems[i] as DataBase.AgentList).ID;
                        DataBase.Agent editMaterial = DataBase.Entities.GetContext().Agent.Where(x => x.ID == idEditAgent).First();
                        editMaterial.Priority = int.Parse(TextBoxEditPrioritet.Text);
                        DataBase.Entities.GetContext().SaveChanges();
                    }
                    AllAgentList = DataBase.Entities.GetContext().AgentList.ToList();
                    drawDataGrid();
                }
                catch
                {
                    MessageBox.Show("Warning x0\nПроизошла непредвиденная ошибка\nПотеряно соединение с базой данных");
                }
            }
        }

        // Контроль ввода минимального кол-ва шт материала при реадактировании
        private void textChangedTextBoxEditProcent(object sender, TextChangedEventArgs e)
        {
            int editPrioritet = 0;
            if (!int.TryParse(TextBoxEditPrioritet.Text, out editPrioritet) || editPrioritet <= 0)
            {
                MessageBox.Show("Процент, может быть только положительным целым цислом!");
                TextBoxEditPrioritet.Text = "1";
            }
        }

        //Добавление нового агента
        private void clickButtonAddNewProcent(object sender, RoutedEventArgs e)
        {
            //AddAgentWindow ownedWindow = new AddAgentWindow();
            //ownedWindow.Owner = this;
            //ownedWindow.Show();
        }

        //Редактирование выделенного материала
        private void clickButtonEditNewProcent(object sender, RoutedEventArgs e)
        {
            if (DataGridAgentList.SelectedItems.Count > 0)
            {
               // EditWindow ownedWindow = new EditWindow(DataBase.Entities.GetContext().Agent.Find((DataGridAgentList.SelectedItems[0] as DataBase.AgentList).ID));
               // ownedWindow.Owner = this;
               // ownedWindow.Show();
            }
            else
            {
                MessageBox.Show("Не выбран материал, для редактирования!\nВыберите необходимый материал и нажмите кнопку редактировать");
            }
        }


    }
}
