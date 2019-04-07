using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PwSW_Projekt2
{
    public partial class Form1 : Form
    {
        private LAS lasFile;
        private List<double> chart;
        private string chartName;
        private bool loadedFile;
        private bool logScaleAvailable;
        private bool useLogScale;

        public Form1()
        {
            InitializeComponent();

            loadedFile = false;
            chart1.Legends.Clear();

            checkBox1.Enabled = false;
            useLogScale = false;
            logScaleAvailable = false;
            chart = new List<double>();

            this.MinimumSize = new System.Drawing.Size(500, 300);
            comboBox1.DropDownClosed += comboBoxClosed;
            chart1.MouseWheel += chart1_MouseWheel;
        }

        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;
            var yAxis = chart.ChartAreas[0].AxisY;

            try
            {
                if (e.Delta < 0)
                {
                    yAxis.ScaleView.ZoomReset();
                }
                else if (e.Delta > 0)
                {
                    var yMin = yAxis.ScaleView.ViewMinimum;
                    var yMax = yAxis.ScaleView.ViewMaximum;

                    var posYStart = yAxis.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 4;
                    var posYFinish = yAxis.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 4;

                    yAxis.ScaleView.Zoom(posYStart, posYFinish);
                }
            }
            catch { }
        }

        private void comboBoxClosed(object sender, EventArgs e)
        {
            if (loadedFile)
            {
                checkBox1.Checked = false;
                chartName = comboBox1.SelectedItem.ToString();
                chart = lasFile.chartsData[chartName];

                drawChart();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "LAS Files (*.las, *.LAS) | *.LAS; *.las | All files(*.*) | *.* ";
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var filename in openFileDialog.FileNames)
                {
                    var name = filename.Split('\\').Last();
                    var lasObject = new LAS(filename);
                    lasFile = lasObject;

                    loadedFile = true;
                }
            }

            try
            {
                setComboBoxChartsList();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show("No File loaded, please choose LAS file again.", "No File Loaded Exception");
            }
        }

        private void setComboBoxChartsList()
        {
            checkBox1.Checked = false;
            comboBox1.Items.Clear();
            label1.Text = "File: " + lasFile.fileName;

            foreach (var chartType in lasFile.chartsData)
            {
                if (chartType.Key == "DEPT" || chartType.Key == "DEPTH") continue;
                comboBox1.Items.Add(chartType.Key);
                Console.WriteLine(comboBox1.Items.Count.ToString());
                comboBox1.ResetText();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                useLogScale = true;
            else
                useLogScale = false;

            drawChart();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void drawChart()
        {
            chart1.Size = new Size(this.Width-30, this.Height - 105);
            chart1.ChartAreas.Clear();
            chart1.Titles.Clear();
            chart1.Legends.Clear();
            chart1.Series.Clear();
            chart1.Annotations.Clear();
            logScaleAvailable = true;
            chart1.Invalidate();

            if (loadedFile)
            {
                var chartArea = new ChartArea();

                chartArea.AxisY.MinorGrid.LineColor = Color.LightGray;
                chartArea.AxisX.MinorGrid.LineColor = Color.LightGray;
                chartArea.AxisY.MinorGrid.Enabled = true;
                chartArea.AxisX.MinorGrid.Enabled = true;
                chartArea.AxisX.Title = chartName;
                chartArea.AxisY.Title = "DEPTH";
                chartArea.AxisY.IsReversed = true;
                chartArea.AxisY.Minimum = lasFile.startValue;
                chartArea.AxisX.ScaleView.Zoomable = true;
                chartArea.AxisY.ScaleView.Zoomable = true;

                var series = new Series();
                series.ChartType = SeriesChartType.Line;
                series.BorderWidth = 1;

                var y = lasFile.chartsData.First().Value.ToList();
                var x = chart.ToArray().ToList();

                for (var i = 0; i < x.Count; i++)
                {
                    if (Math.Abs(lasFile.nullValue - x[i]) < 0.001)
                    {
                        y.RemoveAt(i);
                        x.RemoveAt(i);
                        i--;
                    }

                    else
                    {
                        series.Points.AddXY(x[i], y[i]);
                        if (x[i] < 0.001)
                        {
                            logScaleAvailable = false;
                        }
                    }
                }

                checkBox1.Enabled = logScaleAvailable;

                if (useLogScale)
                {
                    chartArea.AxisX.IsLogarithmic = true;
                }

                chart1.ChartAreas.Add(chartArea);
                chart1.Series.Add(series);
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e) => drawChart();
    }
}
