using ScottPlot;

namespace VGC50x.Plot
{
    public class ReadingsModel
    {
        public ReadingsModel()
        { }

        public static void TestReadings(WpfPlot plot)
        {
            double[] dataX = new double[] { 1, 2, 3, 4, 5 };
            double[] dataY = new double[] { 1, 4, 9, 16, 25 };
            plot.Plot.AddScatter(dataX, dataY);
            plot.Refresh();
        }
    }
}