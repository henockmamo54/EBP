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
        Dictionary<string, double> _weightvalues = new Dictionary<string, double>();
        Random randomNumGenerator = new Random();
        Dictionary<string, double> _svalues = new Dictionary<string, double>();
        Dictionary<string, double> _uvalues = new Dictionary<string, double>();
        Dictionary<string, double> _dvalues = new Dictionary<string, double>();
        public Form1()
        {
            InitializeComponent();

            _layers.Add(2);
            _layers.Add(2);
            _layers.Add(1);

            //intialize the network
            intializeTheNetwork();

            //read data
            data = readFile().Take(5).ToList();
            double[] t = { data[0][0], data[0][1] };
            var output = computeTheOutput(t);
            computeDeltaValues(output, data[0][2]);
            updateWeights(t);

            previousCode();

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
                        //_weightvalues.Add(i+1 + "|" + destination + "" + source, rand / 10);
                        _weightvalues.Add(i + 1 + "|" + destination + "" + source, 1);
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
                        _svalues[i + 1 + "" + j] = c;
                        _uvalues[i + 1 + "" + j] = calculateSigmoid(c);
                    }
                    else
                    {
                        double c = 0;
                        for (int k = 1; k < _layers[i - 1] + 1; k++)
                        {
                            c += _weightvalues[i + "|" + j + "" + k] * _uvalues[i + "" + k];
                        }
                        _svalues[i + 1 + "" + j] = c;
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
                            double newWeight = _weightvalues[i + 1 + "|" + destination + "" + source] + learningRate * _dvalues[(i + 2) + "" + destination] * x[source-1];
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

            data = readFile().Take(5).ToList();

            for (int i = 0; i < 1; i++)
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

            return listA;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int count = 0;
            for (int i = 0; i < data.Count; i++)
            {
                double output = calculateTheOutput(data[i][0], data[i][1]);
                if (data[i][2] == (output > 0.5 ? 1 : 0)) count++;
            }

            System.Diagnostics.Debug.Print("Count =>" + count);
        }
    }
}
