using System;
using System.Collections.Generic;

namespace SmartHouseWPF
{
    public partial class UserSystem : UpClassDB
    {
        public UserSystem()
        {
            Action = new HashSet<Action>();
        }

        public int Id { get; set; }
        public int UserGroupId { get; set; }
        public string FullName { get; set; }
        public string Pswd { get; set; }
        public bool DelFlag { get; set; }

        public virtual ICollection<Action> Action { get; set; }
        public virtual UserGroup UserGroup { get; set; }
    }
}
