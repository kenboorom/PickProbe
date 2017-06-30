using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PickProbe
{
    public partial class PickProbe : Form
    {
        Waveform stage1;

        int numberSymbols = 8;
        double numberSamplesPerSymbol = 10;

        public PickProbe()
        {
            InitializeComponent();
            RedrawGraphs(true);
            WindowState = FormWindowState.Maximized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RedrawGraphs(true);
        }

        // ---------------------------------------------------------------------------------------------------------------
        // 
        //  Update graphs when user changes probe selected
        // 
        // ---------------------------------------------------------------------------------------------------------------

        private void probe1SelectedButton_CheckedChanged(object sender, EventArgs e)
        {
            if (probe1SelectedButton.Checked)               // Prevent two calls to redraw (one from de-select, one from select)
                RedrawGraphs(false);

        }

        private void probe2SelectedButton_CheckedChanged(object sender, EventArgs e)
        {
            if (probe2SelectedButton.Checked)              // Prevent two calls to redraw (one from de-select, one from select)
                RedrawGraphs(false);
        }

        private void probe3SelectedButton_CheckedChanged(object sender, EventArgs e)
        {
            if (probe3SelectedButton.Checked)              // Prevent two calls to redraw (one from de-select, one from select)
                RedrawGraphs(false);
        }

        private void probe4SelectedButton_CheckedChanged(object sender, EventArgs e)
        {
            if (probe4SelectedButton.Checked)              // Prevent two calls to redraw (one from de-select, one from select)
                RedrawGraphs(false);
        }

        private void probe5SelectedButton_CheckedChanged(object sender, EventArgs e)
        {
            if (probe5SelectedButton.Checked)              // Prevent two calls to redraw (one from de-select, one from select)
                RedrawGraphs(false);
        }

        private void label7_Click(object sender, EventArgs e)
        {
            RedrawGraphs(false);
        }

        // ---------------------------------------------------------------------------------------------------------------
        // 
        //  Update graphs when user changes SIGNAL SOURCE
        // 
        // ---------------------------------------------------------------------------------------------------------------

        private void signalSource1Button_CheckedChanged(object sender, EventArgs e)
        {
            if (signalSource1Button.Checked)            // Prevent two calls to redraw (one from de-select, one from select)
                RedrawGraphs(true);
        }

        private void signalSource2Button_CheckedChanged(object sender, EventArgs e)
        {
            if (signalSource2Button.Checked)            // Prevent two calls to redraw (one from de-select, one from select)
                RedrawGraphs(true);
        }

        private void signalSource3Button_CheckedChanged(object sender, EventArgs e)
        {
            if (signalSource3Button.Checked)            // Prevent two calls to redraw (one from de-select, one from select)
                RedrawGraphs(true);
        }

        private void compensationUnderCompButton_CheckedChanged(object sender, EventArgs e)
        {
            if (compensationUnderCompButton.Checked)
                RedrawGraphs(false);
        }

        private void compenstationOverCompButton_CheckedChanged(object sender, EventArgs e)
        {
            if (compenstationOverCompButton.Checked)
                RedrawGraphs(false);

        }

        private void compensationCorrectCompButton_CheckedChanged(object sender, EventArgs e)
        {
            if (compensationCorrectCompButton.Checked)
                RedrawGraphs(false);
        }

        private void signalSource4Button_CheckedChanged(object sender, EventArgs e)
        {
            if (signalSource3Button.Checked)            // Prevent two calls to redraw (one from de-select, one from select)
                RedrawGraphs(true);
        }

        private void label5_Click(object sender, EventArgs e)
        {
            RedrawGraphs(true);
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
        }

        private void samplingChoice1_CheckedChanged(object sender, EventArgs e)
        {
            if (samplingChoice1.Checked)
                RedrawGraphs(false);
        }

        private void samplingChoice2_CheckedChanged(object sender, EventArgs e)
        {
            if (samplingChoice2.Checked)
                RedrawGraphs(false);
        }

        private void samplingChoice3_CheckedChanged(object sender, EventArgs e)
        {
            if (samplingChoice3.Checked)
                RedrawGraphs(false);
        }

        private void RedrawGraphs(bool replaceData)
        {
            if (replaceData || (stage1==null))
            {

                // Find the time per symbol based on user's selection
                double symbolFrequency = 0;
                if (signalSource1Button.Checked)
                    symbolFrequency = 480E6;
                else if (signalSource2Button.Checked)
                    symbolFrequency = 5E9;
                else if (signalSource3Button.Checked)
                    symbolFrequency = 2.133E9;
                else
                {
                    string c = DateTime.Now.ToString("h:mm:ss tt");
                    statusBox.AppendText($"{c} Invalid symbol frequency \r\n");
                    symbolFrequency = 2E9;
                }

                double timePerSymbol = 1 / symbolFrequency;
                double timePerSample = timePerSymbol / numberSamplesPerSymbol;
                int numberSamples = (int)(numberSymbols * numberSamplesPerSymbol);

                // CREATE STAGE 1 - Go for 480 Mhz, 2 nanosec/cycle, 10 bits, 20 nsec, 20 samples per bit
                stage1 = new Waveform(timePerSample, numberSamples, 250);
                Random r = new Random();
                int sampleIndex = 0;
                for (int symbolNumber = 0; symbolNumber < numberSymbols; symbolNumber++)
                {
                    int symbolValue = r.Next(2);   // Upper bound is exclusive, so this is 0 or 1
                    for (int symbolSample = 0; symbolSample < numberSamplesPerSymbol; symbolSample++)
                        stage1.data[sampleIndex++] = symbolValue * 200;
                }
            }
            stage1.PlotWaveform(stage1Chart);

            Waveform stage2 = stage1.Clone();
            stage2.PlotWaveform(stage2Chart);

            // Sample 1k-ohm, 10nF, gives 15.9Khz
            // 1 / (2 * pi * RC) = frequency

            // RC = (1 / (2 * pi * f))  f=1E9
            // RC = 1.59E-10    Assume C = 10 pF
            // R = 1.59E-10 / (10E-12)
            // R=15.9


            // Figure out desired cutoff frequency
            double desiredCutoff = -1;
            System.Reflection.Assembly thisExe;
            thisExe = System.Reflection.Assembly.GetExecutingAssembly();

            if (probe1SelectedButton.Checked)
            {
                desiredCutoff = 4E9;
                // probeImage.ImageLocation = @"C:\temp\tek\P7240.png";
                System.IO.Stream file =
                    thisExe.GetManifestResourceStream("PickProbe.Resources.P7240.png");
                this.probeImage.Image = Image.FromStream(file);
            }
            else if (probe2SelectedButton.Checked)
            {
                desiredCutoff = 2.5E9;
                System.IO.Stream file =
                    thisExe.GetManifestResourceStream("PickProbe.Resources.P7225.png");
                this.probeImage.Image = Image.FromStream(file);
            }
            else if (probe3SelectedButton.Checked)
            {
                desiredCutoff = 1.5E9;
                System.IO.Stream file =
                    thisExe.GetManifestResourceStream("PickProbe.Resources.P6248.png");
                this.probeImage.Image = Image.FromStream(file);
            }
            else if (probe4SelectedButton.Checked)
            {
                desiredCutoff = 220E6;
                System.IO.Stream file =
                    thisExe.GetManifestResourceStream("PickProbe.Resources.P2220.png");
                this.probeImage.Image = Image.FromStream(file);
            }
            else if (probe5SelectedButton.Checked)
            {
                desiredCutoff = 100E6;
                System.IO.Stream file =
                    thisExe.GetManifestResourceStream("PickProbe.Resources.P3010.png");
                this.probeImage.Image = Image.FromStream(file);
            }

            else if (probe6SelectedButton.Checked)
            {
                double myResult;
                if (double.TryParse(probeMhzAlternate.Text, out myResult) == false)
                    return;
                if (myResult < 1)
                    return;
                desiredCutoff = myResult * 1E6;
            }
            string currentTime = DateTime.Now.ToString("h:mm:ss tt");
            statusBox.AppendText($"{currentTime} cutoff= {desiredCutoff / 1E6} Mhz\r\n");
            statusBox.SelectionStart = statusBox.TextLength;
            statusBox.ScrollToCaret();

            // Puts below at 100 Mhz
            double resistor = 159;
            double capacitor = 10E-12;

            // Adjust capacitor to desired value
            capacitor = capacitor * (100E6 / desiredCutoff);        // Higher desired cutoff = lower capacitance
            
            // Create Stage3
            Waveform stage3 = stage2.ApplyRCFilter(resistor, capacitor, 20);
            stage3.PlotWaveform(stage3Chart);

            Waveform stage4 = stage3.Clone();

            if (compensationCorrectCompButton.Checked)
            {   
            }

            else if (compensationUnderCompButton.Checked)
                stage4 = stage3.ApplyRCFilter(15.9, 10E-12, 20);

            else if (compenstationOverCompButton.Checked)
            {
                Waveform temp = stage3.ApplyRCFilter(15.9, 10E-12, 20);
                stage4 = stage3.Clone();
                for (int i = 0; i < stage4.maximumDataPoints; i++)
                    stage4.data[i] = stage4.data[i] + (stage3.data[i] - temp.data[i]);
            }

            stage4.PlotWaveform(stage4Chart);

            // DO sampling
            Waveform stage5 = stage4.ApplySampling(500);
            stage5.PlotWaveform(stage5Chart, SeriesChartType.Point);

        }

    }
}
