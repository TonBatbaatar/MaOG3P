using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Selection;

namespace HeuristicLab.Algorithms.CFG.MultiObjective
{
    [Item("CFGMultiObjectiveSelector", "select for next generation, for multiple objective use")]
    [StorableClass]
    class CFGMultiObjectiveSelector : Selector
    {
        private IValueParameter<BoolValue> CopySelectedParameter
        {
            get { return (IValueParameter<BoolValue>)Parameters["CopySelected"]; }
        }
        public IValueLookupParameter<IntValue> NumberOfSelectedSubScopesParameter
        {
            get { return (ValueLookupParameter<IntValue>)Parameters["NumberOfSelectedSubScopes"]; }
        }
        public ILookupParameter<ItemArray<DoubleArray>> QualitiesParameter
        {
            get { return (ILookupParameter<ItemArray<DoubleArray>>)Parameters["Qualities"]; }
        }
        public IValueLookupParameter<StringValue> NextGenerationSelectionParameter
        {
            get { return (ValueLookupParameter<StringValue>)Parameters["NextGenerationSeleciton"]; }
        }

        public BoolValue CopySelected
        {
            get { return CopySelectedParameter.Value; }
            set { CopySelectedParameter.Value = value; }
        }

        [StorableConstructor]
        private CFGMultiObjectiveSelector(bool deserializing) : base(deserializing) { }
        private CFGMultiObjectiveSelector(CFGMultiObjectiveSelector original, Cloner cloner)
          : base(original, cloner)
        {
        }
        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new CFGMultiObjectiveSelector(this, cloner);
        }

        public CFGMultiObjectiveSelector()
          : base()
        {
            Parameters.Add(new ValueParameter<BoolValue>("CopySelected", "True if the selected sub-scopes should be copied, otherwise false.", new BoolValue(true)));
            Parameters.Add(new ValueLookupParameter<IntValue>("NumberOfSelectedSubScopes", "The number of sub-scopes which should be selected."));
            Parameters.Add(new ScopeTreeLookupParameter<DoubleArray>("Qualities", "The solutions' qualities vector."));
            Parameters.Add(new ValueLookupParameter<StringValue>("NextGenerationSeleciton", "The algorithm to select solution for next generation"));
            CopySelectedParameter.Hidden = true;
        }

        protected override IScope[] Select(List<IScope> scopes)
        {
            IScope[] original = new IScope[scopes.Count];
            scopes.CopyTo(original);
            int count = NumberOfSelectedSubScopesParameter.ActualValue.Value;
            bool copy = CopySelectedParameter.Value.Value;
            IScope[] selected = new IScope[count];

            List<double> qualities1 = new List<double>();// to store quality(firness) value on objective 1
            List<double> qualities2 = new List<double>();
            List<double> qualities3 = new List<double>();
            List<double> qualities4 = new List<double>();
            List<double> qualities5 = new List<double>();

            foreach (DoubleArray value in QualitiesParameter.ActualValue)// assign quality value into list
            {
                qualities1.Add(value[0]);
                qualities2.Add(value[1]);
                if (NextGenerationSelectionParameter.ActualValue.Value.ToString().Equals("Half-OneFourth"))
                {
                    qualities3.Add(value[2]);
                    qualities4.Add(value[3]);
                    qualities5.Add(value[4]);
                }
            }

            //throw new ArgumentException(scopes.Count.ToString() + qualities1.Count.ToString());

            int size = scopes.Count; // individual size
            int[] indices = Enumerable.Range(0, size).ToArray();

            if (NextGenerationSelectionParameter.ActualValue.Value.ToString().Equals("Half-Half"))
            {
                scopes.Sort(new CustomComparer(qualities1, qualities2, qualities3, qualities4, qualities5, 1, original));
                for (int i = 0; i < count / 2; i++)
                {
                    if (copy)
                    {
                        selected[i] = (IScope)scopes[i].Clone();
                    }
                    else
                    {
                        selected[i] = scopes[0];
                        scopes.RemoveAt(0);
                    }
                }
                scopes.Sort(new CustomComparer(qualities1, qualities2, qualities3, qualities4, qualities5, 2, original));
                for (int i = count / 2; i < count; i++)
                {
                    if (copy)
                    {
                        selected[i] = (IScope)scopes[i].Clone();
                    }
                    else
                    {
                        selected[i] = scopes[0];
                        scopes.RemoveAt(0);
                    }
                }
            }
            else if (NextGenerationSelectionParameter.ActualValue.Value.ToString().Equals("All-Objective1"))
            {
                scopes.Sort(new CustomComparer(qualities1, qualities2, qualities3, qualities4, qualities5, 1, original));
                for (int i = 0; i < count; i++)
                {
                    if (copy)
                    {
                        selected[i] = (IScope)scopes[i].Clone();
                    }
                    else
                    {
                        selected[i] = scopes[0];
                        scopes.RemoveAt(0);
                    }
                }
            }
            else if (NextGenerationSelectionParameter.ActualValue.Value.ToString().Equals("All-Objective2"))
            {
                scopes.Sort(new CustomComparer(qualities1, qualities2, qualities3, qualities4, qualities5, 2, original));
                for (int i = 0; i < count; i++)
                {
                    if (copy)
                    {
                        selected[i] = (IScope)scopes[i].Clone();
                    }
                    else
                    {
                        selected[i] = scopes[0];
                        scopes.RemoveAt(0);
                    }
                }
            }
            else if (NextGenerationSelectionParameter.ActualValue.Value.ToString().Equals("Half-OneFourth"))
            {
                int i = 0;
                scopes.Sort(new CustomComparer(qualities1, qualities2, qualities3, qualities4, qualities5, 1, original));
                while (i < count / 2)
                {
                    if (copy)
                    {
                        selected[i] = (IScope)scopes[i].Clone();
                    }
                    else
                    {
                        selected[i] = scopes[0];
                        scopes.RemoveAt(0);
                    }
                    i++;
                }
                scopes.Sort(new CustomComparer(qualities1, qualities2, qualities3, qualities4, qualities5, 2, original));
                while (i < ((count / 2) + ((count / 8) * 1)))
                {
                    if (copy)
                    {
                        selected[i] = (IScope)scopes[i].Clone();
                    }
                    else
                    {
                        selected[i] = scopes[0];
                        scopes.RemoveAt(0);
                    }
                    i++;
                }
                scopes.Sort(new CustomComparer(qualities1, qualities2, qualities3, qualities4, qualities5, 3, original));
                while (i < ((count / 2) + ((count / 8) * 2)))
                {
                    if (copy)
                    {
                        selected[i] = (IScope)scopes[i].Clone();
                    }
                    else
                    {
                        selected[i] = scopes[0];
                        scopes.RemoveAt(0);
                    }
                    i++;
                }
                scopes.Sort(new CustomComparer(qualities1, qualities2, qualities3, qualities4, qualities5, 4, original));
                while (i < ((count / 2) + ((count / 8) * 3)))
                {
                    if (copy)
                    {
                        selected[i] = (IScope)scopes[i].Clone();
                    }
                    else
                    {
                        selected[i] = scopes[0];
                        scopes.RemoveAt(0);
                    }
                    i++;
                }
                scopes.Sort(new CustomComparer(qualities1, qualities2, qualities3, qualities4, qualities5, 5, original));
                while (i < ((count / 2) + ((count / 8) * 4)))
                {
                    if (copy)
                    {
                        selected[i] = (IScope)scopes[i].Clone();
                    }
                    else
                    {
                        selected[i] = scopes[0];
                        scopes.RemoveAt(0);
                    }
                    i++;
                }
            }

            return selected;
        }

        private class CustomComparer : IComparer<IScope>
        {
            List<double> quality1;
            List<double> quality2;
            List<double> quality3;
            List<double> quality4;
            List<double> quality5;
            int qualityNumber;
            IScope[] scopes;

            public CustomComparer(List<double> quality1, List<double> quality2, List<double> quality3, List<double> quality4, List<double> quality5, int qualitynumber, IScope[] scopes)
            {
                this.quality1 = quality1;
                this.quality2 = quality2;
                this.quality3 = quality3;
                this.quality4 = quality4;
                this.quality5 = quality5;
                this.qualityNumber = qualitynumber;
                this.scopes = scopes;
                
            }

            #region IComparer<int> Members
            
            //return 1 if x>y and -1 when x<y, return 0 when equal
            public int Compare(IScope x, IScope y)
            {
                if (qualityNumber == 1)
                {
                    if (quality1[Array.IndexOf(scopes, x)] < quality1[Array.IndexOf(scopes, y)]) return -1;
                    else if (quality1[Array.IndexOf(scopes, x)] > quality1[Array.IndexOf(scopes, y)]) return 1;
                    else return 0;
                }
                else if (qualityNumber == 2)
                {
                    if (quality2[Array.IndexOf(scopes, x)] < quality2[Array.IndexOf(scopes, y)]) return -1;
                    else if (quality2[Array.IndexOf(scopes, x)] > quality2[Array.IndexOf(scopes, y)]) return 1;
                    else return 0;
                }
                else if (qualityNumber == 3)
                {
                    if (quality3[Array.IndexOf(scopes, x)] < quality3[Array.IndexOf(scopes, y)]) return -1;
                    else if (quality3[Array.IndexOf(scopes, x)] > quality3[Array.IndexOf(scopes, y)]) return 1;
                    else return 0;
                }
                else if (qualityNumber == 4)
                {
                    if (quality4[Array.IndexOf(scopes, x)] < quality4[Array.IndexOf(scopes, y)]) return -1;
                    else if (quality4[Array.IndexOf(scopes, x)] > quality4[Array.IndexOf(scopes, y)]) return 1;
                    else return 0;
                }
                else
                {
                    if (quality5[Array.IndexOf(scopes, x)] < quality5[Array.IndexOf(scopes, y)]) return -1;
                    else if (quality5[Array.IndexOf(scopes, x)] > quality5[Array.IndexOf(scopes, y)]) return 1;
                    else return 0;
                }
            }
            #endregion
        }
    }
}
