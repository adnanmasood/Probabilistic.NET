using System;

namespace edu.nova.scis.Probabilistic.BeliefNetwork
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var net = new BNet();
            net.Build("..\\..\\data\\bn_wetGrass.xml");

            net.PrintNet("..\\..\\data\\BNet_layout.txt");

            BNInfer infer = new BElim(net);

            double pr = infer.GetBelief("Rain=1", "WetGrass=1");
            Console.WriteLine(" Result: {0}", pr);

            pr = infer.GetBelief("Sprinkler=1", "WetGrass=1");
            Console.WriteLine(" Result: {0}", pr);

            Console.ReadLine();
        }
    }
}