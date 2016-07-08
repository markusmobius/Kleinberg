using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Kleinberg
{
    class Program
    {
        static void Main(string[] args)
        {
            //open test
            var timeseries = new List<double>();
            StreamReader sr = new StreamReader("test.txt");
            string line = null;
            while ((line = sr.ReadLine()) != null)
            {
                timeseries.Add(double.Parse(line));
            }
            sr.Close();

            var bursts=KleinbergBurst.detectBursts(timeseries);
            var burstlist = KleinbergBurst.getList(bursts, timeseries);

            Console.WriteLine(JsonConvert.SerializeObject(burstlist));

            Console.ReadLine();
        }
    }
}
