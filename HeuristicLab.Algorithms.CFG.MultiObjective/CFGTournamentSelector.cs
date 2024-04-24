using System;
using System.Collections.Generic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Selection;

namespace HeuristicLab.Algorithms.CFG.MultiObjective
{
    [Item("CFG Tournament Selector", "A customized parent selector used for multiple objective CFG algorithm")]
    [StorableClass]
    public class CFGTournamentSelector : Selector, IMultiObjectiveSelector, IStochasticOperator
    {
        public ILookupParameter<BoolArray> MaximizationParameter
        {
            get { return (ILookupParameter<BoolArray>)Parameters["Maximization"]; }
        }
        public IValueLookupParameter<IntValue> NumberOfSelectedSubScopesParameter
        {
            get { return (IValueLookupParameter<IntValue>)Parameters["NumberOfSelectedSubScopes"]; }
        }
        protected IValueParameter<BoolValue> CopySelectedParameter
        {
            get { return (IValueParameter<BoolValue>)Parameters["CopySelected"]; }
        }
        public ILookupParameter<IRandom> RandomParameter
        {
            get { return (ILookupParameter<IRandom>)Parameters["Random"]; }
        }
        public ILookupParameter<ItemArray<DoubleArray>> QualitiesParameter
        {
            get { return (ILookupParameter<ItemArray<DoubleArray>>)Parameters["Qualities"]; }
        }
        public IValueLookupParameter<IntValue> TournamentSizeParameter
        {
            get { return (IValueLookupParameter<IntValue>)Parameters["TournemantSize"]; }
        }
        public IConstrainedValueParameter<StringValue> ParentSelectionParameter
        {
            get { return (IConstrainedValueParameter<StringValue>)Parameters["ParentSelection"]; }
        }

        public BoolValue CopySelected
        {
            get { return CopySelectedParameter.Value; }
            set { CopySelectedParameter.Value = value; }
        }

        [StorableConstructor]
        protected CFGTournamentSelector(bool deserializing) : base(deserializing) { }
        protected CFGTournamentSelector(CFGTournamentSelector original, Cloner cloner) : base(original, cloner) { }
        public CFGTournamentSelector()
          : base()
        {
            Parameters.Add(new LookupParameter<BoolArray>("Maximization", "For each objective determines whether it should be maximized or minimized."));
            Parameters.Add(new ValueLookupParameter<IntValue>("NumberOfSelectedSubScopes", "The number of sub-scopes that should be selected."));
            Parameters.Add(new ValueParameter<BoolValue>("CopySelected", "True if the selected scopes are to be copied (cloned) otherwise they're moved."));
            Parameters.Add(new LookupParameter<IRandom>("Random", "The random number generator."));
            Parameters.Add(new ScopeTreeLookupParameter<DoubleArray>("Qualities", "The solutions' qualities vector."));
            Parameters.Add(new ValueLookupParameter<IntValue>("TournemantSize", "The size of the group from which the best will be chosen.", new IntValue(7)));
            ItemSet<StringValue> validValueTemp = new ItemSet<StringValue>();
            validValueTemp.Add(new StringValue("OneThird"));
            validValueTemp.Add(new StringValue("Half-Half"));
            validValueTemp.Add(new StringValue("Half-Half-random"));
            validValueTemp.Add(new StringValue("All-Objective1"));
            validValueTemp.Add(new StringValue("All-Objective2"));
            validValueTemp.Add(new StringValue("OneFifth"));
            validValueTemp.Add(new StringValue("Half-OneFourth"));
            Parameters.Add(new ConstrainedValueParameter<StringValue>("ParentSelection", "the algorithm to select parent for crossover and mutation.", validValueTemp, new StringValue("Half-OneFourth")));
            CopySelectedParameter.Hidden = true;
        }

        protected override IScope[] Select(List<IScope> scopes)
        {
            IRandom random = RandomParameter.ActualValue; //random number generator
            int count = NumberOfSelectedSubScopesParameter.ActualValue.Value; // number of parents should be selected
            int groupSize = TournamentSizeParameter.ActualValue.Value;
            // add the qualities(fitness value) into list
            List<double> qualities1 = new List<double>();
            List<double> qualities2 = new List<double>();
            List<double> qualities3 = new List<double>();
            List<double> qualities4 = new List<double>();
            List<double> qualities5 = new List<double>();

            foreach (DoubleArray value in QualitiesParameter.ActualValue)
            {
                qualities1.Add(value[0]);
                qualities2.Add(value[1]);
                if (ParentSelectionParameter.Value.ToString().Equals("OneFifth"))
                {
                    qualities3.Add(value[2]);
                    qualities4.Add(value[3]);
                    qualities5.Add(value[4]);
                }
                else if(ParentSelectionParameter.Value.ToString().Equals("Half-OneFourth"))
                {
                    qualities3.Add(value[2]);
                    qualities4.Add(value[3]);
                    qualities5.Add(value[4]);
                }
            }
            
            bool copy = CopySelected.Value;
            IScope[] selected = new IScope[count];
            // One third from pure objective one, one third from pure objective 2 and rest from combined objective
            if (ParentSelectionParameter.Value.ToString().Equals("OneThird"))
            {
                for (int i = 0; i < count / 3; i++)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    for (int j = 1; j < groupSize; j++)
                    {
                        index = random.Next(scopes.Count);
                        if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                            ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                        {
                            best = index;
                        }
                    }

                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                }
                //second 1/3
                for (int i = count / 3; i < 2 * count / 3; i++)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    for (int j = 1; j < groupSize; j++)
                    {
                        index = random.Next(scopes.Count);
                        if (((MaximizationParameter.ActualValue[1]) && (qualities2[index] > qualities2[best])) ||
                            ((!MaximizationParameter.ActualValue[1]) && (qualities2[index] < qualities2[best])))
                        {
                            best = index;
                        }
                    }

                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                } //rest
                for (int i = 2 * count / 3; i < count; i++)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities2[index] > qualities2[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities2[index] < qualities2[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                }
            }
            //select one from fist objective and then select next from second objective untill reach the number
            else if (ParentSelectionParameter.Value.ToString().Equals("Half-Half-random"))
            {
                for (int i = 0; i < count; i++)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities2[index] > qualities2[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities2[index] < qualities2[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                }
            }
            else if (ParentSelectionParameter.Value.ToString().Equals("Half-Half"))
            {
                //first half
                for (int i = 0; i < count / 2; i++)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    for (int j = 1; j < groupSize; j++)
                    {
                        index = random.Next(scopes.Count);
                        if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                            ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                        {
                            best = index;
                        }
                    }

                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                }
                //second half
                for (int i = count / 2; i < count; i++)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    for (int j = 1; j < groupSize; j++)
                    {
                        index = random.Next(scopes.Count);
                        if (((MaximizationParameter.ActualValue[1]) && (qualities2[index] > qualities2[best])) ||
                            ((!MaximizationParameter.ActualValue[1]) && (qualities2[index] < qualities2[best])))
                        {
                            best = index;
                        }
                    }

                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                }
            }
            else if (ParentSelectionParameter.Value.ToString().Equals("All-Objective1"))
            {
                for (int i = 0; i < count; i++)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    for (int j = 1; j < groupSize; j++)
                    {
                        index = random.Next(scopes.Count);
                        if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                            ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                        {
                            best = index;
                        }
                    }

                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                }
            }
            else if (ParentSelectionParameter.Value.ToString().Equals("All-Objective2"))
            {
                for (int i = 0; i < count; i++)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    for (int j = 1; j < groupSize; j++)
                    {
                        index = random.Next(scopes.Count);
                        if (((MaximizationParameter.ActualValue[1]) && (qualities2[index] > qualities2[best])) ||
                            ((!MaximizationParameter.ActualValue[1]) && (qualities2[index] < qualities2[best])))
                        {
                            best = index;
                        }
                    }

                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                }
            }

            else if (ParentSelectionParameter.Value.ToString().Equals("OneFifth"))
            {
                int i = 0;
                while (i < (count / 5)*1)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    for (int j = 1; j < groupSize; j++)
                    {
                        index = random.Next(scopes.Count);
                        if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                            ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                        {
                            best = index;
                        }
                    }

                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
                while (i < (count / 5) * 2)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities2[index] > qualities2[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities2[index] < qualities2[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
                while (i < (count / 5) * 3)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities3[index] > qualities3[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities3[index] < qualities3[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
                while (i < (count / 5) * 4)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities4[index] > qualities4[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities4[index] < qualities4[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
                while (i < (count / 5) * 5)
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities5[index] > qualities5[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities5[index] < qualities5[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
            }
            else if (ParentSelectionParameter.Value.ToString().Equals("Half-OneFourth"))
            {
                int i = 0;
                while (i < (count / 2))
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    for (int j = 1; j < groupSize; j++)
                    {
                        index = random.Next(scopes.Count);
                        if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                            ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                        {
                            best = index;
                        }
                    }

                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
                while (i < ((count / 2) + ((count / 8) * 1)))
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities2[index] > qualities2[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities2[index] < qualities2[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
                while (i < ((count / 2) + ((count / 8) * 2)))
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities3[index] > qualities3[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities3[index] < qualities3[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
                while (i < ((count / 2) + ((count / 8) * 3)))
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities4[index] > qualities4[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities4[index] < qualities4[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
                while (i < ((count / 2) + ((count / 8) * 4)))
                {
                    int best = random.Next(scopes.Count);
                    int index;
                    if (i % 2 == 0)
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[0]) && (qualities1[index] > qualities1[best])) ||
                                ((!MaximizationParameter.ActualValue[0]) && (qualities1[index] < qualities1[best])))
                            {
                                best = index;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 1; j < groupSize; j++)
                        {
                            index = random.Next(scopes.Count);
                            if (((MaximizationParameter.ActualValue[1]) && (qualities5[index] > qualities5[best])) ||
                                ((!MaximizationParameter.ActualValue[1]) && (qualities5[index] < qualities5[best])))
                            {
                                best = index;
                            }
                        }
                    }


                    if (copy)
                        selected[i] = (IScope)scopes[best].Clone();
                    else
                    {
                        selected[i] = scopes[best];
                        scopes.RemoveAt(best);
                    }
                    i++;
                }
            }

            return selected;
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new CFGTournamentSelector(this, cloner);
        }
    }
}
