using System;
using System.Collections.Generic;

namespace SmartHouseWPF
{
    public partial class Device : UpClassDB
    {
        public Device()
        {
            Action = new HashSet<Action>();
            Statistic = new HashSet<Statistic>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int DeviceGroupId { get; set; }
        public bool DelFlag { get; set; }

        public virtual ICollection<Action> Action { get; set; }
        public virtual ICollection<Statistic> Statistic { get; set; }
        public virtual DeviceGroup DeviceGroup { get; set; }
    }
}
