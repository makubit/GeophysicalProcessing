using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PwSW_Projekt2
{
    internal class LAS
    {
        public Dictionary<string, List<double>> chartsData;
        private string path { get; set; }
        public string fileName { get; set; }
        public double startValue { get; set; }
        public double stopValue { get; set; }
        public double stepValue { get; set; }
        public double nullValue { get; set; }

        public LAS(string pathToLASFile)
        {
            path = pathToLASFile;
            var name = path.Split('\\');
            fileName = name.Last();

            using (var file = new StreamReader(pathToLASFile))
            {
                string line;
                var initialIndex = 0;
                var initialValues = new[] {"STRT.M", "STOP.M", "STEP.M", "NULL." };
                var chartHeaders = new[] { "~A", "#" };
                var delimiter = new[] { ':' };
                var startValues = new double[initialValues.Length];
                var readHeader = true;
                var numberOfColumns = 0;
                var chartsTitleList = new List<string>();

                chartsData = new Dictionary<string, List<double>>();

                while ((line = file.ReadLine()) != null)
                {
                    if(line == "") continue;

                    var columns = Regex.Split(line, @"\s+");
                    columns = columns.Where(x => !string.IsNullOrEmpty(x)).ToArray(); //tu jeszcze dziala

                    if (readHeader)
                    {
                        for (var i = 0; i < columns.Length; i++)
                        {
                            if (initialIndex == initialValues.Length)
                            {
                                initialIndex = 0;
                                readHeader = false;
                                startValue = startValues[0];
                                stopValue = startValues[1];
                                stepValue = startValues[2];
                                nullValue = startValues[3];
                                break;
                            }

                            if (columns[i] == initialValues[initialIndex])
                            {
                                var data = columns[i + 1].Split(delimiter)[0];
                                startValues[initialIndex++] = double.Parse(data, CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    else
                    {
                        if (columns[0] == chartHeaders[0])
                        {
                            numberOfColumns = columns.Length - 1;
                            foreach (var data in columns.Skip(1))
                            {
                                chartsData.Add(data, new List<double>());
                                chartsTitleList.Add(data);
                            }
                        }

                        if ((columns[0] != chartHeaders[0]) && (columns[0] != chartHeaders[1]))
                        {
                            for (var i = 0; i < numberOfColumns; i++)
                            {
                                var dataString = columns[i].Split(delimiter)[0];
                                var negativeFormat = new NumberFormatInfo() { NegativeSign = "-" };
                                var dataDouble = double.Parse(dataString, negativeFormat);

                                chartsData[chartsTitleList[i]].Add(dataDouble);
                            }
                        }
                    }
                }
            }
        }
    }
}