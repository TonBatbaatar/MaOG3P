using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;


namespace HeuristicLab.Algorithms.CFG.MultiObjective
{
    [Item("OutputScopes", "output the scope into file")]
    [StorableClass]
    class OutputScopes : SingleSuccessorOperator
    {

        [StorableConstructor]
        protected OutputScopes(bool deserializing) : base(deserializing) { }
        protected OutputScopes(OutputScopes original, Cloner cloner) : base(original, cloner) { }
        public OutputScopes()
        {
        }

        public override Task<IOperation> Apply()
        {
            
            IScope[] scopes = ExecutionContext.Scope.SubScopes.ToArray();// individuals of the GP
            int size = scopes.Length; // individual size
            int[] indices = Enumerable.Range(0, size).ToArray();

            for(int i=0; i<size; i++)
            {
                scopes[i].
                File.WriteAllText("WriteText.txt", text);
            }

            return base.Apply();
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new OutputScopes(this, cloner);
        }
    }
}
