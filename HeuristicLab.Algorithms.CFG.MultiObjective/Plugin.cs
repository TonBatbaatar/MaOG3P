using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.Algorithms.CFG.MultiObjective
{
    [Plugin("HeuristicLab.Algorithms.CFG.MultiObjective", "The multiple objective genetic algorithm for CFG problem", "1.0.0.0")]
    [PluginFile("HeuristicLab.Algorithms.CFG.MultiObjective.dll", PluginFileType.Assembly)]
    [PluginDependency("HeuristicLab.Analysis", "3.3")]
    [PluginDependency("HeuristicLab.Collections", "3.3")]
    [PluginDependency("HeuristicLab.Common", "3.3")]
    [PluginDependency("HeuristicLab.Core", "3.3")]
    [PluginDependency("HeuristicLab.Data", "3.3")]
    [PluginDependency("HeuristicLab.Operators", "3.3")]
    [PluginDependency("HeuristicLab.Optimization", "3.3")]
    [PluginDependency("HeuristicLab.Optimization.Operators", "3.3")]
    [PluginDependency("HeuristicLab.Parameters", "3.3")]
    [PluginDependency("HeuristicLab.Persistence", "3.3")]
    [PluginDependency("HeuristicLab.Random", "3.3")]
    [PluginDependency("HeuristicLab.Selection", "3.3")]
    public class HeuristicLabAlgorithmsCFGMultiObjectivePlugin : PluginBase
    {
    }
}
