using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kleinberg
{
    public static class KleinbergBurst
    {
        public class Node
        {
            public int state; // burst level: 0, 1, ..., K-1
            public Node parent;
            public Node(int s, Node p)
            {
                this.state = s;
                this.parent = p;
            }
        }

        public class burstInterval
        {
            public int level;
            public double start;
            public double end;
            public bool finished;
            public burstInterval()
            {
                finished = false;
            }
        }

        static double minElement(double[] arr)
        {
            double min = arr[0];
            for(int i = 1; i < arr.Length; i++)
            {
                if (arr[i] < min)
                {
                    min = arr[i];
                }
            }
            return min;
        }

        // Cost function for reproducing a given interval.  
        // 'i': the burst level  
        // 'x': interval  
        static double logf(double[] alpha, double[] ln_alpha, int i, double x)
        {
            return ln_alpha[i] - alpha[i] * x;
        }
        // Cost function for changing a burst level.  
        // 'i': the previous burst level  
        // 'j': the next burst level  
        static double tau(double gammalogn, int i, int j)
        {
            if (i >= j)
            {
                return 0.0;
            }
            return (j - i) * gammalogn;
        }

        //timeseries should not be empty and sorted, s>1.0, gamme>0
        public static int[] detectBursts(List<double> timeseries, double s = 2, double gamma = 1)
        {
            // The number of events.  
            var N = timeseries.Count;

            // Calculate time intervals between successive events.  
            double[] intervals = new double[N - 1];
            for (int i = 0; i < N-1; i++)
            {
                intervals[i] = timeseries[i + 1] - timeseries[i];
            }

            // The minimum interval.  
            double delta = minElement(intervals);
            // The time length of the whole timeseries.  
            double T = timeseries[timeseries.Count - 1] - timeseries[0];
            // The upper limit of burst levels.  
            int K = (int) Math.Ceiling(1 + Math.Log(T / delta) / Math.Log(s));

            double[] alpha = new double[K]; // Event rate at each burst level.
            double[] ln_alpha = new double[K];
            alpha[0] = N / T; // Average event rate.
            ln_alpha[0] = Math.Log(alpha[0]);

            for (int i = 1; i < K; ++i)
            {
                alpha[i] = s * alpha[i - 1];
                ln_alpha[i] = Math.Log(alpha[i]);
            }
            double gammalogn = gamma * Math.Log((double)N);

            // Initialize.  
            Node[] q = new Node[K]; // state chains.
            for(int i=0;i<q.Length;i++)
            {
                q[i] = new Node(0, null);
            }
            double[] C = new double[K]; // costs.
            C[0] = 0;


            // Start optimization.  
            for (int ss=0; ss<intervals.Length;ss++)
            {
                double interval = intervals[ss];
                Node[] q_new = new Node[K];
                double[] C_new = new double[K];
                for (int i = 0; i < K; ++i)
                {
                    var c = new double[K];
                    for (int j = 0; j < K; ++j)
                    {
                        c[j] = C[j] + tau(gammalogn, j, i);
                    }
                    int j_min = -1;
                    double cmin = 0;
                    for(int k = 0; k < c.Length; k++)
                    {
                        if (j_min==-1 || c[k] < cmin)
                        {
                            cmin = c[k];
                            j_min = k;
                        }
                    }
                    // Store the cost for setting the burst level i.
                    C_new[i] = -logf(alpha,ln_alpha, i, interval) + c[j_min];
                    q_new[i] = new Node(i, q[j_min]);
                }

                q = q_new;
                C = C_new;
            }
            var bursts = new int[N];
            int seq_min = -1;
            double currentmin = 0;
            for (int k = 0; k < C.Length; k++)
            {
                if (seq_min == -1 || C[k] < currentmin)
                {
                    currentmin = C[k];
                    seq_min = k;
                }
            }
            int count = 0;
            Node p = q[seq_min];
            while (p != null)
            {
                count++;
                bursts[N - count] = p.state;
                p = p.parent;
            }
            return bursts;
        }

        //convert this into burst interval format
        public static List<burstInterval> getList(int[] bursts, List<double> timeseries)
        {
            var output = new List<burstInterval>();
            int previous = 0;
            for(int i = 0; i < bursts.Length; i++)
            {
                //should we start a new burst?
                if (i==0 || bursts[i] > previous)
                {
                    output.Add(new burstInterval() { level = bursts[i], start = timeseries[i]});
                }
                previous = bursts[i];
                //check if we should close old bursts
                for(int j = 0; j < output.Count; j++)
                {
                    if (!output[j].finished && (bursts[i] < output[j].level))
                    {
                        output[j].end = timeseries[i - 1];
                        output[j].finished = true;
                    }
                }
            }
            for (int j = 0; j < output.Count; j++)
            {
                if (!output[j].finished)
                {
                    output[j].end = timeseries.Last();
                    output[j].finished = true;
                }
            }
            return output;
        }
    }

}
