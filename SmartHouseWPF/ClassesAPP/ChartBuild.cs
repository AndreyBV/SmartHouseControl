using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace SmartHouseWPF
{
    class ChartBuild
    {

        //public Dictionary<string, List<double>> point = new Dictionary<string, List<double>>();

        //public Dictionary<double, double> point_gr = new Dictionary<double, double>();
        //public double minAxis;
        //public double maxAxis;
        //public string param;

        //System.Drawing.Point pt = new System.Drawing.Point(0, 0);
        //int countElInRow = 0;


        //public void DrawGraph()
        //{
        //    ZedGraphControl zedGraph = new ZedGraphControl();
        //    zedGraph.Height = 500;
        //    zedGraph.Width = 500;
        //    zedGraph.Location = pt;
        //    //Controls.Add(zedGraph);
        //    // Получим панель для рисования
        //    GraphPane pane = zedGraph.GraphPane;
        //    pane.Title.FontSpec.Size = 18;
        //    pane.Title.Text = "Параметр: " + param;
        //    pane.XAxis.MajorGrid.IsVisible = true;
        //    pane.YAxis.MajorGrid.IsVisible = true;
        //    // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
        //    // pane.CurveList.Clear();

        //    // Создадим список точек
        //    PointPairList list = new PointPairList();

        //    // Заполняем список точек


        //    foreach (var el in point_gr)
        //    {
        //        double x = 0;
        //        double y = 0;
        //        x = el.Key;
        //        y = el.Value;

        //        list.Add(x, y);
        //    }
        //    LineItem myCurve = pane.AddCurve("Sinc", list, Color.Blue, SymbolType.None);






        //    // Устанавливаем интересующий нас интервал по оси X
        //    AxisLabel al = new AxisLabel("", "Microsoft Sans Serif", 18, Color.Black, false, false, false);
        //    pane.XAxis.Title = al;
        //    pane.XAxis.Scale.Min = -100;
        //    pane.XAxis.Scale.Max = maxAxis;

        //    // Устанавливаем интересующий нас интервал по оси Y
        //    al = new AxisLabel("", "Microsoft Sans Serif", 18, Color.Black, false, false, false);
        //    pane.YAxis.Title = al;
        //    pane.YAxis.Scale.Min = 0;
        //    pane.YAxis.Scale.Max = maxAxis + 500;

        //    // Вызываем метод AxisChange (), чтобы обновить данные об осях. 
        //    // В противном случае на рисунке будет показана только часть графика, 
        //    // которая умещается в интервалы по осям, установленные по умолчанию
        //    zedGraph.AxisChange();

        //    // Обновляем график
        //    zedGraph.Invalidate();


        //}
    }
}
