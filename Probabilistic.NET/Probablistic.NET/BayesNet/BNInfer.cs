using System.Collections;

namespace edu.nova.scis.Probabilistic.BeliefNetwork
{
    internal abstract class BNInfer
    {
        protected BNet m_net;

        protected BNInfer(BNet net)
        {
            m_net = net;
        }

        public abstract double GetBelief(string x, string o);
    }

    internal class BElim : BNInfer
    {
        private readonly ArrayList m_buckets;

        public BElim(BNet net) : base(net)
        {
            m_buckets = new ArrayList();

            PrepareBuckets();
        }

        public override double GetBelief(string x, string o)
        {
            m_net.ResetNodes();

            double norm = 1.0;

            if (o.Length > 0)
            {
                m_net.SetNodes(o);
                norm = Sum(0, 0);
            }

            m_net.SetNodes(x);

            return Sum(0, 0)/norm;
        }

        private void PrepareBuckets()
        {
            ArrayList nodes = m_net.Nodes;

            for (int i = 0; i < nodes.Count; ++i)
                m_buckets.Add(new Bucket(i));

            // go through all buckets from buttom up
            for (int i = nodes.Count - 1; i >= 0; --i)
            {
                var theNode = (BNode) nodes[i];
                var theBuck = (Bucket) m_buckets[i];

                foreach (BNode node in theNode.Parents)
                    theBuck.parentNodes.Add(node);

                foreach (Bucket nxtBuck in theBuck.childBuckets)
                {
                    foreach (BNode node in nxtBuck.parentNodes)
                        if (node.ID != i && !theBuck.parentNodes.Contains(node))
                            theBuck.parentNodes.Add(node);
                }

                int max_nid = FindMaxNodeId(theBuck.parentNodes);

                if (max_nid >= 0)
                    ((Bucket) m_buckets[max_nid]).childBuckets.Add(theBuck);
            }
        }

        protected double Sum(int nid, int para)
        {
            var theNode = (BNode) m_net.Nodes[nid];
            var theBuck = (Bucket) m_buckets[nid];

            int p_cnt = theNode.Parents.Count;

            int cond = (para & ((1 << p_cnt) - 1));

            double pr = 0.0;

            // sum over all possible values
            for (int e = 0; e < 2; ++e)
            {
                if (theNode.Evidence != -1 && theNode.Evidence != e)
                    continue;

                double tmpPr = theNode.CPT[cond, e];

                // count child bucket's contribution
                foreach (Bucket nxtBuck in theBuck.childBuckets)
                {
                    int next_para = 0;

                    for (int j = 0; j < nxtBuck.parentNodes.Count; ++j)
                    {
                        var pnode = (BNode) nxtBuck.parentNodes[j];

                        int pos = theBuck.parentNodes.IndexOf(pnode);

                        next_para += (pos >= 0) ? ((para >> pos & 1) << j) : (e << j);
                    }

                    tmpPr *= Sum(nxtBuck.id, next_para);
                }

                pr += tmpPr;
            }

            return pr;
        }

        private int FindMaxNodeId(ArrayList nodes)
        {
            int max = -1;
            foreach (BNode node in nodes)
                if (node.ID > max) max = node.ID;
            return max;
        }

        private class Bucket
        {
            public readonly ArrayList childBuckets;
            public readonly int id;
            public readonly ArrayList parentNodes;

            public Bucket(int i)
            {
                id = i;
                parentNodes = new ArrayList();
                childBuckets = new ArrayList();
            }
        }
    }
}

// end namespace