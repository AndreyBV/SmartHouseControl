using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHouseWPF
{
    public class UpClassDB
    {
        //public virtual int Id { get; set; }
        //public virtual bool DelFalg { get; set; }
        //public virtual bool DelFlag { get; set; }
        //Поиск обекта в БД
        public UpClassDB FindObjDB(string name, string type = "Device")
        {
            if (type == "Room")
            {
                using (smart_houseContext con = new smart_houseContext())
                {
                    Room obj = con.Room
                                        .FirstOrDefault(c => c.SerialNumber == Convert.ToInt32(name));
                    return obj;
                }
            }
            else if (type == "Device")
            {
                using (smart_houseContext con = new smart_houseContext())
                {
                    Device obj = con.Device
                                        .FirstOrDefault(c => c.Name == name);
                    return obj;
                }
            }
            return null;
        }

       public string GetFullNameDevice(string shortName, string type = "Sensor")
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
    }
}
