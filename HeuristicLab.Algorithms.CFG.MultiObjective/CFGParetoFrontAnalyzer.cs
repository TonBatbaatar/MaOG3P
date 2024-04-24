using System.Collections.Generic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Analysis;

namespace HeuristicLab.Algorithms.CFG.MultiObjective
{
    [Item("CFGParetoFrontAnalyzer", "get pareto front for multi objective CFG algorithm")]
    [StorableClass]
    public class CFGParetoFrontAnalyzer : ParetoFrontAnalyzer
    {

        #region parameter properties
        public ILookupParameter<DataTable> BestQualitiesParameter
        {
            get { return (ILookupParameter<DataTable>)Parameters["BestQualities"]; }
        }
        #endregion

        [StorableConstructor]
        protected CFGParetoFrontAnalyzer(bool deserializing) : base(deserializing) { }
        protected CFGParetoFrontAnalyzer(CFGParetoFrontAnalyzer original, Cloner cloner) : base(original, cloner) { }
        public CFGParetoFrontAnalyzer()
        {
            Parameters.Add(new LookupParameter<DataTable>("BestQualities", "The data table to store the best qualities of objectives."));
        }

        protected override void Analyze(ItemArray<DoubleArray> qualities, ResultCollection results)
        {

            int objectives = qualities[0].Length;//number of the objectives
            int frontSize = 20;//front size shown in the result
            int sizeofqualites = qualities.Length;

            DoubleMatrix front = new DoubleMatrix(frontSize, objectives);
            for (int i = 0; i < frontSize; i++)// store into front untill front is full(20)
            {
                for (int k = 0; k < objectives; k++) //store each objective
                    front[i, k] = qualities[i][k];
            }

            front.RowNames = GetRowNames(front);
            front.ColumnNames = GetColumnNames(front);

            if (results.ContainsKey("Pareto Front"))
                results["Pareto Front"].Value = front;
            else results.Add(new Result("Pareto Front", front));

            DataTable bestQualities = BestQualitiesParameter.ActualValue;
            if (bestQualities == null)
            {
                bestQualities = new DataTable("best qualities", "best qualities of each objectives over the whole population.");
                bestQualities.VisualProperties.YAxisTitle = "Fitness value";

                BestQualitiesParameter.ActualValue = bestQualities;
                results.Add(new Result("Best Qualities", bestQualities));
            }

            for (int k = 0; k < objectives; k++)
            {
                string name = "objective " + k.ToString();
                string avgname = "objective " + k.ToString() + " average";
                if (k == 0) {
                    name = "Best input/output error rate";
                    avgname = "Average input/output error rate";
                }
                else if(k == 1){
                    name = "Best code dissimilarity";
                    avgname = "Average code dissimilarity";
                }
                
                double data = qualities[0][k];
                double sum = data;
                for (int i = 1; i < sizeofqualites; i++)
                {
                    if (data > qualities[i][k])
                    {
                        data = qualities[i][k];
                    }
                    sum += qualities[i][k];
                }
                double avg = sum / sizeofqualites;

                DataRow row;
                bestQualities.Rows.TryGetValue(name, out row);
                if (row == null)
                {
                    row = new DataRow(name, null);
                    row.VisualProperties.StartIndexZero = true;
                    row.Values.Add(data);
                    bestQualities.Rows.Add(row);
                }
                else
                {
                    row.Values.Add(data);
                }

                row = null;
                bestQualities.Rows.TryGetValue(avgname, out row);
                if (row == null)
                {
                    row = new DataRow(avgname, null);
                    row.VisualProperties.StartIndexZero = true;
                    row.Values.Add(avg);
                    bestQualities.Rows.Add(row);
                }
                else
                {
                    row.Values.Add(avg);
                }
            }

        }

        private IEnumerable<string> GetRowNames(DoubleMatrix front)
        {
            for (int i = 1; i <= front.Rows; i++)
                yield return "Solution " + i.ToString();
        }

        private IEnumerable<string> GetColumnNames(DoubleMatrix front)
        {
            for (int i = 1; i <= front.Columns; i++)
                yield return "Objective " + i.ToString();
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new CFGParetoFrontAnalyzer(this, cloner);
        }
    }
}
