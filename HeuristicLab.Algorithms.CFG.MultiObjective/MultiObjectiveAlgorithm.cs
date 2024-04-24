using System;
using System.Linq;
using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Optimization.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.Problems.CFG;
using HeuristicLab.Problems.CFG.Python.MultiObjective;
using HeuristicLab.Random;

namespace HeuristicLab.Algorithms.CFG.MultiObjective
{
    [Item("CFG Multiple Objective Genetic Algorithm", "The multiple objective genetic algorithm for CFG problem")]
    [Creatable(CreatableAttribute.Categories.Algorithms, Priority = 100)]
    [StorableClass]
    public class MultiObjectiveAlgorithm : HeuristicOptimizationEngineAlgorithm, IStorableContent
    {
        public string Filename { get; set; }

        #region Problem Properties
        public override Type ProblemType
        {
            get { return typeof(IMultiObjectiveHeuristicOptimizationProblem); }
        }
        public new IMultiObjectiveHeuristicOptimizationProblem Problem
        {
            get { return (IMultiObjectiveHeuristicOptimizationProblem)base.Problem; }
            set { base.Problem = value; }
        }
        #endregion

        #region Parameter Properties
        private ValueParameter<IntValue> SeedParameter
        {
            get { return (ValueParameter<IntValue>)Parameters["Seed"]; }
        }
        private ValueParameter<BoolValue> SetSeedRandomlyParameter
        {
            get { return (ValueParameter<BoolValue>)Parameters["SetSeedRandomly"]; }
        }
        private ValueParameter<IntValue> PopulationSizeParameter
        {
            get { return (ValueParameter<IntValue>)Parameters["PopulationSize"]; }
        }
        public IConstrainedValueParameter<ISelector> SelectorParameter
        {
            get { return (IConstrainedValueParameter<ISelector>)Parameters["Selector"]; }
        }
        private ValueParameter<PercentValue> CrossoverProbabilityParameter
        {
            get { return (ValueParameter<PercentValue>)Parameters["CrossoverProbability"]; }
        }
        public IConstrainedValueParameter<ICrossover> CrossoverParameter
        {
            get { return (IConstrainedValueParameter<ICrossover>)Parameters["Crossover"]; }
        }
        private ValueParameter<PercentValue> MutationProbabilityParameter
        {
            get { return (ValueParameter<PercentValue>)Parameters["MutationProbability"]; }
        }
        public IConstrainedValueParameter<IManipulator> MutatorParameter
        {
            get { return (IConstrainedValueParameter<IManipulator>)Parameters["Mutator"]; }
        }
        private ValueParameter<MultiAnalyzer> AnalyzerParameter
        {
            get { return (ValueParameter<MultiAnalyzer>)Parameters["Analyzer"]; }
        }
        private ValueParameter<IntValue> MaximumGenerationsParameter
        {
            get { return (ValueParameter<IntValue>)Parameters["MaximumGenerations"]; }
        }
        private ValueParameter<IntValue> SelectedParentsParameter
        {
            get { return (ValueParameter<IntValue>)Parameters["SelectedParents"]; }
        }

        private ValueParameter<ILog> Log
        {
            get { return (ValueParameter<ILog>)Parameters["Log"]; }
        }
        #endregion

        #region Properties
        public IntValue Seed
        {
            get { return SeedParameter.Value; }
            set { SeedParameter.Value = value; }
        }
        public BoolValue SetSeedRandomly
        {
            get { return SetSeedRandomlyParameter.Value; }
            set { SetSeedRandomlyParameter.Value = value; }
        }
        public IntValue PopulationSize
        {
            get { return PopulationSizeParameter.Value; }
            set { PopulationSizeParameter.Value = value; }
        }
        public ISelector Selector
        {
            get { return SelectorParameter.Value; }
            set { SelectorParameter.Value = value; }
        }
        public PercentValue CrossoverProbability
        {
            get { return CrossoverProbabilityParameter.Value; }
            set { CrossoverProbabilityParameter.Value = value; }
        }
        public ICrossover Crossover
        {
            get { return CrossoverParameter.Value; }
            set { CrossoverParameter.Value = value; }
        }
        public PercentValue MutationProbability
        {
            get { return MutationProbabilityParameter.Value; }
            set { MutationProbabilityParameter.Value = value; }
        }
        public IManipulator Mutator
        {
            get { return MutatorParameter.Value; }
            set { MutatorParameter.Value = value; }
        }
        public MultiAnalyzer Analyzer
        {
            get { return AnalyzerParameter.Value; }
            set { AnalyzerParameter.Value = value; }
        }
        public IntValue MaximumGenerations
        {
            get { return MaximumGenerationsParameter.Value; }
            set { MaximumGenerationsParameter.Value = value; }
        }
        public IntValue SelectedParents
        {
            get { return SelectedParentsParameter.Value; }
            set { SelectedParentsParameter.Value = value; }
        }

        private RandomCreator RandomCreator
        {
            get { return (RandomCreator)OperatorGraph.InitialOperator; }
        }
        private CFGSolutionCreator SolutionsCreator
        {
            get { return (CFGSolutionCreator)RandomCreator.Successor; }
        }
        private CFGSorter CFGSorter
        {
            get { return (CFGSorter)((SubScopesCounter)SolutionsCreator.Successor).Successor; }
        }


        private MultipleObjectiveAlgorithmMainLoop MainLoop
        {
            get { return FindMainLoop(CFGSorter.Successor); }
        }
        #endregion

        [Storable]
        private CFGParetoFrontAnalyzer paretoFrontAnalyzer;

        [StorableConstructor]
        protected MultiObjectiveAlgorithm(bool deserializing) : base(deserializing) { }
        protected MultiObjectiveAlgorithm(MultiObjectiveAlgorithm original, Cloner cloner)
          : base(original, cloner)
        {
            paretoFrontAnalyzer = (CFGParetoFrontAnalyzer)cloner.Clone(original.paretoFrontAnalyzer);
            AfterDeserialization();
        }
        public MultiObjectiveAlgorithm()
        {
            Parameters.Add(new ValueParameter<IntValue>("Seed", "The random seed used to initialize the new pseudo random number generator.", new IntValue(0)));
            Parameters.Add(new ValueParameter<BoolValue>("SetSeedRandomly", "True if the random seed should be set to a random value, otherwise false.", new BoolValue(true)));
            Parameters.Add(new ValueParameter<IntValue>("PopulationSize", "The size of the population of solutions.", new IntValue(1000)));
            Parameters.Add(new ConstrainedValueParameter<ISelector>("Selector", "The operator used to select solutions for reproduction.Parent selection."));
            ItemSet<StringValue> validValueTemp = new ItemSet<StringValue>();
            validValueTemp.Add(new StringValue("Half-Half"));
            validValueTemp.Add(new StringValue("All-Objective1"));
            validValueTemp.Add(new StringValue("All-Objective2"));
            validValueTemp.Add(new StringValue("Half-OneFourth"));
            Parameters.Add(new ConstrainedValueParameter<StringValue>("NextGenerationSelection", "the algorithm to select solution to next generation.", validValueTemp, new StringValue("Half-OneFourth")));
            Parameters.Add(new ValueParameter<PercentValue>("CrossoverProbability", "The probability that the crossover operator is applied on two parents.", new PercentValue(0.9)));
            Parameters.Add(new ConstrainedValueParameter<ICrossover>("Crossover", "The operator used to cross solutions."));
            Parameters.Add(new ValueParameter<PercentValue>("MutationProbability", "The probability that the mutation operator is applied on a solution.", new PercentValue(0.05)));
            Parameters.Add(new OptionalConstrainedValueParameter<IManipulator>("Mutator", "The operator used to mutate solutions."));
            Parameters.Add(new ValueParameter<MultiAnalyzer>("Analyzer", "The operator used to analyze each generation.", new MultiAnalyzer()));
            Parameters.Add(new ValueParameter<IntValue>("MaximumGenerations", "The maximum number of generations which should be processed.", new IntValue(300)));
            Parameters.Add(new ValueParameter<IntValue>("SelectedParents", "Each two parents form a new child, typically this value should be twice the population size.", new IntValue(2000)));
            Parameters.Add(new ValueParameter<ILog>("Log","log of Engine", this.Engine.Log));

            RandomCreator randomCreator = new RandomCreator();
            CFGSolutionCreator solutionsCreator = new CFGSolutionCreator();
            SubScopesCounter subScopesCounter = new SubScopesCounter();
            CFGSorter cfgSorter = new CFGSorter();
            ResultsCollector resultsCollector = new ResultsCollector();
            MultipleObjectiveAlgorithmMainLoop mainLoop = new MultipleObjectiveAlgorithmMainLoop();

            this.Engine.Log.LogMessage("Multiple objective algorithm created!");

            OperatorGraph.InitialOperator = randomCreator;

            randomCreator.RandomParameter.ActualName = "Random";
            randomCreator.SeedParameter.ActualName = SeedParameter.Name;
            randomCreator.SeedParameter.Value = null;
            randomCreator.SetSeedRandomlyParameter.ActualName = SetSeedRandomlyParameter.Name;
            randomCreator.SetSeedRandomlyParameter.Value = null;
            randomCreator.Successor = solutionsCreator;

            solutionsCreator.NumberOfSolutionsParameter.ActualName = PopulationSizeParameter.Name;
            solutionsCreator.Successor = subScopesCounter;
            solutionsCreator.InitialGivenProgramCreator.Value.LogParameter.ActualName = Log.Name;

            subScopesCounter.Name = "Initialize EvaluatedSolutions";
            subScopesCounter.ValueParameter.ActualName = "EvaluatedSolutions";
            subScopesCounter.Successor = cfgSorter;

            cfgSorter.QualitiesParameter.ActualName = "Qualities";
            cfgSorter.LogParameter.ActualName = Log.Name;
            cfgSorter.Successor = resultsCollector;

            resultsCollector.CollectedValues.Add(new LookupParameter<IntValue>("Evaluated Solutions", null, "EvaluatedSolutions"));
            resultsCollector.ResultsParameter.ActualName = "Results";
            resultsCollector.Successor = mainLoop;

            mainLoop.PopulationSizeParameter.ActualName = PopulationSizeParameter.Name;
            mainLoop.SelectorParameter.ActualName = SelectorParameter.Name;
            mainLoop.CrossoverParameter.ActualName = CrossoverParameter.Name;
            mainLoop.CrossoverProbabilityParameter.ActualName = CrossoverProbabilityParameter.Name;
            mainLoop.MaximumGenerationsParameter.ActualName = MaximumGenerationsParameter.Name;
            mainLoop.MutatorParameter.ActualName = MutatorParameter.Name;
            mainLoop.MutationProbabilityParameter.ActualName = MutationProbabilityParameter.Name;
            mainLoop.RandomParameter.ActualName = RandomCreator.RandomParameter.ActualName;
            mainLoop.AnalyzerParameter.ActualName = AnalyzerParameter.Name;
            mainLoop.ResultsParameter.ActualName = "Results";
            mainLoop.EvaluatedSolutionsParameter.ActualName = "EvaluatedSolutions";
            mainLoop.NextGenerationSelecitonParameter.ActualName = "NextGenerationSelection";

            foreach (ISelector selector in ApplicationManager.Manager.GetInstances<ISelector>().Where(x => !(x is ISingleObjectiveSelector)).OrderBy(x => x.Name))
                SelectorParameter.ValidValues.Add(selector);
            ISelector defaultSelector = SelectorParameter.ValidValues.FirstOrDefault();// (x => x.GetType().Name.Equals("CFGSelecter"));
            //if (defaultSelector != null) SelectorParameter.Value = defaultSelector;

            ParameterizeSelectors();

            paretoFrontAnalyzer = new CFGParetoFrontAnalyzer();
            paretoFrontAnalyzer.ResultsParameter.ActualName = "Results";
            ParameterizeAnalyzers();
            UpdateAnalyzers();

            AfterDeserialization();
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new MultiObjectiveAlgorithm(this, cloner);
        }

        public override void Prepare()
        {
            if (Problem != null) base.Prepare();
        }

        #region Events
        protected override void OnProblemChanged()
        {
            ParameterizeStochasticOperator(Problem.SolutionCreator);
            ParameterizeStochasticOperator(Problem.Evaluator);
            foreach (IOperator op in Problem.Operators.OfType<IOperator>()) ParameterizeStochasticOperator(op);
            ParameterizeSolutionsCreator();
            ParameterizeCFGSorter();
            ParameterizeMainLoop();
            ParameterizeSelectors();
            ParameterizeAnalyzers();
            ParameterizeIterationBasedOperators();
            UpdateCrossovers();
            UpdateMutators();
            UpdateAnalyzers();
            Problem.Evaluator.QualitiesParameter.ActualNameChanged += new EventHandler(Evaluator_QualitiesParameter_ActualNameChanged);
            Problem.NameChanged += new EventHandler(onProblemNameChanged);
            base.OnProblemChanged();
        }
        protected override void Problem_SolutionCreatorChanged(object sender, EventArgs e)
        {
            ParameterizeStochasticOperator(Problem.SolutionCreator);
            ParameterizeSolutionsCreator();
            base.Problem_SolutionCreatorChanged(sender, e);
        }
        protected override void Problem_EvaluatorChanged(object sender, EventArgs e)
        {
            ParameterizeStochasticOperator(Problem.Evaluator);
            ParameterizeSolutionsCreator();
            ParameterizeCFGSorter();
            ParameterizeMainLoop();
            ParameterizeSelectors();
            ParameterizeAnalyzers();
            Problem.Evaluator.QualitiesParameter.ActualNameChanged += new EventHandler(Evaluator_QualitiesParameter_ActualNameChanged);
            base.Problem_EvaluatorChanged(sender, e);
        }
        protected override void Problem_OperatorsChanged(object sender, EventArgs e)
        {
            foreach (IOperator op in Problem.Operators.OfType<IOperator>()) ParameterizeStochasticOperator(op);
            ParameterizeIterationBasedOperators();
            UpdateCrossovers();
            UpdateMutators();
            UpdateAnalyzers();
            base.Problem_OperatorsChanged(sender, e);
        }
        protected override void Problem_Reset(object sender, EventArgs e)
        {
            base.Problem_Reset(sender, e);
        }
        private void PopulationSizeParameter_ValueChanged(object sender, EventArgs e)
        {
            PopulationSize.ValueChanged += new EventHandler(PopulationSize_ValueChanged);
            ParameterizeSelectors();
        }
        private void PopulationSize_ValueChanged(object sender, EventArgs e)
        {
            ParameterizeSelectors();
        }
        private void Evaluator_QualitiesParameter_ActualNameChanged(object sender, EventArgs e)
        {
            ParameterizeCFGSorter();
            ParameterizeMainLoop();
            ParameterizeSelectors();
            ParameterizeAnalyzers();
        }
        private void SelectedParentsParameter_ValueChanged(object sender, EventArgs e)
        {
            SelectedParents.ValueChanged += new EventHandler(SelectedParents_ValueChanged);
            SelectedParents_ValueChanged(null, EventArgs.Empty);
        }
        private void SelectedParents_ValueChanged(object sender, EventArgs e)
        {
            if (SelectedParents.Value < 2) SelectedParents.Value = 2;
            else if (SelectedParents.Value % 2 != 0)
            {
                SelectedParents.Value = SelectedParents.Value + 1;
            }
        }

        private void onProblemNameChanged(object sender, EventArgs e)
        {
            //this.Engine.Log.LogMessage("--------------------called!");
            SolutionsCreator.InitialGivenProgramCreator.Value.ProblemName = Problem.Name;
        }
        #endregion

        #region Helpers
        [StorableHook(HookType.AfterDeserialization)]
        private void AfterDeserialization()
        {
            PopulationSizeParameter.ValueChanged += new EventHandler(PopulationSizeParameter_ValueChanged);
            PopulationSize.ValueChanged += new EventHandler(PopulationSize_ValueChanged);
            SelectedParentsParameter.ValueChanged += new EventHandler(SelectedParentsParameter_ValueChanged);
            SelectedParents.ValueChanged += new EventHandler(SelectedParents_ValueChanged);
            if (Problem != null)
            {
                Problem.Evaluator.QualitiesParameter.ActualNameChanged += new EventHandler(Evaluator_QualitiesParameter_ActualNameChanged);
            }
        }
        private void ParameterizeSolutionsCreator()
        {
            SolutionsCreator.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
            SolutionsCreator.SolutionCreatorParameter.ActualName = Problem.SolutionCreatorParameter.Name;


            SolutionsCreator.InitialGivenProgramCreator.Value.SymbolicExpressionTreeGrammarParameter.ActualName =
                ((ProbabilisticTreeCreator)Problem.SolutionCreatorParameter.ActualValue).SymbolicExpressionTreeGrammarParameter.ActualName;
            SolutionsCreator.InitialGivenProgramCreator.Value.SymbolicExpressionTreeParameter.ActualName =
                ((ProbabilisticTreeCreator)Problem.SolutionCreatorParameter.ActualValue).SymbolicExpressionTreeParameter.ActualName;
            SolutionsCreator.InitialGivenProgramCreator.Value.MaximumSymbolicExpressionTreeDepthParameter.ActualName =
                ((ProbabilisticTreeCreator)Problem.SolutionCreatorParameter.ActualValue).MaximumSymbolicExpressionTreeDepthParameter.ActualName;
            SolutionsCreator.InitialGivenProgramCreator.Value.MaximumSymbolicExpressionTreeLengthParameter.ActualName =
                ((ProbabilisticTreeCreator)Problem.SolutionCreatorParameter.ActualValue).MaximumSymbolicExpressionTreeLengthParameter.ActualName;

        }
        private void ParameterizeCFGSorter()
        {
            CFGSorter.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
        }
        private void ParameterizeMainLoop()
        {
            MainLoop.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
            MainLoop.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
            MainLoop.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
        }
        private void ParameterizeStochasticOperator(IOperator op)
        {
            if (op is IStochasticOperator)
                ((IStochasticOperator)op).RandomParameter.ActualName = RandomCreator.RandomParameter.ActualName;
        }
        private void ParameterizeSelectors()
        {
            foreach (ISelector selector in SelectorParameter.ValidValues)
            {
                selector.CopySelected = new BoolValue(true);
                selector.NumberOfSelectedSubScopesParameter.ActualName = SelectedParentsParameter.Name;
                ParameterizeStochasticOperator(selector);
            }
            if (Problem != null)
            {
                foreach (IMultiObjectiveSelector selector in SelectorParameter.ValidValues.OfType<IMultiObjectiveSelector>())
                {
                    selector.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
                    selector.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
                }
            }
        }
        private void ParameterizeAnalyzers()
        {
            if (Problem != null)
            {
                paretoFrontAnalyzer.QualitiesParameter.ActualName = Problem.Evaluator.QualitiesParameter.ActualName;
            }
        }
        private void ParameterizeIterationBasedOperators()
        {
            if (Problem != null)
            {
                foreach (IIterationBasedOperator op in Problem.Operators.OfType<IIterationBasedOperator>())
                {
                    op.IterationsParameter.ActualName = "Generations";
                    op.MaximumIterationsParameter.ActualName = "MaximumGenerations";
                }
            }
        }
        private void UpdateCrossovers()
        {
            ICrossover oldCrossover = CrossoverParameter.Value;
            ICrossover defaultCrossover = Problem.Operators.OfType<ICrossover>().FirstOrDefault();
            CrossoverParameter.ValidValues.Clear();
            foreach (ICrossover crossover in Problem.Operators.OfType<ICrossover>().OrderBy(x => x.Name))
                CrossoverParameter.ValidValues.Add(crossover);
            if (oldCrossover != null)
            {
                ICrossover crossover = CrossoverParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldCrossover.GetType());
                if (crossover != null) CrossoverParameter.Value = crossover;
                else oldCrossover = null;
            }
            if (oldCrossover == null && defaultCrossover != null)
                CrossoverParameter.Value = defaultCrossover;
        }
        private void UpdateMutators()
        {
            IManipulator oldMutator = MutatorParameter.Value;
            IManipulator defaultMutator = Problem.Operators.OfType<IManipulator>().FirstOrDefault(x => x.GetType().Name.Equals("MultiSymbolicExpressionTreeManipulator"));
            MutatorParameter.ValidValues.Clear();
            foreach (IManipulator mutator in Problem.Operators.OfType<IManipulator>().OrderBy(x => x.Name))
                MutatorParameter.ValidValues.Add(mutator);
            if (oldMutator != null)
            {
                IManipulator mutator = MutatorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldMutator.GetType());
                if (mutator != null) MutatorParameter.Value = mutator;
            }
            if (oldMutator == null && defaultMutator != null)
                MutatorParameter.Value = defaultMutator;
        }
        private void UpdateAnalyzers()
        {
            Analyzer.Operators.Clear();
            if (Problem != null)
            {
                foreach (IAnalyzer analyzer in Problem.Operators.OfType<IAnalyzer>())
                {
                    foreach (IScopeTreeLookupParameter param in analyzer.Parameters.OfType<IScopeTreeLookupParameter>())
                        param.Depth = 1;
                    Analyzer.Operators.Add(analyzer, analyzer.EnabledByDefault);
                }
            }
            Analyzer.Operators.Add(paretoFrontAnalyzer, paretoFrontAnalyzer.EnabledByDefault);
        }
        private MultipleObjectiveAlgorithmMainLoop FindMainLoop(IOperator start)
        {
            IOperator mainLoop = start;
            while (mainLoop != null && !(mainLoop is MultipleObjectiveAlgorithmMainLoop))
                mainLoop = ((SingleSuccessorOperator)mainLoop).Successor;
            if (mainLoop == null) return null;
            else return (MultipleObjectiveAlgorithmMainLoop)mainLoop;
        }
        #endregion
    }
}
