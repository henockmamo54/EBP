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
        public Form1()
        {
            InitializeComponent();

            // asigne number of nodes per layer
            nodesPerLayer = new int[numberOfLayer];
            nodesPerLayer[0] = 2;
            nodesPerLayer[1] = 2;
            nodesPerLayer[2] = 1;

            foreach (int i in nodesPerLayer) totalNodeCount += i;
            deltavalues = new double[totalNodeCount+1];
            svalues = new double[totalNodeCount+1];
            uvalues = new double[totalNodeCount+1];

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
                weightValues[i] = rand / 10;
                //weightValues[i] = 1;
            }

            var data=readFile();

            for (int i = 0; i < data.Count; i++) {
                double output = calculateTheOutput(data[i][0], data[i][1]);
                calculateDeltaValues(data[i][2], output, data[i][0], data[i][1]);
            }

        }

        public void calculateDeltaValues(double target, double output, double x1,double x2)
        {
            //output = output > 0.55 ? 1 : 0;

            //first calculate the delta values for the output node
            deltavalues[5] = (target - output) * (1 - output) * output;
            deltavalues[4] = (deltavalues[5]) * (uvalues[4]) * (1 - uvalues[4]) * weightValues[5];
            deltavalues[3] = (deltavalues[5]) * (uvalues[3]) * (1 - uvalues[3]) * weightValues[4];

            weightValues[5] = weightValues[5] + learningRate * deltavalues[5]*uvalues[4];
            weightValues[4] = weightValues[4] + learningRate * deltavalues[4]*uvalues[3];

            weightValues[0] = weightValues[0] + learningRate * deltavalues[3]*x1;
            weightValues[1] = weightValues[1] + learningRate * deltavalues[4]*x1;
            weightValues[2] = weightValues[2] + learningRate * deltavalues[3] * x2;
            weightValues[3] = weightValues[3] + learningRate * deltavalues[4] * x2;

        }

        public double calculateTheOutput(double x1, double x2)
        {
            // first layer
            svalues[3] = x1 * weightValues[0] + x2 * weightValues[1];
            svalues[4]= x1 * weightValues[2] + x2 * weightValues[3];

            uvalues[3] = calculateSigmoid(svalues[3]);
            uvalues[4] = calculateSigmoid(svalues[4]);

            // second layer
            svalues[5] = uvalues[3] * weightValues[4] + uvalues[4] * weightValues[5];

            uvalues[5] = calculateSigmoid(svalues[5]);

            return uvalues[5];

        }

        public double calculateSigmoid(double s) {
            return  1 / (1 + Math.Pow(Math.E, -s)); 
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

    }
}
