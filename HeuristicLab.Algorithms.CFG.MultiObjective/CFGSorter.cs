using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Algorithms.CFG.MultiObjective
{
    [Item("CFGSorter", "sorter based on objective 1")]
    [StorableClass]
    public class CFGSorter : SingleSuccessorOperator
    {
        public ILookupParameter<ItemArray<DoubleArray>> QualitiesParameter
        {
            get { return (ILookupParameter<ItemArray<DoubleArray>>)Parameters["Qualities"]; }
        }

        public ILookupParameter<ILog> LogParameter
        {
            get { return (ILookupParameter<ILog>)Parameters["Log"]; }
        }



        [StorableConstructor]
        protected CFGSorter(bool deserializing) : base(deserializing) { }
        protected CFGSorter(CFGSorter original, Cloner cloner) : base(original, cloner) { }
        public CFGSorter() {
            Parameters.Add(new ScopeTreeLookupParameter<DoubleArray>("Qualities", "The solutions' qualities vector."));
            Parameters.Add(new LookupParameter<ILog>("Log", "log of Engine"));
        }

        public override IOperation Apply()
        {
            List<double> qualities1 = new List<double>();// to store quality(firness) value on objective 1
            List<double> qualities2 = new List<double>();

            

            foreach (DoubleArray value in QualitiesParameter.ActualValue)// assign quality value into list
            {
                qualities1.Add(value[0]);
                qualities2.Add(value[1]);
            }

          /*  for (int i = 0; i < 20; i++)
            {
                LogParameter.ActualValue.LogMessage(qualities1[i].ToString());
            }

            for (int i = 0; i < 20; i++)
            {
                LogParameter.ActualValue.LogMessage(qualities2[i].ToString());
            }*/

            IScope[] scopes = ExecutionContext.Scope.SubScopes.ToArray();// individuals of the GP
            int size = scopes.Length; // individual size
            int[] indices = Enumerable.Range(0, size).ToArray();

            Array.Sort(indices, scopes, new CustomComparer(qualities1, qualities2));

            /*for (int i = 0; i < 20; i++)
            {
                LogParameter.ActualValue.LogMessage(qualities1[i].ToString());
            }

            for (int i = 0; i < 20; i++)
            {
                LogParameter.ActualValue.LogMessage(qualities2[i].ToString());
            }*/
            ExecutionContext.Scope.SubScopes.Clear();//delete unsorted individual
            ExecutionContext.Scope.SubScopes.AddRange(scopes);//replace with new sorted individuals

            return base.Apply();

        }

        //private class for comparing two individuals
        private class CustomComparer : IComparer<int>
        {
            List<double> quality1;
            List<double> quality2;

            public CustomComparer(List<double> quality1, List<double> quality2)
            {
                this.quality1 = quality1;
                this.quality2 = quality2;
            }

            #region IComparer<int> Members
            //return 1 if x>y and -1 when x<y, return 0 when equal
            public int Compare(int x, int y)
            {
                if (quality1[x] < quality1[y]) return -1;
                else if (quality1[x] > quality1[y]) return 1;
                else return 0; 
            }
            #endregion
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new CFGSorter(this, cloner);
        }
    }
}
