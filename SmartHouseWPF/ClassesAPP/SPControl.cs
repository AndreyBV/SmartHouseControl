using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Threading;

namespace SmartHouseWPF
{
    public class SPControl : SerialPort
    {
        //объект порта
        //SerialPort serialPort;

        //настройки порта
        string portName;
        int dataBits;
        Parity parity;
        StopBits stopBit;
        int baudRate;
        Handshake handshake;
        int readTimeout;
        int writeTimeout;
        Paragraph paragraph = new Paragraph();
        FlowDocument mcFlowDoc = new FlowDocument();

        //свойство открыт ли порт
        //public bool IsOpen { get; private set; }
        //выходные данные
        public int Display { get; private set; }
        //строка ошибки
        public string ErrorString { get; private set; }


        public SPControl() : base()
        {
            new SerialPort();
        }
        public SPControl (string portName, int baundRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, int readTimeout, int writeTimeout) : base(portName, baundRate, parity, dataBits, stopBits)
        {
            //this.handshake = handshake;
            //this.readTimeout = readTimeout;
            //this.writeTimeout = writeTimeout;
            new SerialPort(portName, baundRate, parity, dataBits, stopBits);
            this.Handshake = handshake;
            this.WriteTimeout = writeTimeout;
            this.ReadTimeout = readTimeout;
        }



        //// делегат
        //public delegate void DataRecive(object sender);
        //// Событие, возникающее при обновлении данных
        //public event DataRecive NewDataRecieve;

        public void OpenConnection()
        {
            ErrorString = string.Empty;
            if (!this.IsOpen)
            {
                try
                {
                    this.Open();
                }
                catch (Exception ex)
                {
                    ErrorString = ex.Message;
                    return;
                }
            }
        }
        public void CloseConnection()
        {
            ErrorString = string.Empty;
            Thread.Sleep(500);
            if (this.IsOpen)
            {
                try
                {
                    this.Close();
                }
                catch (Exception ex)
                {
                    ErrorString = ex.Message;
                    return;
                }
            }
        }

     


        public void SendData(string strCommand)
        {
            try
            {
                if (this.IsOpen)
                {
                    string command = strCommand + Environment.NewLine/*.Replace(Environment.NewLine, "")*/;
                    this.Write(command);
                }
                else
                {
                    throw new Exception("Не установлено сединение с COM портом!");
                }
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return;
            }
        }
        public string GetData()
        {
            string serialPortData = "";
            try
            {
                if (this.IsOpen)
                {
                    serialPortData = this.ReadExisting();
                }
                else
                {
                    throw new Exception("Не установлено сединение с COM портом!");
                }
            }
            catch (Exception ex)
            {
                ErrorString = ex.Message;
                return "";
            }
            return serialPortData;
        }















        //private void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        //{
        //    //при ошибки переполнения буфера очищаем
        //    if (e.EventType == SerialError.TXFull)
        //        this.serialPort.DiscardOutBuffer();
        //    else
        //        this.serialPort.DiscardInBuffer();
        //}

        //private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    if (serialPort.IsOpen == false) return;
        //    try
        //    {
        //        /*
        //        некоторая логика по обработке пакета
        //        в итоге присваиваем полю Display значение
        //        

        //        int tryI;
        //        if (int.TryParse(result, out tryI))
        //        {
        //            //новое событие
        //            Display = tryI;
        //            if (NewDataRecieve != null)
        //                NewDataRecieve.Invoke(this);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Err = ex.Message;
        //    }
        //}



    }
}
