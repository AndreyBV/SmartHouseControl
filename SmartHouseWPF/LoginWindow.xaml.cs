using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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



namespace SmartHouseWPF
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            try
            {
                //throw new System.Net.Sockets.SocketException();
                CreateConfigFileDB();
                SetConnection();

                GetListGroup();
                tbLogin.Text = "Иванов Иван Ивановuч";
                tbPassword.Password = "z";
            }
            catch (Exception ex)
            {
                //если нет возможности подключения к базе данных
                if (ex is System.Net.Sockets.SocketException)
                {
                    grdFieldDataUser.Visibility = Visibility.Collapsed;
                    this.Height = 90;
                }
                else
                    MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }
        //установка подключения из внешнего файла
        void SetConnection()
        {
            //try
            //{

            //}
            //catch ()
            //{

            //}
            var builder = new ConfigurationBuilder();
            // установка пути к текущему каталогу
            builder.SetBasePath(Directory.GetCurrentDirectory());
            // получаем конфигурацию из файла appsettings.json
            builder.AddJsonFile("SettingConnection.json");
            // создаем конфигурацию
            var config = builder.Build();
            // получаем строку подключения
            string connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<smart_houseContext>();
            var options = optionsBuilder
                .UseNpgsql(connectionString)
                .Options;
            //получение строки подключения
            connectionString = config.GetConnectionString("DefaultConnection");
        }

        static DataBaseConfig dataBaseConfig = new DataBaseConfig();    //объект подключеня к базе данных
        BaseDBClass objBaseClass = new BaseDBClass(dataBaseConfig);
        DataSet dataSet = new DataSet();                                //объект для хранения таблиц полученных из базы данных
        DataTable dataTable = new DataTable();                          //объект - таблица, хранящая данные из базы 
        string connectionString = "";
        //bool npgsqlDll = true;

        //создание файла подключения к БД если его нет
        void CreateConfigFileDB()
        {
            if (!System.IO.File.Exists("SettingConnection.json"))
            {
                string jsonConnection = @"{ ""ConnectionStrings"": { ""DefaultConnection"": 
                ""Host=localhost;Port=5432;Database=smart_house;Username=postgres;Password=22"" } }";
                System.IO.File.WriteAllText("SettingConnection.json", jsonConnection);
            }
        }
        ////формирование строки подключения к БД
        //void GetConnectionStringDB()
        //{
        //    connectionString = System.IO.File.ReadAllLines("ConfigDB.cfg")[0];
        //}
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            if (grdFieldDataUser.Visibility != Visibility.Collapsed)
            {
                UserSystem us = GetUser();
                if (us != null)
                    us.UserGroup = GetUserGroup(us.UserGroupId);
                if ((us == null || us != null && us.UserGroup != null && us.UserGroup.Name != "Администратор") && (cbxUserGroup.SelectedItem as UserGroup).Name == "Гость")
                {
                    OpenBaseWindowGuestes();
                    return;
                }
                if (us != null && (us.UserGroup.Name == "Администратор" || us.UserGroupId == Convert.ToInt32(cbxUserGroup.SelectedValue)))
                {

                    MainWindow mw = new MainWindow();
                    //mw.UserSystem = new Classes.User(tbLogin.Text, group);
                    mw.ConnectionString = connectionString;
                    //mw.ObjBaseClass = objBaseClass;

                  
                    mw.UserCon = us;
                    mw.BuildUI();

                    mw.Show();
                    this.Close();
                }
                else
                {
                    tbPassword.Clear();
                    MessageBox.Show("Данные пользователя не корректны!");
                }
            }
            else
            {
                OpenBaseWindowGuestes();
            }

          

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Data["Detail"].ToString());
            //}
        }

        void OpenBaseWindowGuestes()
        {
            MainWindow mw = new MainWindow();
            UserSystem us = new UserSystem();
            us.UserGroup = new UserGroup();
            us.UserGroup.Name = "Гость";
            mw.UserCon = us;
            mw.BuildUI();
            mw.Show();
            this.Close();
        }

        //событие регистрации пользователя в сисетме
        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {
            Registration();
        }
        //регистрация
        void Registration()
        {
            if ((cbxUserGroup.SelectedItem as UserGroup).Name != "Гость")
            {
                if (FindUser() == 0)
                {
                    using (smart_houseContext con = new smart_houseContext())
                    {
                        UserSystem us = new UserSystem();
                        us.FullName = tbLogin.Text;
                        us.UserGroupId = Convert.ToInt32(cbxUserGroup.SelectedValue);
                        us.Pswd = GetHashString(tbPassword.Password);
                        var objUser = con.UserSystem.Add(us);
                        con.SaveChanges();
                        MessageBox.Show("Регистрация прошла успешно!\r\nМожете войти в систему ");
                    }
                }
                else
                    MessageBox.Show("Данный логин занят!");
            }
            else
                MessageBox.Show("Регистрация для группы \"Гость\" не требуется!");
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
        //проверка наличия логина
        int FindUser(bool pswd = false)
        {
            using (smart_houseContext con = new smart_houseContext())
            {
                var objUser = con.UserSystem.Where(c => c.FullName == tbLogin.Text && !c.DelFlag);
                if (objUser.Count() > 0)
                    return 1;
                else
                    return 0;
            }


            //    DataBaseConfig dbc = new DataBaseConfig();
            //dbc.SetConnection(connectionString);
            //using (dbc.Connection)
            //{
            //    dbc.Connection.Open();
            //    if (!pswd)
            //    {
            //        string sqlStr = "SELECT id FROM user_system WHERE full_name = @full_name";
            //        return (int)dbc.GoInquiry(sqlStr, "S", new object[] { "full_name", NpgsqlTypes.NpgsqlDbType.Varchar, tbLogin.Text });
            //    }
            //    else
            //    {
            //        string sqlStr = "SELECT id FROM user_system WHERE full_name = @full_name AND pswd = @pswd";
            //        return (int)dbc.GoInquiry(sqlStr, "S", new object[] {
            //                "full_name", NpgsqlTypes.NpgsqlDbType.Varchar, tbLogin.Text,
            //                "pswd", NpgsqlTypes.NpgsqlDbType.Varchar, tbPassword.Password});

            //    }

            //}
        }
        UserSystem GetUser()
        {
            using (smart_houseContext con = new smart_houseContext())
            {
                //con.UserGroup.Load();
                //IQueryable<string> objGroup = from us in con.UserSystem
                //               join ug in con.UserGroup on us.IdUserGroup equals ug.Id
                //               where us.FullName == tbLogin.Text && us.Pswd == tbPassword.Password
                //               select ug.Name;
                UserSystem us = con.UserSystem.FirstOrDefault(obj => obj.FullName == tbLogin.Text && obj.Pswd == GetHashString(tbPassword.Password) && !obj.DelFlag);
                return us;
                //if (objGroup.Count() == 1)
                //    return objGroup.Take(0).ToString();


                //con.UserSystem.Join(con.UserGroup,
                //o => o.IdUserGroup,
                //t => t.Id,
                //(o, t) => new
                //{
                //    fullName = o.FullName,
                //    pswd = o.Pswd,
                //    group = t.Name
                //})
                //.Where(f => f.fullNameFullName == tbLogin.Text && c.Pswd == tbPassword.Password);

                //cbxUserGroup.ItemsSource = con.UserGroup.Local.ToBindingList();
                //if (cbxUserGroup.Items.Count > 0)
                //{
                //    cbxUserGroup.SelectedValuePath = "Id";
                //    cbxUserGroup.DisplayMemberPath = "Name";
                //    cbxUserGroup.SelectedIndex = 0;
                //}
            }


            //DataBaseConfig dbc = new DataBaseConfig();
            //dbc.SetConnection(connectionString);
            //using (dbc.Connection)
            //{
            //    dbc.Connection.Open();
            //    string sqlStr = "SELECT user_group.name FROM user_system INNER JOIN user_group ON user_group.id = user_system.id_user_group WHERE full_name = @full_name AND pswd = @pswd";
            //    return Convert.ToString(dbc.GoInquiry(sqlStr, "SCAL", new object[] {
            //            "full_name", NpgsqlTypes.NpgsqlDbType.Varchar, tbLogin.Text,
            //            "pswd", NpgsqlTypes.NpgsqlDbType.Varchar, tbPassword.Password}));

            //}
        }
        UserGroup GetUserGroup(int id)
        {
            using (smart_houseContext con = new smart_houseContext())
            {
                UserGroup ug = con.UserGroup.Find(id);
                return ug;
            }
        }
        //получение списка групп
        void GetListGroup()
        {
            using (smart_houseContext con = new smart_houseContext())
            {
                con.UserGroup.Load();
                UserGroup ugGuest = new UserGroup();
                ugGuest.Name = "Гость";
                ugGuest.Id = -1;
                //con.UserGroup.Add(ugGuest);
                //con.SaveChanges();

                var listGroup = con.UserGroup.Where(n => n.Name != "Администратор" && !n.DelFalg);
                cbxUserGroup.Items.Add(ugGuest);
                if (listGroup.Count() > 0)
                foreach (UserGroup el in listGroup)
                    cbxUserGroup.Items.Add(el);

                //cbxUserGroup.ItemsSource = listGroup;
                //cbxUserGroup.ItemsSource = con.UserGroup.Local.ToBindingList();
                if (cbxUserGroup.Items.Count > 0)
                {
                    cbxUserGroup.SelectedValuePath = "Id";
                    cbxUserGroup.DisplayMemberPath = "Name";
                    cbxUserGroup.SelectedIndex = 0;
                }

            }
        }
    }
}
