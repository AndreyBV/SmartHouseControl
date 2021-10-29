using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHouseWPF
{
    public class BaseDBClass
    {
        string user;

        DataBaseConfig dataBaseConfig;

        public DataBaseConfig DataBaseConfigObj
        {
            get { return dataBaseConfig; }
            set { dataBaseConfig = value; }
        }

        public void BuildAuditAct(string obj)
        {
            //dataBaseConfig.Command = new Npgsql.NpgsqlCommand(
            //  @"INSERT INTO audit_action(date_action,user_name,obj) 
            //    VALUES(now(), @user, @obj)");
            //dataBaseConfig.Command.Parameters.AddWithValue("@user", dataBaseConfig.Connection.UserName);
            //dataBaseConfig.Command.Parameters.AddWithValue("@obj", obj);
            //dataBaseConfig.Command.Connection = dataBaseConfig.Connection;
            //dataBaseConfig.Command.ExecuteNonQuery();
        }

        public BaseDBClass(DataBaseConfig dataBaseConfig)
        {
            this.dataBaseConfig = dataBaseConfig;
        }
    }
}