using System;
using System.Collections.Generic;

namespace SmartHouseWPF
{
    public partial class Statistic : UpClassDB
    {
        public long Id { get; set; }
        public int DeviceId { get; set; }
        public int RoomId { get; set; }
        public DateTime DateTime { get; set; }
        public string Value { get; set; }
        public bool Emergency { get; set; }
        public bool DelFlag { get; set; }

        public virtual Device Device { get; set; }
        public virtual Room Room { get; set; }
    }
}
