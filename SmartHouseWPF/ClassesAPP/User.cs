using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHouseWPF.Classes
{
    public class User
    {
        string login;
        string group;
        string pswd;

        public string Login
        { get { return login; } set { login = value; } }
        public string Group
        { get { return group; } set { group = value; } }
        public string Pswd
        { get { return pswd; } set { pswd = value; } }


        public User()
        {
            this.login = "";
            this.group = "";
            this.pswd = "";
        }
        public User(string login, string group)
        {
            this.login = login;
            this.group = group;
        }
        public User(string login, string group, string pswd)
        {
            this.login = login;
            this.group = group;
            this.pswd = pswd;
        }
    }
}
