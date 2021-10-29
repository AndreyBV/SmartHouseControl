using System;
using System.Collections.Generic;

namespace SmartHouseWPF
{
    public partial class UserGroup : UpClassDB
    {
        public UserGroup()
        {
            UserSystem = new HashSet<UserSystem>();
        }

        public int Id { get; set; }

        public string Name { get; set; }
        public bool DelFalg { get; set; }

        public virtual ICollection<UserSystem> UserSystem { get; set; }
    }
}
