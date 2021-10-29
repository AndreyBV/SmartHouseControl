using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace SmartHouseWPF
{
    class ControleOjbectsHouse
    {
        //метод изменения цвета кнопок
        public static SolidColorBrush ToBrush(string HexColorString)
        {
            return (SolidColorBrush)(new BrushConverter().ConvertFrom(HexColorString));
        }




        #region Исполнительные устройства
        //освещение
        public void LightControl(SPControl serialPort, object sender, string numberRoom, int value)
        {
            string sendValue = value.ToString("0");
            if (value == 10)
                sendValue = "q";      
            serialPort.SendData("XL" + numberRoom + sendValue);
        }
        //освещение - плавно
        public void SmoothLightControl(SPControl serialPort, object sender, string numberRoom, int value)
        {
            string sendValue = value.ToString("0");
            if (value == 10)
                sendValue = "q";
            serialPort.SendData("XS" + numberRoom + sendValue);
        }
        //освещение фоновое
        public void BackgroundLightControl(SPControl serialPort, object sender, string numberRoom, int R, int G, int B)
        {
            string sendR = R.ToString("0");
            string sendG = G.ToString("0");
            string sendB = B.ToString("0");
            if (R == 10)
                sendR = "q";
            if (G == 10)
                sendG = "q";
            if (B == 10)
                sendB = "q";
            serialPort.SendData("XB" + numberRoom + sendR + sendG + sendB);
        }
        //дверь
        public void DoorControl(SPControl serialPort, object sender, string numberRoom, int value)
        {
            string sendValue = value.ToString("0");
            if (value == 10)
                sendValue = "q";
            serialPort.SendData("XD" + numberRoom + sendValue);
        }
        //сигнализация
        public void SignalingControl(SPControl serialPort, object sender, string frequency, int value)
        {
            string sendValue = value.ToString("0");
            if (value == 10)
                sendValue = "q";
            serialPort.SendData("XA" + frequency + sendValue);
        }
        //отопление
        public void HeatingCooling(SPControl serialPort, object sender, string numberRoom, int value)
        {
            //string nameBtn = (sender as Slider).Name;
            //string roomNum = nameBtn[nameBtn.Length - 1].ToString();
            string sendValue = value.ToString("0");
            if (value == 10 || value == -10)
                sendValue = "q";
            if (value > 0)
            {
                string command = "XH" + numberRoom + sendValue;
                serialPort.SendData(command);
            }
            else if (value < 0)
            {
                string command = "";
                if (sendValue != "q")
                    command = "XC" + numberRoom + Math.Abs(Convert.ToInt32(sendValue));
                else
                    command = "XC" + numberRoom + sendValue;
                serialPort.SendData(command);
            }
            else
            {
                string command1 = "XH" + numberRoom + sendValue;
                string command2 = "XC" + numberRoom + sendValue;
                serialPort.SendData(command1);
                serialPort.SendData(command2);
            }

        }
        public void SetDateTimeArudino(SPControl serialPort, DateTime dateTime)
        {
            char pp = dateTime.Year.ToString("00")[0];
            serialPort.SendData("ZR" + dateTime.Year.ToString("0000")[2] + dateTime.Year.ToString("0000")[3]);
            serialPort.SendData("ZN" + dateTime.Month.ToString("00")[0] + dateTime.Month.ToString("00")[1]);
            serialPort.SendData("ZD" + dateTime.Day.ToString("00")[0] + dateTime.Day.ToString("00")[1]);
            serialPort.SendData("ZH" + dateTime.Hour.ToString("00")[0] + dateTime.Hour.ToString("00")[1]);
            serialPort.SendData("ZM" + dateTime.Minute.ToString("00")[0] + dateTime.Minute.ToString("00")[1]);
            serialPort.SendData("ZS" + dateTime.Second.ToString("00")[0] + dateTime.Second.ToString("00")[1]);
        }

        #endregion


        #region Дата и время - натройка
        public void ClockControl(SPControl serialPort, object sender, string type, string unit, int tens, int units)
        {
            //Возможные значения type :
            //D - дата
            //T - время
            //Возможные значения unit:
            //Для времени:
            //H - часы 
            //M - минуты
            //S - секунды
            //Для даты:
            //Y - год
            //M - месяц
            //D - день
            //W - день недели
            serialPort.SendData("Z" + type + unit + tens + units /*+ numberDevice*/); //+ "\r\n"
        }
        #endregion

        #region Датчики
        public void SensorPolling(SPControl serialPort,/* object sender,*/ string type, int numberRoom/*, int numberDevice*/)
        {
            //Возможные значения type:
            //L - освещения
            //M - движения
            //T - температуры
            //V - вибрации
            //H - влажности
            //P - атм.давления
            //W - протечки воды
            //G - утечки газа
            //D - открытия двери
            //C - тока
            serialPort.SendData("Y" + type + numberRoom /*+ numberDevice*/); //+ "\r\n"
            //string serialPortData = serialPort.ReadExisting();
            ////string serialPortData = serialPort.ReadLine();
            //return serialPortData;
        }
        #endregion



        #region Дата и время - запрос состояния
        public void ClockPolling(SPControl serialPort, object sender, string type, int value/*, int numberDevice*/)
        {
            //Возможные значения type:
            //R - дата и время
            //Возможные значения value:
            //1 - часы 
            //2 - минуты
            //3 - секунды
            //4 - год
            //5 - месяц
            //6 - день
            //7 - день недели
            //8 - дата
            //9 - время
            //10 - дата и время
            serialPort.SendData("Z" + type + value /*+ numberDevice*/); //+ "\r\n"
        }
        #endregion




        //public void DataReceivedParsing(string data, char separator)
        //{
        //    string[] valuesAnswer = data.Split(separator);
        //    foreach (var el in valuesAnswer)
        //    {
        //       string[]
        //    }
        //}

        //public void tmp(string[] arrParam)
        //{
        //    Control
        //    string type = arrParam[0].Substring(0, 1);
        //    string numRoom = arrParam[0].Substring(1, 1);
        //    string param1 = arrParam[1];
        //    string param2 = arrParam[2];
        //    string param3 = arrParam[3];
        //}


        //public void LightSensor(SPControl serialPort, object sender, string type, string numberRoom, int numberDevice)
        //{
        //    serialPort.SendData(type + numberRoom + numberDevice); //+ "\r\n"
        //}
        //public void MotionSensor(SPControl serialPort, object sender, string value)
        //{

        //}
        //public void TemperatureSensor(SPControl serialPort, object sender, string value)
        //{

        //}
        //public void HumiditySensor(SPControl serialPort, object sender, string value)
        //{

        //}
        //public void AtmosphericPressureSensor(SPControl serialPort, object sender, string value)
        //{

        //}
        //public void WaterLeakageSensor(SPControl serialPort, object sender, string value)
        //{

        //}
        //public void GasLeakageSensor(SPControl serialPort, object sender, string value)
        //{

        //}
        //public void DoorSensor(SPControl serialPort, object sender, string value)
        //{

        //}
        //public void CurrentSensor(SPControl serialPort, object sender, string value)
        //{

        //}
        //public void VibrationSensor(SPControl serialPort, object sender, string value)
        //{

        //}


        #region Дата и время
        public void DateTime(SPControl serialPort, object sender, string value)
        {

        }
        #endregion
    }
}
