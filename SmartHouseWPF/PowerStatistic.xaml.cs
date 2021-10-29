using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZedGraph;


namespace SmartHouseWPF
{
    /// <summary>
    /// Логика взаимодействия для PowerStatistic.xaml
    /// </summary>
    public partial class PowerStatistic : Window
    {
        public PowerStatistic()
        {
            InitializeComponent();              //инициализация компонет
            ShowCharts();                       //начальное отображение графиков
            SetStartDateTimePicer();            //инициализация элементов выбора времени для периода
        }

        //инициализация DateTimePicer
        void SetStartDateTimePicer()
        {
            //dtpTimeEnd.Format = Xceed.Wpf.Toolkit.DateTimeFormat.Custom;
            //dtpTimeEnd.FormatString = "dd MMMM yyyy г. H:mm";
            //dtpTimeStart.Format = Xceed.Wpf.Toolkit.DateTimeFormat.Custom;
            //dtpTimeStart.FormatString = "dd MMMM yyyy г. H:mm";

            dtpTimeEnd.Value = DateTime.Now;
            dtpTimeStart.Value = new DateTime(dtpTimeEnd.Value.Value.Year, dtpTimeEnd.Value.Value.Month, dtpTimeEnd.Value.Value.Day - 1, dtpTimeEnd.Value.Value.Hour, dtpTimeEnd.Value.Value.Minute, 0);
        }

        #region инициализация объектов, определяющих графики (хоста с WinForm, ZedGraph и списка точек графиков)
        MainWindow mainWin = null;                                                  //главное окно
        WindowsFormsHost hostNow = new WindowsFormsHost();                          //объект хоста winForm для текущего графика
        WindowsFormsHost hostPeriod = new WindowsFormsHost();                       //объект хоста winForm для графика периода
        WindowsFormsZedGraph.ZedGraphUserControl chartNow = new WindowsFormsZedGraph.ZedGraphUserControl();                  //объект графика реального времени
        WindowsFormsZedGraph.ZedGraphUserControl chartPeriod = new WindowsFormsZedGraph.ZedGraphUserControl();                      //объект графика за период
        Dictionary<string, Dictionary<DateTime, double>> valueSensors = new Dictionary<string, Dictionary<DateTime, double>>();     //коллекция показаний с датчиков
        PointPairList listPointNow = new PointPairList();                           //список точек графика реального времени
        PointPairList listPointPeriod = new PointPairList();                        //список точек графика за период
        int limitShowDataNow = 20;                                                  //предельно допустимое количество отображаемых точек
        public bool viewNowChart = false;                                           //переменная, определяющая необходимость динамического отображения графика                                                         //Thread thrRefreshChart = null;                                  

        public Dictionary<string, Dictionary<DateTime, double>> ValueSensors
        {
            get { return valueSensors; }
            set { valueSensors = value; }
        }
        public int LimitShowDataNow
        {
            get { return limitShowDataNow; }
            set { limitShowDataNow = value; }
        }
        public PointPairList ListPointNow
        {
            get { return listPointNow; }
            set { listPointNow = value; }
        }
        public WindowsFormsZedGraph.ZedGraphUserControl ChartNow
        {
            get { return chartNow; }
            set { chartNow = value; }
        }

        #endregion

        //public delegate string MyDelegate();
        //метод обработки получения данных в реальном времени
        public void RefreshChart()
        {
            string numRoom = "";
            string typeSensor = "";
            //если строиться график для электроотопления

            //chartNow.GrPane.XAxis.Scale.MinAuto = true; ;
          

            if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "C")
            {
                numRoom = (cbxNameCurrent.SelectedItem as ComboBoxItem).Uid;
                if (!rbCurrent.IsChecked.Value)
                    numRoom += "Power";
            }
            else
                numRoom = (cbxNameRoom.SelectedItem as ComboBoxItem).Uid;
            typeSensor = (tcTypeSensor.SelectedValue as TabItem).Tag.ToString();

            string sensor = typeSensor + numRoom;
            //проверка коллекции на наличие данных датчика
            if (valueSensors.ContainsKey(sensor))
            {
                //помещение новых точек-показаний
                foreach (var el in valueSensors[sensor])
                {
                    //удаление старых точек-показаний
                    while (listPointNow.Count > limitShowDataNow - 1)
                        listPointNow.RemoveAt(0);
                    listPointNow.Add(new XDate(el.Key.Year, el.Key.Month, el.Key.Day, el.Key.Hour, el.Key.Minute, el.Key.Second), el.Value);
                    //if (listPointNow.Count < 10)
                    //{
                    //    chartNow.GrPane.XAxis.Scale.Min = new XDate(el.Key.Year, el.Key.Month, el.Key.Day, el.Key.Hour, el.Key.Minute, el.Key.Second - 2);
                    //    chartNow.GrPane.XAxis.Scale.Max = new XDate(el.Key.Year, el.Key.Month, el.Key.Day, el.Key.Hour, el.Key.Minute, el.Key.Second + 2);
                    //    //MessageBox.Show(chartNow.GrPane.XAxis.Scale.Max - chartNow.GrPane.XAxis.Scale.Min + "");
                    //}
                    //else 
                    //{
                    //    chartNow.GrPane.XAxis.Scale.MinAuto = true;
                    //    chartNow.GrPane.XAxis.Scale.MaxAuto = true;
                    //}

                }
                //очистка источника точек
                valueSensors[sensor].Clear();
                //Обновление графиков
                chartNow.GrPane.AxisChange();
                chartNow.Invalidate();
                chartNow.Refresh();
            }

        }
        //Настройка отображения графиков в зависимсоти от выбранных компонент 
        private void tcTypeSensor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.Source is TabControl)
            //{

            //очистка графиков при смене вкладки
            chartNow.GrPane.CurveList.Clear();
            chartNow.GrPane.GraphObjList.Clear();
            chartPeriod.GrPane.CurveList.Clear();
            chartPeriod.GrPane.GraphObjList.Clear();
            valueSensors.Clear();
            listPointNow.Clear();
            listPointPeriod.Clear();

            //начальное скртытие и отображение необходимых элементов интерфейса
            if (tbVoltage != null && DUpDownVoltage != null)
            {
                cbxNameRoom.Visibility = Visibility.Visible;
                //tbVoltage.Visibility = Visibility.Collapsed;
                DUpDownVoltage.Visibility = Visibility.Collapsed;
                cbxNameCurrent.Visibility = Visibility.Collapsed;
                //spCurrontPower.Visibility = Visibility.Collapsed;
                lbNameType.Content = "Комната:";
            }
            grdSettingPanel.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Auto);

            if (tcTypeSensor.SelectedValue != null)
            {
                //график -ток
                if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "C")
                {
                    if (rbCurrent.IsChecked.Value)
                    {
                        lblChartNow.Content = "Текущее потребление тока";
                        lblChartPeriod.Content = "Потребление тока за период";
                        SetChartProperties(chartNow, lblChartNow.Content.ToString(), listPointNow, System.Drawing.Color.Red, "Ток [м/а]");
                        SetChartProperties(chartPeriod, lblChartPeriod.Content.ToString(), listPointPeriod, System.Drawing.Color.Blue, "Ток [м/а]");
                        //tbVoltage.Visibility = Visibility.Collapsed;
                        //DUpDownVoltage.Visibility = Visibility.Collapsed;
                        grdSettingPanel.RowDefinitions[0].Height = new GridLength(170, GridUnitType.Pixel);
                    }
                    //if (rbPower.IsChecked.Value)
                    //{
                    //    lblChartNow.Content = "Текущее потребление мощности";
                    //    lblChartPeriod.Content = "Потребление мощности за период";
                    //    SetChartProperties(chartNow, lblChartNow.Content.ToString(), listPointNow, System.Drawing.Color.Red, "Мощность [кВт]");
                    //    SetChartProperties(chartPeriod, lblChartPeriod.Content.ToString(), listPointPeriod, System.Drawing.Color.Blue, "Мощность [кВт]");
                    //    if (cbxNameCurrent.SelectedValue != cbxNameCurrent.Items[0])
                    //    {
                    //        tbVoltage.Visibility = Visibility.Visible;
                    //        DUpDownVoltage.Visibility = Visibility.Visible;
                    //        grdSettingPanel.RowDefinitions[0].Height = new GridLength(210, GridUnitType.Pixel);
                    //    }
                    //    else
                    //    {
                    //        tbVoltage.Visibility = Visibility.Collapsed;
                    //        DUpDownVoltage.Visibility = Visibility.Collapsed;
                    //        grdSettingPanel.RowDefinitions[0].Height = new GridLength(170, GridUnitType.Pixel);
                    //    }

                    //}

                    cbxNameRoom.Visibility = Visibility.Collapsed;
                    cbxNameCurrent.Visibility = Visibility.Visible;
                    //spCurrontPower.Visibility = Visibility.Visible;
                    lbNameType.Content = "Вид датчика:";
                    if (!(sender is string))
                        cbxNameCurrent.SelectedValue = (cbxNameCurrent.Items[0] as ComboBoxItem);

                }
                //график - температура
                else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "T")
                {
                    lblChartNow.Content = "Текущие изменения температуры";
                    lblChartPeriod.Content = "Изменение температуры за период";
                    SetChartProperties(chartNow, lblChartNow.Content.ToString(), listPointNow, System.Drawing.Color.Red, "Температура [градусы Цельсия]");
                    SetChartProperties(chartPeriod, lblChartPeriod.Content.ToString(), listPointPeriod, System.Drawing.Color.Blue, "Температура [градусы Цельсия]");
                    (cbxNameRoom.Items[0] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[1] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[2] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[3] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[4] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[5] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    if (!(sender is string))
                        cbxNameRoom.SelectedValue = (cbxNameRoom.Items[0] as ComboBoxItem);
                }
                //график - влажность
                else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "H")
                {
                    lblChartNow.Content = "Текущие изменения влажности";
                    lblChartPeriod.Content = "Изменение влажности за период";
                    SetChartProperties(chartNow, lblChartNow.Content.ToString(), listPointNow, System.Drawing.Color.Red, "Влажность [%]");
                    SetChartProperties(chartPeriod, lblChartPeriod.Content.ToString(), listPointPeriod, System.Drawing.Color.Blue, "Влажность [%]");
                    (cbxNameRoom.Items[0] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[1] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[2] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[3] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[4] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[5] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    if (!(sender is string))
                        cbxNameRoom.SelectedValue = (cbxNameRoom.Items[3] as ComboBoxItem);
                }
                //график  - давление
                else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "P")
                {
                    lblChartNow.Content = "Текущие изменения атм. давления";
                    lblChartPeriod.Content = "Изменение атм. давления за период";
                    SetChartProperties(chartNow, lblChartNow.Content.ToString(), listPointNow, System.Drawing.Color.Red, "Атм. давление []");
                    SetChartProperties(chartPeriod, lblChartPeriod.Content.ToString(), listPointPeriod, System.Drawing.Color.Blue, "Атм. давление []");
                    (cbxNameRoom.Items[0] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[1] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[2] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[3] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[4] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[5] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    if (!(sender is string))
                        cbxNameRoom.SelectedValue = (cbxNameRoom.Items[3] as ComboBoxItem);
                }
                //график -газ
                else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "G")
                {
                    lblChartNow.Content = "Текущие изменения газа";
                    lblChartPeriod.Content = "Изменение газа за период";
                    SetChartProperties(chartNow, lblChartNow.Content.ToString(), listPointNow, System.Drawing.Color.Red, "Газ [у.е.]");
                    SetChartProperties(chartPeriod, lblChartPeriod.Content.ToString(), listPointPeriod, System.Drawing.Color.Blue, "Газ [у.е.]");
                    (cbxNameRoom.Items[0] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[1] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[2] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[3] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[4] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[5] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    if (!(sender is string))
                        cbxNameRoom.SelectedValue = (cbxNameRoom.Items[1] as ComboBoxItem);
                }
                //график - освещенность
                else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "L")
                {
                    lblChartNow.Content = "Текущие изменения освещенности";
                    lblChartPeriod.Content = "Изменение освещенности за период";
                    SetChartProperties(chartNow, lblChartNow.Content.ToString(), listPointNow, System.Drawing.Color.Red, "Освещенность [у.е.]");
                    SetChartProperties(chartPeriod, lblChartPeriod.Content.ToString(), listPointPeriod, System.Drawing.Color.Blue, "Освещенность [у.е.]");
                    (cbxNameRoom.Items[0] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[1] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[2] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[3] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[4] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[5] as ComboBoxItem).Visibility = Visibility.Visible;
                    if (!(sender is string))
                        cbxNameRoom.SelectedValue = (cbxNameRoom.Items[0] as ComboBoxItem);
                }
                //график - протечк
                else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "W")
                {
                    lblChartNow.Content = "Текущие изменения протечки";
                    lblChartPeriod.Content = "Изменение протечки за период";
                    SetChartProperties(chartNow, lblChartNow.Content.ToString(), listPointNow, System.Drawing.Color.Red, "Протечка [у.е.]");
                    SetChartProperties(chartPeriod, lblChartPeriod.Content.ToString(), listPointPeriod, System.Drawing.Color.Blue, "Протечка [у.е.]");
                    (cbxNameRoom.Items[0] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[1] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[2] as ComboBoxItem).Visibility = Visibility.Visible;
                    (cbxNameRoom.Items[3] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[4] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    (cbxNameRoom.Items[5] as ComboBoxItem).Visibility = Visibility.Collapsed;
                    if (!(sender is string))
                        cbxNameRoom.SelectedValue = (cbxNameRoom.Items[2] as ComboBoxItem);
                }
                //обновлеие графиков
                chartNow.GrPane.AxisChange();
                chartNow.Invalidate();
                chartNow.Refresh();
                chartPeriod.GrPane.AxisChange();
                chartPeriod.Invalidate();
                chartPeriod.Refresh();
            }
        }
        //метод отображения графиков в соответствующих groupbox
        void ShowCharts()
        {
            // график отображения статистики за период
            SetChartProperties(chartPeriod, "Потребление тока за период", listPointPeriod, System.Drawing.Color.Blue, "Ток [м/а]");
            hostPeriod.Child = chartPeriod;
            grdChartPeriod.Children.Add(hostPeriod);

            // график отображения статистики в реальном времени
            SetChartProperties(chartNow, "Текущее потребление тока", listPointNow, System.Drawing.Color.Red, "Ток [м/а]");
            hostNow.Child = chartNow;
            grdChartNow.Children.Add(hostNow);
        }
        //метод определения свойств графиков
        void SetChartProperties(WindowsFormsZedGraph.ZedGraphUserControl ZGUElement, string nameCurve, PointPairList listPoint, System.Drawing.Color colorPoint, string YAxisTitle, string typeChart = "Line")
        {
        
            //очиска имеющихся показаний
            if (ZGUElement.GrPane.CurveList.Count > 0)
                ZGUElement.GrPane.CurveList.RemoveAt(0);
            //ZGUElement.GrPane.CurveList.Clear();
            //доступ к зуммированию
            ZGUElement.ZGUControl.IsEnableHZoom = true;
            ZGUElement.ZGUControl.IsEnableVZoom = true;
            ZGUElement.GrPane.Title.IsVisible = false;
            ZGUElement.GrPane.XAxis.Title.Text = "Дата и время";
            ZGUElement.GrPane.YAxis.Title.Text = YAxisTitle;
            ZGUElement.GrPane.XAxis.Type = AxisType.Date;
            ZGUElement.GrPane.XAxis.Scale.Format = "dd.MM.yyyy г. \n\r H:mm";

            ZGUElement.GrPane.XAxis.Scale.FontSpec.Size = 12;
            ZGUElement.GrPane.IsBoundedRanges = true;

            //ZGUElement.GrPane.XAxis.Scale.IsVisible = false;
            ZGUElement.GrPane.XAxis.Scale.MinorStep = new XDate(0, 0, 0, 0, 0, 10);
            ZGUElement.GrPane.XAxis.Scale.MajorStep = new XDate(0, 0, 0, 0, 1, 0);
            //ZGUElement.GrPane.YAxis.Scale.MinorStep = 0.05;
            //отображение прямой на графике на основе данных коллекции (в виде линии или гистограммы)
            if (typeChart == "Line")
            {
                LineItem curvePowerNow = ZGUElement.GrPane.AddCurve(nameCurve,
             listPoint, colorPoint, SymbolType.Diamond);
            }
            else if (typeChart == "Gist")
            {
                ZGUElement.GrPane.AddBar("Гистограмма", listPoint, colorPoint);
            }
         
        }
        //кнопка для изменения расположения графиков (в строку или в столбик)
        private void btnChangeView_Click(object sender, RoutedEventArgs e)
        {
            //Window_SizeChanged(null, null);
            if (grdBase.RowDefinitions[2].Height != new GridLength(1, GridUnitType.Star))
            {
                grdBase.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                grdBase.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Star);
                Grid.SetColumn(grpChartPeriod, 0);
                Grid.SetRow(grpChartPeriod, 2);
            }
            else 
            {
                grdBase.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Star);
                grdBase.ColumnDefinitions[1].Width = new GridLength(2, GridUnitType.Star);
                Grid.SetColumn(grpChartPeriod, 1);
                Grid.SetRow(grpChartPeriod, 1);
            }
       
            WindowPowerStatistic.Width = WindowPowerStatistic.Width - 1;
            WindowPowerStatistic.Width = WindowPowerStatistic.Width + 1;
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                this.WindowState = WindowState.Maximized;
                //chartPeriod.ZGUControl.Size = new System.Drawing.Size((int)grpChartPeriod.ActualWidth - 15, (int)grpChartPeriod.ActualHeight - 20);
                //hostPeriod.Child = chartPeriod;
                //chartNow.ZGUControl.Size = new System.Drawing.Size((int)grpChartNow.ActualWidth - 15, (int)grpChartNow.ActualHeight - 20);
                //hostNow.Child = chartNow;
            }


        }
     
        //обработка события выбора элемента в комбобоксе, очистка всех списков имещихся точек 
        private void cbxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            valueSensors.Clear();
            listPointNow.Clear();
            listPointPeriod.Clear();
            tcTypeSensor_SelectionChanged("", null);
        }

        //смена  режима тока или напряжния
        private void rbCurrentPower_Checked(object sender, RoutedEventArgs e)
        {
            tcTypeSensor_SelectionChanged("", null);
        }



        //перерисовка графиков при изменении размеров окна
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            chartPeriod.ZGUControl.Size = new System.Drawing.Size((int)grdChartPeriod.ActualWidth - 15, (int)grdChartPeriod.ActualHeight - 20);
            hostPeriod.Child = chartPeriod;
            chartNow.ZGUControl.Size = new System.Drawing.Size((int)grdChartNow.ActualWidth - 15, (int)grdChartNow.ActualHeight - 20);
            hostNow.Child = chartNow;

            if (this.WindowState == WindowState.Maximized)
                btnChangeView.IsEnabled = false;
            else
                btnChangeView.IsEnabled = true;
        }
        //событие закрытия окна статистики
        private void WindowPowerStatistic_Closed(object sender, EventArgs e)
        {
            listPointNow.Clear();
            listPointPeriod.Clear();
            valueSensors.Clear();
            mainWin = (this.Owner as MainWindow);
            mainWin.powerWin = null;
        }



 

        //обработка события скрытия боковой панели настроеек
        private void btnArrowSettingPanel_Click(object sender, RoutedEventArgs e)
        {
            if (gbSettingChart.Visibility == Visibility.Visible)
            {
                gbSettingChart.Visibility = Visibility.Collapsed;
                MatrixTransform mir = new MatrixTransform(-1, 0, 0, 1, ((sender as Button).Content as Image).Width, 0);
                ((sender as Button).Content as Image).RenderTransform = mir;
            }
            else
            {
                gbSettingChart.Visibility = Visibility.Visible;
                MatrixTransform mir = new MatrixTransform(1, 0, 0, -1, 0, ((sender as Button).Content as Image).Width);
                ((sender as Button).Content as Image).RenderTransform = mir;
            }
        }

        //обработка события включения/выключения динамического отображения графика
        private void btnOnOffRTChart_Click(object sender, RoutedEventArgs e)
        {
            try { 
            if (viewNowChart)
            {
                (sender as Button).Content = "Запустить";
                viewNowChart = false;
            }
            else
            {
                (sender as Button).Content = "Остановить";
                viewNowChart = true;
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //обработка события изменения количества отображаемых точек
        private void PointUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
                //tcTypeSensor.SelectedValue = tiCurrent;
                if (PointUpDown != null)
                limitShowDataNow = PointUpDown.Value.Value;
        }





        //кнопка отображения статистики за указанный пользователем период
        private void btnShowChart_Click(object sender, RoutedEventArgs e)
        {
            GetListValueDB();
        }

        //получение списка точек и дат для выбранного устройства
        void GetListValueDB()
        {
            //chartPeriod.GrPane.CurveList.Clear();
            //chartPeriod.GrPane.GraphObjList.Clear();
            listPointPeriod.Clear();
            //int idDevice = -1;
            //int idRoom = -1;
            UpClassDB objFind = new UpClassDB();
            Device device = null;
            Room room = null;
            // обнаружение имени устройства и его комнаты
            if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "C") 
            {
                if (rbCurrent.IsChecked.Value)
                {
                    string nameDevice = objFind.GetFullNameDevice("C" + (cbxNameCurrent.SelectedItem as ComboBoxItem).Uid);
                    device = objFind.FindObjDB(nameDevice, "Device") as Device;
                    room = (objFind.FindObjDB("0", "Room") as Room);
                }         
                else if (rbPower.IsChecked.Value)
                {
                    string nameDevice = objFind.GetFullNameDevice("C" + (cbxNameCurrent.SelectedItem as ComboBoxItem).Uid + "Power");
                    device = objFind.FindObjDB(nameDevice, "Device") as Device;
                    room = (objFind.FindObjDB("0", "Room") as Room);
                } 
            }
            else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "T")
            {
                string nameDevice = objFind.GetFullNameDevice("T");
                device = objFind.FindObjDB(nameDevice, "Device") as Device;
            }
            else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "H")
            {
                string nameDevice = objFind.GetFullNameDevice("H");
                device = objFind.FindObjDB(nameDevice, "Device") as Device;
            }
            else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "P")
            {
                string nameDevice = objFind.GetFullNameDevice("P");
                device = objFind.FindObjDB(nameDevice, "Device") as Device;
            }
            else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "L")
            {
                string nameDevice = objFind.GetFullNameDevice("L");
                device = objFind.FindObjDB(nameDevice, "Device") as Device;
            }
            else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "W")
            {
                string nameDevice = objFind.GetFullNameDevice("W");
                device = objFind.FindObjDB(nameDevice, "Device") as Device;
            }
            else if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() == "G")
            {
                string nameDevice = objFind.GetFullNameDevice("G");
                device = objFind.FindObjDB(nameDevice, "Device") as Device;
            }
            if ((tcTypeSensor.SelectedValue as TabItem).Tag.ToString() != "C")
            {
                room = (objFind.FindObjDB((cbxNameRoom.SelectedItem as ComboBoxItem).Uid, "Room") as Room);
            }
            List<Statistic> pointStatistic = null;
            //заполнение списка точек в соответсвии с устройством и комнатой
            using (smart_houseContext con = new smart_houseContext())
            {
                if (device != null && room != null)
                {
                    pointStatistic = (from obj in con.Statistic
                               where !obj.DelFlag && !obj.Device.DelFlag && !obj.Room.DelFlag && obj.DateTime >= dtpTimeStart.Value && obj.DateTime <= dtpTimeEnd.Value
                               && obj.Device.Id == device.Id && obj.Room.Id == room.Id
                               orderby obj.DateTime ascending
                               select new Statistic
                               {
                                   DateTime = obj.DateTime,
                                   Value = obj.Value
                               }).ToList();
                }
            }
            //вывод на график
            if (pointStatistic.Count > 0)
            {
                DateTime dtOld = new DateTime();
                foreach (Statistic el in pointStatistic)
                {
               
                    if ((el.DateTime - dtOld) < new TimeSpan(0,0,5))
                        listPointPeriod.Add(new XDate(el.DateTime), (Convert.ToDouble(el.Value.Replace('.',','))));
                    else
                        listPointPeriod.Add(PointPairBase.Missing, PointPairBase.Missing);
                    dtOld = el.DateTime;
                }
                pointStatistic.Clear();
                //Обновление графиков

                chartPeriod.ZGUControl.RestoreScale(chartPeriod.GrPane);
                chartPeriod.GrPane.AxisChange();
                chartPeriod.Invalidate();
                chartPeriod.Refresh();
            }
        }
        //обработка события при смене диапазонов дат отображения графиков из БД 
        private void dateTimePicerDB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (dtpTimeStart.Value != null && dtpTimeEnd.Value != null)
            {
                TimeSpan dif = new TimeSpan();
                if (dtpTimeStart.Value.Value > dtpTimeEnd.Value.Value)
                    dif = (dtpTimeStart.Value - dtpTimeEnd.Value).Value;
                else if (dtpTimeStart.Value.Value <= dtpTimeEnd.Value.Value)
                    dif = (dtpTimeEnd.Value - dtpTimeStart.Value).Value;
                if (dif > new TimeSpan(0, 0, 0, 0) && dif > new TimeSpan(3, 0, 0, 0) || dtpTimeStart.Value.Value > dtpTimeEnd.Value.Value)
                {
                    dtpTimeStart.Value = dtpTimeEnd.Value - new TimeSpan(3, 0, 0, 0);
                }
            }
        }

        //Получение типа графика для отображения
        //string GetTypeSensorShow()
        //{
        //    string typeSensor = (tcTypeSensor.SelectedValue as TabItem).Tag.ToString();
        //    string numRoom = (cbxNameRoom.SelectedItem as ComboBoxItem).Uid;
        //    return typeSensor + numRoom;
        //    //"T"
        //}

    }
}
