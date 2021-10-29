using System;
using System.Collections.Generic;

namespace SmartHouseWPF
{
    public partial class DeviceGroup : UpClassDB
    {
        public DeviceGroup()
        {
            Device = new HashSet<Device>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool DelFlag { get; set; }

        public virtual ICollection<Device> Device { get; set; }
    }
}
