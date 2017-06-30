using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using System.Windows.Forms.DataVisualization.Charting;


namespace PickProbe
{
    public class Waveform
    {
        public double secondsPerSample;
        public double[] data;
        public int maximumDataPoints;
        public double maximumVoltageForPlots;

        public Waveform(double passedSecondsPerSample, int passedMaximumDataPoints, double passedMaximumVoltageForPlots)
        {
            maximumDataPoints = passedMaximumDataPoints;
            maximumVoltageForPlots = passedMaximumVoltageForPlots;
            secondsPerSample = passedSecondsPerSample;
            data = new double [maximumDataPoints];
        }

        public void PlotWaveform(Chart targetChart, SeriesChartType chartType)
        {
            targetChart.Series.Clear();
            Series s = targetChart.Series.Add("Waveform");
            s.ChartType = chartType;
            s.BorderWidth = 5;
            targetChart.ChartAreas[0].AxisY.Maximum = maximumVoltageForPlots;
            targetChart.ChartAreas[0].AxisX.LabelStyle.Format = "##.##";
            targetChart.ChartAreas[0].AxisX.Title = "psec";
            targetChart.ChartAreas[0].AxisX.Minimum = 0;



            for (int i=0; i<maximumDataPoints; i++)
            {
                // We have to round the sample time because the Chart control goes nuts with precision
                s.Points.AddXY(i * secondsPerSample*1E12, data[i]);
            }
        }

        public void PlotWaveform(Chart targetChart)
        {
            PlotWaveform(targetChart, SeriesChartType.Line);
        }

        public Waveform ApplyRCFilter(double resistorValue, double capacitorValue, int timestepsPerSample)
        {
            checked
            {
                Waveform resultWaveform = new Waveform(secondsPerSample, maximumDataPoints, this.maximumVoltageForPlots);
                double capacitorCharge = 0, capacitorVoltage = 0;
                double secondsPerTimeStep = secondsPerSample / timestepsPerSample;
                for (int sampleNumber = 0; sampleNumber < maximumDataPoints; sampleNumber++)
                {
                    double inputVoltage = data[sampleNumber];

                    for (int timeStepNumber = 0; timeStepNumber < timestepsPerSample; timeStepNumber++)
                    {
                        double currentThroughResistor = (inputVoltage - capacitorVoltage) / resistorValue;
                        capacitorCharge = capacitorCharge + currentThroughResistor * secondsPerTimeStep;
                        capacitorVoltage = capacitorCharge / capacitorValue;
                    }
                    resultWaveform.data[sampleNumber] = capacitorVoltage;
                }
                return resultWaveform;
            }
        }

        public Waveform ApplyCompensation(double resistorValue, double capacitorValue, int timestepsPerSample)
        {
            Waveform resultWaveform = new Waveform(secondsPerSample, maximumDataPoints, this.maximumVoltageForPlots);
            double capacitorCharge = 0, capacitorVoltage = 0;
            double secondsPerTimeStep = secondsPerSample / timestepsPerSample;
            for (int sampleNumber = 0; sampleNumber < maximumDataPoints; sampleNumber++)
            {
                double inputVoltage = data[sampleNumber];
                for (int timeStepNumber = 0; timeStepNumber < timestepsPerSample; timeStepNumber++)
                {
                    double currentThroughResistor = (inputVoltage - capacitorVoltage) / resistorValue;
                    capacitorCharge = capacitorCharge + currentThroughResistor * secondsPerTimeStep;
                    capacitorVoltage = capacitorCharge / capacitorValue;
                }
                resultWaveform.data[sampleNumber] = capacitorVoltage;
            }
            return resultWaveform;
        }

        public Waveform ApplySampling(double newSamplingInMhz)
        {
            checked
            {
                double totalWaveformTime = this.maximumDataPoints * this.secondsPerSample;
                // Calculate parameters for the new waveform and create it
                double newSecondsPerSample = 1 / (1E6 * newSamplingInMhz);
                double newNumberSamples = totalWaveformTime / newSecondsPerSample;
                int newNumberSamplesInt = (int)newNumberSamples;                    // For debugging
                Waveform resultWaveform = new Waveform(secondsPerSample, newNumberSamplesInt, this.maximumVoltageForPlots);
                for (int newSampleIndex = 0; newSampleIndex < newNumberSamplesInt; newSampleIndex++)
                {
                    double sampleTime = ((double)newSampleIndex) * newSecondsPerSample;
                    double oldSampleIndex = sampleTime / this.secondsPerSample;
                    resultWaveform.data[newSampleIndex] = this.data[(int) oldSampleIndex];
                }
                return resultWaveform;
            }
        }

        public Waveform Clone()
        {
            Waveform newWaveform = new Waveform(this.secondsPerSample, this.maximumDataPoints, this.maximumVoltageForPlots);
            for (int i = 0; i < this.maximumDataPoints; i++)
                newWaveform.data[i] = this.data[i];
            return newWaveform;
        }
    }
}
