using System;
using System.Collections;

namespace edu.nova.scis.Probabilistic.BeliefNetwork.NET
{
    public class BeliefNetwork
    {
        private readonly ArrayList _nodes = new ArrayList();
        private string fileName = "BN_Alarm.xml";
        private BNInfer infer;
        private BNet m_net;
        private ArrayList m_query;

        public bool IsValid(string x, string o)
        {
            bool valid = true;
            if (o.Length > 0)
                valid = IsNameValid(o);
            if (x.Length == 0)
            {
                Console.WriteLine("Please provide query");
                valid = false;
            }
            else if (valid)
                valid = IsNameValid(x);
            return valid;
        }

        private bool IsNameValid(string s)
        {
            string[] items = s.Split(';');
            foreach (string item in items)
            {
                string[] pair = item.Split('=');
                if (pair.Length != 2 || pair[1].Length == 0)
                {
                    Console.WriteLine("Value is missing!");
                    return false;
                }
                else if (!_nodes.Contains(pair[0].Trim().ToLower()))
                {
                    Console.WriteLine("'" + pair[0] + "' is not a valid node");
                    return false;
                }
                else if (m_query.Contains(pair[0].Trim().ToLower()))
                {
                    Console.WriteLine("'" + pair[0] + "' has been used more than once");
                    return false;
                }
                else
                    m_query.Add(pair[0].Trim().ToLower());
            }
            return true;
        }

        public void ProcessBeliefNetwork()
        {
            m_net = new BNet();
            m_query = new ArrayList();

            m_net.Build(fileName);
            m_net.PrintNet("BNet_layout.txt");

            infer = new BElim(m_net);

            _nodes.Clear();
            foreach (BNode node in m_net.Nodes)
                _nodes.Add(node.Name);

            Console.WriteLine("For example: " + _nodes[0] + "=1; " + _nodes[1] + "=0");
            Console.WriteLine("For example: " + _nodes[2] + "=1");

            string evidence = "cloudy=1; sprinkler=0".TrimEnd(';'); // cloudy=1; sprinkler=0
            string query = "wetgrass=1".TrimEnd(';'); //wetgrass=1
            string obs = evidence.Trim();
            string x = query.Trim();

            if (IsValid(x, obs))
            {
                double pr = infer.GetBelief(x, obs);

                string out1 = " P( " + x;
                out1 += (obs.Length > 0) ? " | " + obs + " )" : " )";
                string out2 = "  = " + pr.ToString("F4");
                string result = "";
                result += out1;
                result += out2;
            }
        }
    }
}