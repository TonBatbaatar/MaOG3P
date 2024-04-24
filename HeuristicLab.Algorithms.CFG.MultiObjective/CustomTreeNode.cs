using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.Algorithms.CFG.MultiObjective
{
    public class CustomTreeNode
    {
        public Dictionary<string, object> Components;
        public string connectionTypeToParent;
        public int numberOfLines;
        public CustomTreeNode parentNode;

        public CustomTreeNode()
        {
            Components = new Dictionary<string, object>();
            numberOfLines = 0;
            parentNode = this;
        }

        public void addComponent(string key, object value)
        {
            Components.Add(key, value);
        }

    }
}
