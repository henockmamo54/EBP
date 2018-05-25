using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ERROR_BACK_PROPAGATION
{
    public partial class Form1 : Form
    {
        int numberOfLayer = 3;
        int[] nodesPerLayer;
        double[] weightValues;
        double[] svalues;
        double[] uvalues;
        double[] deltavalues;
        int totalNodeCount = 0;
        double learningRate = 0.01;
        List<double[]> data;
        bool withbias = false;

        List<int> _layers = new List<int>();
        List<double> error = new List<double>();
        Random randomNumGenerator = new Random();
        Dictionary<string, double> _svalues = new Dictionary<string, double>();
        Dictionary<string, double> _uvalues = new Dictionary<string, double>();
        Dictionary<string, double> _dvalues = new Dictionary<string, double>();
        Dictionary<string, double> _weightvalues = new Dictionary<string, double>();
        public Form1()
        {
            InitializeComponent();

            _layers.Add(2);
            _layers.Add(2);
            _layers.Add(1);

            //intialize the network
            intializeTheNetwork();

            //read data
            data = readFile();

            //draw the dataset
            drawDataPoints();

            for (int iter = 0; iter < 3000; iter++)
            {
                Shuffle(data);
                int errorcount = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    double[] t = { data[i][0], data[i][1] };
                    var output = computeTheOutput(t);
                    computeDeltaValues(output, data[i][2]);
                    updateWeights(t);

                    errorcount += (int)Math.Abs(data[i][2] - (Math.Abs(output) >= 0.5 ? 1 : 0));
                }
                error.Add(errorcount);
            }

            drawErrorChart();

            previousCode();

            using (StreamWriter sw = File.CreateText("list.csv"))
            {
                for (int i = 0; i < error.Count; i++)
                {
                    sw.WriteLine(error[i]);
                }
            }



        }

        private void drawErrorChart()
        {
            
            chart_error.Series["Series1"].ChartType = SeriesChartType.Line;

            List<double> x_col_c1 = new List<double>();
            List<double> y_col_c1 = new List<double>();

            for (int i = 0; i < error.Count; i++)
            {
                    y_col_c1.Add(error[i]);
                    x_col_c1.Add(i);                
            }

            chart_error.Series["Series1"].Points.DataBindXY(x_col_c1, y_col_c1);
            chart_error.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart_error.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            Refresh();
        }

        public void intializeTheNetwork()
        {

            //assign random values for the weight values
            for (int i = 0; i < _layers.Count - 1; i++)
            {
                for (int source = 1; source < _layers[i] + 1; source++)
                {
                    for (int destination = 1; destination < _layers[i + 1] + 1; destination++)
                    {
                        double rand = randomNumGenerator.Next(-10, 10);
                        _weightvalues.Add(i + 1 + "|" + destination + "" + source, rand / 10);
                        //_weightvalues.Add(i + 1 + "|" + destination + "" + source, 1);
                    }
                }
            }

            //intialize u and s values
            for (int i = 0; i < _layers.Count; i++)
            {
                for (int j = 1; j < _layers[i] + 1; j++)
                {
                    _svalues.Add(i + 1 + "" + j, 0);
                    _uvalues.Add(i + 1 + "" + j, 0);
                    _dvalues.Add(i + 1 + "" + j, 0);
                }
            }

        }

        public double computeTheOutput(double[] x1)
        {
            int sum = 0;
            for (int i = 1; i < _layers.Count; i++)
            {
                for (int j = 1; j < _layers[i] + 1; j++)
                {
                    if (i == 1)
                    {
                        double c = 0;
                        for (int k = 1; k < _layers[i - 1] + 1; k++)
                        {
                            c += _weightvalues[i + "|" + j + "" + k] * x1[k - 1];
                        }
                        _svalues[i + 1 + "" + j] = c + (withbias ? 1 : 0);
                        _uvalues[i + 1 + "" + j] = calculateSigmoid(c);
                    }
                    else
                    {
                        double c = 0;
                        for (int k = 1; k < _layers[i - 1] + 1; k++)
                        {
                            c += _weightvalues[i + "|" + j + "" + k] * _uvalues[i + "" + k];
                        }
                        _svalues[i + 1 + "" + j] = c + (withbias ? 1 : 0);
                        _uvalues[i + 1 + "" + j] = calculateSigmoid(c);
                    }
                }
            }
            return _uvalues.Last().Value;
        }

        public void computeDeltaValues(double output, double target)
        {
            for (int i = _layers.Count; i > 1; i--)
            {
                for (int j = 1; j < _layers[i - 1] + 1; j++)
                    if (i == _layers.Count) _dvalues[i + "" + j] = (target - output) * (1 - output) * output;
                    else
                    {
                        //_dvalues[i + "" + j]
                        double c = _uvalues[i + "" + j] * (1 - _uvalues[i + "" + j]);
                        double d = 0;
                        for (int k = 1; k < _layers[i] + 1; k++)
                        {
                            d += _weightvalues[i + "|" + k + "" + j] * _dvalues[i + 1 + "" + k];
                        }
                        _dvalues[i + "" + j] = c * d;
                    }
            }
        }

        public void updateWeights(double[] x)
        {
            for (int i = _layers.Count - 2; i >= 0; i--)
            {
                for (int source = 1; source < _layers[i] + 1; source++)
                {
                    for (int destination = 1; destination < _layers[i + 1] + 1; destination++)
                    {
                        if (i == 0)
                        {
                            double newWeight = _weightvalues[i + 1 + "|" + destination + "" + source] + learningRate * _dvalues[(i + 2) + "" + destination] * x[source - 1];
                            _weightvalues[(i + 1) + "|" + destination + "" + source] = newWeight;
                        }
                        else
                        {
                            double newWeight = _weightvalues[i + 1 + "|" + destination + "" + source] + learningRate * _dvalues[(i + 2) + "" + destination] * _uvalues[(i + 1) + "" + source];
                            _weightvalues[(i + 1) + "|" + destination + "" + source] = newWeight;
                        }
                    }
                }

                //weightValues[5] = weightValues[5] + learningRate * deltavalues[5] * uvalues[4];
                //weightValues[4] = weightValues[4] + learningRate * deltavalues[4] * uvalues[3];

                //weightValues[0] = weightValues[0] + learningRate * deltavalues[3] * x1;
                //weightValues[1] = weightValues[1] + learningRate * deltavalues[4] * x1;
                //weightValues[2] = weightValues[2] + learningRate * deltavalues[3] * x2;
                //weightValues[3] = weightValues[3] + learningRate * deltavalues[4] * x2;
            }
        }

        public void drawDataPoints()
        {
            chart1.Series.Add("Series2");
            chart1.Series.Add("Series3");
            chart1.Series["Series1"].ChartType = SeriesChartType.Point;
            chart1.Series["Series2"].ChartType = SeriesChartType.Point;
            chart1.Series["Series3"].ChartType = SeriesChartType.Point;

            List<double> x_col_c1 = new List<double>();
            List<double> y_col_c1 = new List<double>();
            List<double> x_col_c2 = new List<double>();
            List<double> y_col_c2 = new List<double>();

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i][2] == 0)
                {
                    x_col_c1.Add(data[i][0]);
                    y_col_c1.Add(data[i][1]);
                }
                else if (data[i][2] == 1)
                {
                    x_col_c2.Add(data[i][0]);
                    y_col_c2.Add(data[i][1]);
                }
            }

            chart1.Series["Series1"].Points.DataBindXY(x_col_c1, y_col_c1);
            chart1.Series["Series2"].Points.DataBindXY(x_col_c2, y_col_c2);
            Refresh();
        }

        public void previousCode()
        {

            // asigne number of nodes per layer
            nodesPerLayer = new int[numberOfLayer];
            nodesPerLayer[0] = 2;
            nodesPerLayer[1] = 2;
            nodesPerLayer[2] = 1;

            foreach (int i in nodesPerLayer) totalNodeCount += i;
            deltavalues = new double[totalNodeCount + 1];
            svalues = new double[totalNodeCount + 1];
            uvalues = new double[totalNodeCount + 1];

            // calculate the number of weights and assigne an array to hold the weights
            int weightsCount = 0;
            for (int i = 0; i < nodesPerLayer.Length - 1; i++)
            {
                weightsCount += nodesPerLayer[i] * nodesPerLayer[i + 1];
            }

            // asigne random weights for the edges
            Random randomNumGenerator = new Random();
            weightValues = new double[weightsCount];
            for (int i = 0; i < weightsCount; i++)
            {
                double rand = randomNumGenerator.Next(-10, 10);
                //weightValues[i] = rand / 10;
                weightValues[i] = 1;
            }

            data = readFile();

            for (int i = 0; i < data.Count; i++)
            {
                double output = calculateTheOutput(data[i][0], data[i][1]);
                calculateDeltaValues(data[i][2], output, data[i][0], data[i][1]);
            }
        }

        public void calculateDeltaValues(double target, double output, double x1, double x2)
        {
            //output = output > 0.55 ? 1 : 0;

            //first calculate the delta values for the output node
            deltavalues[5] = (target - output) * (1 - output) * output;
            deltavalues[4] = (deltavalues[5]) * (uvalues[4]) * (1 - uvalues[4]) * weightValues[5];
            deltavalues[3] = (deltavalues[5]) * (uvalues[3]) * (1 - uvalues[3]) * weightValues[4];

            weightValues[5] = weightValues[5] + learningRate * deltavalues[5] * uvalues[4];
            weightValues[4] = weightValues[4] + learningRate * deltavalues[4] * uvalues[3];

            weightValues[0] = weightValues[0] + learningRate * deltavalues[3] * x1;
            weightValues[1] = weightValues[1] + learningRate * deltavalues[4] * x1;
            weightValues[2] = weightValues[2] + learningRate * deltavalues[3] * x2;
            weightValues[3] = weightValues[3] + learningRate * deltavalues[4] * x2;

        }

        public double calculateTheOutput(double x1, double x2)
        {
            // first layer
            svalues[3] = x1 * weightValues[0] + x2 * weightValues[1] + (withbias ? 1 : 0);
            svalues[4] = x1 * weightValues[2] + x2 * weightValues[3] + (withbias ? 1 : 0);

            uvalues[3] = calculateSigmoid(svalues[3]);
            uvalues[4] = calculateSigmoid(svalues[4]);

            // second layer
            svalues[5] = uvalues[3] * weightValues[4] + uvalues[4] * weightValues[5] + (withbias ? 1 : 0);

            uvalues[5] = calculateSigmoid(svalues[5]);

            return uvalues[5];

        }

        public double calculateSigmoid(double s)
        {
            return 1 / (1 + Math.Pow(Math.E, -s));
        }

        public static List<double[]> readFile(string fileLocation = "data.csv")
        {

            List<double[]> listA = new List<double[]>();

            using (var reader = new StreamReader(fileLocation))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    listA.Add(Array.ConvertAll(values, Double.Parse));
                }
            }

            List<double[]> list50 = new List<double[]>();
            // take only 25 for each group
            for (int i = 0; i < listA.Count; i++)
            {
                if (list50.Count == 25) break;
                if (listA[i][2] == 1) list50.Add(listA[i]);
            }
            for (int i = 0; i < listA.Count; i++)
            {
                if (list50.Count == 50) break;
                if (listA[i][2] == 0) list50.Add(listA[i]);
            }

            Shuffle(list50);
            Shuffle(list50);

            return list50;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //int count = 0;
            //for (int i = 0; i < data.Count; i++)
            //{
            //    double output = calculateTheOutput(data[i][0], data[i][1]);
            //    if (data[i][2] == (output > 0.5 ? 1 : 0)) count++;
            //}

            //System.Diagnostics.Debug.Print("Count =>" + count);
            try
            {
                double[] val = { double.Parse(txt_x1.Text), double.Parse(txt_x2.Text) };
                double output = computeTheOutput(val);

                lbl_output.Text = output + "";

                List<double> x_col_c1 = new List<double>();
                List<double> y_col_c1 = new List<double>();
                x_col_c1.Add(double.Parse(txt_x1.Text));
                y_col_c1.Add(double.Parse(txt_x2.Text));
                chart1.Series["Series3"].Points.DataBindXY(x_col_c1, y_col_c1);
                Refresh();
            }
            catch (Exception ee)
            {
                lbl_output.Text = "Please Check your input";
            }
        }

        public static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
