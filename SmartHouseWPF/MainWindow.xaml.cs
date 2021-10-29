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
using System.Timers;
using System.IO.Ports;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.Data;
using ZedGraph;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;

namespace SmartHouseWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //userCon = new UserSystem();
            //userCon.UserGroup.Name = "Гость";
            SetBinding();
            //CreateConfigFileDB(); //инициализация файла конфигурации подключения к БД
            //GetConnectionStringDB(); //инициализация строки подключения к БД
            Init(); //инициализация компонент
            baseTabControl.SelectedValue = elementTIAppSetting; //установка вкладки настройки подключения к COM в качестве исходной


            //      string sqlStr =
            //@"SELECT user_group.id, user_group.name
            //          FROM user_group";
            //      GetDataDB(kkk, sqlStr);


            //GetConfigDevice(); // !!!!! тестовое получение данных из БД

            SerialPortDedector(); //циклическое обнаружение подключенных портов
            cbRageSensor_SelectionChanged(null, null); //инициализация диапазонов датчиков

            //инициализация диапазовнов дат для БД
      

           
        }

        #region Инициализация переменных
        //bool isConnected = false; //переменная соединения
        bool isGettable = false;                                            //режим отображения данных в консоли
        string[] serialPorts;                                               //доступные порты
        public SPControl serialPort = new SPControl();                      //объект - последовательный порт
        ControleOjbectsHouse controleObj = new ControleOjbectsHouse();      //объект - контроллер Ардуино
        FlowDocument mcFlowDoc = new FlowDocument();                        //потоковый документ для вывода данных в консоль
        Paragraph paragraph = new Paragraph();                              //абзац для управления в потоковом документе 
        private delegate void UpdateUiTextDelegate(string data);            //делегат для обновления данных в консоли отображения
        private delegate void UpdateUiTextSensorDelegate(FrameworkElement obj, string data, char separator); //делегат для обновления данных на интерфейсе
        private delegate void UpdateUiTextSensorDelegate2(FrameworkElement obj, string data, string separator, string[] dataa); //делегат для обновления данных на интерфейсе
        private delegate void CyclicPollingBuildChart(string type, string num, string param, DateTime dt); //делегат циклического построения графиков датчиков
        private delegate void CyclicPollingBuildDB(string type, string num, string param);
        private delegate void DBDelegate();                                 //делегат для отображения данных из БД
        private delegate void DBDelegateFixedData(string typeCmd, string numRoom, string value); //делегат для фиксации данных в БД 


        public PowerStatistic powerWin = null;                              //окно отображения статистки
        int timeSleepCyclicPolling = 1000;                                  //частота опроса датчиков
        Thread thrCyclicPolling = null;                                     //поток циклического опроса
        Thread thrCyclicChart= null;                                     //поток циклического опроса
        Thread thrStartSignalization = null;                                //поток циклического опроса
        //Thread thrCyclicBuildChart = null;                                //поток построения графиков

        Dictionary<object, string> listLastActivatedDevice = new Dictionary<object, string>(); //список последних активированных устройств
        string lastCommandSend = "";                                        //последняя отправленная команда
        object lastActivatedDevice = null;                                  //последнее устройство с которым было совершено взаимодействие
        string lastImgIco = "";                                             //изображение устройства до аткивации                                                 
        DispatcherTimer disTimerCommandX = new DispatcherTimer();           //таймер ожидания получения ответа на отправленную команду управления
        DispatcherTimer disTimerSyncClock = new DispatcherTimer();          //таймер синхронизации часов
        DispatcherTimer disTimerSignalization = new DispatcherTimer();      //таймер-интервал сигнализации
        DispatcherTimer disTimerKWH = new DispatcherTimer();                //таймер для расчета клловат/часов
        DateTime timerPower = new System.DateTime();                        //дата начала расчета милливат/часов
        double powerSum = 0;
        double powerSum1 = 0;
        DateTime newDt = new DateTime();                                    //часы контроллера
        TimeSpan diffTime = new TimeSpan();                                 //Интервал - разница между часами котроллера и часами компьютера
        bool handRequest = false;                                           //идентификатор ручного совершения ручного опроса

        static DataBaseConfig dataBaseConfig = new DataBaseConfig();        //объект подключеня к базе данных
        BaseDBClass objBaseClass = new BaseDBClass(dataBaseConfig);
        DataSet dataSet = new DataSet();                                    //объект для хранения таблиц полученных из базы данных
        DataTable dataTable = new DataTable();                              //объект - таблица, хранящая данные из базы 
        string connectionString = "";                                       //строка подключения к базе данных
        int threadSleepGetData = 300;


        UserSystem userCon = new UserSystem();
        bool writeDB = false;
        //Classes.User userSystem = null;

        public string ConnectionString
        { get { return connectionString; } set { connectionString = value; } }
        public BaseDBClass ObjBaseClass
        { get { return objBaseClass; } set { objBaseClass = value; } }
        public UserSystem UserCon
        { get { return userCon; } set { userCon = value; } }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuildNotification("Вход пользователя [" + userCon.FullName + " - " + userCon.UserGroup.Name + "] в систему");
            FixedAction("Вход пользователя [" + userCon.FullName + " - " + userCon.UserGroup.Name + "] в систему");
        }
        #endregion




        #region Анимация шторки боковой панели
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (elementTIAppControl.IsSelected)
                if (panel_options_fr.ActualWidth != 0)
                {
                    DoubleAnimation panel_anim = new DoubleAnimation();
                    panel_anim.From = panel_options_fr.ActualWidth;
                    panel_anim.To = 0;
                    panel_anim.Duration = TimeSpan.FromSeconds(0.3);
                    panel_options_fr.BeginAnimation(StackPanel.WidthProperty, panel_anim);
                    MatrixTransform mir = new MatrixTransform(-1, 0, 0, 1, arrow_img_fr.Width, 0);
                    arrow_img_fr.RenderTransform = mir;

                }
                else
                {
                    DoubleAnimation panel_anim = new DoubleAnimation();
                    panel_anim.From = panel_options_fr.ActualWidth;
                    panel_anim.To = 200;
                    panel_anim.Duration = TimeSpan.FromSeconds(0.3);
                    panel_options_fr.BeginAnimation(StackPanel.WidthProperty, panel_anim);
                    MatrixTransform mir = new MatrixTransform(1, 0, 0, -1, 0, arrow_img_fr.Width);
                    arrow_img_fr.RenderTransform = mir;
                }
        }
        #endregion

        #region Инициализация полей настройки для подключения COM-порта

        //void GetPortNm()
        //{
        //    while (true)
        //    {
        //        serialPorts = SPControl.GetPortNames();
        //        Dictionary<ComboBox, string[]> listSettingParam = new Dictionary<ComboBox, string[]> {
        //        { cbPortName, serialPorts}
        //    };
        //        foreach (KeyValuePair<ComboBox, string[]> elList in listSettingParam)
        //        {
        //            GetSettingParam(elList.Value, elList.Key);
        //        }
        //    }
        //}
        //Инициализация компонент 
        void Init()
        {
            serialPorts = SPControl.GetPortNames();
            string[] arrBaundRate = new string[] { "115200", "74880", "57600", "38400", "19200", "9600", "4800", "2400", "1200", "300" };
            string[] arrParityBits = new string[] { "None", "Odd", "Even", "Mark", "Space" };
            string[] arrDataBits = new string[] { "5", "6", "7", "8" };
            string[] arrStopBits = new string[] { "1", "2", "1.5", "None" };
            string[] arrFlowControl = new string[] { "None", "RTS", "RTS/X", "Xon/Xoff" };
            Dictionary<ComboBox, string[]> listSettingParam = new Dictionary<ComboBox, string[]> {
                { cbPortName, serialPorts},
                { cbBaundRate, arrBaundRate},
                { cbParityBits, arrParityBits},
                { cbDataBits, arrDataBits},
                { cbStopBits, arrStopBits},
                { cbFlowControl, arrFlowControl}
            };
            foreach (KeyValuePair<ComboBox, string[]> elList in listSettingParam)
            {
                GetSettingParam(elList.Value, elList.Key);
            }
            cbBaundRate.SelectedValue = "115200";
            cbDataBits.SelectedValue = "8";

            //dtpTimeArduino.Format = Xceed.Wpf.Toolkit.DateTimeFormat.Custom;
            //dtpTimeArduino.FormatString = "dd MMMM yyyy г. H:mm";
            dtpTimeArduino.Value = new DateTime(2019, 1, 1, 0, 0, 0);
        }
        //заполнение Combobox во вкладке настрокек
        void GetSettingParam(string[] arr, ComboBox obj)
        {
            foreach (string elArr in arr)
            {
                //this.Dispatcher.Invoke((Action)(() =>
                //{
                if (!obj.Items.Contains(elArr))
                    obj.Items.Add(elArr);

                if (arr.Count() > 0 && arr[0] != null)
                {
                    obj.SelectedItem = arr[0];
                }

                //}));
            }
        }
        #endregion

        #region ОБнаружение COM-порта постоянным опросом 
        //запуск таймера опроса порта
        void SerialPortDedector()
        {
            DispatcherTimer disTimerSP = new DispatcherTimer();
            disTimerSP.Interval = new TimeSpan(10);
            disTimerSP.Tick += DisTimerSP_Tick;
            disTimerSP.Start();
        }
        bool isConnetPort = false;
        //обработчик таймера поиска ком-порта
        private void DisTimerSP_Tick(object sender, EventArgs e)
        {
            //auto detect COM port//
            string[] ports = SerialPort.GetPortNames();
            foreach (var el in ports)
            {
                if (!cbPortName.Items.Contains(el))
                    cbPortName.Items.Add(el);
            }
            for (int i = 0; i < cbPortName.Items.Count; i++)
            {
                if (!ports.Contains(cbPortName.Items[i].ToString()))
                {
                    cbPortName.Items.Remove(cbPortName.Items[i]);
                    i = 0;
                }
            }
            //if (cbPortName.Items.Count > 0 && cbPortName.Items.Contains(serialPort.PortName) && serialPort.PortName != "" )
            //{
            //    //cbPortName.SelectedValue = serialPort.PortName;
            //    //isConnetPort = true;
            //}
            if (cbPortName.Items.Count > 0 && cbPortName.SelectedItem == null)
            {
                cbPortName.SelectedIndex = 0;
                //MessageBox.Show("q");
                //isConnetPort = true;
            }
            else if (cbPortName.Items.Count <= 0 || !serialPort.IsOpen)
            {
                //cbPortName.Text = "";
                //если провод был выдернут, то отключиться от стенда 
                if (btnSetConnection.Content.ToString() != "Подключиться")
                {
                    ResetPicture(grdHousePlan);
                    btnSetConnection.Content = "Подключиться";
                }
                //MessageBox.Show(cbPortName.Items.Count + "");
            }
            //rbPollSensorOnOff_Checked(null, null);

        }
        #endregion

        #region Утсновка связи с COM, запуск циклического опроса датчиков
        //Установка/разрыв соединения c COM портом
        private void btnSetConnection_Click(object sender, RoutedEventArgs e)
        {
            ControlConnection();
            //if (serialPort.IsOpen)
            //    StartCyclicPolling();
            //else
            //    if (thrCyclicPolling != null)
            //        thrCyclicPolling.Abort();
        }
        private void ControlConnection()
        {
            //try
            //{
            if (cbPortName.Items.Count == 0)
            {
                System.Windows.MessageBox.Show("COM-порт не выбран!");
                return;
            }
            if (!serialPort.IsOpen)
            {
                string portName = cbPortName.SelectedItem.ToString();
                int baundRate = Convert.ToInt32(cbBaundRate.Text);
                var parityBits = (Parity)Enum.Parse(typeof(Parity), cbParityBits.SelectedIndex.ToString(), true);
                var dataBits = Convert.ToInt32(cbDataBits.Text);
                var stopBits = (StopBits)Enum.Parse(typeof(StopBits), (cbStopBits.SelectedIndex + 1).ToString(), true);
                var handshake = (Handshake)Enum.Parse(typeof(Handshake), cbFlowControl.SelectedIndex.ToString(), true);
                int readTimeout = 100;
                int writeTimeout = 100;

                serialPort = new SPControl(portName, baundRate, parityBits, dataBits, stopBits, handshake, readTimeout, writeTimeout);
                serialPort.OpenConnection();
                if (serialPort.IsOpen)
                serialPort.Write("QU");
                //    MessageBox.Show(serialPort.IsOpen + "");
                btnSetConnection.Content = "Отключиться";
                serialPort.ErrorReceived += SerialPort_ErrorReceived;
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
                BuildNotification("Установка соединения с COM-портом: " + serialPort.PortName);
                FixedAction("Установка соединения с COM-портом: " + serialPort.PortName);
                if (rbPollSensorOn.IsChecked.Value)
                {
                    StartCyclicPolling();
                    BuildNotification("Включение автономного опроса датчиков");
                    FixedAction("Включение автономного опроса датчиков");
                }
                SyncClock(); //синхронизация часов
                //фиксация времени подключения к стенду для расчета мощности в милливат часах
                timerPower = DateTime.Now;
            }
            else if (serialPort.IsOpen)
            {
                //serialPort.Write("QR");
                serialPort.ErrorReceived -= SerialPort_ErrorReceived;
                serialPort.DataReceived -= new SerialDataReceivedEventHandler(serialPort_DataReceived);
                BuildNotification("Разрыв соединения с COM-портом: " + serialPort.PortName);
                FixedAction("Разрыв соединения с COM-портом: " + serialPort.PortName);
                serialPort.CloseConnection();

                btnSetConnection.Content = "Подключиться";
                ResetPicture(grdHousePlan);
                isConnetPort = false;


            }

            //catch /*{ }*/
            //(Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

        }
        //обуление изображений при отключении от COM-порта
        void ResetPicture(FrameworkElement obj)
        {
            if (obj != null)
            {
                var listChildrenControl = LogicalTreeHelper.GetChildren(obj);
                foreach (var el in listChildrenControl)
                {
                    if (el != null)
                    {
                        //датчик температуры, тока, влажности, атм. давления
                        if (el is LiveCharts.Wpf.Gauge)
                        {
                            LiveCharts.Wpf.Gauge elGauge = el as LiveCharts.Wpf.Gauge;
                            elGauge.Value = 0;
                        }

                        if (el is Image)
                        {
                            string pic = "pack://application:,,,/" + (el as Image).Tag.ToString();
                            Image elImg = el as Image;
                            (el as Image).Source = new BitmapImage(new Uri(pic));
                        }
                    }
                    //поиск дочерних UI элементов 
                    ResetPicture(el as FrameworkElement);


                }
            }
        }
        //выбор необходимости в автономном опросе датчиков
        private void rbPollSensorOnOff_Checked(object sender, RoutedEventArgs e)
        {
            if (rbPollSensorOn.IsChecked.Value)
            {
                if (serialPort.IsOpen)
                {
                    //BuildNotification("Включение автономного опроса датчиков");
                    StartCyclicPolling();
                    BuildNotification("Включение автономного опроса датчиков");
                    FixedAction("Включение автономного опроса датчиков");
                    return;
                }
                //else if (thrCyclicPolling != null)
                //{
                //    thrCyclicPolling.Abort();
                //}
            }
            else if (rbPollSensorOff.IsChecked.Value)
            {

                if (thrCyclicPolling != null)
                {
                    //BuildNotification("Выключение автономного опроса датчиков");
                    thrCyclicPolling.Abort();
                    thrCyclicPolling = null;
                }
                BuildNotification("Выключение автономного опроса датчиков");
                FixedAction("Выключение автономного опроса датчиков");
            }
        }
        //метод начала циклического опроса датчиков
        void StartCyclicPolling()
        {
            if (thrCyclicPolling == null)
            {
                thrCyclicPolling = new Thread(new ThreadStart(CyclicSensorPolling));
                thrCyclicPolling.Priority = ThreadPriority.AboveNormal;
                thrCyclicPolling.IsBackground = true;
                thrCyclicPolling.Start();
            }
        }
        //команды опроса состояния всех датчиков
        void CyclicSensorPolling()
        {
            //L - освещение
            //M - движение
            //T - температура
            //V - вибрация
            //H - влажность
            //P - атм. давление
            //W - протечка воды
            //G - утечка газа
            //D - открытие двери 
            //C - ток
            //R - дата и время
            while (serialPort.IsOpen)
            {
                List<string> typeSensor = new List<string>() { "L", "M", "T", "V", "H", "P", "W", "G", "D", "C",/* "R"*/ };
                foreach (var el in typeSensor)
                {
                    if (el != "C")
                    {
                        controleObj.SensorPolling(serialPort, el, 0);
                        Thread.Sleep(timeSleepCyclicPolling);
                    }
                    else
                        for (int i = 1; i <= 4; i++)
                        {
                            //отдельный опрос для датчиков тока для получения тока и мощностей
                            controleObj.SensorPolling(serialPort, el, i);
                            Thread.Sleep(timeSleepCyclicPolling);
                        }
                }

                //controleObj.SensorPolling(serialPort, "T", 7);
                //Thread.Sleep(timeSleepCyclicPolling / 2);
                //controleObj.SensorPolling(serialPort, "L", 7);
                //Thread.Sleep(timeSleepCyclicPolling / 2);
            }
        }
        //установка частоты опроса датчиков
        private void btnSetFrPollingSensor_Click(object sender, RoutedEventArgs e)
        {
            timeSleepCyclicPolling = IUpDown.Value.Value;
            timeSleepCyclicPolling = (timeSleepCyclicPolling / 13);
            threadSleepGetData = IUpDown2.Value.Value;
            BuildNotification("Установка частоты опроса датчиков: " + IUpDown.Value.Value + " [мс]");
            FixedAction("Установка частоты опроса датчиков: " + IUpDown.Value.Value + " [мс]");
        }
        //Ручной опрос датчиков
        private void HandSensorPolling_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string type = "";
            if (sender is Image)
                type = (sender as Image).Uid;
            else if (sender is LiveCharts.Wpf.Gauge)
                type = (sender as LiveCharts.Wpf.Gauge).Uid;
            controleObj.SensorPolling(serialPort, type.Substring(0, 1), Convert.ToInt32(type.Substring(1, 1)));
            handRequest = true; //фиксация того, что производиться локальный-ручной запрос состояния датчика
            if (type.Contains("Power"))
            {
                BuildNotification("Запрос состояния датчика " + GetTypeDeviceRoom(type.Substring(0, 2), "sensor") + " для получения мощности");
                FixedAction("Запрос состояния датчика " + GetTypeDeviceRoom(type.Substring(0, 2), "sensor") + " для получения мощности", type.Substring(0, 1), type.Substring(1, 1));
                return;
            }
            else if (type.Contains("C"))
            {
                BuildNotification("Запрос состояния датчика " + GetTypeDeviceRoom(type, "sensor"));
                FixedAction("Запрос состояния датчика " + GetTypeDeviceRoom(type.Substring(0, 2), "sensor"), type.Substring(0, 1), type.Substring(1, 1));
                return;
            }
            else if (type.Contains("V"))
            {
                BuildNotification("Запрос состояния датчика " + GetTypeDeviceRoom(type, "sensor"));
                FixedAction("Запрос состояния датчика " + GetTypeDeviceRoom(type.Substring(0, 2), "sensor"), type.Substring(0, 1), type.Substring(1, 1));
                return;
            }
            BuildNotification("Запрос состояния датчика " + GetTypeDeviceRoom(type, "sensor") + " в комнате: " + GetTypeDeviceRoom(type.Substring(1, 1), "room"));
            FixedAction("Запрос состояния датчика " + GetTypeDeviceRoom(type.Substring(0, 2), "sensor"), type.Substring(0, 1), type.Substring(1, 1));
        }

        #endregion

        #region Потоковое ПОЛУЧЕНИЕ ДАННЫХ с COM-порта 
        //обработка события ошибки считывания данных
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.EventType == SerialError.TXFull)
                this.serialPort.DiscardOutBuffer();
            else
                this.serialPort.DiscardInBuffer();
        }
        //обработка события обновления данных с COM порта
        public void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(threadSleepGetData);
                string serialPortData = serialPort.ReadExisting();
                //обработка получения показаний и вывод интерфейс
                Dispatcher.BeginInvoke(new UpdateUiTextSensorDelegate(DataReceivedParsing), grdHousePlan, serialPortData, '#');
                //обработка получения показаний и вывод в консоль
                Dispatcher.BeginInvoke(new UpdateUiTextDelegate(LineReceived), serialPortData);

            }
            catch
            (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //получение строк из COM-порта и отображение их во встроенную консоль
        private void LineReceived(string data)
        {
            if (isGettable)
            {
                paragraph.Inlines.Add(data);
                mcFlowDoc.Blocks.Add(paragraph);
                //this.Title = paragraph.Inlines.Count + "";
                if (paragraph.Inlines.Count > 200)
                {
                    mcFlowDoc.Blocks.Clear();
                    paragraph.Inlines.Clear();
                }
                rtbReadData.Document = mcFlowDoc;
                rtbReadData.ScrollToEnd();
            }
            //tbMessage.Text += data.Trim() + "\r\n";*/
        }
        //синхронизация с часами котроллера
        void SyncClock()
        {
            controleObj.SensorPolling(serialPort, "R", 0);
            Thread.Sleep(timeSleepCyclicPolling);
        }
        //Таймер для расчета киловат часов
        void ControlTimerKWH(string type = "start")
        {
            if (type == "start")
            {
                disTimerKWH.Interval = new TimeSpan(0, 0, 1);
                //disTimerKWH.Tick += ; 
                disTimerKWH.Start();
            }
            else
            {
                disTimerKWH.Stop();
                disTimerKWH = new DispatcherTimer();
            }
        }
        private void DisTimerSyncClock_Tick(object sender, EventArgs e)
        {
            newDt = DateTime.Now - diffTime;
            tbDate.Text = newDt.Day.ToString("00") + "." + newDt.Month.ToString("00") + "." + newDt.Year.ToString("0000");
            tbTime.Text = newDt.Hour.ToString("00") + ":" + newDt.Minute.ToString("00");
        }
        //спрос таймера для расчета милливат часов
        private void btnRefreshTimerPower_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(Math.Abs((timerPower - DateTime.Now).TotalHours)+"");
            timerPower = DateTime.Now;
            powerSum = 0;
        }

        #region Парсинг ответа полученного с контроллера "Умное здание" и их отображение в интерфейсе 
        //L - освещение
        //M - движение
        //T - температура
        //V - вибрация
        //H - влажность
        //P - атм. давление
        //W - протечка воды
        //G - утечка газа
        //D - открытие двери 
        //C - ток
        //R - дата и время

        //парсинг ответа полученного с контроллера "Умное здание"
        void DataReceivedParsing(FrameworkElement obj, string data, char separator)
        {
            string[] arrObj = data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var el in arrObj)
            {
                string[] valuesAnswer = el.Split(separator);

                if (valuesAnswer.Count() >= 2 /*&& valuesAnswer[0].Length == 3*/)
                {
                    //if (valuesAnswer[0].Length == 2 && valuesAnswer[0].Substring(0, 1) == "Q" && valuesAnswer[0].Substring(1, 1) == "U")
                    //{
                    //    BuildNotification("Подключение к стенду прошло успешно");
                    //}



                    if (valuesAnswer[0].Length == 3 && valuesAnswer[0].Substring(0, 1) == "Y"/* && valuesAnswer.Count() == 8*/)
                    {
                        string typeCmd = valuesAnswer[0].Substring(1, 1);
                        string numRoom = valuesAnswer[0].Substring(2, 1);
                        string[] arrParam = new string[valuesAnswer.Length - 2 /*2*/];
                        Array.Copy(valuesAnswer, 1, arrParam, 0, valuesAnswer.Length - 2);
                        //поиск UI -элемента для отображения показаний


                        //ChartAndDBCreate(typeCmd, numRoom, arrParam);
                        Dispatcher.BeginInvoke(new UpdateUiTextSensorDelegate2(FindControl), obj, typeCmd, numRoom, arrParam);
                         //FindControl(obj, typeCmd, numRoom, arrParam);
                        return;
                    }

                    if (valuesAnswer[0].Length == 3 && valuesAnswer[0].Substring(0, 1) == "X"/* && valuesAnswer.Count() == 8*/)
                    {
                        string typeCmd = valuesAnswer[0].Substring(1, 1);
                        string numRoom = valuesAnswer[0].Substring(2, 1);

                        if ((typeCmd + numRoom) == lastCommandSend)
                        {
                            BuildNotification("Успешный ответ на последнюю запрошенную команду управления устройствами");
                            FixedAction("Успешный ответ на последнюю запрошенную команду управления устройствами", typeCmd, numRoom); ///////////////////////////////////////////////
                            disTimerCommandX.Tick -= DisTimerCommandX_Tick;
                            disTimerCommandX.Stop();
                            disTimerCommandX = new DispatcherTimer();
                            //MessageBox.Show("Пришел ответ на последнюю команду!");
                            lastCommandSend = "";
                            lastActivatedDevice = null;
                            lastImgIco = "";
                        }
                        return;
                    }

                }
            }

        }
        //Поиск элемента интрефейса на котором надо отобразить полученные показания
        /*async*/
        void FindControl(FrameworkElement obj, string typeCmd, string numRoom, string[] arrParam)
        {
            if (obj != null)
            {
                var listChildrenControl = LogicalTreeHelper.GetChildren(obj);
                //для широковещательного обращения к датчикам
                if (numRoom == "0")
                {
                    foreach (var el in listChildrenControl)
                    {
                        if (el != null)
                        {
                            //датчик температуры, влажности, атм. давления
                            if (typeCmd == "T" || typeCmd == "C" || typeCmd == "H" || typeCmd == "P" || typeCmd == "L" || typeCmd == "G" || typeCmd == "W")
                            {
                                for (int i = 0; i < arrParam.Count(); i++)
                                {
                                    //если UI элемент - полукруг
                                    if (el is LiveCharts.Wpf.Gauge)
                                    {
                                        LiveCharts.Wpf.Gauge elGauge = el as LiveCharts.Wpf.Gauge;
                                        if (elGauge.Uid == typeCmd + (i + 1) && arrParam[i] != "" && arrParam[i] != "@")
                                        {
                                            double val = Convert.ToDouble(arrParam[i].Replace(".", ","));
                                            elGauge.Value = val;
                                            //фиксация данных в БД
                                            //await Task.Run(() => FixedStatistic(typeCmd, (i + 1).ToString(), val.ToString()));
                                            //Dispatcher.CurrentDispatcher.BeginInvoke(new DBDelegateFixedData(FixedStatistic), DispatcherPriority.SystemIdle, typeCmd, (i + 1).ToString(), val.ToString());
                                            //построение графиков на основе полученных показаний - в отдельном потоке
                                            //BuildChart(typeCmd, Convert.ToString(i + 1), arrParam[i]);
                                            //await Task.Run(() => BuildChart(typeCmd, Convert.ToString(i + 1), arrParam[i]));
                                            //Dispatcher.CurrentDispatcher.BeginInvoke(new CyclicPollingBuildChart(BuildChart), DispatcherPriority.Normal, typeCmd, Convert.ToString(i + 1), arrParam[i]);
                                            //Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new CyclicPollingBuildChart(BuildChart), typeCmd, Convert.ToString(i + 1), arrParam[i]);
                                        }
                                    }
                                }

                            }
                            //дата и время - расчет разницы между времем котроллера и текущей, запуск синхронизации
                            else if (typeCmd == "R")
                            {
                                string[] dt = arrParam[0].Split(':', ' ', '.');
                                tbDate.Text = dt[2] + "." + dt[1] + "." + dt[0];
                                tbTime.Text = dt[3] + ":" + dt[4];

                                diffTime = DateTime.Now - new DateTime(Convert.ToInt32(dt[0]),
                                                                        Convert.ToInt32(dt[1]),
                                                                        Convert.ToInt32(dt[2]),
                                                                        Convert.ToInt32(dt[3]),
                                                                        Convert.ToInt32(dt[4]),
                                                                        Convert.ToInt32(dt[5]));

                                disTimerSyncClock.Interval = new TimeSpan(0, 0, 1);
                                disTimerSyncClock.Tick += DisTimerSyncClock_Tick;
                                disTimerSyncClock.Start();

                                //tbDate.Text = arrParam[0].Split(' ')[0].Split(':')[0] + ":" + arrParam[0].Split(' ')[0].Split(':')[1];
                                //tbTime.Text = arrParam[0].Split(' ')[1];
                                //tbTime.Text = arrParam[0] + ":" + arrParam[1];
                                //tbDate.Text = arrParam[2] + "." + arrParam[3] + "." + arrParam[4];
                            }
                            //прочие датчики (движение, вибрация)
                            else
                            {
                                if (el is Image)
                                {
                                    Image elImg = el as Image;
                                    for (int i = 0; i < arrParam.Count(); i++)
                                    {
                                        if (elImg.Uid == typeCmd + (i + 1) && arrParam[i] != "" && arrParam[i] != "@")
                                        {
                                            //Dispatcher.BeginInvoke(new DBDelegateFixedData(FixedStatistic), DispatcherPriority.Background, typeCmd, (i + 1).ToString(), arrParam[i].ToString());
                                            if (Convert.ToInt32(arrParam[i]) == 0)
                                                (el as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + typeCmd + "_no.png"));
                                            else
                                                (el as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + typeCmd + "_yes.png"));
                                        }
                                    }

                                    //if (elImg.Uid == typeCmd + numRoom)
                                    //{
                                    //    if (Convert.ToInt32(arrParam[0]) == 0)
                                    //        (el as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + typeCmd + "_no.png"));
                                    //    else
                                    //        (el as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + typeCmd + "_yes.png"));
                                    //}
                                }
                            }
                            //поиск дочерних UI элементов 
                            FindControl(el as FrameworkElement, typeCmd, numRoom, arrParam);
                        }
                    }
                }
                //для локального обращения к датчикам
                else
                {
                    foreach (var el in listChildrenControl)
                    {
                        if (el != null)
                        {
                            //датчик температуры, влажности, атм. давления
                            if (typeCmd == "T" || typeCmd == "H" || typeCmd == "P" || typeCmd == "L" || typeCmd == "G" || typeCmd == "W")
                            {
                                if (el is LiveCharts.Wpf.Gauge)
                                {
                                    LiveCharts.Wpf.Gauge elGauge = el as LiveCharts.Wpf.Gauge;
                                    if (elGauge.Uid == typeCmd + numRoom && arrParam[0] != "" && arrParam[0] != "@")
                                    {
                                        elGauge.Value = Convert.ToDouble(arrParam[0].Replace(".", ","));
                                        BuildChart(typeCmd, numRoom, arrParam[0], DateTime.Now);
                                        Dispatcher.BeginInvoke(new DBDelegateFixedData(FixedStatistic), DispatcherPriority.Background, typeCmd, numRoom, arrParam[0]);
                                
                                        BuildNotification("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd, "sensor") + " в комнате:" + GetTypeDeviceRoom(numRoom, "room") + ". Значение: " + elGauge.Value);
                                        FixedAction("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd, "sensor") + " в комнате:" + GetTypeDeviceRoom(numRoom, "room"), typeCmd, numRoom);

                                    }
                                }
                                //построение графиков на основе полученных показаний
                                //Dispatcher.BeginInvoke(new CyclicPolling(BuildChart), typeCmd, numRoom, arrParam);
                                //BuildChart(typeCmd, numRoom, arrParam);
                            }
                            else if (typeCmd == "C")
                            {
                                //если UI элемент - полукруг
                                if (el is LiveCharts.Wpf.Gauge)
                                {
                                    LiveCharts.Wpf.Gauge elGauge = el as LiveCharts.Wpf.Gauge;
                                    //вывод только тока
                                    if (elGauge.Uid == typeCmd + numRoom /*+ "1"*/ && arrParam[0] != "" && arrParam[0] != "@")
                                    {
                                        double current = Convert.ToDouble(arrParam[0].Replace(".", ","));
                                        if (current != 0)
                                            elGauge.Value = Convert.ToDouble(current.ToString("#.#"));
                                        else
                                            elGauge.Value = current;
                                        //построение графиков на основе полученных показаний
                                        BuildChart(typeCmd, numRoom, arrParam[0], DateTime.Now);
                                        Dispatcher.BeginInvoke(new DBDelegateFixedData(FixedStatistic), DispatcherPriority.SystemIdle, typeCmd + numRoom, "0", arrParam[0]);
                                        //Dispatcher.BeginInvoke(new CyclicPolling(BuildChart), typeCmd, numRoom, arrParam);
                                        if (handRequest)
                                            BuildNotification("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd + numRoom, "sensor") + ". Значение: " + arrParam[0]);
                                        FixedAction("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd + numRoom, "sensor" + ". Значение: " + arrParam[0]), typeCmd + numRoom, "0");
                                      
                                    }
                                    //если датчик тока не выдает напряжение
                                    //if (elGauge.Uid == typeCmd + numRoom + "Power" && arrParam[0] != "" && arrParam[0] != "@")
                                    //{
                                    //    //определние напряжения
                                    //    double voltageValue = 0;
                                    //    foreach (var elin in LogicalTreeHelper.GetChildren((elGauge.Parent as FrameworkElement)))
                                    //        if (elin is StackPanel)
                                    //            foreach (var elinin in LogicalTreeHelper.GetChildren(elin as FrameworkElement))
                                    //                if (elinin is Xceed.Wpf.Toolkit.DoubleUpDown)
                                    //                    voltageValue = (elinin as Xceed.Wpf.Toolkit.DoubleUpDown).Value.Value;
                                    //    //отображение мощности по напряжению
                                    //    powerSum1 += Convert.ToDouble(arrParam[0].Replace(".", ",")) * voltageValue * Math.Abs((timerPower - DateTime.Now).TotalHours);
                                    //    if (powerSum1 != 0)
                                    //        elGauge.Value = Convert.ToDouble(powerSum1.ToString("#.#"));
                                    //     else
                                    //        elGauge.Value = powerSum1;
                                    //}
                                    ////если датчик тока сразу выдает мощность
                                    if (arrParam.Count() >= 3)
                                        if (elGauge.Uid == typeCmd + numRoom + "Power" && arrParam[2] != "" && arrParam[2] != "@")
                                        {
                                            //powerSum += Convert.ToDouble(arrParam[2].Replace(".", ",")) * Math.Abs((timerPower - DateTime.Now).TotalHours);
                                            //if (powerSum != 0)
                                            //    elGauge.Value = Convert.ToDouble(powerSum.ToString("#.#"));
                                            //else
                                            //    elGauge.Value = powerSum;
                                            powerSum = Convert.ToDouble(arrParam[2].Replace(".", ","));
                                            if (powerSum != 0)
                                                elGauge.Value = Convert.ToDouble(powerSum.ToString("#.#"));
                                            else
                                                elGauge.Value = powerSum;
                                         
                                            //построение графиков на основе полученных показаний
                                            //BuildChart(typeCmd, numRoom + "Power", powerSum.ToString(), DateTime.Now);
                                            Dispatcher.BeginInvoke(new DBDelegateFixedData(FixedStatistic), DispatcherPriority.Background, typeCmd + numRoom + "Power", "0", arrParam[2]);
                                            if (handRequest)
                                                BuildNotification("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd + numRoom, "sensor") + "в режиме мощности. Значение: " + arrParam[2]);
                                            FixedAction("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd + numRoom, "sensor") + "в режиме мощности. Значение: " + arrParam[2], typeCmd + numRoom + "Power", "0");

                                            //Dispatcher.BeginInvoke(new CyclicPolling(BuildChart), typeCmd, numRoom, arrParam);
                                        }

                                }

                            }
                            //дата и время
                            else if (typeCmd == "R")
                            {
                                tbTime.Text = arrParam[0] + ":" + arrParam[1];
                                tbDate.Text = arrParam[2] + "." + arrParam[3] + "." + arrParam[4];
                            }
                            //прочие датчики (двери, движение, вибрация)
                            else
                            {
                                if (el is Image)
                                {
                                    Image elImg = el as Image;
                                    if (elImg.Uid == typeCmd + numRoom)
                                    {
                                        if (arrParam[0] != "" && (arrParam[0] == "@" || Convert.ToInt32(arrParam[0]) == 0))
                                        {
                                            (el as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + typeCmd + "_no.png"));
                                        }
                                        else
                                        {
                                            (el as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + typeCmd + "_yes.png"));
                                        }
                                        if (typeCmd != "V")
                                        {
                                            BuildNotification("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd, "sensor") + " в комнате:" + GetTypeDeviceRoom(numRoom, "room") + ". Значение: " + arrParam[0]);
                                            FixedAction("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd, "sensor") + " в комнате:" + GetTypeDeviceRoom(numRoom, "room") + ". Значение: " + arrParam[0], typeCmd, numRoom);

                                        }
                                        else
                                        {
                                            BuildNotification("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd, "sensor") + ". Значение: " + arrParam[0]);
                                            FixedAction("Получен ответ на запрос состояния датчика " + GetTypeDeviceRoom(typeCmd, "sensor") + ". Значение: " + arrParam[0], typeCmd, "0");
                                        }
                                        Dispatcher.BeginInvoke(new DBDelegateFixedData(FixedStatistic), DispatcherPriority.Background, typeCmd, numRoom, arrParam[0]);
                                    }
                                }
                            }
                            handRequest = false;
                            //поиск дочерних UI элементов 
                            FindControl(el as FrameworkElement, typeCmd, numRoom, arrParam);
                        }
                    }
                }


                //if (typeCmd == "T" || typeCmd == "C" || typeCmd == "H" || typeCmd == "P")
                //{

                //}
            }
        }

        int ggg = 0;
        int fff = 0;
         void ChartAndDBCreate(string typeCmd, string numRoom, string[] arrParam)
        {
            //MessageBox.Show("12");
            //для широковещательного обращения к датчикам
            if (numRoom == "0")
            {
                //датчик температуры, влажности, атм. давления
                if (typeCmd == "T" || typeCmd == "C" || typeCmd == "H" || typeCmd == "P" || typeCmd == "L" || typeCmd == "G" || typeCmd == "W" || typeCmd == "L" || typeCmd == "V" || typeCmd == "D")
                {
                    int count = arrParam.Count();
                    for (int i = 0; i < count; i++)
                    {
                        if (arrParam[i] != "" && arrParam[i] != "@")
                        {
                            //если UI элемент - полукруг

                            //фиксация данных в БД
                            //await Task.Run(() => FixedStatistic(typeCmd, (i + 1).ToString(), val.ToString()));
                            //Dispatcher.CurrentDispatcher.BeginInvoke(new DBDelegateFixedData(FixedStatistic), DispatcherPriority.SystemIdle, typeCmd, (i + 1).ToString(), val.ToString());
                            //построение графиков на основе полученных показаний - в отдельном потоке

                            //FixedStatistic(typeCmd, (i + 1).ToString(), arrParam[i].ToString());
                            //ins(typeCmd, (i + 1).ToString(), arrParam[i].ToString());
                            //Task.Run(() => { FixedStatistic(typeCmd, (i + 1).ToString(), arrParam[i].ToString()); });
                            //BuildChart(typeCmd, Convert.ToString(i + 1), arrParam[i]);
                            //Task.Run(() => { BuildChart(typeCmd, Convert.ToString(i + 1), arrParam[i]); });


                            //await Task.Run(() => BuildChart(typeCmd, Convert.ToString(i + 1), arrParam[i]));
                            //Dispatcher.BeginInvoke(new CyclicPollingBuildChart(BuildChart), DispatcherPriority.Normal, typeCmd, Convert.ToString(i + 1), arrParam[i]);

                            //if (powerWin != null && powerWin.viewNowChart && typeCmd == "T" && i == 0)
                            //{
                            //    ggg++;
                            //    powerWin.Title = ggg.ToString()   + " " + fff ;
                            //}
                            //Task.Factory.StartNew(() => { BuildChart(typeCmd, Convert.ToString(i + 1), arrParam[i], DateTime.Now); });
                            //BuildChart(typeCmd, Convert.ToString(i + 1), arrParam[i], DateTime.Now);


                            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new CyclicPollingBuildChart(BuildChart), typeCmd, Convert.ToString(i + 1), arrParam[i], DateTime.Now);
                            //using (smart_houseContext con = new smart_houseContext())
                            //{
                            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new CyclicPollingBuildDB(FixedStatistic), typeCmd, Convert.ToString(i + 1), arrParam[i]);
                            //}
                          
                        }
                    }
                }
            }
        }
 


        //построение графиков температуры, давления, влажности, тока на освнове полученных показаний
        void BuildChart(string typeCmd, string numRoom, string param, DateTime dt)
        {
            //если открыто окно статистики
            if (powerWin != null && powerWin.viewNowChart)
            {
                //если словарь данных существует и не содержит данного ключа, то добавить ключ и его даные, иначе просто добавить данные
                if (powerWin.ValueSensors != null)
                {
                    string key = typeCmd + numRoom;
                    if ((powerWin.tcTypeSensor.SelectedItem as TabItem).Tag.ToString() == typeCmd)
                    {
                        if (!powerWin.ValueSensors.ContainsKey(key))
                        //Dispatcher.BeginInvoke((Action)(() => { ;})) 
                        {
                            powerWin.ValueSensors.Add(key, new Dictionary<DateTime, double> { [dt] = Convert.ToDouble(param.Replace(".", ",")) } /*.Substring(0, 2)) }*/);
                            //Thread potok2 = new Thread(powerWin.RefreshChart); // создание отдельного потока
                            //potok2.IsBackground = true;
                            //potok2.Start();
                            powerWin.RefreshChart();
                        }
                        else /*if (!powerWin.ValueSensors[key].ContainsKey(DateTime.Now))*/
                        {
                            if (powerWin.ValueSensors[key].Count > powerWin.LimitShowDataNow)
                            {
                                powerWin.ValueSensors[key].Remove(powerWin.ValueSensors[key].FirstOrDefault().Key);
                            }
                            powerWin.ValueSensors[key].Add(dt, Convert.ToDouble(param.Replace(".", ",")));
                            //Thread potok2 = new Thread(powerWin.RefreshChart); // создание отдельного потока
                            //potok2.IsBackground = true;
                            //potok2.Start();
                            powerWin.RefreshChart();

                        }
                        
                    }
                }
                //powerWin.RefreshChart();
            }
          
        }
        #endregion
        #endregion

        #region Работа таймера ожидания ответа на запрос
        //Таймер возврата изображения в исходное состояние в случае отсутвия ответа
        void StartTimerLastCommandX()
        {
            //таймер ожидания получения ответа на отправленную команду управления
            disTimerCommandX.Interval = new TimeSpan(0, 0, 3);
            disTimerCommandX.Tick += DisTimerCommandX_Tick;
            disTimerCommandX.Start();
        }
        private void DisTimerCommandX_Tick(object sender, EventArgs e)
        {
            //для одного устройства 
            if (listLastActivatedDevice.Count == 0 && lastActivatedDevice != null)
            {
                ChangeImgButton((lastActivatedDevice as Button), lastImgIco);
                BuildNotification("Ответ на последнюю запрошенную команду управления устройствами отсутствует, сброс состояний устройств");
                FixedAction("Ответ на последнюю запрошенную команду управления устройствами отсутствует, сброс состояний устройств");
            }
            //при управлении всей группой устройств
            else if (listLastActivatedDevice.Count > 0)
            {
                ChangeImgButton();
                BuildNotification("Ответ на последнюю запрошенную команду управления устройствами отсутствует, сброс состояний устройств");
                FixedAction("Ответ на последнюю запрошенную команду управления устройствами отсутствует, сброс состояний устройств");
            }

            disTimerCommandX.Tick -= DisTimerCommandX_Tick;
            disTimerCommandX.Stop();
            disTimerCommandX = new DispatcherTimer();
            lastCommandSend = "";
            lastActivatedDevice = null;
            lastImgIco = "";
            listLastActivatedDevice = new Dictionary<object, string>();
        }
        //Установка последних параметров
        void SetLastCommandXParam(string command, object objSource = null, string imgIco = "", Dictionary<object, string> listObjImg = null)
        {
            lastCommandSend = command;
            if (objSource != null && imgIco != "")
            {
                lastActivatedDevice = objSource;
                lastImgIco = imgIco;
            }
            if (listObjImg != null && listObjImg.Count > 0)
                listLastActivatedDevice = listObjImg;
        }
        //Получение текущего изображения кнопки устройства управления
        string GetNowImgButtom(Button btn)
        {
            string[] arr = (btn.Content as Image).Source.ToString().Split('/');
            return arr[arr.Count() - 1];
        }

        #endregion

        #region Управление исполнительными устройствами

        #region Включение исполнительных устройств
        private void btnLight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (sender as Button);
                string uid = btn.Uid;
                int value = Convert.ToInt32(GetSetValueDevice(sender));
                controleObj.LightControl(serialPort, sender, uid, value);

                if (!disTimerCommandX.IsEnabled)
                {
                    SetLastCommandXParam("L" + uid, btn, GetNowImgButtom(btn));
                    StartTimerLastCommandX();
                }
                if (value != 0)
                {
                    ChangeImgButton(btn, "L_yes.png");
                    BuildNotification("Включение основного освещения: комната: " + GetTypeDeviceRoom(uid, "room") + ". Bнтенсивность: " + value);
                    FixedAction("Включение основного освещения: комната: " + GetTypeDeviceRoom(uid, "room") + ". Bнтенсивность: " + value, "L", uid, "Device");
                }
                else
                {
                    ChangeImgButton(btn, "L_no.png");
                    BuildNotification("Выключение основного освещения: комната: " + GetTypeDeviceRoom(uid, "room"));
                    FixedAction("Выключение основного освещения: комната: " + GetTypeDeviceRoom(uid, "room"), "L", uid, "Device");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
}
        private void btnLight_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Button btn = (sender as Button);
                string uid = btn.Uid;
                int value = Convert.ToInt32(GetSetValueDevice(sender));
                controleObj.SmoothLightControl(serialPort, sender, uid, value);
                if (!disTimerCommandX.IsEnabled)
                {
                    SetLastCommandXParam("S" + uid, btn, GetNowImgButtom(btn));
                    StartTimerLastCommandX();
                }
                if (value != 0)
                {
                    ChangeImgButton(btn, "L_yes.png");
                    BuildNotification("Плавное включение основного освещения: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: " + value);
                    FixedAction("Плавное включение основного освещения: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: " + value, "L", uid, "Device");
                }
                else
                {
                    ChangeImgButton(btn, "L_no.png");
                    BuildNotification("Плавное выключение основного освещения: комната: " + GetTypeDeviceRoom(uid, "room"));
                    FixedAction("Плавное выключение основного освещения: комната: " + GetTypeDeviceRoom(uid, "room"), "L", uid, "Device");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnBackLight_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
            //   MessageBox.Show(serialPort.IsOpen + "");
            Button btn = (sender as Button);
            string uid = btn.Uid;
            //int value = Convert.ToInt32(GetValueDevice(sender));
            Xceed.Wpf.Toolkit.ColorPicker colorLamp = GetColorRGB(btn);
            int R = GetValueColor(colorLamp.SelectedColor.Value.R);
            int G = GetValueColor(colorLamp.SelectedColor.Value.G);
            int B = GetValueColor(colorLamp.SelectedColor.Value.B);
            controleObj.BackgroundLightControl(serialPort, sender, uid, R, G, B);
            if (!disTimerCommandX.IsEnabled)
            {
                SetLastCommandXParam("B" + uid, btn, GetNowImgButtom(btn));
                StartTimerLastCommandX();
            }
            //if (R == 0 && G == 0 && B == 0)
            //     ChangeImgButton(btn, "light_no.png");
            //else
            ChangeImgButton(btn, "L_rgb.png");
            BuildNotification("Включение фонового освещения: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: R(" + R + "), G(" + G + "), B(" + B + ")");
            FixedAction("Включение фонового освещения: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: R(" + R + "), G(" + G + "), B(" + B + ")", "B", uid, "Device");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        Xceed.Wpf.Toolkit.ColorPicker GetColorRGB(Button btn)
        {
            if (btn.Parent is Grid)
                foreach (var el in (btn.Parent as Grid).Children)
                    if (el is Xceed.Wpf.Toolkit.ColorPicker)
                        return (el as Xceed.Wpf.Toolkit.ColorPicker);
            return null;
        }
        int GetValueColor(int value)
        {
            if (value > 0 && value <= 25)
                return 1;
            if (value > 25 && value <= 50)
                return 2;
            if (value > 50 && value <= 75)
                return 3;
            if (value > 75 && value <= 100)
                return 4;
            if (value > 100 && value <= 125)
                return 5;
            if (value > 125 && value <= 150)
                return 6;
            if (value > 150 && value <= 175)
                return 7;
            if (value > 175 && value <= 200)
                return 8;
            if (value > 200 && value <= 225)
                return 9;
            if (value > 225 && value <= 255)
                return 10;
            return 0;
        }
        private void btnDoor_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
            Button btn = (sender as Button);
            string uid = btn.Uid;
            int value = Convert.ToInt32(GetSetValueDevice(sender));
            controleObj.DoorControl(serialPort, sender, uid, value);
            if (!disTimerCommandX.IsEnabled)
            {
                SetLastCommandXParam("D" + uid, btn, GetNowImgButtom(btn));
                StartTimerLastCommandX();
            }
            if (value != 0)
            {
                ChangeImgButton(btn, "D_yes.png");
                BuildNotification("Открытие двери: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: " + value);
                FixedAction("Открытие двери: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: " + value, "D", uid, "Device");
            }
            else
            {
                ChangeImgButton(btn, "D_no.png");
                BuildNotification("Закрытие двери: комната: " + GetTypeDeviceRoom(uid, "room"));
                FixedAction("Закрытие двери: комната: " + GetTypeDeviceRoom(uid, "room"), "D", uid, "Device");
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnProp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (sender as Button);
                string uid = btn.Uid;
                int value = Convert.ToInt32(GetSetValueDevice(sender));
                controleObj.HeatingCooling(serialPort, sender, uid, value);

                if (value < 0)
                {
                    if (!disTimerCommandX.IsEnabled)
                    {
                        SetLastCommandXParam("C" + uid, btn, GetNowImgButtom(btn));
                        StartTimerLastCommandX();
                    }
                    ChangeImgButton(btn, "F_c.png");
                    BuildNotification("Включение электроотопления - охлаждение: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: " + value);
                    FixedAction("Включение электроотопления - охлаждение: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: " + value, "C", uid, "Device");
                }
                else if (value > 0)
                {
                    if (!disTimerCommandX.IsEnabled)
                    {
                        SetLastCommandXParam("H" + uid, btn, GetNowImgButtom(btn));
                        StartTimerLastCommandX();
                    }
                    ChangeImgButton(btn, "F_h.png");
                    BuildNotification("Включение электроотопления - нагрев: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: " + value);
                    FixedAction("Включение электроотопления - нагрев: комната: " + GetTypeDeviceRoom(uid, "room") + ", интенсивность: " + value, "H", uid, "Device");
                }
                else
                {
                    if (!disTimerCommandX.IsEnabled)
                    {
                        SetLastCommandXParam("H" + uid, btn, GetNowImgButtom(btn));
                        StartTimerLastCommandX();
                    }
                    ChangeImgButton(btn, "F_no.png");
                    BuildNotification("Выключение электроотопления: комната: " + GetTypeDeviceRoom(uid, "room"));
                    FixedAction("Выключение электроотопления: комната: " + GetTypeDeviceRoom(uid, "room"), "H", uid, "Device");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnSignaling_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
            Button btn = (sender as Button);
            string uid = btn.Uid;
            controleObj.SignalingControl(serialPort, sender, uid, 1);
            if (!disTimerCommandX.IsEnabled)
            {
                SetLastCommandXParam("A" + uid, btn, GetNowImgButtom(btn));
                StartTimerLastCommandX();
            }
            ChangeImgButton(btn, "A_yes.png");
            BuildNotification("Включение сигнализации");
            FixedAction("Включение сигнализации", "A", "0", "Device");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //установка времени Arduino
        private void btnSetDateTimeArduino_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                controleObj.SetDateTimeArudino(serialPort, dtpTimeArduino.Value.Value);
                BuildNotification("Установка времени контроллера: " + dtpTimeArduino.Value.Value);
                if (serialPort.IsOpen)
                {
                    controleObj.SensorPolling(serialPort, "R", 0);
                    Thread.Sleep(timeSleepCyclicPolling);
                    DisTimerSyncClock_Tick(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnSetDateTimeNowArduino_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                controleObj.SetDateTimeArudino(serialPort, DateTime.Now);
                BuildNotification("Установка времени контроллера: " + DateTime.Now);
                if (serialPort.IsOpen)
                {
                    controleObj.SensorPolling(serialPort, "R", 0);
                    Thread.Sleep(timeSleepCyclicPolling );
                    DisTimerSyncClock_Tick(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Выключение исполнительных устройств
        private void btnBackLight_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { 
            Button btn = (sender as Button);
            string uid = btn.Uid;
            controleObj.BackgroundLightControl(serialPort, sender, uid, 0, 0, 0);
            if (!disTimerCommandX.IsEnabled)
            {
                SetLastCommandXParam("B" + uid, btn, GetNowImgButtom(btn));
                StartTimerLastCommandX();
            }
            ChangeImgButton(btn, "L_no.png");
            BuildNotification("Выключение фонового освещения: комната: " + GetTypeDeviceRoom(uid, "room"));
            FixedAction("Выключение фонового освещения: комната: " + GetTypeDeviceRoom(uid, "room"), "B", uid, "Device");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnDoor_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { 
            Button btn = (sender as Button);
            string uid = btn.Uid;
            controleObj.DoorControl(serialPort, sender, uid, 0);
            if (!disTimerCommandX.IsEnabled)
            {
                SetLastCommandXParam("D" + uid, btn, GetNowImgButtom(btn));
                StartTimerLastCommandX();
            }
            ChangeImgButton(btn, "D_no.png");
            BuildNotification("Закрытие двери: комната: " + GetTypeDeviceRoom(uid, "room"));
            FixedAction("Закрытие двери: комната: " + GetTypeDeviceRoom(uid, "room"), "D", uid, "Device");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnProp_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { 
            Button btn = (sender as Button);
            string uid = btn.Uid;
            controleObj.HeatingCooling(serialPort, sender, uid, 0);
            if (!disTimerCommandX.IsEnabled)
            {
                SetLastCommandXParam("H" + uid, btn, GetNowImgButtom(btn));
                StartTimerLastCommandX();
            }
            ChangeImgButton(btn, "F_no.png");
            BuildNotification("Выключение электроотопления: комната: " + GetTypeDeviceRoom(uid, "room"));
            FixedAction("Выключение электроотопления: комната: " + GetTypeDeviceRoom(uid, "room"), "H", uid, "Device");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnSignaling_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { 
            Button btn = (sender as Button);
            string uid = btn.Uid;
            controleObj.SignalingControl(serialPort, sender, uid, 0);
            if (!disTimerCommandX.IsEnabled)
            {
                SetLastCommandXParam("A" + uid, btn, GetNowImgButtom(btn));
                StartTimerLastCommandX();
            }
            ChangeImgButton(btn, "A_no.png");
            BuildNotification("Выключение сигнализации");
            FixedAction("Выключение сигнализации", "A", "0", "Device");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Управление сразу всеми устройствами одного типа
        //обработка события включени/выключения всех устройств одного типа
        private void btnAllSensorControl_Click(object sender, RoutedEventArgs e)
        {
            try
            {

         
            if (sender is Button)
            {
                string uidBtn = (sender as Button).Uid.ToString();

                //отключение устройств
                if (uidBtn == "AL0")
                {
                    controleObj.LightControl(serialPort, sender, "0", 0);
                    ChangePictureAllControl(new List<Button>() { btnLight1, btnLight2, btnLight3, btnLight4, btnLight5 }, "L");
                    BuildNotification("Выключение всего освещения");
                    FixedAction("Выключение всего освещения", "L", "", "Device");
                }
                else if (uidBtn == "AS0")
                {
                    controleObj.SmoothLightControl(serialPort, sender, "0", 0);
                    ChangePictureAllControl(new List<Button>() { btnLight1, btnLight2, btnLight3, btnLight4, btnLight5 }, "S");
                    BuildNotification("Плавное выключение всего освещения");
                    FixedAction("Плавное выключение всего освещения", "L", "", "Device");
                }
                else if (uidBtn == "AB0")
                {
                    controleObj.BackgroundLightControl(serialPort, sender, "0", 0, 0, 0);
                    ChangePictureAllControl(new List<Button>() { btnBackLight1, btnBackLight2, btnBackLight3, btnBackLight4, btnBackLight5, btnBackLight6 }, "B");
                    BuildNotification("Выключение всего фонового освещения");
                    FixedAction("Выключение всего фонового освещения", "B", "", "Device");
                }
                else if (uidBtn == "AHC0")
                {
                    controleObj.HeatingCooling(serialPort, sender, "0", 0);
                    ChangePictureAllControl(new List<Button>() { btnProp1, btnProp4 }, "H");
                    BuildNotification("Выключение электроотопления");
                    FixedAction("Выключение электроотопления", "H", "", "Device");
                }
                else if (uidBtn == "AA0")
                {
                    if (disTimerSignalization != null)
                    {
                        btnSignOn.Background = new SolidColorBrush(Color.FromRgb(211, 243, 253));
                        disTimerSignalization.Tick -= DisTimerSignalization_Tick;
                        disTimerSignalization.Stop();
                        disTimerSignalization = null;
                        BuildNotification("Выключение сигнализации");
                        FixedAction("Выключение сигнализации", "A", "0", "Device");
                    }

                }

                //включение устройств
                else if (uidBtn == "AL1")
                {
                    int value = (int)sldAllL.Value;
                    controleObj.LightControl(serialPort, sender, "0", value);
                    ChangePictureAllControl(new List<Button>() { btnLight1, btnLight2, btnLight3, btnLight4, btnLight5 }, "L", true, value);
                    BuildNotification("Включение всего освещения: Интенсивность:" + value);
                    FixedAction("Включение всего освещения: Интенсивность:" + value, "L", "", "Device");
                }
                else if (uidBtn == "AS1")
                {
                    int value = (int)sldAllS.Value;
                    controleObj.SmoothLightControl(serialPort, sender, "0", value);
                    ChangePictureAllControl(new List<Button>() { btnLight1, btnLight2, btnLight3, btnLight4, btnLight5 }, "L", true, value);
                    BuildNotification("Плавное включение всего освещения: " + value);
                    FixedAction("Плавное включение всего освещения: Интенсивность:" + value, "L", "", "Device");
                }
                else if (uidBtn == "AB1")
                {
                    Xceed.Wpf.Toolkit.ColorPicker colorLamp = cpAllSensor;
                    int R = GetValueColor(colorLamp.SelectedColor.Value.R);
                    int G = GetValueColor(colorLamp.SelectedColor.Value.G);
                    int B = GetValueColor(colorLamp.SelectedColor.Value.B);
                    controleObj.BackgroundLightControl(serialPort, sender, "0", R, G, B);
                    ChangePictureAllControl(new List<Button>() { btnBackLight1, btnBackLight2, btnBackLight3, btnBackLight4, btnBackLight5, btnBackLight6 }, "B", true, colorLamp.SelectedColor);
                    BuildNotification("Включение всего фонового освещения: Интенсивность:" + " R(" + R + "), G(" + G + "), B(" + B + ")");
                    FixedAction("Включение всего фонового освещения: " + " R(" + R + "), G(" + G + "), B(" + B + ")", "B", "", "Device");
                }
                else if (uidBtn == "AHC1")
                {
                    int value = (int)sldAllC.Value;
                    controleObj.HeatingCooling(serialPort, sender, "0", value);
                    if (value > 0)
                    {
                        ChangePictureAllControl(new List<Button>() { btnProp1, btnProp4 }, "H" /*"Hhot"*/, true, value);
                        BuildNotification("Включение всего электроотопления - нагрев: Интенсивность" + value);
                        FixedAction("Включение всего электроотопления - нагрев: Интенсивность" + value, "H", "", "Device");
                    }
                    else if (value < 0)
                    {
                        ChangePictureAllControl(new List<Button>() { btnProp1, btnProp4 }, "C" /*"Hcold"*/, true, value);
                        BuildNotification("Включение всего электроотопления - охлаждение: Интенсивность" + value);
                        FixedAction("Включение всего электроотопления - охлаждение: Интенсивность" + value, "C", "", "Device");
                    }
                    else if (value == 0)
                    {
                        ChangePictureAllControl(new List<Button>() { btnProp1, btnProp4 }, "H");
                        BuildNotification("Выключение всего электроотопления");
                        FixedAction("Выключение всего электроотопления", "H", "", "Device");
                    }
                }
                else if (uidBtn == "AA1")
                {
                    string fr = sldSigFr.Value.ToString();
                    int len = (int)sldSigLen.Value;

                    //thrCyclicPolling = new Thread(new ThreadStart(CyclicSensorPolling));
                    //thrCyclicPolling.IsBackground = true;
                    //thrCyclicPolling.Start();
                    if (disTimerSignalization != null)
                        disTimerSignalization.Tick -= DisTimerSignalization_Tick;
                    disTimerSignalization = new DispatcherTimer();
                    disTimerSignalization.Interval = new TimeSpan(0,0,0,0, IUpDownSign.Value.Value);
                    disTimerSignalization.Tick += DisTimerSignalization_Tick;
                    disTimerSignalization.Start();
                    btnSignOn.Background = new SolidColorBrush(Color.FromRgb(110, 200, 130));


                    BuildNotification("Включение сигнализации: частота: " + fr + ", длительность: " + len);
                    FixedAction("Включение сигнализации: частота: " + fr + ", длительность: " + len, "A", "0", "Device");
                }
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void IUpDownSign_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            btnAllSensorControl_Click(btnSignOn, null);
        }
        // запуск сигнализации
        private void DisTimerSignalization_Tick(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                string fr = sldSigFr.Value.ToString();
                int len = (int)sldSigLen.Value;
                controleObj.SignalingControl(serialPort, null, fr, len);
            }
        }

        //изменение изображений при управлении всеми типами устройств
        void ChangePictureAllControl(List<Button> listBtn, string typeSensor = "", bool offOn = false, object value = null)
        {
            foreach (Button el in listBtn)
                if (el.Content is Image)
                {
                    //заполнение коллекции кнопок-устройств и их изображений
                    if (!listLastActivatedDevice.Keys.Contains(el))
                        listLastActivatedDevice.Add(el, GetNowImgButtom(el));
                    SetLastCommandXParam(typeSensor + "0");
                    StartTimerLastCommandX();

                    Image img = (el.Content as Image);
                    string pic = "pack://application:,,,/" + img.Tag.ToString();
                    //выключение всех устройств
                    if (!offOn)
                    {
                        img.Source = new BitmapImage(new Uri(pic));
                        if (typeSensor != "B")
                            GetSetValueDevice(el, false, 0);
                        else
                            SetColorRGB(el, (Color)ColorConverter.ConvertFromString("#FF000000"));

                    }
                    //включение всех устройств
                    else
                    {
                        if (typeSensor == "L")
                            img.Source = new BitmapImage(new Uri(pic.Replace("no", "yes")));
                        else if (typeSensor == "H") /*"Hhot"*/
                            img.Source = new BitmapImage(new Uri(pic.Replace("no", "h")));
                        else if (typeSensor == "C") /*"Hcold"*/
                            img.Source = new BitmapImage(new Uri(pic.Replace("no", "c")));
                        else if (typeSensor == "B")
                        {
                            img.Source = new BitmapImage(new Uri(pic.Replace("no", "rgb")));
                            SetColorRGB(el, (Color)value);
                            continue;
                        }
                        GetSetValueDevice(el, false, (int)value);
                    }
                }
        }
        //установка цвета для RGB светодиода
        void SetColorRGB(object sender, Color clr)
        {
            foreach (var el in ((sender as FrameworkElement).Parent as Grid).Children)
            {
                if (el is Xceed.Wpf.Toolkit.ColorPicker)
                {
                    (el as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor = clr;
                }

            }
        }
        //обработка изменения значения слайдера
        private void sldAllL_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider)
            {
                Slider sld = (sender as Slider);
                sld.Value = (int)e.NewValue;
                foreach (var el in ((sld as FrameworkElement).Parent as StackPanel).Children)
                {
                    if (el is TextBlock)
                        (el as TextBlock).Text = sld.Value.ToString();
                }
            }
        }

        #endregion

        #region Установка необходимых изображений; Получение значений со слайдеров, передаваемых на исполнительные устройства 
        void ChangeImgButton(Button btn = null, string nameIco = "")
        {
            if (btn != null && nameIco != "")
            {
                //Image img = new Image();
                if (btn != null && btn.Content is Image)
                    (btn.Content as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + nameIco));
                else if (btn != null && btn.Content is StackPanel)
                {
                    foreach (var el in (btn.Content as StackPanel).Children)
                    {
                        if (el is Image)
                        {
                            (el as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + nameIco));
                        }
                    }
                }
            }
            else if (listLastActivatedDevice.Count > 0)
            {
                foreach (var el in listLastActivatedDevice)
                {
                    Button btnEl = el.Key as Button;
                    string pathImg = el.Value as string;

                    if (btnEl != null && btnEl.Content is Image)
                        (btnEl.Content as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + pathImg));
                    else if (btnEl != null && btnEl.Content is StackPanel)
                    {
                        foreach (var elin in (btnEl.Content as StackPanel).Children)
                        {
                            if (elin is Image)
                            {
                                (elin as Image).Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/" + pathImg));
                            }
                        }
                    }
                }
            }
        }
        double GetSetValueDevice(object sender, bool getSet = true, int value = 0)
        {
            double valueSlider = 0;
            foreach (var el in ((sender as FrameworkElement).Parent as StackPanel).Children)
            {
                if (el is StackPanel)
                {
                    foreach (var elin in (el as StackPanel).Children)
                    {
                        if (elin is Slider)
                        {
                            if (getSet)
                                valueSlider = (elin as Slider).Value;
                            else
                                (elin as Slider).Value = value;
                        }

                    }
                }

            }

            //double valueSlider = 0;
            //if (sender is Button)
            //{
            //    Button btn = (sender as Button);
            //    StackPanel sp = ((btn.ToolTip as ToolTip).Content as StackPanel);

            //    foreach (var el in sp.Children)
            //    {
            //        if (el is Slider)
            //            valueSlider = (el as Slider).Value;
            //    }
            //}

            return valueSlider;
        }
        #endregion

        #endregion

        #region Управление диапазонами некоторых датчиков
        //Установка нового диапазона для выбранной катерогии датчиков 
        private void btnSetRangeSensor_Click(object sender, RoutedEventArgs e)
        {
            string uid = (cbRageSensor.SelectedValue as ComboBoxItem).Uid;
            int startRange = IUpDownStartRange.Value.Value;
            int endRange = IUpDownEndRange.Value.Value;
            FindSensorSetRage(grdHousePlan, uid, startRange, endRange);
            BuildNotification("Изменение диапазона датчика: " + (cbRageSensor.SelectedValue as ComboBoxItem).Content + ": [" + startRange + "," + endRange + "]");
            FixedAction("Изменение диапазона датчика: " + (cbRageSensor.SelectedValue as ComboBoxItem).Content + ": [" + startRange + "," + endRange + "]");
        }
        //поиск датчика для установки новых диапазонов
        void FindSensorSetRage(FrameworkElement obj, string uid, int startRange, int endRange)
        {
            if (obj != null)
            {
                var listChildrenControl = LogicalTreeHelper.GetChildren(obj);
                foreach (var el in listChildrenControl)
                {
                    if (el is LiveCharts.Wpf.Gauge && (el as LiveCharts.Wpf.Gauge).Uid.Contains(uid))
                    {
                        (el as LiveCharts.Wpf.Gauge).From = startRange;
                        (el as LiveCharts.Wpf.Gauge).To = endRange;
                    }
                    FindSensorSetRage(el as FrameworkElement, uid, startRange, endRange);
                }
            }

        }

        //определение выбранного датчика и отображение его максимальных и минимальных диапазонов для изменения
        private void cbRageSensor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRageSensor.SelectedValue != null && IUpDownStartRange != null)
            {
                string cbiUid = (cbRageSensor.SelectedValue as ComboBoxItem).Uid;
                double startVal = 0;
                double endVal = 0;
                if (cbiUid == "T")
                {
                    IUpDownStartRange.Minimum = -25;
                    IUpDownStartRange.Maximum = 50;
                    startVal = lgTemperature1.From;
                    endVal = lgTemperature1.To;
                }
                else if (cbiUid == "H")
                {
                    IUpDownStartRange.Minimum = 0;
                    IUpDownStartRange.Maximum = 100;
                    startVal = lgHumidity4.From;
                    endVal = lgHumidity4.To;
                }
                else if (cbiUid == "P")
                {
                    IUpDownStartRange.Minimum = 900;
                    IUpDownStartRange.Maximum = 1100;
                    startVal = lgAtmPressure4.From;
                    endVal = lgAtmPressure4.To;
                }
                else if (cbiUid == "C")
                {
                    IUpDownStartRange.Minimum = -3000;
                    IUpDownStartRange.Maximum = 20000;
                    startVal = lgCurrent61.From;
                    endVal = lgCurrent61.To;
                }
                else if (cbiUid == "Power")
                {
                    IUpDownStartRange.Minimum = -3000;
                    IUpDownStartRange.Maximum = 20000;
                    startVal = lgPower61.From;
                    endVal = lgPower61.To;
                }
                IUpDownStartRange.ToolTip = "Min: " + IUpDownStartRange.Minimum + " Max: " + IUpDownStartRange.Maximum;
                IUpDownEndRange.Minimum = IUpDownStartRange.Minimum;
                IUpDownEndRange.Maximum = IUpDownStartRange.Maximum;

                if (startVal >= IUpDownStartRange.Minimum)
                    IUpDownStartRange.Value = (int)startVal;
                else
                    IUpDownStartRange.Value = IUpDownStartRange.Minimum;
                if (startVal <= IUpDownEndRange.MaxHeight)
                    IUpDownEndRange.Value = (int)endVal;
                else
                    IUpDownEndRange.Value = IUpDownEndRange.Maximum;
            }

        }

        #endregion

        #region Управление уведомлениями, получение полног наименования датчика или устройства по сокращению
        //добавление информации в поле уведомлейний
        void BuildNotification(string message)
        {
            if (rtbNotification != null)
            {
                //newDt
                rtbNotification.AppendText(DateTime.Now + " - " + message + "\r");
                rtbNotification.ScrollToEnd();
            }
            //FixedAction("");
        }
        //очистка поля уведомлений
        void CleaNotification()
        {
            rtbNotification.Document.Blocks.Clear();
        }
        //обработка события двойного нажатия на область уведомлений - очистка уведомлений
        private void rtbNotification_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CleaNotification();
        }
        //получение полного названия устройства или датчика по его сокращению
        string GetTypeDeviceRoom(string typeShortName, string type = "device")
        {
            if (type == "device")
                if (typeShortName.Contains("L"))
                    return "основного освещения";
                else if (typeShortName.Contains("S"))
                    return "плавного освещения";
                else if (typeShortName.Contains("B"))
                    return "фонового освещения";
                else if (typeShortName.Contains("D"))
                    return "";
                else if (typeShortName.Contains("A"))
                    return "сигнализации";
                else if (typeShortName.Contains("C"))
                    return "электроотопления - охлаждение";
                else if (typeShortName.Contains("H"))
                    return "электроотопления - нагрев";
            if (type == "sensor")
                if (typeShortName.Contains("L"))
                    return "освещенности";
                else if (typeShortName.Contains("M"))
                    return "движения";
                else if (typeShortName.Contains("T"))
                    return "температуры";
                else if (typeShortName.Contains("V"))
                    return "вибрации";
                else if (typeShortName.Contains("H"))
                    return "влажности воздуха";
                else if (typeShortName.Contains("P"))
                    return "атм. давления";
                else if (typeShortName.Contains("W"))
                    return "протечки воды";
                else if (typeShortName.Contains("G"))
                    return "газа";
                else if (typeShortName.Contains("D"))
                    return "открытия двери";
                else if (typeShortName.Contains("C"))
                {
                    string res = "тока";
                    if (typeShortName.Contains("1"))
                        res += " INA219 3.2A";
                    else if (typeShortName.Contains("2"))
                        res += " MAX471 3A";
                    else if (typeShortName.Contains("3"))
                        res += " ACS712 5A";
                    else if (typeShortName.Contains("4"))
                        res += " ACS712 20A";
                    return res;
                }
                else if (typeShortName.Contains("R"))
                    return "даты и времени";
            if (type == "room")
                if (typeShortName == "1")
                    return "гостиная (1)";
                else if (typeShortName == "2")
                    return "кухня (2)";
                else if (typeShortName == "3")
                    return "ванная (3)";
                else if (typeShortName == "4")
                    return "спальня (4)";
                else if (typeShortName == "5")
                    return "мансарда - право (5)";
                else if (typeShortName == "6")
                    return "мансарда - лево (6)";
                else if (typeShortName == "6")
                    return "внешний корпус";
            return "";
        }
        #endregion

        #region Управление консолью
        //отображение или скрытие получаемых данных с COM порта в окне консоли
        private void btnGetData_Click(object sender, RoutedEventArgs e)
        {
            if (isGettable)
            {
                isGettable = false;
                btnGetData.Content = "Отображать";
            }
            else if (!isGettable)
            {
                isGettable = true;
                mcFlowDoc.Blocks.Clear();
                paragraph.Inlines.Clear();
                btnGetData.Content = "Прекратить отображение";
            }
        }
        //Обработка нажатия кнопки "Отправить" - отправка данных на COM-порт из консоли
        private void btnSendData_Click(object sender, RoutedEventArgs e)
        {
            string commandStr = "";
            if (serialPort.IsOpen)
            {
                if (rtbWriteData.Selection.Text != "")
                {
                    // MessageBox.Show(rtbWriteData.Selection.Text);
                    //serialPort.DiscardOutBuffer();
                    //serialPort.DiscardInBuffer();
                    commandStr = rtbWriteData.Selection.Text/*.Replace(Environment.NewLine, "")*/;
                    serialPort.Write(commandStr);
                }
                else
                {
                    TextRange txtRange = new TextRange(rtbWriteData.Document.ContentStart, rtbWriteData.Document.ContentEnd);
                    serialPort.DiscardNull = true;
                    //serialPort.DiscardOutBuffer();
                    //serialPort.DiscardInBuffer();
                    commandStr = txtRange.Text/*.Replace(Environment.NewLine, "")*/;
                    serialPort.Write(commandStr);
                }
            }
            else MessageBox.Show("Соединение с COM-портом не установлено!");

            BuildNotification("Отправка команды на COM-портом: " + commandStr);
            FixedAction("Отправка команды на COM-портом: Команда:" + commandStr);
        }

        #region Обработки нажания кнопок "Очистить"
        private void btnClearRtbReadData_Click(object sender, RoutedEventArgs e)
        {
            mcFlowDoc.Blocks.Clear();
            paragraph.Inlines.Clear();
            ClearRtbReadWrite();
        }
        private void btnClearRtbWriteData_Click(object sender, RoutedEventArgs e)
        {
            ClearRtbReadWrite("write");
        }
        // "read" -очистить поле для чтения сообщений, "write" - очисить поле для отправки сообщений
        void ClearRtbReadWrite(string typeRtb = "read")
        {
            if (typeRtb == "read")
                rtbReadData.Document.Blocks.Clear();
            else if (typeRtb == "write")
                rtbWriteData.Document.Blocks.Clear();
        }
        #endregion
        #endregion

        #region Отображение окна-статистики энергопотребления
        //открытие окна для отображения статистики электропотребления по нажатию на кнопку
        private void btnPowerStatistic_Click(object sender, RoutedEventArgs e)
        {
            if (findOpenOwnedWindow())
            {


                powerWin = new PowerStatistic();
                powerWin.Owner = this;
                if (userCon.UserGroup.Name == "Гость")
                {
                    powerWin.gbParamPerion.Visibility = Visibility.Collapsed;
                    powerWin.gbChangeUI.Visibility = Visibility.Collapsed;
                    powerWin.grpChartPeriod.Visibility = Visibility.Hidden;
                    Grid.SetColumnSpan(powerWin.grpChartNow, 2);
                }


                //thrCyclicBuildChart = new Thread(new ThreadStart(powerWin.Show));
                //thrCyclicBuildChart.Priority = ThreadPriority.AboveNormal;
                //thrCyclicBuildChart.IsBackground = true;
                //thrCyclicBuildChart.Start();
                powerWin.Show();
                BuildNotification("Открытие окна для просмотра статистики датчиков в виде графиков");
                FixedAction("Открытие окна для просмотра статистики датчиков в виде графиков");
            }
            else
                powerWin.Focus();
        }
        //проверка окна программы на открытость
        bool findOpenOwnedWindow()
        {
            foreach (var el in this.OwnedWindows)
            {
                if (el.GetType() == powerWin.GetType())
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region изменение вида UI главного экнара (проекции умного здания)
        private void btnReBuildUI_Click(object sender, RoutedEventArgs e)
        {
        
            ChangeViewUI(grdRoom1, gbSensor1);
            ChangeViewUI(grdRoom2, gbSensor2);
            ChangeViewUI(grdRoom3, gbSensor3);
            ChangeViewUI(grdRoom4, gbSensor4);
            ChangeViewUI(grdRoom5, gbSensor5);
            if (grdRoom1.RowDefinitions[1].Height != new GridLength(1, GridUnitType.Star))
            {
                BuildNotification("Установка горизонтального вида интерфейса");
                FixedAction("Установка горизонтального вида интерфейса");
            }
            else
            {
                BuildNotification("Установка вертикального вида интерфейса");
                FixedAction("Установка вертикального вида интерфейса");
            }
            
        }
        //метод изменения вида
        public void ChangeViewUI(Grid grd, GroupBox gb)
        {
            if (grd.RowDefinitions[1].Height != new GridLength(1, GridUnitType.Star))
            {
                grd.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                grd.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Star);
                Grid.SetColumn(gb, 0);
                Grid.SetRow(gb, 1);
            }
            else
            {
                grd.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Star);
                grd.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                Grid.SetColumn(gb, 1);
                Grid.SetRow(gb, 0);
            }
        }
        //отображение датчиков, устройств или всего сразу
        private void RbElementView_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton pressed = (RadioButton)sender;
          
            if (pressed.Content.ToString() == "Все" && gbSensor1 != null)
            {
                btnReBuildUI.IsEnabled = true;
                gbSensor1.Visibility = Visibility.Visible;
                gbSensor2.Visibility = Visibility.Visible;
                gbSensor3.Visibility = Visibility.Visible;
                gbSensor4.Visibility = Visibility.Visible;
                gbSensor5.Visibility = Visibility.Visible;

                gbDevice1.Visibility = Visibility.Visible;
                gbDevice2.Visibility = Visibility.Visible;
                gbDevice3.Visibility = Visibility.Visible;
                gbDevice4.Visibility = Visibility.Visible;
                gbDevice5.Visibility = Visibility.Visible;
                BuildNotification("Изменение вида интерфейса: отображение всех элементов");
                FixedAction("Изменение вида интерфейса: отображение всех элементов");
            }
            else if (pressed.Content.ToString() == "Устройства")
            {
                if (grdRoom1.RowDefinitions[1].Height == new GridLength(0, GridUnitType.Star))
                {
                    btnReBuildUI_Click(null, null);
                }
                btnReBuildUI.IsEnabled = false;
                gbSensor1.Visibility = Visibility.Collapsed;
                gbSensor2.Visibility = Visibility.Collapsed;
                gbSensor3.Visibility = Visibility.Collapsed;
                gbSensor4.Visibility = Visibility.Collapsed;
                gbSensor5.Visibility = Visibility.Collapsed;

                gbDevice1.Visibility = Visibility.Visible;
                gbDevice2.Visibility = Visibility.Visible;
                gbDevice3.Visibility = Visibility.Visible;
                gbDevice4.Visibility = Visibility.Visible;
                gbDevice5.Visibility = Visibility.Visible;
                BuildNotification("Изменение вида интерфейса: отображение только устройств");
                FixedAction("Изменение вида интерфейса: отображение только устройств");
            }
            else if (pressed.Content.ToString() == "Датчики")
            {
                if (grdRoom1.RowDefinitions[1].Height == new GridLength(0, GridUnitType.Star))
                {
                    btnReBuildUI_Click(null, null);
                }
                btnReBuildUI.IsEnabled = false;
                gbDevice1.Visibility = Visibility.Collapsed;
                gbDevice2.Visibility = Visibility.Collapsed;
                gbDevice3.Visibility = Visibility.Collapsed;
                gbDevice4.Visibility = Visibility.Collapsed;
                gbDevice5.Visibility = Visibility.Collapsed;

                gbSensor1.Visibility = Visibility.Visible;
                gbSensor2.Visibility = Visibility.Visible;
                gbSensor3.Visibility = Visibility.Visible;
                gbSensor4.Visibility = Visibility.Visible;
                gbSensor5.Visibility = Visibility.Visible;

                grdRoom1.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                grdRoom2.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                grdRoom3.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                grdRoom4.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                grdRoom5.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                BuildNotification("Изменение вида интерфейса: отображение только датчиков");
                FixedAction("Изменение вида интерфейса: отображение только датчиков");
            }

        }
        #endregion

        #region РАБОТА с БД
        //#region Файл конфигурации подключения к БД
        ////инициализация файла конфигурации подключения к базе данных
        //void CreateConfigFileDB()
        //{
        //    if (!System.IO.File.Exists("ConfigDB.cfg"))
        //    {
        //        System.IO.File.WriteAllText("ConfigDB.cfg", "Server=127.0.0.1; Port=5432; User Id=postgres; Password=22; Database=smarthome;");
        //    }
        //}
        //void GetConnectionStringDB()
        //{
        //    connectionString = System.IO.File.ReadAllLines("ConfigDB.cfg")[0];
        //}
        //#endregion

        //построение графического интерфейса в зависимости от группы пользователя
        public void BuildUI()
        {
            if (userCon.UserGroup.Name != "Гость")
            {
                this.Title = "Система управления - \"Умное здание\" // Пользователь: " + userCon.FullName + " // Группа: " + userCon.UserGroup.Name + "";
            }
            //else
            //    this.Title += " // Пользователь: Гость";
            //userCon.UserGroup.Name = "Гость";
            if (userCon.UserGroup.Name != "Администратор")
            {
                if (userCon.UserGroup.Name == "Гость")
                {
                    DBTab.Visibility = Visibility.Collapsed;
                    spWriteDBBool.Visibility = Visibility.Collapsed;
                }
                else
                    foreach (TabItem el in tcTabDB.Items)
                    {
                        if (el.Tag.ToString() != "ST" && el.Tag.ToString() != "AA")
                            el.Visibility = Visibility.Collapsed;
                    }
            }
        }
        //обработка события смены пользователя - открытие стартового окна
        private void btnOpenLoginWindow_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow lw = new LoginWindow();
            lw.Show();
            this.Close();
        }

        //Скрытие столбцов, передаваемых в качестве параметров из таблицы отобращающей БД
        void SetVisibleColumn(string[] nameColumn)
        {
            foreach (var el in nameColumn)
            {
                var col = dgDB.Columns.FirstOrDefault(w => w.Header.ToString() == el);
                if (col == null)
                    continue;
                else
                    col.Visibility = Visibility.Collapsed;
            }
        }
        //отображение содержимого таблиц для каждой вкладки при работе с БД
        void ShowTable()
        {
            if (tcTabDB != null)
            {
                double tmpAllPage = 0;
                dgDB.ItemsSource = null;
                string tabTag = "";
                if (tcTabDB.SelectedValue != null)
                {
                    //определение текущей вкладки
                    tabTag = (tcTabDB.SelectedValue as TabItem).Tag.ToString();
                    //Установка необъодимых элементов управления для работы с БД
                    GetSetParamTab(tabTag);
                }


                if (tabTag == "ST" || tabTag == "AA")
                {
                    //Отображение данных для Статистики и Аудита действий
                    QueryExecutionSTAA(tabTag);
                    using (smart_houseContext con = new smart_houseContext())
                    {
                        //если запрос на выборку то не надо обновлять кобобоксы
                        if (!isEnterQuery)
                        {
                            //Наполнене комбобоксов
                            if (tabTag == "AA")
                            {
                                con.UserSystem.Load();
                                cbxAAUser.ItemsSource = con.UserSystem.Where(l => !l.DelFlag).ToList();
                                if (cbxAAUser.Items.Count > 0)
                                {
                                    cbxAAUser.SelectedValuePath = "Id";
                                    cbxAAUser.DisplayMemberPath = "FullName";
                                    cbxAAUser.SelectedIndex = 0;
                                }
                            }
                            con.Device.Load();
                            cbxSTAADevice.ItemsSource = con.Device.Where(l => !l.DelFlag).ToList();
                            if (cbxSTAADevice.Items.Count > 0)
                            {
                                cbxSTAADevice.SelectedValuePath = "Id";
                                cbxSTAADevice.DisplayMemberPath = "Name";
                                cbxSTAADevice.SelectedIndex = 0;
                            }
                            con.Room.Load();
                            cbxSTAARoom.ItemsSource = con.Room.Where(l => !l.DelFlag).ToList();
                            if (cbxSTAARoom.Items.Count > 0)
                            {
                                cbxSTAARoom.SelectedValuePath = "Id";
                                cbxSTAARoom.DisplayMemberPath = "Name";
                                cbxSTAARoom.SelectedIndex = 0;
                            }

                        }


                        //con.Action.Load();
                        //dgDB.ItemsSource = con.Action.Local.ToBindingList();
                    }
                }
                else if (tabTag == "DEV")
                {
                    using (smart_houseContext con = new smart_houseContext())
                    {
                        var sql = (from obj in con.Device
                                   where !obj.DelFlag && !obj.DeviceGroup.DelFlag
                                   select new
                                   {
                                       id = obj.Id,
                                       Наименование = obj.Name,
                                       Группа = obj.DeviceGroup.Name
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Device.Where(x => !x.DelFlag && !x.DeviceGroup.DelFlag).Count() / visiblePageDB;
                        tbDEVName.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("Наименование"), Mode = BindingMode.OneWay });
                        cbxDEVGroup.SetBinding(ComboBox.TextProperty, new Binding { Path = new PropertyPath("Группа"), Mode = BindingMode.OneWay });
                        dgDB.ItemsSource = sql;

                        con.DeviceGroup.Load();
                        cbxDEVGroup.ItemsSource = con.DeviceGroup.Where(l => !l.DelFlag).ToList();
                        if (cbxDEVGroup.Items.Count > 0)
                        {
                            cbxDEVGroup.SelectedValuePath = "Id";
                            cbxDEVGroup.DisplayMemberPath = "Name";
                            cbxDEVGroup.SelectedIndex = 0;
                        }
                    }
                }
                else if (tabTag == "DEVGR")
                {
                    using (smart_houseContext con = new smart_houseContext())
                    {
                        var sql = (from obj in con.DeviceGroup
                                   where !obj.DelFlag
                                   select new
                                   {
                                       id = obj.Id,
                                       Наименование = obj.Name,
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.DeviceGroup.Where(x => !x.DelFlag).Count() / visiblePageDB;
                        tbDEVGRName.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("Наименование"), Mode = BindingMode.OneWay });
                        dgDB.ItemsSource = sql;

                    }
                }
                else if (tabTag == "RM")
                {

                    using (smart_houseContext con = new smart_houseContext())
                    {
                        var sql = (from obj in con.Room
                                   where !obj.DelFlag
                                   select new
                                   {
                                       id = obj.Id,
                                       Наименование = obj.Name,
                                       Порядковый_номер = obj.SerialNumber
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Room.Where(x => !x.DelFlag).Count() / visiblePageDB;
                        tbRMName.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("Наименование"), Mode = BindingMode.OneWay });
                        iudRMSerialNumber.SetBinding(Xceed.Wpf.Toolkit.IntegerUpDown.ValueProperty, new Binding { Path = new PropertyPath("Порядковый_номер"), Mode = BindingMode.OneWay });
                        //iudRMSerialNumber.Value = 0;
                        dgDB.ItemsSource = sql;
                        dgDB.Columns[2].Header = "Порядковый номер";
                    }
                }
                else if (tabTag == "US")
                {
                    using (smart_houseContext con = new smart_houseContext())
                    {
                        var sql = (from obj in con.UserSystem
                                   where !obj.DelFlag && !obj.UserGroup.DelFalg
                                   select new
                                   {
                                       id = obj.Id,
                                       ФИО = obj.FullName,
                                       Группа = obj.UserGroup.Name
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.UserSystem.Where(x => !x.DelFlag && !x.UserGroup.DelFalg).Count() / visiblePageDB;
                        tbUSFullName.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("ФИО"), Mode = BindingMode.OneWay });
                        cbxUSGroup.SetBinding(ComboBox.TextProperty, new Binding { Path = new PropertyPath("Группа"), Mode = BindingMode.OneWay });
                        dgDB.ItemsSource = sql;

                        con.UserGroup.Load();
                        cbxUSGroup.ItemsSource = con.UserGroup.Where(l => !l.DelFalg).ToList();
                        if (cbxUSGroup.Items.Count > 0)
                        {
                            cbxUSGroup.SelectedValuePath = "Id";
                            cbxUSGroup.DisplayMemberPath = "Name";
                            cbxUSGroup.SelectedIndex = 0;
                        }
                    }
                }
                else if (tabTag == "USGR")
                {
                    using (smart_houseContext con = new smart_houseContext())
                    {

                        var sql = (from obj in con.UserGroup
                                   where !obj.DelFalg
                                   select new
                                   {
                                       id = obj.Id,
                                       Наименование = obj.Name,
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.UserGroup.Where(x => !x.DelFalg).Count() / visiblePageDB;
                        tbUSGRName.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("Наименование"), Mode = BindingMode.OneWay });
                        dgDB.ItemsSource = sql;
                    }
                }
                SetVisibleColumn(new string[] { "id" });
                idSel.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("id"), Mode = BindingMode.OneWay });
                if (tabTag != "ST" && tabTag != "AA")
                {
                    allPageDB = (tmpAllPage - Math.Truncate(tmpAllPage)) == 0 ? (int)tmpAllPage : (int)(tmpAllPage + 1);
                    tbNowPage.Text = nowPageDB + "/" + allPageDB;
                }
                  
            }
        }

        //обработка события смены вкладки при работе с БД
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new DBDelegate(ShowTable));
            //ShowTable();
            nowPageDB = 0;
            //ShowTable();
        }
        //отобажение необходимых полей во вкладке параметров при работе с БД
        void GetSetParamTab(string tagPanel)
        {
            if (tagPanel == "ST" || tagPanel == "AA")
            {
                btnControlDB.Visibility = Visibility.Collapsed;
                btnEnterQuery.Visibility = Visibility.Visible;

                tbAAUser.Visibility = Visibility.Visible;
                cbxAAUser.Visibility = Visibility.Visible;
                checkBAAUser.Visibility = Visibility.Visible;
                if (tagPanel == "ST")
                {
                    tbAAUser.Visibility = Visibility.Collapsed;
                    cbxAAUser.Visibility = Visibility.Collapsed;
                    checkBAAUser.Visibility = Visibility.Collapsed;
                }
                tagPanel = "STAA";
            }
            else
            {
                btnEnterQuery.Visibility = Visibility.Collapsed;
                btnControlDB.Visibility = Visibility.Visible;
            }
            var listChildrenControl = LogicalTreeHelper.GetChildren(spListParamDB);
            foreach (var el in listChildrenControl)
            {
                if (el is Grid && (el as Grid).Tag.ToString() != tagPanel)
                {
                    (el as Grid).Visibility = Visibility.Collapsed;
                }
                else
                    if ((el as Grid) != null)
                    (el as Grid).Visibility = Visibility.Visible;
            }


        }




        //фиксирование в БД данных полученных с датчиков
        void FixedStatistic(string typeCmd, string numRoom, string value)
        {
            //MessageBox.Show("11");
            if (userCon.UserGroup.Name != "Гость")
            {
                if (checkBWiteDB.IsChecked.Value)
                {
             
                    //await Task.Delay(500);
                    //Thread.Sleep(500);
                    Device device = null;
                    Room room = null;
                    //if (typeCmd == "C")
                    //{
                    //    typeCmd += numRoom;
                    //    numRoom = "0";
                    //}
                    if (typeCmd == "V")
                        numRoom = "0";
                    string nameDev = GetFullNameDevice(typeCmd);
                    //device = FindObjDB(nameDev) as Device;
                    //room = FindObjDB(numRoom, "Room") as Room;
                    using (smart_houseContext con = new smart_houseContext())
                    {
                        device = con.Device.FirstOrDefault(c => c.Name == nameDev);
                        room = con.Room.FirstOrDefault(c => c.SerialNumber == Convert.ToInt32(numRoom));
                    }
                    if (device != null && room != null)
                    {
                        //await Task.Factory.StartNew(() => { DBInsertStatistic(device, room, DateTime.Now, value); });
                        DBInsertStatistic(device, room, DateTime.Now, value);
                    }

                }
            }
        }
        //фиксирование в БД действий пользователя

        void FixedAction(string description, string typeCmd = "", string numRoom = "", string typeDevice = "Sensor")
        {
            if (userCon.UserGroup != null && userCon.UserGroup.Name != "Гость")
            {
                //if (checkBWiteDB != null && checkBWiteDB.IsChecked.Value)
                //{
                    Device device = null;
                    Room room = null;
                    //if (typeCmd == "C")
                    //{
                    //    typeCmd += numRoom;
                    //    numRoom = "0";
                    //}
                    if (typeCmd == "V")
                        numRoom = "0";
                    string nameDev = GetFullNameDevice(typeCmd, typeDevice);
                    if (typeCmd != "" && numRoom != "")
                    {
                        device = FindObjDB(nameDev) as Device;
                        room = FindObjDB(numRoom, "Room") as Room;
                    }
                    if (userCon != null && device != null && room != null)
                        DBInsertAction(device, room, DateTime.Now, description);
                    else
                        DBInsertAction(null, null, DateTime.Now, description);

                //}
            }
            //DBInsertAction(device, room, DateTime.Now, "Включение 10");
        }
        //получение полного названия устройства по сокращенному 
        string GetFullNameDevice(string shortName, string type = "Sensor")
        {
            if (type == "Sensor")
            {
                if (shortName == "L")
                    return "Датчик освещения";
                else if (shortName == "M")
                    return "Датчик движения";
                else if (shortName == "T")
                    return "Датчик температуры";
                else if (shortName == "W")
                    return "Датчик протечки воды";
                else if (shortName == "H")
                    return "Датчик влажности";
                else if (shortName == "P")
                    return "Датчик атмосферного давления";
                else if (shortName == "G")
                    return "Датчик газа";
                else if (shortName == "D")
                    return "Датчик открытия двери";
                else if (shortName == "V")
                    return "Датчик вибрации";
                else if (shortName == "C1")
                    return "Датчик тока INA219 3.2A";
                else if (shortName == "C2")
                    return "Датчик тока MAX471 3A";
                else if (shortName == "C3")
                    return "Датчик тока ACS712 5A";
                else if (shortName == "C4")
                    return "Датчик тока ACS712 20A";
                else if (shortName == "C1Power")
                    return "Датчик тока INA219 3.2A - мощность";
                else if (shortName == "C2Power")
                    return "Датчик тока MAX471 3A - мощность";
                else if (shortName == "C3Power")
                    return "Датчик тока ACS712 5A - мощность";
                else if (shortName == "C4Power")
                    return "Датчик тока ACS712 20A - мощность";
            }
            else if (type == "Device")
            {
                if (shortName == "L")
                    return "Основное освещение";
                else if (shortName == "B")
                    return "Фоновое освещение";
                else if (shortName == "D")
                    return "Привод двери";
                else if (shortName == "A")
                    return "Сигнализация";
                else if (shortName == "C" || shortName == "H")
                    return "Нагревательный элемент";
            }

            return "";
        }
        //добавление в БД сущности - Статистика
        void DBInsertStatistic(Device device, Room room, DateTime dateTime, string value, bool emergency = false)
        {
            using (smart_houseContext con = new smart_houseContext())
            {
                Statistic obj = new Statistic
                {
                    DeviceId = device.Id,
                    RoomId = room.Id,
                    DateTime = dateTime,
                    Value = value,
                    Emergency = emergency
                };

                con.Statistic.Add(obj);
                con.SaveChanges();
            }
        }
        //добавление в БД сущности - Аудит действия
        void DBInsertAction(Device device, Room room, DateTime dateTime, string description)
        {
            if (userCon.UserGroup.Name != "Гость")
            {
                using (smart_houseContext con = new smart_houseContext())
                {
                    Action obj = null;
                    if (device != null && room != null)
                    {
                        obj = new Action
                        {
                            UserSystemId = userCon.Id,
                            DeviceId = device.Id,
                            RoomId = room.Id,
                            DateTime = dateTime,
                            Description = description
                        };
                    }
                    else
                    {
                        obj = new Action
                        {
                            UserSystemId = userCon.Id,
                            DateTime = dateTime,
                            Description = description
                        };
                    }

                    if (obj != null)
                    {
                        con.Action.Add(obj);
                        con.SaveChanges();
                    }
                }
            }
           
        }

        //Поиск обекта в БД
        public UpClassDB FindObjDB(string name, string type = "Device")
        {
            if (type == "Room")
            {
                using (smart_houseContext con = new smart_houseContext())
                {
                    Room obj = con.Room.FirstOrDefault(c => c.SerialNumber == Convert.ToInt32(name));
                    return obj;
                }
            }
            else if (type == "Device")
            {
                using (smart_houseContext con = new smart_houseContext())
                {
                    Device obj = con.Device.FirstOrDefault(c => c.Name == name);
                    return obj;
                }
            }
            return null;
        }
        //хэширование пароля
        private string GetHashString(string s)
        {
            //переводим строку в байт-массим  
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            //создаем объект для получения средст шифрования  
            MD5CryptoServiceProvider CSP =
                new MD5CryptoServiceProvider();
            //вычисляем хеш-представление в байтах  
            byte[] byteHash = CSP.ComputeHash(bytes);
            string hash = string.Empty;
            //формируем одну цельную строку из массива  
            foreach (byte b in byteHash)
                hash += string.Format("{0:x2}", b);
            return hash;
        }

        //добавление данных БД
        void AddDataDB()
        {
            try
            {
                long idObj = -1;
                using (smart_houseContext con = new smart_houseContext())
                {
                    //con.GetService<ILoggerFactory>().AddProvider(new LoggerData());
                    //получение текущей вкладки
                    string tabTag = (tcTabDB.SelectedValue as TabItem).Tag.ToString();

                    if (tabTag == "DEV")
                    {
                        //проверка на заполнненость всех полей
                        if (tbDEVName.Text == String.Empty || cbxDEVGroup.SelectedValue == null)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        //формирование объекта для добавления
                        Device obj = new Device
                        {
                            Name = tbDEVName.Text,
                            DeviceGroupId = Convert.ToInt32(cbxDEVGroup.SelectedValue)
                        };
                        //if (con.Device.First(x => x.Name == obj.Name && x.DeviceGroup == obj.DeviceGroup) != null)
                        //{
                        //доавление объекта и обновление данных
                        var elObj = con.Device.FirstOrDefault(x => x.Name == obj.Name);
                        if (elObj == null)
                        {
                            con.Device.Add(obj);
                            con.SaveChanges();
                            BuildNotification("Добавление данных в БД. Таблица [device]. Id объекта: " + obj.Id);
                            FixedAction("Добавление данных в БД. Таблица [device]. Id объекта: " + obj.Id);
                            //TabControl_SelectionChanged(null, null);
                            //}
                            //else
                            //    MessageBox.Show("Объект существует!");
                        }
                        else
                        {
                            if (elObj.DelFlag)
                            {
                                if (MessageBox.Show("Устройство [" + elObj.Name + "] в группе [" + con.DeviceGroup.Find(elObj.DeviceGroupId).Name + "] уже существует. Восстановить?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                    }
                    else if (tabTag == "DEVGR")
                    {
                        if (tbDEVGRName.Text == String.Empty)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        DeviceGroup obj = new DeviceGroup
                        {
                            Name = tbDEVGRName.Text
                        };
                        var elObj = con.DeviceGroup.FirstOrDefault(x => x.Name == obj.Name);
                        if (elObj == null)
                        {
                            con.DeviceGroup.Add(obj);
                            con.SaveChanges();
                            BuildNotification("Добавление данных в БД. Таблица [device_group]. Id объекта: " + obj.Id);
                            FixedAction("Добавление данных в БД. Таблица [device_group]. Id объекта: " + obj.Id);
                        }
                        else
                        {
                            if (elObj.DelFlag)
                            {
                                if (MessageBox.Show("Данный объект уже существует. Восстановить?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        //TabControl_SelectionChanged(null, null);
                    }
                    else if (tabTag == "RM")
                    {
                        if (tbRMName.Text == String.Empty || iudRMSerialNumber.Value == null)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        Room obj = new Room
                        {
                            Name = tbRMName.Text,
                            SerialNumber = iudRMSerialNumber.Value.Value
                        };
                        var elObj = con.Room.FirstOrDefault(x => x.Name == obj.Name || x.SerialNumber == obj.SerialNumber);
                        if (elObj == null)
                        {
                            con.Room.Add(obj);
                            con.SaveChanges();
                            BuildNotification("Добавление данных в БД. Таблица [room]. Id объекта: " + obj.Id);
                            FixedAction("Добавление данных в БД. Таблица [room]. Id объекта: " + obj.Id);
                            //TabControl_SelectionChanged(null, null);
                        }
                        else
                        {
                            if (elObj.DelFlag)
                            {
                                if (MessageBox.Show("Комната [" + elObj.Name + "] с порядковым номером ["+ elObj.SerialNumber + "] уже существует. Восстановить?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                    }
                    else if (tabTag == "US")
                    {
                        if (tbUSFullName.Text == String.Empty || tbUSPswd.Password == String.Empty || cbxUSGroup.SelectedValue == null)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        UserSystem obj = new UserSystem
                        {
                            FullName = tbUSFullName.Text,
                            Pswd = GetHashString(tbUSPswd.Password),
                            UserGroupId = Convert.ToInt32(cbxUSGroup.SelectedValue),

                        };
                        UserGroup ug = (cbxUSGroup.SelectedItem as UserGroup);
                        ug.UserSystem.Add(obj);
                        var elObj = con.UserSystem.FirstOrDefault(x => x.FullName == obj.FullName);
                        if (elObj == null)
                        {
                            con.UserSystem.Add(obj);
                            con.SaveChanges();
                            BuildNotification("Добавление данных в БД. Таблица [user_system]. Id объекта: " + obj.Id);
                            FixedAction("Добавление данных в БД. Таблица [user_system]. Id объекта: " + obj.Id);
                        }
                        else
                        {
                            if (elObj.DelFlag)
                            {
                                if (MessageBox.Show("Пользователь [" + elObj.FullName + "] в группе [" + con.UserGroup.Find(elObj.UserGroupId).Name + "] уже существует. Восстановить?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        //TabControl_SelectionChanged(null, null);
                    }
                    else if (tabTag == "USGR")
                    {
                        // Проверяем, что в текстовых полях есть данные
                        if (tbUSGRName.Text == String.Empty)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        // Заполняем объект класса
                        UserGroup obj = new UserGroup
                        {
                            Name = tbUSGRName.Text,
                        };
                        var elObj = con.UserGroup.FirstOrDefault(x => x.Name == obj.Name);
                        if (elObj == null)
                        {
                            // Заносим данные в нашу таблицу
                            // Обязательно сохраняем изменения
                            con.UserGroup.Add(obj);
                            con.SaveChanges();
                            BuildNotification("Добавление данных в БД. Таблица [user_group. Id объекта: " + obj.Id);
                            FixedAction("Добавление данных в БД. Таблица [user_group. Id объекта: " + obj.Id);
                            //TabControl_SelectionChanged(null, null);
                        }
                        else
                        {
                            if (elObj.DelFalg)
                            {
                                if (MessageBox.Show("Данный объект уже существует. Восстановить?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex is DbUpdateException + "");
                //if (ex.InnerException is Npgsql.NpgsqlException)
                //{
                //    Npgsql.NpgsqlException exDB = (ex.InnerException as Npgsql.NpgsqlException);
                //    //код ошибки на уникальность записи
                //    if (exDB.Data["Code"].ToString() == "23505")
                //        if (MessageBox.Show("Данный объект уже существует. Восстановить?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                //        {
                //            idSel.Text = elObj.Id.ToString();
                //            DeleteDataDB(false);
                //        }
                //    //else
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }

        }

        //обновление данных БД
        void UpdateDataDB()
        {
            if (idSel.Text != null && idSel.Text != "" )
            {
                using (smart_houseContext con = new smart_houseContext())
                {
                    //получение текущей вкладки
                    string tabTag = (tcTabDB.SelectedValue as TabItem).Tag.ToString();
                    long id = Convert.ToInt32(idSel.Text);
                    if (tabTag == "DEV")
                    {
                        //проверка заполненности полей

                        if (tbDEVName.Text == String.Empty || cbxDEVGroup.SelectedValue == null)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        //получение объекта по id
                        Device obj = con.Device.Find(Convert.ToInt32(id));
                        if (obj == null) return;

                        string oldName = obj.Name;
                        int oldDeviceGroupId = obj.DeviceGroupId;
                        //изменение полей 
                        obj.Name = tbDEVName.Text;
                        obj.DeviceGroupId = Convert.ToInt32(cbxDEVGroup.SelectedValue);

                        var elObj = con.Device.FirstOrDefault(x => x.Name == obj.Name && x.Name != oldName);
                        if (elObj == null)
                        {
                            //сохранение параметров
                            con.Device.Update(obj);
                            con.SaveChanges();
                            BuildNotification("Обновление данных в БД. Таблица [device]. Id объекта: " + obj.Id);
                            FixedAction("Обновление данных в БД. Таблица [device]. Id объекта: " + obj.Id);
                            //TabControl_SelectionChanged(null, null);
                        }
                        else
                        {
                            if (elObj.DelFlag)
                            {
                                if (MessageBox.Show("Устройство [" + elObj.Name + "] в группе [" + con.DeviceGroup.Find(elObj.DeviceGroupId).Name + "] уже существует. Восстановить и заменить текущий?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    con.Device.Remove(obj);
                                    con.SaveChanges();
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else if (tabTag == "DEVGR")
                    {
                        if (tbDEVGRName.Text == String.Empty)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        DeviceGroup obj = con.DeviceGroup.Find(Convert.ToInt32(id));
                        if (obj == null) return;

                        obj.Name = tbDEVGRName.Text;

                        var elObj = con.DeviceGroup.FirstOrDefault(x => x.Name == obj.Name);
                        if (elObj == null)
                        {
                            con.DeviceGroup.Update(obj);
                            con.SaveChanges();
                            BuildNotification("Обновление данных в БД. Таблица [device_group]. Id объекта: " + obj.Id);
                            FixedAction("Обновление данных в БД. Таблица [device_group]. Id объекта: " + obj.Id);
                            //TabControl_SelectionChanged(null, null);
                        }
                        else
                        {
                            if (elObj.DelFlag)
                            {
                                if (MessageBox.Show("Данный объект уже существует. Восстановить и заменить текущий?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    con.DeviceGroup.Remove(obj);
                                    con.SaveChanges();
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else if (tabTag == "RM")
                    {
                        if (tbRMName.Text == String.Empty || iudRMSerialNumber.Value == null)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        Room obj = con.Room.Find(Convert.ToInt32(id));
                        if (obj == null) return;

                        //if (obj.Name == tbRMName.Text || obj.SerialNumber == iudRMSerialNumber.Value.Value)
                        string oldName = obj.Name;
                        int oldSN = obj.SerialNumber;

                        obj.Name = tbRMName.Text;
                        obj.SerialNumber = iudRMSerialNumber.Value.Value;

                        var elObj = con.Room.FirstOrDefault(x => (x.Name == obj.Name || x.SerialNumber == obj.SerialNumber) && (x.Name != oldName && x.SerialNumber != oldSN));
                        if (elObj == null)
                        {
                            con.Room.Update(obj);
                            con.SaveChanges();
                            BuildNotification("Обновление данных в БД. Таблица [room]. Id объекта: " + obj.Id);
                            FixedAction("Обновление данных в БД. Таблица [room]. Id объекта: " + obj.Id);
                            //TabControl_SelectionChanged(null, null);
                        }
                        else
                        {
                            if (elObj.DelFlag)
                            {
                                if (MessageBox.Show("Комната [" + elObj.Name + "] с порядковым номером [" + elObj.SerialNumber + "] уже существует. Восстановить и заменить текущий?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    con.Room.Remove(obj);
                                    con.SaveChanges();
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else if (tabTag == "US")
                    {
                        if (tbUSFullName.Text == String.Empty || tbUSPswd.Password == String.Empty || cbxUSGroup.SelectedValue == null)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        UserSystem obj = con.UserSystem.Find(Convert.ToInt32(id));
                        if (obj == null) return;

                        string oldFullName = obj.FullName;

                        obj.FullName = tbUSFullName.Text;
                        obj.UserGroupId = Convert.ToInt32(cbxUSGroup.SelectedValue);
                        obj.Pswd = GetHashString(tbUSPswd.Password);

                        var elObj = con.UserSystem.FirstOrDefault(x => x.FullName == obj.FullName && x.FullName != oldFullName);
                        if (elObj == null)
                        {
                            con.UserSystem.Update(obj);
                            con.SaveChanges();
                            BuildNotification("Обновление данных в БД. Таблица [user_system]. Id объекта: " + obj.Id);
                            FixedAction("Обновление данных в БД. Таблица [user_system]. Id объекта: " + obj.Id);
                            if (obj.Id == userCon.Id)
                            {
                                userCon = obj;
                                userCon.UserGroup = con.UserGroup.Find(obj.UserGroupId);
                                this.Title = "Система управления - \"Умное здание\" // Пользователь: " + userCon.FullName + " // Группа: " + userCon.UserGroup.Name + "";
                                if (userCon.UserGroup.Name != "Администратор")
                                {
                                    BuildUI();
                                    tiStatistic.IsSelected = true;
                                }
                            }
                        }
                        else
                        {
                            if (elObj.DelFlag)
                            {
                                if (MessageBox.Show("Пользователь [" + elObj.FullName + "] в группе [" + con.UserGroup.Find(elObj.UserGroupId).Name + "] уже существует. Восстановить и заменить текущий?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    con.UserSystem.Remove(obj);
                                    con.SaveChanges();
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        //TabControl_SelectionChanged(null, null);
                    }
                    else if (tabTag == "USGR")
                    {
                        if (tbUSGRName.Text == String.Empty)
                        {
                            MessageBox.Show("Заполнены не все поля!");
                            return;
                        }
                        UserGroup obj = con.UserGroup.Find(Convert.ToInt32(id));
                        if (obj == null) return;

                        obj.Name = tbUSGRName.Text;

                        var elObj = con.UserGroup.FirstOrDefault(x => x.Name == obj.Name);
                        if (elObj == null)
                        {
                            con.UserGroup.Update(obj);
                            con.SaveChanges();
                            BuildNotification("Обновление данных в БД. Таблица [user_group]. Id объекта: " + obj.Id);
                            FixedAction("Обновление данных в БД. Таблица [user_group]. Id объекта: " + obj.Id);
                            //TabControl_SelectionChanged(null, null);
                        }
                        else
                        {
                            if (elObj.DelFalg)
                            {
                                if (MessageBox.Show("Данный объект уже существует. Восстановить и заменить текущий?", "ERROR", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                                {
                                    con.UserGroup.Remove(obj);
                                    con.SaveChanges();
                                    idSel.Text = elObj.Id.ToString();
                                    DeleteDataDB(false);
                                }
                            }
                            else
                                MessageBox.Show("Данный объект уже существует!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }
            else
                MessageBox.Show("Выберите строку для изменения!");
        }
        //удаление данных БД
        void DeleteDataDB(bool doDel = true)
        {
            if (idSel.Text != null && idSel.Text != "" || !doDel)
            {
                using (smart_houseContext con = new smart_houseContext())
                {
                    //foreach (dynamic propety in dgDB.SelectedItems)
                    //{
                    //    MessageBox.Show(propety[0] + "");
                    //}
                    //получение текущей вкладки
                    string tabTag = (tcTabDB.SelectedValue as TabItem).Tag.ToString();
                    //получение id объекта

                    long id = Convert.ToInt32(idSel.Text);
                    if (tabTag == "ST")
                    {
                        //получение объекта по id
                        Statistic obj = con.Statistic.Find(id);
                        if (obj == null) return;

                        //обновление данных объекта
                        var objUpd = con.Statistic
                            .Where(c => c.Id == id)
                            .FirstOrDefault();
                        objUpd.DelFlag = doDel;

                        con.SaveChanges();
                        BuildNotification("Логическое удаление данных из БД. Таблица [statistic]. Id объекта: " + obj.Id);
                        FixedAction("Логическое удаление данных из БД. Таблица [statistic]. Id объекта: " + obj.Id);
                        //TabControl_SelectionChanged(null, null);
                        return;
                    }
                    else if (tabTag == "AA")
                    {
                        Action obj = con.Action.Find(id);
               
                        if (obj == null) return;

                        var objUpd = con.Action
                            .Where(c => c.Id == id)
                            .FirstOrDefault();
                        objUpd.DelFlag = doDel;

                        con.SaveChanges();
                        BuildNotification("Логическое удаление данных из БД. Таблица [action]. Id объекта: " + obj.Id);
                        FixedAction("Логическое удаление данных из БД. Таблица [action]. Id объекта: " + obj.Id);
                        //TabControl_SelectionChanged(null, null);
                        return;
                    }
                    else if (tabTag == "DEV")
                    {
                        Device obj = con.Device.Find(Convert.ToInt32(id));
                        if (obj == null) return;
                        //MessageBox.Show(obj.Id + " " + obj.Name);
                        var objUpd = con.Device
                            .Where(c => c.Id == id)
                            .FirstOrDefault();
                        objUpd.DelFlag = doDel;

                        con.SaveChanges();
                        BuildNotification("Логическое удаление данных из БД. Таблица [device]. Id объекта: " + obj.Id);
                        FixedAction("Логическое удаление данных из БД. Таблица [device]. Id объекта: " + obj.Id);
                        //TabControl_SelectionChanged(null, null);
                        return;
                    }
                    else if (tabTag == "DEVGR")
                    {
                        DeviceGroup obj = con.DeviceGroup.Find(Convert.ToInt32(id));
                        if (obj == null) return;
                        //MessageBox.Show(obj.Id + " " + obj.Name);
                        var objUpd = con.DeviceGroup
                            .Where(c => c.Id == id)
                            .FirstOrDefault();
                        objUpd.DelFlag = doDel;

                        con.SaveChanges();
                        BuildNotification("Логическое удаление данных из БД. Таблица [device_group]. Id объекта: " + obj.Id);
                        FixedAction("Логическое удаление данных из БД. Таблица [device_group]. Id объекта: " + obj.Id);
                        //TabControl_SelectionChanged(null, null);
                        return;
                    }
                    else if (tabTag == "RM")
                    {
                        Room obj = con.Room.Find(Convert.ToInt32(id));
                        if (obj == null) return;
                        //MessageBox.Show(obj.Id + " " + obj.Name);
                        var objUpd = con.Room
                            .Where(c => c.Id == id)
                            .FirstOrDefault();
                        objUpd.DelFlag = doDel;

                        con.SaveChanges();
                        BuildNotification("Логическое удаление данных из БД. Таблица [room]. Id объекта: " + obj.Id);
                        FixedAction("Логическое удаление данных из БД. Таблица [room]. Id объекта: " + obj.Id);
                        //TabControl_SelectionChanged(null, null);
                        return;
                    }
                    else if (tabTag == "US")
                    {
                        UserSystem obj = con.UserSystem.Find(Convert.ToInt32(id));
                        if (obj == null) return;

                        var objUpd = con.UserSystem
                            .Where(c => c.Id == id)
                            .FirstOrDefault();
                        objUpd.UserGroup = con.UserGroup.Find(objUpd.UserGroupId);
                        if (objUpd.FullName == userCon.FullName && objUpd.UserGroup.Name == userCon.UserGroup.Name && objUpd.Pswd == userCon.Pswd)
                        {
                            MessageBox.Show("Вошедший в систему пользователь не может удалить из системы сам себя!");
                            return;
                        }
                        if (objUpd.UserGroup.Name == "Администратор")
                        {
                            var countAdmin = con.UserSystem
                                           .Where(c => c.UserGroup.Name == "Администратор" && !c.DelFlag)
                                           .Count();
                            if (countAdmin == 1)
                            {
                                MessageBox.Show("В системе должен быть хотя бы один администратор!");
                                return;
                            }
                        }
                        objUpd.DelFlag = doDel;

                        con.SaveChanges();
                        BuildNotification("Логическое удаление данных из БД. Таблица [user_system]. Id объекта: " + obj.Id);
                        FixedAction("Логическое удаление данных из БД. Таблица [user_system]. Id объекта: " + obj.Id);
                        //TabControl_SelectionChanged(null, null);
                        return;
                    }
                    else if (tabTag == "USGR")
                    {
                        UserGroup obj = con.UserGroup.Find(Convert.ToInt32(id));
                        if (obj == null) return;
                        //MessageBox.Show(obj.Id + " " + obj.Name);
                        var objUpd = con.UserGroup
                            .Where(c => c.Id == id)
                            .FirstOrDefault();
                        if (objUpd.Name != "Администратор")
                        {
                            objUpd.DelFalg = doDel;

                            con.SaveChanges();
                            BuildNotification("Логическое удаление данных из БД. Таблица [user_group]. Id объекта: " + obj.Id);
                            FixedAction("Логическое удаление данных из БД. Таблица [user_group]. Id объекта: " + obj.Id);
                            //TabControl_SelectionChanged(null, null);     
                        }
                        else
                            MessageBox.Show("Данная группа не подлежит удалению!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            else
                MessageBox.Show("Выберите строку для удаления!");
           
        }

        //обработчик события добавления данных в БД
        private void btnAddDataDB_Click(object sender, RoutedEventArgs e)
        {
            try { 
            isEnterQuery = false;
            AddDataDB();
            TabControl_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //обработчик события обновления данных из БД
        private void btnUpdateDataDB_Click(object sender, RoutedEventArgs e)
        {
            try { 
            isEnterQuery = false;
            UpdateDataDB();
            TabControl_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //обработчик события удаления данных из БД
        private void btnDeleteDataDB_Click(object sender, RoutedEventArgs e)
        {
            try { 
            isEnterQuery = false;
            DeleteDataDB();
            TabControl_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        bool isEnterQuery = false;
        //обработчик события нажатия кнопки для выполнения запроса статистики или аудита действий
        private void btnEnterQuery_Click(object sender, RoutedEventArgs e)
        {
            try { 
            isEnterQuery = true;
            string tabTag = (tcTabDB.SelectedValue as TabItem).Tag.ToString();
            QueryExecutionSTAA(tabTag);
            TabControl_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //выполнение запросов для отображения статистики или аудита действий
        void QueryExecutionSTAA(string type = "ST")
        {
            //переменные определяющие состояние чекбоксов
            bool isDevice = checkBSTAADevice.IsChecked.Value;
            bool isRoom = checkBSTAARoom.IsChecked.Value;
            bool isUser = checkBAAUser.IsChecked.Value;

            int idDevice = Convert.ToInt32(cbxSTAADevice.SelectedValue);
            int idUser = Convert.ToInt32(cbxAAUser.SelectedValue);
            int idRoom = Convert.ToInt32(cbxSTAARoom.SelectedValue);

            using (smart_houseContext con = new smart_houseContext())
            {
                double tmpAllPage = 0;
                DateTime dtFrom = dtpAAFrom.Value.Value /*- new TimeSpan(5,0,0)*/;
                DateTime dtTo = dtpAATo.Value.Value /*- new TimeSpan(5, 0, 0)*/; 
                //если выбрана вкладка - Статистика
                if (type == "ST")
                {
                    //в зависимости от выбраных чекбоксов формируем запрос
                    if (isRoom && isDevice)
                    {
                        var sql = (from obj in con.Statistic
                                   where !obj.DelFlag && !obj.Device.DelFlag && !obj.Room.DelFlag && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                   && obj.Device.Id == idDevice && obj.Room.Id == idRoom
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                       Значение = obj.Value
                                       //Критичность = obj.Emergency
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Statistic.Where(x => !x.DelFlag && !x.Device.DelFlag && !x.Room.DelFlag && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.Device.Id == idDevice && x.Room.Id == idRoom).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                    }
                    else if (isDevice)
                    {
                        var sql = (from obj in con.Statistic
                                   where !obj.DelFlag && !obj.Device.DelFlag && !obj.Room.DelFlag && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                   && obj.Device.Id == idDevice
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                       Значение = obj.Value
                                       //Критичность = obj.Emergency
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Statistic.Where(x => !x.DelFlag && !x.Device.DelFlag && !x.Room.DelFlag && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.Device.Id == idDevice).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                    }
                    else if (isRoom)
                    {
                        var sql = (from obj in con.Statistic
                                   where !obj.DelFlag && !obj.Device.DelFlag && !obj.Room.DelFlag && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                   && obj.Room.Id == idRoom
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                       Значение = obj.Value
                                       //Критичность = obj.Emergency
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Statistic.Where(x => !x.DelFlag && !x.Device.DelFlag && !x.Room.DelFlag && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.Room.Id == idRoom).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                    }
                    else
                    {
                        var sql = (from obj in con.Statistic
                                   where !obj.DelFlag && !obj.Device.DelFlag && !obj.Room.DelFlag && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                       Значение = obj.Value
                                       //Критичность = obj.Emergency
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Statistic.Where(x => !x.DelFlag && !x.Device.DelFlag && !x.Room.DelFlag && x.DateTime >= dtFrom && x.DateTime <= dtTo).Count() / visiblePageDB;
                        //this.Title = tmpAllPage +"";
                        dgDB.ItemsSource = sql;
                    }
                    dgDB.Columns[1].Header = "Дата события";
                    (dgDB.Columns[1] as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy HH:mm:ss";
                }
                //если выбрана вкладка - Аудит действий
                else if (type == "AA")
                {
                    
                    //в зависимости от выбраных чекбоксов формируем запрос
                    if (isUser && isRoom && isDevice)
                    {
                        var sql = (from obj in con.Action
                                   where !obj.DelFlag /*&& !obj.Device.DelFlag && !obj.UserSystem.DelFlag && !obj.Room.DelFlag*/ && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                    && obj.Device.Id == idDevice && obj.Room.Id == idRoom && obj.UserSystem.Id == idUser
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Описание_действия = obj.Description,
                                       Пользователь = obj.UserSystem.FullName + " (" + obj.UserSystem.UserGroup.Name + ")",
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Action.Where(x => x.DelFlag == false && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.Device.Id == idDevice && x.Room.Id == idRoom && x.UserSystem.Id == idUser).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                    }
                    else if (isRoom && isUser)
                    {
                        var sql = (from obj in con.Action
                                   where !obj.DelFlag /*&& !obj.Device.DelFlag && !obj.UserSystem.DelFlag && !obj.Room.DelFlag*/ && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                   && obj.Room.Id == idRoom && obj.UserSystem.Id == idUser
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Описание_действия = obj.Description,
                                       Пользователь = obj.UserSystem.FullName + " (" + obj.UserSystem.UserGroup.Name + ")",
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Action.Where(x => x.DelFlag == false && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.Room.Id == idRoom && x.UserSystem.Id == idUser).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                    }
                    else if (isDevice && isUser)
                    {
                        var sql = (from obj in con.Action
                                   where !obj.DelFlag /*&& !obj.Device.DelFlag && !obj.UserSystem.DelFlag && !obj.Room.DelFlag*/ && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                   && obj.Device.Id == idDevice && obj.UserSystem.Id == idUser
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Описание_действия = obj.Description,
                                       Пользователь = obj.UserSystem.FullName + " (" + obj.UserSystem.UserGroup.Name + ")",
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Action.Where(x => x.DelFlag == false && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.Device.Id == idDevice && x.UserSystem.Id == idUser).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                    }
                    else if (isDevice && isRoom)
                    {
                        var sql = (from obj in con.Action
                                   where !obj.DelFlag /*&& !obj.Device.DelFlag && !obj.UserSystem.DelFlag && !obj.Room.DelFlag*/ && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                    && obj.Device.Id == idDevice && obj.Room.Id == idRoom
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Описание_действия = obj.Description,
                                       Пользователь = obj.UserSystem.FullName + " (" + obj.UserSystem.UserGroup.Name + ")",
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Action.Where(x => x.DelFlag == false && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.Device.Id == idDevice && x.Room.Id == idRoom).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                    }
                    else if (isUser)
                    {
                        var sql = (from obj in con.Action
                                   where !obj.DelFlag /*&& !obj.Device.DelFlag && !obj.UserSystem.DelFlag && !obj.Room.DelFlag*/ && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                   && obj.UserSystem.Id == idUser
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Описание_действия = obj.Description,
                                       Пользователь = obj.UserSystem.FullName + " (" + obj.UserSystem.UserGroup.Name + ")",
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Action.Where(x => x.DelFlag == false && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.UserSystem.Id == idUser).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                    }
                    else if (isRoom)
                    {
                        var sql = (from obj in con.Action
                                   where !obj.DelFlag /*&& !obj.Device.DelFlag && !obj.UserSystem.DelFlag && !obj.Room.DelFlag*/ && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                   && obj.Room.Id == idRoom
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Описание_действия = obj.Description,
                                       Пользователь = obj.UserSystem.FullName + " (" + obj.UserSystem.UserGroup.Name + ")",
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Action.Where(x => x.DelFlag == false && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.Room.Id == idRoom).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;

                    }
                    else if (isDevice)
                    {
                        var sql = (from obj in con.Action
                                   where !obj.DelFlag /*&& !obj.Device.DelFlag && !obj.UserSystem.DelFlag && !obj.Room.DelFlag*/ && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                     && obj.Device.Id == idDevice
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Описание_действия = obj.Description,
                                       Пользователь = obj.UserSystem.FullName + " (" + obj.UserSystem.UserGroup.Name + ")",
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Action.Where(x => x.DelFlag == false && x.DateTime >= dtFrom && x.DateTime <= dtTo && x.Device.Id == idDevice).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                    }
                    else
                    {
                        var sql = (from obj in con.Action
                                   where !obj.DelFlag /*&& !obj.Device.DelFlag && !obj.UserSystem.DelFlag && !obj.Room.DelFlag*/ && obj.DateTime >= dtFrom && obj.DateTime <= dtTo
                                   orderby obj.DateTime
                                   select new
                                   {
                                       id = obj.Id,
                                       Дата_события = obj.DateTime,
                                       Описание_действия = obj.Description,
                                       Пользователь = obj.UserSystem.FullName + " (" + obj.UserSystem.UserGroup.Name + ")",
                                       Устройство = obj.Device.Name + " (" + obj.Device.DeviceGroup.Name + ")",
                                       Комната = obj.Room.Name + " (" + obj.Room.SerialNumber + ")",
                                   }).Skip(nowPageDB * visiblePageDB).Take(visiblePageDB).ToList();
                        tmpAllPage = con.Action.Where(x => x.DelFlag == false && x.DateTime >= dtFrom && x.DateTime <= dtTo).Count() / visiblePageDB;
                        dgDB.ItemsSource = sql;
                        //dgDB.ItemsSource = sql;
                    }
                    dgDB.Columns[1].Header = "Дата события";
                    (dgDB.Columns[1] as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy HH:mm:ss";
                    dgDB.Columns[2].Header = "Описание действия";
                }
                SetVisibleColumn(new string[] { "id" });
                allPageDB = (tmpAllPage - Math.Truncate(tmpAllPage)) == 0 ? (int)tmpAllPage : (int)(tmpAllPage + 1);
                tbNowPage.Text = nowPageDB + "/" + allPageDB;
            }
        }

        #region Пейджинг страниц таблиц
        int nowPageDB = 0; //текущая страница 
        int allPageDB = 0;  //страниц всего
        int visiblePageDB = 40;
        private void btnControlPage_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            //проктрутка в начало
            if (btn.Name == btnPrevALLPage.Name)
                nowPageDB = 0;
            //проктрутка назад на одну страницу 
            else if (btn.Name == btnPrevPage.Name && nowPageDB != 0)
                nowPageDB--;
            //проктрутка вперед на одну страницу 
            else if (btn.Name == btnNextPage.Name && nowPageDB != allPageDB)
                nowPageDB++;
            //проктрутка в конец
            else if (btn.Name == btnNextALLPage.Name)
                nowPageDB = allPageDB;
            tbNowPage.Text = nowPageDB + "/" + allPageDB;
            ShowTable();
        }
        #endregion

        //изменение диапазона дат для отображения статистики или действий пользователей (не должно превышать заданного значения)
        private void dateTimePicerDB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (dtpAAFrom.Value != null && dtpAATo.Value != null)
            {
                TimeSpan dif = new TimeSpan();
                if (dtpAAFrom.Value.Value > dtpAATo.Value.Value)
                    dif = (dtpAAFrom.Value - dtpAATo.Value).Value;
                else if (dtpAAFrom.Value.Value <= dtpAATo.Value.Value)
                    dif = (dtpAATo.Value - dtpAAFrom.Value).Value;
                if (dif > new TimeSpan(0, 0, 0, 0) && dif > new TimeSpan(3, 0, 0, 0) || dtpAAFrom.Value.Value > dtpAATo.Value.Value)
                {
                    dtpAAFrom.Value = dtpAATo.Value - new TimeSpan(3, 0, 0, 0);
                }
            }
        }

        //скрытие панели настроек
        private void btnOpenPanelParamDB_Click(object sender, RoutedEventArgs e)
        {
            if (svPanelParamDB.Visibility == Visibility.Visible)
            {
                svPanelParamDB.Visibility = Visibility.Collapsed;
                MatrixTransform mir = new MatrixTransform(-1, 0, 0, 1, ((sender as Button).Content as Image).Width, 0);
                ((sender as Button).Content as Image).RenderTransform = mir;
            }
            else
            {
                svPanelParamDB.Visibility = Visibility.Visible;
                MatrixTransform mir = new MatrixTransform(1, 0, 0, -1, 0, ((sender as Button).Content as Image).Width);
                ((sender as Button).Content as Image).RenderTransform = mir;
            }
        }

        //обработка события выбора необходимости записи данынх в БД
        private void checkBWiteDB_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBWiteDB.IsChecked.Value)
                writeDB = true;
        }



        //автовыбор комбобоксов
        private void cbxGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxDEVGroup.Items.Count > 0 && cbxDEVGroup.SelectedIndex == -1)
                cbxDEVGroup.SelectedIndex = 0;
            if (cbxUSGroup.Items.Count > 0 && cbxUSGroup.SelectedIndex == -1)
                cbxUSGroup.SelectedIndex = 0;
        }
        string oldSelectedBaseTab = "";
        private void baseTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        
            if (baseTabControl.SelectedValue != null && (baseTabControl.SelectedValue as TabItem).Tag != null && (baseTabControl.SelectedValue as TabItem).Tag.ToString() == "DBInfoSystem")
            {
                if (oldSelectedBaseTab != (baseTabControl.SelectedValue as TabItem).Tag.ToString())
                {
                    tcTabDB.SelectedIndex = 0;
                    Dispatcher.BeginInvoke(new DBDelegate(ShowTable));
                    dtpAAFrom.Value = DateTime.Now.AddDays(-3);
                    dtpAATo.Value = DateTime.Now;
                }
                //using (smart_houseContext con = new smart_houseContext())
                //{
                //    this.Title = con.Statistic.Count() + "";
                //}
                //MessageBox.Show("123");
                //TabControl_SelectionChanged(null, null);
                ////Dispatcher.BeginInvoke(new System.Action(() => TabControl_SelectionChanged(null, null)), System.Windows.Threading.DispatcherPriority.Background);
                //baseTabControl.SelectedValue = null;
            }
            if (baseTabControl.SelectedValue != null && (baseTabControl.SelectedValue as TabItem).Tag != null)
                oldSelectedBaseTab = (baseTabControl.SelectedValue as TabItem).Tag.ToString();
            else
                oldSelectedBaseTab = "";
            //
        }
        #endregion

        #region Сохранение конфигурации интерфейса
        void BindingConfig(object obj, string path, DependencyProperty dp)
        {
            Binding bindObj = new Binding();
            bindObj.Source = Properties.Settings.Default;
            bindObj.Path = new PropertyPath(path);
            bindObj.Mode = BindingMode.TwoWay;
            if (obj is Slider)
                (obj as Slider).SetBinding(dp, bindObj);
            else if (obj is RadioButton)
                (obj as RadioButton).SetBinding(dp, bindObj);
            else if (obj is CheckBox)
                (obj as CheckBox).SetBinding(dp, bindObj);
            else if (obj is Xceed.Wpf.Toolkit.IntegerUpDown)
                (obj as Xceed.Wpf.Toolkit.IntegerUpDown).SetBinding(dp, bindObj);
            else if (obj is Xceed.Wpf.Toolkit.ColorPicker)
                (obj as Xceed.Wpf.Toolkit.ColorPicker).SetBinding(dp, bindObj);

            ////sldAllS.Value = Properties.Settings.Default.SApp;
            //Properties.Settings.Default.sldAllL = 8;
        }
        void SetBinding()
        {
            BindingConfig(sldAllL, sldAllL.Name, Slider.ValueProperty);
            BindingConfig(sldAllS, sldAllS.Name, Slider.ValueProperty);
            BindingConfig(sldAllC, sldAllC.Name, Slider.ValueProperty);
            BindingConfig(sldSigFr, sldSigFr.Name, Slider.ValueProperty);
            BindingConfig(sldSigLen, sldSigLen.Name, Slider.ValueProperty);
            BindingConfig(cpAllSensor, cpAllSensor.Name, Xceed.Wpf.Toolkit.ColorPicker.ShowAvailableColorsProperty);

            //BindingConfig(rbPollSensorOn, rbPollSensorOn.Name, RadioButton.IsCheckedProperty);
            //BindingConfig(checkBWiteDB, checkBWiteDB.Name, RadioButton.IsCheckedProperty);
            ////BindingConfig(IUpDown, IUpDown.Name, Xceed.Wpf.Toolkit.IntegerUpDown.ValueProperty);
            //BindingConfig(rbChangeUi, rbChangeUi.Name, RadioButton.IsCheckedProperty);

        }
        void LoadBindingConfig()
        {
            sldAllS.Value = Properties.Settings.Default.sldAllL;
        }
        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(cpAllSensor.SelectedColor + "");
            //Properties.Settings.Default.Save();
            DialogSaveFile();
        }
        private void btnOpenConfig_Click(object sender, RoutedEventArgs e)
        {
            DialogOpenFile();
        }
        void DialogSaveFile()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "(*.cfg)|*.cfg";

            if (saveFileDialog1.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile(), System.Text.Encoding.Default))
                {
                    GetValueAllElement(grdHousePlan);
                    Dictionary<string, string> listControls = GetValueAllElement(panel_options_fr);
                    foreach (var el in listControls)
                    {
                        sw.WriteLine(el.Key + Environment.NewLine + el.Value);
                    }
                    listControls.Clear();
                    sw.Close();
                }
            }
        }
        void DialogOpenFile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "(*.cfg)|*.cfg";

            if (openFileDialog1.ShowDialog() == true)
            {
                using (StreamReader sr = new StreamReader(openFileDialog1.OpenFile(), System.Text.Encoding.Default))
                {
                    string configUi = sr.ReadToEnd();
                    string[] arrParam = configUi.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < arrParam.Count(); i += 2)
                    {
                        listControls.Add(arrParam[i],arrParam[i+1]);
                    }
                    GetValueAllElement(panel_options_fr, "");
                    GetValueAllElement(grdHousePlan, "");
                    listControls.Clear();
                    sr.Close();
                }
            }
        }
        Dictionary<string, string> listControls = new Dictionary<string, string>();
        Dictionary<string, string> GetValueAllElement(FrameworkElement obj, string openData = null)
        {
            if (obj != null)
            {
             
                var listChildrenControl = LogicalTreeHelper.GetChildren(obj);
                foreach (var el in listChildrenControl)
                {
                    //режим сохранения
                    if (openData == null)
                    {
                   
                        if (el is Slider)
                        {
                            Slider sld = (el as Slider);
                            listControls.Add(sld.Name, sld.Value.ToString());
                        }
                        else if (el is Xceed.Wpf.Toolkit.ColorPicker)
                        {
                            Xceed.Wpf.Toolkit.ColorPicker cp = (el as Xceed.Wpf.Toolkit.ColorPicker);
                            listControls.Add(cp.Name, cp.SelectedColor.Value.ToString());
                        }
                        else if (el is Xceed.Wpf.Toolkit.IntegerUpDown)
                        {
                            Xceed.Wpf.Toolkit.IntegerUpDown iud = (el as Xceed.Wpf.Toolkit.IntegerUpDown);
                            listControls.Add(iud.Name, iud.Value.Value.ToString());
                        }
                        else if (el is RadioButton)
                        {
                            RadioButton rb = (el as RadioButton);
                            listControls.Add(rb.Name, rb.IsChecked.ToString());
                        }
                        else if (el is CheckBox)
                        {
                            CheckBox cb = (el as CheckBox);
                            listControls.Add(cb.Name, cb.IsChecked.ToString());
                        }
                        GetValueAllElement(el as FrameworkElement);
                    }
                    //режим чтения
                    else
                    {
                        if (listControls.Count > 0)
                            foreach (var elin in listControls)
                            {
                                //if ((el as FrameworkElement).Name != null && (el as FrameworkElement).Name == elin.Key)
                                //{
                                double doubleTmp;
                                bool boolTmp;
                                if (el is Slider && (el as Slider).Name == elin.Key && double.TryParse(elin.Value, out doubleTmp))

                                {
                                    (el as Slider).Value = Convert.ToDouble(elin.Value);   
                                }
                                else if (el is Xceed.Wpf.Toolkit.ColorPicker &&  (el as Xceed.Wpf.Toolkit.ColorPicker).Name == elin.Key && new Regex("^#......").IsMatch(elin.Value))
                                {
                                    (el as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor = (Color)ColorConverter.ConvertFromString(elin.Value); 
                                }
                                else if (el is Xceed.Wpf.Toolkit.IntegerUpDown && (el as Xceed.Wpf.Toolkit.IntegerUpDown).Name == elin.Key && double.TryParse(elin.Value, out doubleTmp))
                                {
                                    (el as Xceed.Wpf.Toolkit.IntegerUpDown).Value = Convert.ToInt32(elin.Value);
                                }
                                else if (el is RadioButton && (el as RadioButton).Name == elin.Key && bool.TryParse(elin.Value, out boolTmp))
                                {
                                    (el as RadioButton).IsChecked = Convert.ToBoolean(elin.Value);
                                }
                                else if (el is CheckBox && (el as CheckBox).Name == elin.Key && bool.TryParse(elin.Value, out boolTmp))
                                {
                                    (el as CheckBox).IsChecked = Convert.ToBoolean(elin.Value);
                                }
                                //}
                            }
                        GetValueAllElement(el as FrameworkElement, "");
                    }
                  
                }
                
            }
            return listControls;
        }

        #endregion

        private void tiUpdateTabDB_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(new DBDelegate(ShowTable));
        }
    }

}
