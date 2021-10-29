using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data;
using System.Windows.Controls;

namespace SmartHouseWPF
{
    public class DataBaseConfig
    {
        NpgsqlDataAdapter dataAdapter; //объект позволяющий заполнить dataset
        NpgsqlConnection connection; //объект подключения к бд
        NpgsqlCommand command; //объект представляющий инструкцию, выполняемую над бд 
        NpgsqlTransaction transaction; //объект, представлющий транзакцию, которую необходимо выполнить 
        NpgsqlCommandBuilder commandBuilder; //объект для сборки запросов
        public NpgsqlDataAdapter DataAdapter
        {
            get { return dataAdapter; }
            set { dataAdapter = value; }
        }
        public NpgsqlConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }
        public NpgsqlCommand Command
        {
            get { return command; }
            set { command = value; }
        }
        public NpgsqlCommandBuilder CommandBuilder
        {
            get { return commandBuilder; }
            set { commandBuilder = value; }
        }
        public NpgsqlTransaction Transaction
        {
            get { return transaction; }
            set { transaction = value; }
        }

        public DataBaseConfig()
        {
            this.dataAdapter = new NpgsqlDataAdapter();
            this.connection = new NpgsqlConnection();
            this.command = new NpgsqlCommand();
        }
        public DataBaseConfig(NpgsqlDataAdapter dataAdapter)
        {
            this.dataAdapter = dataAdapter;
        }
        public DataBaseConfig(NpgsqlConnection connection)
        {
            this.connection = connection;
        }
        public DataBaseConfig(NpgsqlCommand command)
        {
            this.command = command;
        }
        public DataBaseConfig(NpgsqlDataAdapter dataAdapter, NpgsqlConnection connection, NpgsqlCommand command, NpgsqlTransaction transaction)
        {
            this.dataAdapter = dataAdapter;
            this.connection = connection;
            this.command = command;
            this.transaction = transaction;

        }

        public void SetConnection(string connectionStr)
        {
            this.connection = new NpgsqlConnection(connectionStr);
        } //метод для настройки подключения
        public void SetDataAdapter(string sqlStr, NpgsqlConnection connection)
        {
            this.dataAdapter = new NpgsqlDataAdapter(sqlStr, connection);
        } //метод для настройки DataAdapter через строку подключения и объект соединения
        public void SetDataAdapter(NpgsqlCommand cmd)
        {
            this.dataAdapter = new NpgsqlDataAdapter(cmd);
        } //метод для настройки DataAdapter через объект NpgsqlCommand
        public void SetCommand(string cmdText, NpgsqlConnection connection)
        {
            this.command = new NpgsqlCommand(cmdText, connection);
        } //метод настройки команды для выполнения
        public void SetCommandBuilder()
        {
            this.commandBuilder = new NpgsqlCommandBuilder(dataAdapter);

        } //метод настройки сборщика запроса



        public void LoadedTable(ref DataSet dataSet, ref DataTable dataTable, string sql, string connection, string nic = "table", int indexTable = 0, bool reset = true, DataBaseConfig dbc = null)
        {
            if (dbc != null)
                this.SetDataAdapter(dbc.Command);
            if (reset)
                dataSet.Reset();
            string str = nic + indexTable;

            this.SetConnection(connection);
            using (this.Connection)
            {
                this.Command = new Npgsql.NpgsqlCommand(sql);
                this.Command.Connection = this.Connection;
                this.SetDataAdapter(this.Command);

                this.DataAdapter.Fill(dataSet, str);
                dataTable = dataSet.Tables[indexTable];
            }
        }

        public void ShowLoadData(object objLoadData, DataTable dataTable, string cbxPath = "", string cbxDisplay = "")
        {
            if (objLoadData is DataGrid)
            {
                (objLoadData as DataGrid).ItemsSource = dataTable.DefaultView;
                //if ((objLoadData as DataGrid).Columns.Count > 0 && (objLoadData as DataGrid).Columns[0].Header.ToString() == "id")
                //    (objLoadData as DataGrid).Columns[0].Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (objLoadData is ComboBox)
            {
                ComboBox cbx = (objLoadData as ComboBox);
                cbx.ItemsSource = dataTable.DefaultView;
                if (cbx.Items.Count > 0)
                {
                    cbx.SelectedValuePath = cbxPath;
                    cbx.DisplayMemberPath = cbxDisplay;
                    cbx.SelectedIndex = 0;
                }
              
            }
        }



        public object GoInquiry(string sqlStr, string typeSql, object[] arrParam = null)
        {
            //int result = -1;

            //dbc.Connection = dataBaseConfig.Connection;
            this.Command = new Npgsql.NpgsqlCommand(sqlStr, this.Connection);
            if (arrParam.Count() > 0) //условие того, что вызыватся хранимая процедура с параметрами
            {
                for (int i = 0; i < arrParam.Count(); i = i + 3)
                    this.Command.Parameters.Add((arrParam[i] as string), ((NpgsqlTypes.NpgsqlDbType)arrParam[i + 1])).Value = arrParam[i + 2]; //добавление параметров: названия, типа, значения
            }

            if (typeSql == "IUD")
                return this.Command.ExecuteNonQuery();                 //INSERT UPDATE DELETE, возвращает количесто измененых записей
            if (typeSql == "SCAL")
                return this.Command.ExecuteScalar();  //MIN MAX COUNT SUM, возвращает одно скалярное выражение в том числе и строковое
            if (typeSql == "S")
                if (this.Command.ExecuteReader().HasRows)
                    return 1;           //SELECT, возвращает строки из таблицы
            return 0;
        } //метод, реализующий выполнение запроса, typeSql: 0 - INSERT UPDATE DELETE, 1 - MIN MAX COUNT SUM, 2 - SELECT



        //public void RequestInsert()
        //{
        //    GoInquiry(CallStoredProcedureDataBase("ins_payment", new object[] {
        //                "f_id_client", NpgsqlTypes.NpgsqlDbType.Integer, textBoxIdClientPayment.Text,
        //                "f_id_subscription", NpgsqlTypes.NpgsqlDbType.Integer, comboBoxSubscriptionPayment.SelectedValue,
        //                "f_id_payment_options", NpgsqlTypes.NpgsqlDbType.Integer, comboBoxPaymentOptionsPayment.SelectedValue,
        //                "f_id_lesson", NpgsqlTypes.NpgsqlDbType.Integer, idLesson}, 1), 0);
        //}

        //public void RequestSelect()
        //{

        //}
        //public void RequestUpdate()
        //{

        //}
        //public void RequestDelete()
        //{

        //}



    }
}
