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

            DateTimeOffset start = DateTimeOffset.Now;
            int N = 100;
            for(int i = 0; i < N; i++)
            {
                var bursts = KleinbergBurst.detectBursts(timeseries);
                if (i == 0)
                {
                    var burstlist = KleinbergBurst.getList(bursts, timeseries);
                    Console.WriteLine(JsonConvert.SerializeObject(burstlist));
                }
            }
            Console.WriteLine("{0} replications run in {1:0.00} seconds",N,(DateTimeOffset.Now-start).TotalSeconds);

            Console.ReadLine();
        }
    }
}
