using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Xml.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.Problems.CFG;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Algorithms.CFG.MultiObjective
{
    [StorableClass]
    [Item("InitialGivenProgramCreator", "An operator that creates new symbolic expression trees with given program")]
    class InitialGivenProgramCreator : SymbolicExpressionTreeCreator, ISymbolicExpressionTreeSizeConstraintOperator, ISymbolicExpressionTreeGrammarBasedOperator
    {

        string grammar_outPath = "..\\..\\HeuristicLab.CFGGP\\HeuristicLab.Algorithms.CFG.MultiObjective\\PSB1_Solution\\test.bnf";

        Dictionary<string, string> variable_type_dictionary;
        Dictionary<string, string> variable_name_mapping_dictionary;
        Dictionary<string, string> function_return_type_dictionary;

        private const string jsonSmallOrLarge = @"if in0 < 1000:
    res0 = ""small""
elif in0 >= 2000:
    res0 = ""large""
else:
    res0 = None";
        private const string jsonNumberIO = @"{
    ""_type"": ""Module"",
    ""body"": [
        {
            ""_type"": ""Assign"",
            ""col_offset"": 0,
            ""end_col_offset"": 16,
            ""end_lineno"": 1,
            ""lineno"": 1,
            ""targets"": [
                {
                    ""_type"": ""Name"",
                    ""col_offset"": 0,
                    ""ctx"": {
                        ""_type"": ""Store""
                    },
                    ""end_col_offset"": 4,
                    ""end_lineno"": 1,
                    ""id"": ""res0"",
                    ""lineno"": 1
                }
            ],
            ""type_comment"": null,
            ""value"": {
                ""_type"": ""BinOp"",
                ""col_offset"": 7,
                ""end_col_offset"": 16,
                ""end_lineno"": 1,
                ""left"": {
                    ""_type"": ""Name"",
                    ""col_offset"": 7,
                    ""ctx"": {
                        ""_type"": ""Load""
                    },
                    ""end_col_offset"": 10,
                    ""end_lineno"": 1,
                    ""id"": ""in0"",
                    ""lineno"": 1
                },
                ""lineno"": 1,
                ""op"": {
                    ""_type"": ""Add""
                },
                ""right"": {
                    ""_type"": ""Name"",
                    ""col_offset"": 13,
                    ""ctx"": {
                        ""_type"": ""Load""
                    },
                    ""end_col_offset"": 16,
                    ""end_lineno"": 1,
                    ""id"": ""in1"",
                    ""lineno"": 1
                }
            }
        }
    ],
    ""type_ignores"": []
}";
        private const string jsonCollatzNumbers = @"{
    ""_type"": ""Module"",
    ""body"": [
        {
            ""_type"": ""Assign"",
            ""col_offset"": 0,
            ""end_col_offset"": 8,
            ""end_lineno"": 1,
            ""lineno"": 1,
            ""targets"": [
                {
                    ""_type"": ""Name"",
                    ""col_offset"": 0,
                    ""ctx"": {
                        ""_type"": ""Store""
                    },
                    ""end_col_offset"": 4,
                    ""end_lineno"": 1,
                    ""id"": ""res0"",
                    ""lineno"": 1
                }
            ],
            ""type_comment"": null,
            ""value"": {
                ""_type"": ""Constant"",
                ""col_offset"": 7,
                ""end_col_offset"": 8,
                ""end_lineno"": 1,
                ""kind"": null,
                ""lineno"": 1,
                ""n"": 1,
                ""s"": 1,
                ""value"": 1
            }
        },
        {
            ""_type"": ""While"",
            ""body"": [
                {
                    ""_type"": ""If"",
                    ""body"": [
                        {
                            ""_type"": ""Assign"",
                            ""col_offset"": 8,
                            ""end_col_offset"": 22,
                            ""end_lineno"": 4,
                            ""lineno"": 4,
                            ""targets"": [
                                {
                                    ""_type"": ""Name"",
                                    ""col_offset"": 8,
                                    ""ctx"": {
                                        ""_type"": ""Store""
                                    },
                                    ""end_col_offset"": 11,
                                    ""end_lineno"": 4,
                                    ""id"": ""in0"",
                                    ""lineno"": 4
                                }
                            ],
                            ""type_comment"": null,
                            ""value"": {
                                ""_type"": ""BinOp"",
                                ""col_offset"": 14,
                                ""end_col_offset"": 22,
                                ""end_lineno"": 4,
                                ""left"": {
                                    ""_type"": ""Name"",
                                    ""col_offset"": 14,
                                    ""ctx"": {
                                        ""_type"": ""Load""
                                    },
                                    ""end_col_offset"": 17,
                                    ""end_lineno"": 4,
                                    ""id"": ""in0"",
                                    ""lineno"": 4
                                },
                                ""lineno"": 4,
                                ""op"": {
                                    ""_type"": ""FloorDiv""
                                },
                                ""right"": {
                                    ""_type"": ""Constant"",
                                    ""col_offset"": 21,
                                    ""end_col_offset"": 22,
                                    ""end_lineno"": 4,
                                    ""kind"": null,
                                    ""lineno"": 4,
                                    ""n"": 2,
                                    ""s"": 2,
                                    ""value"": 2
                                }
                            }
                        }
                    ],
                    ""col_offset"": 4,
                    ""end_col_offset"": 25,
                    ""end_lineno"": 6,
                    ""lineno"": 3,
                    ""orelse"": [
                        {
                            ""_type"": ""Assign"",
                            ""col_offset"": 8,
                            ""end_col_offset"": 25,
                            ""end_lineno"": 6,
                            ""lineno"": 6,
                            ""targets"": [
                                {
                                    ""_type"": ""Name"",
                                    ""col_offset"": 8,
                                    ""ctx"": {
                                        ""_type"": ""Store""
                                    },
                                    ""end_col_offset"": 11,
                                    ""end_lineno"": 6,
                                    ""id"": ""in0"",
                                    ""lineno"": 6
                                }
                            ],
                            ""type_comment"": null,
                            ""value"": {
                                ""_type"": ""BinOp"",
                                ""col_offset"": 14,
                                ""end_col_offset"": 25,
                                ""end_lineno"": 6,
                                ""left"": {
                                    ""_type"": ""BinOp"",
                                    ""col_offset"": 14,
                                    ""end_col_offset"": 21,
                                    ""end_lineno"": 6,
                                    ""left"": {
                                        ""_type"": ""Constant"",
                                        ""col_offset"": 14,
                                        ""end_col_offset"": 15,
                                        ""end_lineno"": 6,
                                        ""kind"": null,
                                        ""lineno"": 6,
                                        ""n"": 3,
                                        ""s"": 3,
                                        ""value"": 3
                                    },
                                    ""lineno"": 6,
                                    ""op"": {
                                        ""_type"": ""Mult""
                                    },
                                    ""right"": {
                                        ""_type"": ""Name"",
                                        ""col_offset"": 18,
                                        ""ctx"": {
                                            ""_type"": ""Load""
                                        },
                                        ""end_col_offset"": 21,
                                        ""end_lineno"": 6,
                                        ""id"": ""in0"",
                                        ""lineno"": 6
                                    }
                                },
                                ""lineno"": 6,
                                ""op"": {
                                    ""_type"": ""Add""
                                },
                                ""right"": {
                                    ""_type"": ""Constant"",
                                    ""col_offset"": 24,
                                    ""end_col_offset"": 25,
                                    ""end_lineno"": 6,
                                    ""kind"": null,
                                    ""lineno"": 6,
                                    ""n"": 1,
                                    ""s"": 1,
                                    ""value"": 1
                                }
                            }
                        }
                    ],
                    ""test"": {
                        ""_type"": ""Compare"",
                        ""col_offset"": 7,
                        ""comparators"": [
                            {
                                ""_type"": ""Constant"",
                                ""col_offset"": 18,
                                ""end_col_offset"": 19,
                                ""end_lineno"": 3,
                                ""kind"": null,
                                ""lineno"": 3,
                                ""n"": 0,
                                ""s"": 0,
                                ""value"": 0
                            }
                        ],
                        ""end_col_offset"": 19,
                        ""end_lineno"": 3,
                        ""left"": {
                            ""_type"": ""BinOp"",
                            ""col_offset"": 7,
                            ""end_col_offset"": 14,
                            ""end_lineno"": 3,
                            ""left"": {
                                ""_type"": ""Name"",
                                ""col_offset"": 7,
                                ""ctx"": {
                                    ""_type"": ""Load""
                                },
                                ""end_col_offset"": 10,
                                ""end_lineno"": 3,
                                ""id"": ""in0"",
                                ""lineno"": 3
                            },
                            ""lineno"": 3,
                            ""op"": {
                                ""_type"": ""Mod""
                            },
                            ""right"": {
                                ""_type"": ""Constant"",
                                ""col_offset"": 13,
                                ""end_col_offset"": 14,
                                ""end_lineno"": 3,
                                ""kind"": null,
                                ""lineno"": 3,
                                ""n"": 2,
                                ""s"": 2,
                                ""value"": 2
                            }
                        },
                        ""lineno"": 3,
                        ""ops"": [
                            {
                                ""_type"": ""Eq""
                            }
                        ]
                    }
                },
                {
                    ""_type"": ""AugAssign"",
                    ""col_offset"": 4,
                    ""end_col_offset"": 13,
                    ""end_lineno"": 7,
                    ""lineno"": 7,
                    ""op"": {
                        ""_type"": ""Add""
                    },
                    ""target"": {
                        ""_type"": ""Name"",
                        ""col_offset"": 4,
                        ""ctx"": {
                            ""_type"": ""Store""
                        },
                        ""end_col_offset"": 8,
                        ""end_lineno"": 7,
                        ""id"": ""res0"",
                        ""lineno"": 7
                    },
                    ""value"": {
                        ""_type"": ""Constant"",
                        ""col_offset"": 12,
                        ""end_col_offset"": 13,
                        ""end_lineno"": 7,
                        ""kind"": null,
                        ""lineno"": 7,
                        ""n"": 1,
                        ""s"": 1,
                        ""value"": 1
                    }
                }
            ],
            ""col_offset"": 0,
            ""end_col_offset"": 13,
            ""end_lineno"": 7,
            ""lineno"": 2,
            ""orelse"": [],
            ""test"": {
                ""_type"": ""Compare"",
                ""col_offset"": 6,
                ""comparators"": [
                    {
                        ""_type"": ""Constant"",
                        ""col_offset"": 13,
                        ""end_col_offset"": 14,
                        ""end_lineno"": 2,
                        ""kind"": null,
                        ""lineno"": 2,
                        ""n"": 1,
                        ""s"": 1,
                        ""value"": 1
                    }
                ],
                ""end_col_offset"": 14,
                ""end_lineno"": 2,
                ""left"": {
                    ""_type"": ""Name"",
                    ""col_offset"": 6,
                    ""ctx"": {
                        ""_type"": ""Load""
                    },
                    ""end_col_offset"": 9,
                    ""end_lineno"": 2,
                    ""id"": ""in0"",
                    ""lineno"": 2
                },
                ""lineno"": 2,
                ""ops"": [
                    {
                        ""_type"": ""NotEq""
                    }
                ]
            }
        }
    ],
    ""type_ignores"": []
}";
        private const string json_SumOfSquares = @"{
    ""_type"": ""Module"",
    ""body"": [
        {
            ""_type"": ""Assign"",
            ""col_offset"": 0,
            ""end_col_offset"": 41,
            ""end_lineno"": 1,
            ""lineno"": 1,
            ""targets"": [
                {
                    ""_type"": ""Name"",
                    ""col_offset"": 0,
                    ""ctx"": {
                        ""_type"": ""Store""
                    },
                    ""end_col_offset"": 4,
                    ""end_lineno"": 1,
                    ""id"": ""res0"",
                    ""lineno"": 1
                }
            ],
            ""type_comment"": null,
            ""value"": {
                ""_type"": ""Call"",
                ""args"": [
                    {
                        ""_type"": ""ListComp"",
                        ""col_offset"": 11,
                        ""elt"": {
                            ""_type"": ""BinOp"",
                            ""col_offset"": 12,
                            ""end_col_offset"": 15,
                            ""end_lineno"": 1,
                            ""left"": {
                                ""_type"": ""Name"",
                                ""col_offset"": 12,
                                ""ctx"": {
                                    ""_type"": ""Load""
                                },
                                ""end_col_offset"": 13,
                                ""end_lineno"": 1,
                                ""id"": ""i"",
                                ""lineno"": 1
                            },
                            ""lineno"": 1,
                            ""op"": {
                                ""_type"": ""Mult""
                            },
                            ""right"": {
                                ""_type"": ""Name"",
                                ""col_offset"": 14,
                                ""ctx"": {
                                    ""_type"": ""Load""
                                },
                                ""end_col_offset"": 15,
                                ""end_lineno"": 1,
                                ""id"": ""i"",
                                ""lineno"": 1
                            }
                        },
                        ""end_col_offset"": 40,
                        ""end_lineno"": 1,
                        ""generators"": [
                            {
                                ""_type"": ""comprehension"",
                                ""ifs"": [],
                                ""is_async"": 0,
                                ""iter"": {
                                    ""_type"": ""Call"",
                                    ""args"": [
                                        {
                                            ""_type"": ""Constant"",
                                            ""col_offset"": 31,
                                            ""end_col_offset"": 32,
                                            ""end_lineno"": 1,
                                            ""kind"": null,
                                            ""lineno"": 1,
                                            ""n"": 1,
                                            ""s"": 1,
                                            ""value"": 1
                                        },
                                        {
                                            ""_type"": ""BinOp"",
                                            ""col_offset"": 34,
                                            ""end_col_offset"": 38,
                                            ""end_lineno"": 1,
                                            ""left"": {
                                                ""_type"": ""Name"",
                                                ""col_offset"": 34,
                                                ""ctx"": {
                                                    ""_type"": ""Load""
                                                },
                                                ""end_col_offset"": 36,
                                                ""end_lineno"": 1,
                                                ""id"": ""i0"",
                                                ""lineno"": 1
                                            },
                                            ""lineno"": 1,
                                            ""op"": {
                                                ""_type"": ""Add""
                                            },
                                            ""right"": {
                                                ""_type"": ""Constant"",
                                                ""col_offset"": 37,
                                                ""end_col_offset"": 38,
                                                ""end_lineno"": 1,
                                                ""kind"": null,
                                                ""lineno"": 1,
                                                ""n"": 1,
                                                ""s"": 1,
                                                ""value"": 1
                                            }
                                        }
                                    ],
                                    ""col_offset"": 25,
                                    ""end_col_offset"": 39,
                                    ""end_lineno"": 1,
                                    ""func"": {
                                        ""_type"": ""Name"",
                                        ""col_offset"": 25,
                                        ""ctx"": {
                                            ""_type"": ""Load""
                                        },
                                        ""end_col_offset"": 30,
                                        ""end_lineno"": 1,
                                        ""id"": ""range"",
                                        ""lineno"": 1
                                    },
                                    ""keywords"": [],
                                    ""lineno"": 1
                                },
                                ""target"": {
                                    ""_type"": ""Name"",
                                    ""col_offset"": 20,
                                    ""ctx"": {
                                        ""_type"": ""Store""
                                    },
                                    ""end_col_offset"": 21,
                                    ""end_lineno"": 1,
                                    ""id"": ""i"",
                                    ""lineno"": 1
                                }
                            }
                        ],
                        ""lineno"": 1
                    }
                ],
                ""col_offset"": 7,
                ""end_col_offset"": 41,
                ""end_lineno"": 1,
                ""func"": {
                    ""_type"": ""Name"",
                    ""col_offset"": 7,
                    ""ctx"": {
                        ""_type"": ""Load""
                    },
                    ""end_col_offset"": 10,
                    ""end_lineno"": 1,
                    ""id"": ""sum"",
                    ""lineno"": 1
                },
                ""keywords"": [],
                ""lineno"": 1
            }
        }
    ],
    ""type_ignores"": []
}";

        public string ProblemName;
        public ILookupParameter<ILog> LogParameter
        {
            get { return (ILookupParameter<ILog>)Parameters["Log"]; }
        }

        public ILog Log
        {
            get
            {
                return LogParameter.ActualValue;
            }
        }

        [StorableConstructor]
        protected InitialGivenProgramCreator(bool deserializing) : base(deserializing) { }
        protected InitialGivenProgramCreator(InitialGivenProgramCreator original, Cloner cloner) : base(original, cloner) { }
        public InitialGivenProgramCreator()
          : base()
        {
            Parameters.Add(new LookupParameter<ILog>("Log", "log of Engine"));
            Parameters.Add(new ValueLookupParameter<ICFGProblemData>("ProblemData", "The problem data on which the context free grammer solution should be evaluated."));


            variable_type_dictionary = new Dictionary<string, string>();
            variable_name_mapping_dictionary = new Dictionary<string, string>();
            function_return_type_dictionary = new Dictionary<string, string>();
            ProblemName = "";
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new InitialGivenProgramCreator(this, cloner);
        }

        public override ISymbolicExpressionTree CreateTree(IRandom random, ISymbolicExpressionGrammar grammar, int maxTreeLength, int maxTreeDepth)
        {
            return Create(random, grammar, maxTreeLength, maxTreeDepth);
        }

        protected override ISymbolicExpressionTree Create(IRandom random)
        {
            if (ClonedSymbolicExpressionTreeGrammarParameter.ActualValue != null && MaximumSymbolicExpressionTreeLengthParameter.ActualValue != null && MaximumSymbolicExpressionTreeDepthParameter.ActualValue != null)
            {
                return Create(random, ClonedSymbolicExpressionTreeGrammarParameter.ActualValue,
                MaximumSymbolicExpressionTreeLengthParameter.ActualValue.Value, MaximumSymbolicExpressionTreeDepthParameter.ActualValue.Value);
            }
            else
                throw new ArgumentException("null arguemnt!");
        }

        public ISymbolicExpressionTree Create(IRandom random, ISymbolicExpressionGrammar grammar, int maxTreeLength, int maxTreeDepth)
        {
            
            ProblemName = String.Concat(ProblemName.Where(c => !Char.IsWhiteSpace(c)));
            //LogParameter.ActualValue.LogMessage("problem name:" + ProblemName);

            //initialize variable type
            string VariableRuleStart = "Rule: <";
            string VariableRuleEnd = "_var>";
            var variableSymbols = grammar.Symbols.Where(x => x.Enabled && x is GroupSymbol && x.Name.StartsWith(VariableRuleStart) && x.Name.EndsWith(VariableRuleEnd)).Cast<GroupSymbol>();
            foreach (var varSy in variableSymbols)
            {
                string type = varSy.Name.Substring(VariableRuleStart.Length, varSy.Name.Length - VariableRuleStart.Length - VariableRuleEnd.Length);
                var tempDic = varSy.Symbols.Where(s => s.Enabled).ToDictionary(s => s.Name.Trim(new char[] { '\'', '"' }), x => type);
                foreach (string key in tempDic.Keys)
                {
                    variable_type_dictionary[key] = tempDic[key];
                }
            }

            //initialize function return type
            var functionSymbols = grammar.Symbols.Where(x => x.Enabled && x is GroupSymbol && (x.Name.Equals("Rule: <int>") || x.Name.Equals("Rule: <float>") || x.Name.Equals("Rule: <string>") || x.Name.Equals("Rule: <bool>"))).Cast<GroupSymbol>();
            foreach (var funcSy in functionSymbols)
            {
                var tempType = funcSy.Name.Substring("Rule: <".Length, funcSy.Name.Length - "Rule: <".Length - 1);
                var tempDic = funcSy.Symbols.Where(s => s.Enabled).ToDictionary(s => s.Name.Trim(new char[] { '\'', '"' }), x => tempType);
                foreach (string key in tempDic.Keys)
                {
                    //LogParameter.ActualValue.LogMessage(key + " -------->" + tempDic[key]);
                    function_return_type_dictionary[key] = tempDic[key];
                }
            }

            /*#region generate grammar information from code
            string filePath = "..\\..\\HeuristicLab.CFGGP\\HeuristicLab.Algorithms.CFG.MultiObjective\\PSB1_Solution\\json\\ChatGPT\\" + ProblemName + ".txt";
            if (File.Exists(filePath))
            {
                string fileContents = File.ReadAllText(filePath);
                dynamic jsonTree;
                jsonTree = JsonConvert.DeserializeObject(fileContents);
                JArray treeBody = jsonTree.body;
                //PrintJsonTree(treeBody, 0);

                CustomTreeNode root = new CustomTreeNode();
                ParseJsonToCustomTree(treeBody, root, "root");
                PrintCustomTree(root, 0);

                StringBuilder code_grammar = new StringBuilder();


            }
            else
            {
                LogParameter.ActualValue.LogMessage("Can not find file: " + filePath);
            }
            #endregion*/
            StringBuilder code_grammar = new StringBuilder();

            SymbolicExpressionTree tree = new SymbolicExpressionTree();
            var rootNode = (SymbolicExpressionTreeTopLevelNode)grammar.ProgramRootSymbol.CreateTreeNode();
            if (rootNode.HasLocalParameters) rootNode.ResetLocalParameters(random);
            rootNode.SetGrammar(grammar.CreateExpressionTreeGrammar());

            var startNode = (SymbolicExpressionTreeTopLevelNode)grammar.StartSymbol.CreateTreeNode();
            if (startNode.HasLocalParameters) startNode.ResetLocalParameters(random);
            startNode.SetGrammar(grammar.CreateExpressionTreeGrammar());

            rootNode.AddSubtree(startNode);


            #region tree consturct from json file
            // read json from file
            string filePath = "..\\..\\HeuristicLab.CFGGP\\HeuristicLab.Algorithms.CFG.MultiObjective\\PSB1_Solution\\json\\ChatGPT\\"+ ProblemName + ".txt";
            if (File.Exists(filePath))
            {
                string fileContents = File.ReadAllText(filePath);
                // read json from json string
                dynamic jsonTree;
                jsonTree = JsonConvert.DeserializeObject(fileContents);
                JArray treeBody = jsonTree.body;
                //PrintJsonTree(treeBody, 0);

                CustomTreeNode root = new CustomTreeNode();
                ParseJsonToCustomTree(treeBody, root, "root");
                PrintCustomTree(root, 0);
                LogParameter.ActualValue.LogMessage("-----------------------");


                //predefined
                var allowedChildSymbol = grammar.GetAllowedChildSymbols(startNode.Symbol, 0);
                var node = allowedChildSymbol.First().CreateTreeNode();
                if (node.HasLocalParameters) node.ResetLocalParameters(random);
                startNode.AddSubtree(node);

                //<statement><code> or <statement>
                allowedChildSymbol = grammar.GetAllowedChildSymbols(node.Symbol, 0);
                List<ISymbolicExpressionTreeNode> rule_code_list = new List<ISymbolicExpressionTreeNode>();
                for (int i = 0; i < root.numberOfLines - 1; i++)
                {
                    rule_code_list.Add(allowedChildSymbol.ElementAt(1).CreateTreeNode());//<statement><code>
                }
                rule_code_list.Add(allowedChildSymbol.Last().CreateTreeNode());//<statement>
                foreach (var tempNode in rule_code_list)
                {
                    if (tempNode.HasLocalParameters) tempNode.ResetLocalParameters(random);
                }
                node.AddSubtree(rule_code_list[0]);
                //LogParameter.ActualValue.LogMessage("rule_code_list:" + rule_code_list[0].Symbol.Name);
                if (!BuildSymbolicExpressionTree(random, root, rule_code_list[0], grammar, 1))
                {
                    BuildDummyLine(random, rule_code_list[0], grammar);
                }
                
                for (int i = 1; i < root.numberOfLines; i++)
                {
                    // special for handling type "For", It need to call assign first to initialize loop iterator
                    /*if (root.Components["_type" + (i + 1).ToString()].ToString().Contains("For"))
                    {
                        var additionalNodeForInitIterator = allowedChildSymbol.ElementAt(1).CreateTreeNode();
                        if (additionalNodeForInitIterator.HasLocalParameters) additionalNodeForInitIterator.ResetLocalParameters(random);
                        rule_code_list[i - 1].AddSubtree(additionalNodeForInitIterator);

                        //<assign>
                        var allowedChildForInitIterator = grammar.GetAllowedChildSymbols(additionalNodeForInitIterator.Symbol, 0);
                        var forInitIterator_node1 = allowedChildForInitIterator.First().CreateTreeNode();//initialize random value to avoid error
                        foreach (var symbol in allowedChildForInitIterator)
                        {
                            if (symbol.ToString().Contains("assign"))
                            {
                                forInitIterator_node1 = symbol.CreateTreeNode();//update to actual value
                                if (forInitIterator_node1.HasLocalParameters) forInitIterator_node1.ResetLocalParameters(random);
                                additionalNodeForInitIterator.AddSubtree(forInitIterator_node1);
                                break;
                            }
                        }
                        //<int_assign> note: only support int interator for now
                        allowedChildForInitIterator = grammar.GetAllowedChildSymbols(forInitIterator_node1.Symbol, 0);
                        var forInitIterator_node2 = allowedChildForInitIterator.First().CreateTreeNode();
                        foreach (var symbol in allowedChildForInitIterator)
                        {
                            if (symbol.ToString().Contains("int_assign"))
                            {
                                forInitIterator_node2 = symbol.CreateTreeNode();//update to actual value
                                if (forInitIterator_node2.HasLocalParameters) forInitIterator_node2.ResetLocalParameters(random);
                                forInitIterator_node1.AddSubtree(forInitIterator_node2);
                                break;
                            }
                        }
                        //<int_var> = <int>
                        allowedChildForInitIterator = grammar.GetAllowedChildSymbols(forInitIterator_node2.Symbol, 0);
                        var forInitIterator_node3 = allowedChildForInitIterator.First().CreateTreeNode();
                        if (forInitIterator_node3.HasLocalParameters) forInitIterator_node3.ResetLocalParameters(random);
                        forInitIterator_node2.AddSubtree(forInitIterator_node3);
                        //assign i0 to <int_var> note: mapping between custom variable to fixed variable no implemented
                        allowedChildForInitIterator = grammar.GetAllowedChildSymbols(forInitIterator_node3.Symbol, 0);
                        var forInitIterator_node4_1 = allowedChildForInitIterator.First().CreateTreeNode();
                        if (forInitIterator_node4_1.HasLocalParameters) forInitIterator_node4_1.ResetLocalParameters(random);
                        forInitIterator_node3.AddSubtree(forInitIterator_node4_1);
                        //<number>
                        allowedChildForInitIterator = grammar.GetAllowedChildSymbols(forInitIterator_node3.Symbol, 1);
                        var forInitIterator_node4_2 = allowedChildForInitIterator.First().CreateTreeNode();
                        foreach (var symbol in allowedChildForInitIterator)
                        {
                            if (symbol.ToString().Contains("number"))
                            {
                                forInitIterator_node4_2 = symbol.CreateTreeNode();//update to actual value
                                if (forInitIterator_node4_2.HasLocalParameters) forInitIterator_node4_2.ResetLocalParameters(random);
                                forInitIterator_node3.AddSubtree(forInitIterator_node4_2);
                                break;
                            }
                        }
                        //handle type of initialization value
                        allowedChildForInitIterator = grammar.GetAllowedChildSymbols(forInitIterator_node4_2.Symbol, 0);
                        string iterator_initValueType;
                        if(((CustomTreeNode)((CustomTreeNode)root.Components["iter" + (i + 1).ToString()]).Components["args1"]).Components.ContainsKey("_type2"))
                        {
                            iterator_initValueType = ((CustomTreeNode)((CustomTreeNode)root.Components["iter" + (i + 1).ToString()]).Components["args1"]).Components["_type1"].ToString();
                            if (iterator_initValueType.Contains("Constant"))
                            {
                                //handle multiple digit: <num> OR <num><number>
                                string value = ((CustomTreeNode)((CustomTreeNode)root.Components["iter" + (i + 1).ToString()]).Components["args1"]).Components["value1"].ToString();
                                List<ISymbolicExpressionTreeNode> Number_node_list = new List<ISymbolicExpressionTreeNode>();
                                for (int k = 0; k < value.Length - 1; k++)
                                {
                                    //<num><number>
                                    Number_node_list.Add(allowedChildForInitIterator.ElementAt(1).CreateTreeNode());
                                }
                                Number_node_list.Add(allowedChildForInitIterator.Last().CreateTreeNode());
                                foreach (var tempNode in Number_node_list)
                                {
                                    if (tempNode.HasLocalParameters) tempNode.ResetLocalParameters(random);
                                }
                                // <num> OR <num><number>
                                forInitIterator_node4_2.AddSubtree(Number_node_list[0]);
                                allowedChildForInitIterator = grammar.GetAllowedChildSymbols(Number_node_list[0].Symbol, 0);
                                List<ISymbolicExpressionTreeNode> num_node_list = new List<ISymbolicExpressionTreeNode>();
                                int j = 0;
                                foreach (var symbol in allowedChildForInitIterator)
                                {
                                    if (symbol.ToString().Contains(value[j]))
                                    {
                                        num_node_list.Add(symbol.CreateTreeNode());
                                        if (num_node_list.Last().HasLocalParameters) num_node_list.Last().ResetLocalParameters(random);
                                        Number_node_list[j].AddSubtree(num_node_list.Last());
                                        break;
                                    }
                                }
                                j++;
                                while (j < value.Length)
                                {
                                    Number_node_list[j - 1].AddSubtree(Number_node_list[j]);
                                    foreach (var symbol in allowedChildForInitIterator)
                                    {
                                        if (symbol.ToString().Contains(value[j]))
                                        {
                                            num_node_list.Add(symbol.CreateTreeNode());
                                            if (num_node_list.Last().HasLocalParameters) num_node_list.Last().ResetLocalParameters(random);
                                            Number_node_list[j].AddSubtree(num_node_list.Last());
                                            break;
                                        }
                                    }
                                    j++;
                                }
                            }
                            else
                            {
                                // TODO: handle non constant initial variable for iterator later
                                throw new Exception("TODO: handle non constant initial variable for iterator later");
                            }
                        }
                        else
                        {
                            //initialize as 0 if not given
                            iterator_initValueType = "Constant";
                        }

                        additionalNodeForInitIterator.AddSubtree(rule_code_list[i]);
                        BuildSymbolicExpressionTree(random, root, rule_code_list[i], grammar, i+1);
                    }
                    else
                    {
                        rule_code_list[i - 1].AddSubtree(rule_code_list[i]);
                        BuildSymbolicExpressionTree(random, root, rule_code_list[i], grammar, i+1);
                    }*/

                    rule_code_list[i - 1].AddSubtree(rule_code_list[i]);
                    if (!BuildSymbolicExpressionTree(random, root, rule_code_list[i], grammar, i + 1))
                    {
                        BuildDummyLine(random, rule_code_list[i], grammar);
                    }
                }

                #endregion


                tree.Root = rootNode;
                return tree;
            }
            else
            {
                LogParameter.ActualValue.LogMessage("Can not find file: " + filePath);
                return tree;
            }
        }

        private void BuildDummyLine(IRandom random, ISymbolicExpressionTreeNode symbolicRoot, ISymbolicExpressionGrammar grammar)
        {
            //node1 " <assign>
            var allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, 0);
            List<ISymbolicExpressionTreeNode> assignSymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>
            {
                symbolicRoot
            };

            if (symbolicRoot.Symbol.Name.Contains("statement"))
            {
                foreach (var symbol in allowedChildSymbol_assign)
                {

                    if (symbol.ToString().Contains("<simple_stmt>"))
                    {
                        assignSymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                        if (assignSymbolicTreeNodeList.Last().HasLocalParameters) assignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                        assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count - 2].AddSubtree(assignSymbolicTreeNodeList.Last());

                        allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(assignSymbolicTreeNodeList.Last().Symbol, 0);
                        foreach (var symbol1 in allowedChildSymbol_assign)
                        {
                            if (symbol1.ToString().Contains("<assign>"))
                            {
                                assignSymbolicTreeNodeList.Add(symbol1.CreateTreeNode());
                                if (assignSymbolicTreeNodeList.Last().HasLocalParameters) assignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count - 2].AddSubtree(assignSymbolicTreeNodeList.Last());
                                break;
                            }
                        }
                    }
                    else if (symbol.ToString().Contains("<assign>"))
                    {
                        assignSymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                        if (assignSymbolicTreeNodeList.Last().HasLocalParameters) assignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                        symbolicRoot.AddSubtree(assignSymbolicTreeNodeList.Last());
                        break;
                    }
                }
            }

            allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(assignSymbolicTreeNodeList.Last().Symbol, 0);
            assignSymbolicTreeNodeList.Add(allowedChildSymbol_assign.First().CreateTreeNode());
            assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count - 2].AddSubtree(assignSymbolicTreeNodeList.Last());

            if(assignSymbolicTreeNodeList.Last().Symbol.Name.Contains("assign"))
            {
                allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(assignSymbolicTreeNodeList.Last().Symbol, 0);
                assignSymbolicTreeNodeList.Add(allowedChildSymbol_assign.First().CreateTreeNode());
                assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count - 2].AddSubtree(assignSymbolicTreeNodeList.Last());
            }

            allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(assignSymbolicTreeNodeList.Last().Symbol, 0);
            assignSymbolicTreeNodeList.Add(allowedChildSymbol_assign.First().CreateTreeNode());
            assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count - 2].AddSubtree(assignSymbolicTreeNodeList.Last());

            allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count - 2].Symbol, 1);
            foreach(var symbol in allowedChildSymbol_assign)
            {
                if (symbol.Name.Contains("_var"))
                {
                    assignSymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                    assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count - 3].AddSubtree(assignSymbolicTreeNodeList.Last());
                }
            }

            allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(assignSymbolicTreeNodeList.Last().Symbol, 0);
            assignSymbolicTreeNodeList.Add(allowedChildSymbol_assign.First().CreateTreeNode());
            assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count - 2].AddSubtree(assignSymbolicTreeNodeList.Last());
        }

        void PrintJsonTree(JToken token, int depth)
        {
            
            if (token is JProperty)
            {
                string[] uselessInfo = { "type_comment", "col_offset", "end_col_offset", "end_lineno", "lineno", "ctx", "kind", "n", "s" };

                var jProp = (JProperty)token;
                var spacer = string.Join("", Enumerable.Range(0, depth).Select(_ => "\t"));
                var val = jProp.Value is JValue ? ((JValue)jProp.Value).Value : "-";

                if (!uselessInfo.Contains(jProp.Name))
                {
                    LogParameter.ActualValue.LogMessage($"{spacer}{jProp.Name}  -> {val}");
                }

                

                foreach (var child in jProp.Children())
                {
                    PrintJsonTree(child, depth + 1);
                }
            }
            else if (token is JObject)
            {
                foreach (var child in ((JObject)token).Children())
                {
                    PrintJsonTree(child, depth + 1);
                }
            }
            else if (token is JArray)
            {
                foreach (var item in token)
                {
                    PrintJsonTree(item, depth + 1);
                }
            } 
        }

        void ParseJsonToCustomTree(JToken token, CustomTreeNode node, string connectionTypeToParent)
        {
            if (token is JProperty)
            {
                string[] uselessInfo = { "type_comment", "col_offset", "end_col_offset", "end_lineno", "lineno", "ctx", "kind", "n", "s", "step", "upper" };

                var jProp = (JProperty)token;
                if (!uselessInfo.Contains(jProp.Name))
                {
                    if(jProp.Value is JValue)
                    {
                        string name = jProp.Name;
                        if (name.Contains("_type"))
                        {
                            node.numberOfLines += 1;
                        }
                        name = name + node.numberOfLines;
                        //LogParameter.ActualValue.LogMessage("fisrt!!!!!!!" + ((JValue)jProp.Value).Value.ToString());
                        if (((JValue)jProp.Value).Value != null)
                        {
                            
                            if (((JValue)jProp.Value).Value.ToString().Equals("\n"))
                            {
                                //LogParameter.ActualValue.LogMessage("Here!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                node.Components.Add(name, "\\n");
                            }
                            else
                            {
                                node.Components.Add(name, ((JValue)jProp.Value).Value.ToString());
                            }
                            //LogParameter.ActualValue.LogMessage(((JValue)jProp.Value).Value.ToString() + " --> type:" + ((JValue)jProp.Value).Value.GetType().ToString());
                        }
                        else
                        {
                            node.Components.Add(name, "empty");
                        }
                        
                        //LogParameter.ActualValue.LogMessage($"{name}  -> {((JValue)jProp.Value).Value}");
                    }
                    else
                    {
                        foreach (var childtoken in jProp.Children())
                        {
                            //LogParameter.ActualValue.LogMessage($"{jProp.Name}");
                            CustomTreeNode childNode = new CustomTreeNode();
                            childNode.connectionTypeToParent = jProp.Name;
                            string name = jProp.Name;
                            if (name.Contains("_type"))
                            {
                                node.numberOfLines += 1;
                            }
                            name = name + node.numberOfLines;
                            childNode.parentNode = node;
                            node.Components.Add(name, childNode);
                            ParseJsonToCustomTree(childtoken, childNode, connectionTypeToParent);
                        }
                    }
                }
                
                
            }
            else if (token is JObject)
            {
                foreach (var childtoken in ((JObject)token).Children())
                {
                    ParseJsonToCustomTree(childtoken, node, connectionTypeToParent);
                }
            }
            else if (token is JArray)
            {
                foreach (var tok in token)
                {
                    ParseJsonToCustomTree(tok, node, connectionTypeToParent);
                }
            }
        }

        void PrintCustomTree(CustomTreeNode root, int depth)
        {
            if (root.Components.Count > 0)
            {
                foreach(string key in root.Components.Keys)
                {
                    if (root.Components[key] is string)
                    {
                        var spacer = string.Join("", Enumerable.Range(0, depth).Select(_ => "\t"));
                        LogParameter.ActualValue.LogMessage($"{spacer}{key}  -> {root.Components[key]}");
                    }
                    else
                    {
                        var spacer = string.Join("", Enumerable.Range(0, depth).Select(_ => "\t"));
                        LogParameter.ActualValue.LogMessage($"{spacer}{key}  ->");
                        PrintCustomTree((CustomTreeNode)root.Components[key], depth + 1);
                    }
                    
                    
                }
            }
        }

        bool BuildSymbolicExpressionTree(IRandom random, CustomTreeNode customRoot, ISymbolicExpressionTreeNode symbolicRoot, ISymbolicExpressionGrammar grammar, int lineNumber, int childSubtreeIndex = 0)
        {
            string typeWithLineNumber = "_type" + lineNumber.ToString();
            /*LogParameter.ActualValue.LogMessage(typeWithLineNumber);
            if (!customRoot.Components.ContainsKey(typeWithLineNumber))
            {
                LogParameter.ActualValue.LogMessage("-----------------------");
                PrintCustomTree(customRoot, 0);
                LogParameter.ActualValue.LogMessage("----------------------");
            }*/
            var type = customRoot.Components[typeWithLineNumber];
            //LogParameter.ActualValue.LogMessage((string)type);
            switch(type)
            {
                case "If":
                    #region case if
                    //Log.LogMessage("case if -- Symbolic root:" + symbolicRoot.Symbol.Name);
                    List<ISymbolicExpressionTreeNode> if_SymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>();

                    //<compound_stmt>
                    var allowedChildSymbol_if = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                    bool if_hasCompund_stmt = false;
                    foreach (var symbol in allowedChildSymbol_if)
                    {
                        if (symbol.Name.Contains("<compound_stmt>"))
                        {
                            if_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                            if_hasCompund_stmt = true;
                        }
                    }
                    if (!if_hasCompund_stmt)
                    {
                        Log.LogMessage("case if: this problem do not support if statement");
                        using (StreamWriter writer = new StreamWriter(grammar_outPath))
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("<statement> ::= 'if ");
                            if (((CustomTreeNode)customRoot.Components["test" + lineNumber.ToString()]).Components["_type1"].ToString() == "UnaryOp")
                            {
                                if(((CustomTreeNode)(((CustomTreeNode)customRoot.Components["test" + lineNumber.ToString()]).Components["op1"])).Components["_type1"].ToString() == "Not")
                                {
                                    stringBuilder.Append("not ' ");
                                    stringBuilder.Append("<" + GetVariableType(((CustomTreeNode)(((CustomTreeNode)customRoot.Components["test" + lineNumber.ToString()]).Components["operand1"])).Components["id1"].ToString()) + "_var> ':\\n'");
                                }
                                else
                                {
                                    Log.LogMessage("grammar extraction - case if: Unary Operator not implemented");
                                }
                            }
                            else
                            {
                                Log.LogMessage("grammar extraction - case if: test type not implemented");
                            }
                            stringBuilder.Append(" <code> ");
                            if (customRoot.Components.ContainsKey("orelse" + lineNumber.ToString()))
                            {
                                if (((CustomTreeNode)customRoot.Components["orelse" + lineNumber.ToString()]).Components.Count > 0)
                                {
                                    stringBuilder.Append("'\\n else: ' <code>");
                                }
                            }
                            writer.WriteLine(stringBuilder.ToString());
                        }
                        return false;
                    }


                    //<if>
                    allowedChildSymbol_if = grammar.GetAllowedChildSymbols(if_SymbolicTreeNodeList.Last().Symbol, 0);
                    foreach(var symbol in allowedChildSymbol_if)
                    {
                        if (symbol.Name.Contains("<if>"))
                        {
                            if_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                            if_SymbolicTreeNodeList[if_SymbolicTreeNodeList.Count - 2].AddSubtree(if_SymbolicTreeNodeList.Last());
                            break;
                        }
                    }
                    

                    int if_tempIndex = 0;
                    bool if_hasElse = false;
                    if (customRoot.Components.ContainsKey("orelse" + lineNumber.ToString()))
                    {
                        if (((CustomTreeNode)customRoot.Components["orelse" + lineNumber.ToString()]).Components.Count > 0)
                        {
                            allowedChildSymbol_if = grammar.GetAllowedChildSymbols(if_SymbolicTreeNodeList.Last().Symbol, 0);
                            if_SymbolicTreeNodeList.Add(allowedChildSymbol_if.Last().CreateTreeNode());// 'if '<bool>':{:\n'<code>':}else:{:\n'<code>':}'
                            if_SymbolicTreeNodeList[if_SymbolicTreeNodeList.Count - 2].AddSubtree(if_SymbolicTreeNodeList.Last());
                            if_tempIndex = if_SymbolicTreeNodeList.Count - 1;
                            if_hasElse = true;
                        }
                    }

                    if (!if_hasElse)
                    {
                        allowedChildSymbol_if = grammar.GetAllowedChildSymbols(if_SymbolicTreeNodeList.Last().Symbol, 0);
                        if_SymbolicTreeNodeList.Add(allowedChildSymbol_if.First().CreateTreeNode());// 'if '<bool>':{:\n'<code>':}
                        if_SymbolicTreeNodeList[if_SymbolicTreeNodeList.Count - 2].AddSubtree(if_SymbolicTreeNodeList.Last());
                        if_tempIndex = if_SymbolicTreeNodeList.Count - 1;
                    }

                    
                    //handle <bool>
                    if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["test" + lineNumber.ToString()], if_SymbolicTreeNodeList[if_tempIndex], grammar, 1, 0))
                    {
                        BuildDummyValue(random, if_SymbolicTreeNodeList[if_tempIndex], grammar, 0);


                        Log.LogMessage("case if: unable to handle <bool>");
                        //return false;
                    }


                    //handle if <code>
                    var if_BodyTreeNode = (CustomTreeNode)customRoot.Components["body" + lineNumber.ToString()];
                    List<ISymbolicExpressionTreeNode> if_BodylineSymbolicTreeNode_list = new List<ISymbolicExpressionTreeNode>();
                    //LogParameter.ActualValue.LogMessage(if_SymbolicTreeNodeList[if_tempIndex].Symbol.Name);
                    allowedChildSymbol_if = grammar.GetAllowedChildSymbols(if_SymbolicTreeNodeList[if_tempIndex].Symbol, 1);
                    //LogParameter.ActualValue.LogMessage("number of line: " + if_BodyTreeNode.numberOfLines);
                    for (int i = 0; i < if_BodyTreeNode.numberOfLines - 1; i++)
                    {
                        if_BodylineSymbolicTreeNode_list.Add(allowedChildSymbol_if.ElementAt(1).CreateTreeNode());
                    }
                    if_BodylineSymbolicTreeNode_list.Add(allowedChildSymbol_if.Last().CreateTreeNode());
                    foreach (var tempNode in if_BodylineSymbolicTreeNode_list)
                    {
                        if (tempNode.HasLocalParameters) tempNode.ResetLocalParameters(random);
                    }
                    if_SymbolicTreeNodeList[if_tempIndex].AddSubtree(if_BodylineSymbolicTreeNode_list[0]);
                    if (!BuildSymbolicExpressionTree(random, if_BodyTreeNode, if_BodylineSymbolicTreeNode_list[0], grammar, 1))
                    {
                        Log.LogMessage("case if: unable to handle <code> line 1");
                        BuildDummyLine(random, if_BodylineSymbolicTreeNode_list[0], grammar);
                        Log.LogMessage("case if: unable to handle <code> line 1");
                        //return false;
                    }
                    for (int i = 1; i < if_BodyTreeNode.numberOfLines; i++)
                    {
                        if_BodylineSymbolicTreeNode_list[i - 1].AddSubtree(if_BodylineSymbolicTreeNode_list[i]);
                        if (!BuildSymbolicExpressionTree(random, if_BodyTreeNode, if_BodylineSymbolicTreeNode_list[i], grammar, i+1))
                        {
                            BuildDummyLine(random, if_BodylineSymbolicTreeNode_list[i], grammar);
                            Log.LogMessage("case if: unable to handle <code> line " + (i + 1).ToString());
                            //return false;
                        }
                    }


                    //handle else <code>
                    if (if_hasElse)
                    {
                        var if_elseTreeNode = (CustomTreeNode)customRoot.Components["orelse" + lineNumber.ToString()];
                        List<ISymbolicExpressionTreeNode> if_elselineSymbolicTreeNode_list = new List<ISymbolicExpressionTreeNode>();
                        allowedChildSymbol_if = grammar.GetAllowedChildSymbols(if_SymbolicTreeNodeList[if_tempIndex].Symbol, 2);
                        for (int i = 0; i < if_elseTreeNode.numberOfLines - 1; i++)
                        {
                            if_elselineSymbolicTreeNode_list.Add(allowedChildSymbol_if.ElementAt(1).CreateTreeNode());
                        }
                        if_elselineSymbolicTreeNode_list.Add(allowedChildSymbol_if.Last().CreateTreeNode());
                        foreach (var tempNode in if_elselineSymbolicTreeNode_list)
                        {
                            if (tempNode.HasLocalParameters) tempNode.ResetLocalParameters(random);
                        }
                        if_SymbolicTreeNodeList[if_tempIndex].AddSubtree(if_elselineSymbolicTreeNode_list[0]);
                        if (!BuildSymbolicExpressionTree(random, if_elseTreeNode, if_elselineSymbolicTreeNode_list[0], grammar, 1))
                        {
                            BuildDummyLine(random, if_elselineSymbolicTreeNode_list[0], grammar);
                            Log.LogMessage("case if: unable to handle else <code> line 1");
                            //return false;
                        }
                        for (int i = 1; i < if_elseTreeNode.numberOfLines - 1; i++)
                        {
                            if_elselineSymbolicTreeNode_list[i - 1].AddSubtree(if_elselineSymbolicTreeNode_list[i]);
                            if (!BuildSymbolicExpressionTree(random, if_elseTreeNode, if_elselineSymbolicTreeNode_list[i], grammar, i))
                            {
                                BuildDummyLine(random, if_elselineSymbolicTreeNode_list[i], grammar);
                                Log.LogMessage("case if: unable to handle else <code> line 1");
                                //return false;
                            }
                        }
                    }


                    symbolicRoot.AddSubtree(if_SymbolicTreeNodeList.First());
                    return true;
                #endregion

                case "Assign":
                    #region case assign
                    //node1 " <assign>
                    var allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                    List<ISymbolicExpressionTreeNode> assignSymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>();

                    if (symbolicRoot.Symbol.Name.Contains("statement"))
                    {
                        foreach (var symbol in allowedChildSymbol_assign)
                        {

                            if (symbol.ToString().Contains("<simple_stmt>"))
                            {
                                assignSymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                if (assignSymbolicTreeNodeList.Last().HasLocalParameters) assignSymbolicTreeNodeList.Last().ResetLocalParameters(random);

                                allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(assignSymbolicTreeNodeList.Last().Symbol, 0);
                                foreach (var symbol1 in allowedChildSymbol_assign)
                                {
                                    if (symbol1.ToString().Contains("<assign>"))
                                    {
                                        assignSymbolicTreeNodeList.Add(symbol1.CreateTreeNode());
                                        if (assignSymbolicTreeNodeList.Last().HasLocalParameters) assignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                        assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count-2].AddSubtree(assignSymbolicTreeNodeList.Last());
                                        break;
                                    }
                                }
                            }
                            else if (symbol.ToString().Contains("<assign>"))
                            {
                                assignSymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                if (assignSymbolicTreeNodeList.Last().HasLocalParameters) assignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(assignSymbolicTreeNodeList.Last());
                                break;
                            }
                        }
                    }


                    //node2: determine what type of assign it use
                    string assignVariableName = "";
                    if (((CustomTreeNode)customRoot.Components["targets" + lineNumber.ToString()]).Components.ContainsKey("id1"))
                    {
                        //assignVariableName = ((CustomTreeNode)(((CustomTreeNode)customRoot.Components["targets" + lineNumber.ToString()]).Components["value1"])).Components["id1"].ToString();
                        assignVariableName = ((CustomTreeNode)customRoot.Components["targets" + lineNumber.ToString()]).Components["id1"].ToString();
                    }
                    else
                    {
                        LogParameter.ActualValue.LogMessage("Assign left variable type not supported!");
                        return false;
                    }
                    string assignType = GetVariableType(assignVariableName);
                    if (assignType.Equals("unknown"))
                    {
                        if (((CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Name"))//when right side of assign is variable
                        {
                            string unseenVariableType = GetVariableType(((CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()]).Components["id1"].ToString());
                            if (!unseenVariableType.Equals("unknown"))//when right side variable type is known
                            {
                                foreach(string key in variable_type_dictionary.Keys)
                                {
                                    //LogParameter.ActualValue.LogMessage("reached here!???");
                                    if (!key.Contains("in") && !key.Contains("res"))// can not map input or output varibale with unseen variable
                                    {
                                        //LogParameter.ActualValue.LogMessage("reached here!");
                                        if (variable_type_dictionary[key].Equals(unseenVariableType))//when the type of additional variable matches unknwon variable type
                                        {
                                            if (!variable_name_mapping_dictionary.ContainsValue(key))//when the additonal variable is not mapped yet
                                            {
                                                variable_name_mapping_dictionary[((CustomTreeNode)customRoot.Components["targets" + lineNumber.ToString()]).Components["id1"].ToString()] = key;
                                                assignVariableName = key;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LogParameter.ActualValue.LogMessage("unable to map variable name");
                                return false;
                            }
                        }
                        else if (((CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Constant"))
                        {
                            
                            string assign_temp_constant = ((CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()]).Components["value1"].ToString();
                            string unseenVariableType = GetConstantType(assign_temp_constant);
                            
                            foreach (string key in variable_type_dictionary.Keys)
                            {
                                //LogParameter.ActualValue.LogMessage("reached here!???");
                                if (!key.Contains("in") && !key.Contains("res"))// can not map input or output varibale with unseen variable
                                {
                                    //LogParameter.ActualValue.LogMessage("reached here!");
                                    if (variable_type_dictionary[key].Equals(unseenVariableType))//when the type of additional variable matches unknwon variable type
                                    {
                                        if (!variable_name_mapping_dictionary.ContainsValue(key))//when the additonal variable is not mapped yet
                                        {
                                            variable_name_mapping_dictionary[((CustomTreeNode)customRoot.Components["targets" + lineNumber.ToString()]).Components["id1"].ToString()] = key;
                                            assignVariableName = key;
                                        }
                                    }
                                }
                            }

                        }
                        else if (((CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Call"))
                        {
                            string assign_temp_functionName = "";
                            if(((CustomTreeNode)((CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()]).Components["func1"]).Components.ContainsKey("id1"))
                            {
                                assign_temp_functionName = ((CustomTreeNode)((CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()]).Components["func1"]).Components["id1"].ToString();
                            }else if(((CustomTreeNode)((CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()]).Components["func1"]).Components.ContainsKey("attr1"))
                            {
                                assign_temp_functionName = ((CustomTreeNode)((CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()]).Components["func1"]).Components["attr1"].ToString();
                            }
                            var unseenVariableType = GetFunctionType(assign_temp_functionName);

                            if (unseenVariableType != "unknown")
                            {
                                foreach (string key in variable_type_dictionary.Keys)
                                {
                                    //LogParameter.ActualValue.LogMessage("reached here!???");
                                    if (!key.Contains("in") && !key.Contains("res"))// can not map input or output varibale with unseen variable
                                    {
                                        //LogParameter.ActualValue.LogMessage("reached here!");
                                        if (variable_type_dictionary[key].Equals(unseenVariableType))//when the type of additional variable matches unknwon variable type
                                        {
                                            if (!variable_name_mapping_dictionary.ContainsValue(key))//when the additonal variable is not mapped yet
                                            {
                                                variable_name_mapping_dictionary[((CustomTreeNode)customRoot.Components["targets" + lineNumber.ToString()]).Components["id1"].ToString()] = key;
                                                assignVariableName = key;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return false;
                            }
                            
                        }
                        else
                        {
                            LogParameter.ActualValue.LogMessage("Mapping varialbe name not implemented");
                            return false;
                        }
                    }
                    //LogParameter.ActualValue.LogMessage(((CustomTreeNode)customRoot.Components["targets" + lineNumber.ToString()]).Components["id1"].ToString());
                    //LogParameter.ActualValue.LogMessage(variable_name_mapping_dictionary[((CustomTreeNode)customRoot.Components["targets" + lineNumber.ToString()]).Components["id1"].ToString()]);
                    assignType = GetVariableType(assignVariableName);

                    if (assignType == "int" || assignType == "float")
                    {
                        allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(assignSymbolicTreeNodeList.Last().Symbol, 0);
                        foreach (var symbol in allowedChildSymbol_assign)
                        {
                            //<int_assign> or <float_assign>
                            if (symbol.ToString().Contains("<" + assignType + "_assign>"))
                            {
                                assignSymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                if (assignSymbolicTreeNodeList.Last().HasLocalParameters) assignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                assignSymbolicTreeNodeList[assignSymbolicTreeNodeList.Count-2].AddSubtree(assignSymbolicTreeNodeList.Last());
                                break;
                            }
                        }
                    }
                    

                    //node3: <int_var>' = '<int> OR <float_var>' = '<float>
                    allowedChildSymbol_assign = grammar.GetAllowedChildSymbols(assignSymbolicTreeNodeList.Last().Symbol, 0);
                    var assign_node3 = allowedChildSymbol_assign.First().CreateTreeNode();
                    foreach (var symbol in allowedChildSymbol_assign)
                    {
                        if (symbol.Name.Contains(assignType))
                        {
                            assign_node3 = symbol.CreateTreeNode();
                            if (assign_node3.HasLocalParameters) assign_node3.ResetLocalParameters(random);
                            assignSymbolicTreeNodeList.Last().AddSubtree(assign_node3);
                            break;
                        }
                    }
                    

                    //node4_1: left variable
                    BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["targets" + lineNumber.ToString()], assign_node3, grammar, 1, 0);

                    //LogParameter.ActualValue.LogMessage("Reached here! " + assign_node1.Symbol.Name + " 2:" + assign_node2.Symbol.Name + " 3: " + assign_node3.Symbol.Name);//test

                    //node 4_2: right side: handle <int> or <float>  <-- <int_var> OR <float_var> OR BinOp OR Call
                    if(!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()], assign_node3, grammar, 1, 1))
                    {
                        if(!BuildDummyValue(random, assign_node3, grammar, 1))
                        {
                            return false;
                        }
                    }

                    symbolicRoot.AddSubtree(assignSymbolicTreeNodeList.First());
                    break;
                #endregion

                case "AugAssign":
                    #region case AugAssign

                    //node1 " <assign>
                    var allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                    List<ISymbolicExpressionTreeNode> augassignSymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>
                    {
                        symbolicRoot
                    };
                    if (symbolicRoot.Symbol.Name.Contains("statement"))
                    {
                        foreach (var symbol in allowedChildSymbol_augAssign)
                        {

                            if (symbol.ToString().Contains("<simple_stmt>"))
                            {
                                augassignSymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                if (augassignSymbolicTreeNodeList.Last().HasLocalParameters) augassignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(augassignSymbolicTreeNodeList.Last());

                                allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(augassignSymbolicTreeNodeList.Last().Symbol, 0);
                                foreach (var symbol1 in allowedChildSymbol_augAssign)
                                {
                                    if (symbol1.ToString().Contains("<assign>"))
                                    {
                                        augassignSymbolicTreeNodeList.Add(symbol1.CreateTreeNode());
                                        if (augassignSymbolicTreeNodeList.Last().HasLocalParameters) augassignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                        augassignSymbolicTreeNodeList[augassignSymbolicTreeNodeList.Count - 2].AddSubtree(augassignSymbolicTreeNodeList.Last());
                                        break;
                                    }
                                }
                            }
                            else if (symbol.ToString().Contains("<assign>"))
                            {
                                augassignSymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                if (augassignSymbolicTreeNodeList.Last().HasLocalParameters) augassignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(augassignSymbolicTreeNodeList.Last());
                                break;
                            }
                        }
                    }

                    //node2: determine what type of assign it use
                    //<int_assign> or <float_assign>
                    string augAssignType = GetVariableType(((CustomTreeNode)customRoot.Components["target" + lineNumber.ToString()]).Components["id1"].ToString());
                    if (augAssignType == "unknown")
                    {
                        augAssignType = GetVariableType(variable_name_mapping_dictionary[((CustomTreeNode)customRoot.Components["target" + lineNumber.ToString()]).Components["id1"].ToString()]);
                    }
                    
                    allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(augassignSymbolicTreeNodeList.Last().Symbol, 0);

                    if (augAssignType == "int" || augAssignType == "float")
                    {
                        foreach (var symbol in allowedChildSymbol_augAssign)
                        {
                            //<int_assign> or <float_assign>
                            if (symbol.ToString().Contains(augAssignType))
                            {
                                augassignSymbolicTreeNodeList.Add(symbol.CreateTreeNode());//update node
                                if (augassignSymbolicTreeNodeList.Last().HasLocalParameters) augassignSymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                augassignSymbolicTreeNodeList[augassignSymbolicTreeNodeList.Count-2].AddSubtree(augassignSymbolicTreeNodeList.Last());
                                break;
                            }
                        }

                        if(((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Add") || ((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Sub") || ((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Mult"))
                        {
                            //node3: <float_var>' '<arith_ops>'= '<float> or <int_var>' '<arith_ops>'= '<int>
                            allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(augassignSymbolicTreeNodeList.Last().Symbol, 0);
                            var augAssign_node3 = allowedChildSymbol_augAssign.Last().CreateTreeNode();
                            foreach (var symbol in allowedChildSymbol_augAssign)
                            {
                                if (symbol.Name.Contains("<" + augAssignType + "_var>' '<arith_ops>"))
                                {
                                    augAssign_node3 = symbol.CreateTreeNode();
                                    if (augAssign_node3.HasLocalParameters) augAssign_node3.ResetLocalParameters(random);
                                    augassignSymbolicTreeNodeList.Last().AddSubtree(augAssign_node3);
                                }
                            }


                            //handle target <int> or <float>
                            BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["target" + lineNumber.ToString()], augAssign_node3, grammar, 1, 0);

                            //handle arith ops
                            //LogParameter.ActualValue.LogMessage(augAssign_node3.Symbol.Name);
                            allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(augAssign_node3.Symbol, 1);
                            var augAssign_node4 = allowedChildSymbol_augAssign.Last().CreateTreeNode();
                            foreach (var symbol in allowedChildSymbol_augAssign)
                            {
                                if (((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Add"))
                                {
                                    if (symbol.ToString().Contains("+"))
                                    {
                                        augAssign_node4 = symbol.CreateTreeNode();//update node
                                        if (augAssign_node4.HasLocalParameters) augAssign_node4.ResetLocalParameters(random);
                                        augAssign_node3.AddSubtree(augAssign_node4);
                                        break;
                                    }
                                }
                                else if (((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Sub"))
                                {
                                    if (symbol.ToString().Contains("-"))
                                    {
                                        augAssign_node4 = symbol.CreateTreeNode();//update node
                                        if (augAssign_node4.HasLocalParameters) augAssign_node4.ResetLocalParameters(random);
                                        augAssign_node3.AddSubtree(augAssign_node4);
                                        break;
                                    }
                                }
                                else if (((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Mult"))
                                {
                                    if (symbol.ToString().Contains("*"))
                                    {
                                        augAssign_node4 = symbol.CreateTreeNode();//update node
                                        if (augAssign_node4.HasLocalParameters) augAssign_node4.ResetLocalParameters(random);
                                        augAssign_node3.AddSubtree(augAssign_node4);
                                        break;
                                    }
                                }
                            }

                            //handle value <int> or <float>
                            BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()], augAssign_node3, grammar, 1, 2);

                        }
                        else if(((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("FloorDiv"))
                        {
                            allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(augassignSymbolicTreeNodeList.Last().Symbol, 0);
                            var augAssign_node3 = allowedChildSymbol_augAssign.Last().CreateTreeNode();
                            foreach (var symbol in allowedChildSymbol_augAssign)
                            {
                                if (symbol.Name.Contains("<int_var>' = '<int>"))
                                {
                                    augAssign_node3 = symbol.CreateTreeNode();
                                    if (augAssign_node3.HasLocalParameters) augAssign_node3.ResetLocalParameters(random);
                                    augassignSymbolicTreeNodeList.Last().AddSubtree(augAssign_node3);
                                }
                            }

                            //left
                            if(!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["target" + lineNumber.ToString()], augAssign_node3, grammar, 1, 0))
                            {
                                if (!BuildDummyValue(random, augAssign_node3, grammar, 0))
                                {
                                    LogParameter.ActualValue.LogMessage("Case augAssign: unable to assign variable to for first <variable>");
                                    return false;
                                }
                            }

                            //<int>
                            allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(augAssign_node3.Symbol, 1);
                            var augAssign_node4 = allowedChildSymbol_augAssign.Last().CreateTreeNode();
                            foreach (var symbol in allowedChildSymbol_augAssign)
                            {
                                if (symbol.Name.Contains("<int_arith_ops_protected>'('<int>','<int>')'"))
                                {
                                    augAssign_node4 = symbol.CreateTreeNode();
                                    if (augAssign_node4.HasLocalParameters) augAssign_node4.ResetLocalParameters(random);
                                    augAssign_node3.AddSubtree(augAssign_node4);
                                }
                            }

                            //<int_arith_ops_protected>
                            allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(augAssign_node4.Symbol, 0);
                            var augAssign_node5 = allowedChildSymbol_augAssign.Last().CreateTreeNode();
                            foreach (var symbol in allowedChildSymbol_augAssign)
                            {
                                if (symbol.Name.Contains("divInt"))
                                {
                                    augAssign_node5 = symbol.CreateTreeNode();
                                    if (augAssign_node5.HasLocalParameters) augAssign_node5.ResetLocalParameters(random);
                                    augAssign_node4.AddSubtree(augAssign_node5);
                                }
                            }

                            // left<int>
                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["target" + lineNumber.ToString()], augAssign_node4, grammar, 1, 1))
                            {
                                if (!BuildDummyValue(random, augAssign_node5, grammar, 0))
                                {
                                    LogParameter.ActualValue.LogMessage("Case augAssign: unable to assign variable to for left <int>");
                                    return false;
                                }
                            }

                            // left<int>
                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()], augAssign_node4, grammar, 1, 2))
                            {
                                if (!BuildDummyValue(random, augAssign_node5, grammar, 0))
                                {
                                    LogParameter.ActualValue.LogMessage("Case augAssign: unable to assign variable to for right <int>");
                                    return false;
                                }
                            }


                        }
                        else
                        {
                            LogParameter.ActualValue.LogMessage("case Augassign :unable to handle operator --> " + ((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString());
                        }


                    }
                    else if(augAssignType == "string")
                    {
                        allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(augassignSymbolicTreeNodeList.Last().Symbol, 0);
                        var augAssign_node3 = allowedChildSymbol_augAssign.Last().CreateTreeNode();
                        foreach (var symbol in allowedChildSymbol_augAssign)
                        {
                            if (symbol.Name.Contains("string"))
                            {
                                augAssign_node3 = symbol.CreateTreeNode();
                                if (augAssign_node3.HasLocalParameters) augAssign_node3.ResetLocalParameters(random);
                                augassignSymbolicTreeNodeList.Last().AddSubtree(augAssign_node3);
                            }
                        }

                        //left variable
                        BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["target" + lineNumber.ToString()], augAssign_node3, grammar, 1, 0);
                        //right side
                        allowedChildSymbol_augAssign = grammar.GetAllowedChildSymbols(augAssign_node3.Symbol, 1);
                        var augAssign_node4 = allowedChildSymbol_augAssign.Last().CreateTreeNode();
                        foreach (var symbol in allowedChildSymbol_augAssign)
                        {
                            if (symbol.Name.Contains("'('<string>' + '<string>')'"))
                            {
                                augAssign_node4 = symbol.CreateTreeNode();
                                if (augAssign_node4.HasLocalParameters) augAssign_node4.ResetLocalParameters(random);
                                augAssign_node3.AddSubtree(augAssign_node4);
                            }
                        }
                        //first <string>
                        BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["target" + lineNumber.ToString()], augAssign_node4, grammar, 1, 0);
                        //second <string>
                        BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()], augAssign_node4, grammar, 1, 1);
                    }
                    else
                    {
                        throw new Exception("AugAssign: assign type not implemented!");
                    }

                    

                    break;
                #endregion

                case "For":
                    #region case For
                    //<compound_stmt>
                    var allowedChildSymbol_for = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                    var for_node1 = allowedChildSymbol_for.First().CreateTreeNode();//initialize random for avoid error
                    foreach (var symbol in allowedChildSymbol_for)
                    {
                        if (symbol.ToString().Equals("<compound_stmt>"))
                        {
                            // <compound_stmt> node
                            for_node1 = symbol.CreateTreeNode();//update node to correct one
                            if (for_node1.HasLocalParameters) for_node1.ResetLocalParameters(random);
                            break;
                        }
                    }

                    //<for>
                    allowedChildSymbol_for = grammar.GetAllowedChildSymbols(for_node1.Symbol, 0);
                    var for_node2 = allowedChildSymbol_for.First().CreateTreeNode();//initialize random for avoid error
                    foreach (var symbol in allowedChildSymbol_for)
                    {
                        if (symbol.ToString().Contains("<for>"))
                        {
                            for_node2 = symbol.CreateTreeNode();
                            if (for_node2.HasLocalParameters) for_node2.ResetLocalParameters(random);
                            for_node1.AddSubtree(for_node2);
                            break;
                        } 
                    }

                    //choose what type of <for> loop
                    CustomTreeNode for_iterCTN = (CustomTreeNode)customRoot.Components["iter" + lineNumber.ToString()];
                    string for_type;
                    if (for_iterCTN.Components["_type1"].ToString().Contains("Name"))
                    {
                        for_type = GetVariableType(for_iterCTN.Components["id1"].ToString());
                    }
                    else if (for_iterCTN.Components["_type1"].ToString().Contains("Call"))
                    {
                        for_type = "range";
                    }
                    else
                    {
                        LogParameter.ActualValue.LogMessage("case for: iter type not implemented");
                        return false;
                    }

                    allowedChildSymbol_for = grammar.GetAllowedChildSymbols(for_node2.Symbol, 0);
                    var for_node3 = allowedChildSymbol_for.First().CreateTreeNode();//initialize random for avoid error

                    //LogParameter.ActualValue.LogMessage(for_type);
                    int for_code_index = 2;
                    if (for_type.Contains("string"))
                    {
                        //LogParameter.ActualValue.LogMessage("reach1");
                        foreach (var symbol in allowedChildSymbol_for)
                        {
                            if (symbol.ToString().Contains("'<char_var>' in '<string>'"))
                            {
                                //LogParameter.ActualValue.LogMessage("reach2222222222222");
                                for_node3 = symbol.CreateTreeNode();
                                if (for_node3.HasLocalParameters) for_node3.ResetLocalParameters(random);
                                for_node2.AddSubtree(for_node3);
                                break;
                            }
                        }
                    }
                    else if (for_type.Contains("list_int"))
                    {
                        foreach (var symbol in allowedChildSymbol_for)
                        {
                            if (symbol.ToString().Contains("<int_var>' in '<list_int>"))
                            {
                                //LogParameter.ActualValue.LogMessage("reach2222222222222");
                                for_node3 = symbol.CreateTreeNode();
                                if (for_node3.HasLocalParameters) for_node3.ResetLocalParameters(random);
                                for_node2.AddSubtree(for_node3);
                                break;
                            }
                        }
                    }
                    else if (for_type.Contains("range"))
                    {
                        foreach (var symbol in allowedChildSymbol_for)
                        {
                            if (symbol.ToString().Contains("<int_var>' in saveRange('<int>', '<int>"))
                            {
                                for_node3 = symbol.CreateTreeNode();
                                if (for_node3.HasLocalParameters) for_node3.ResetLocalParameters(random);
                                for_node2.AddSubtree(for_node3);
                                for_code_index = 3;
                                break;
                            }
                        }
                    }
                    else
                    {
                        LogParameter.ActualValue.LogMessage("case for: for_type not implemented! for type: " + for_type);
                        return false;
                    }

                    //LogParameter.ActualValue.LogMessage(for_node3.Symbol.Name);

                    //left variable
                    CustomTreeNode for_targetCTN = (CustomTreeNode)customRoot.Components["target" + lineNumber.ToString()];
                    // map unseen variable
                    if (!for_targetCTN.Components.ContainsKey("id1"))
                    {
                        LogParameter.ActualValue.LogMessage("case for: for loop target type not supported!");
                        return false;
                    }
                    if (!variable_name_mapping_dictionary.ContainsKey(for_targetCTN.Components["id1"].ToString()) && !variable_type_dictionary.ContainsKey(for_targetCTN.Components["id1"].ToString()))
                    {
                        if (for_type.Contains("string"))
                        {
                            foreach (string key in variable_type_dictionary.Keys)
                            {
                                //LogParameter.ActualValue.LogMessage("reached here!???");
                                if (!key.Contains("in") && !key.Contains("res"))// can not map input or output varibale with unseen variable
                                {
                                    //LogParameter.ActualValue.LogMessage("reached here!");
                                    if (variable_type_dictionary[key].Equals("char"))//when the type of additional variable matches unknwon variable type
                                    {
                                        if (!variable_name_mapping_dictionary.ContainsValue(key))//when the additonal variable is not mapped yet
                                        {
                                            variable_name_mapping_dictionary[for_targetCTN.Components["id1"].ToString()] = key;
                                        }
                                    }
                                }
                            }
                        }
                        else if (for_type.Contains("list_int"))
                        {
                            foreach (string key in variable_type_dictionary.Keys)
                            {
                                //LogParameter.ActualValue.LogMessage("reached here!???");
                                if (!key.Contains("in") && !key.Contains("res"))// can not map input or output varibale with unseen variable
                                {
                                    //LogParameter.ActualValue.LogMessage("reached here!");
                                    if (variable_type_dictionary[key].Equals("int"))//when the type of additional variable matches unknwon variable type
                                    {
                                        if (!variable_name_mapping_dictionary.ContainsValue(key))//when the additonal variable is not mapped yet
                                        {
                                            variable_name_mapping_dictionary[for_targetCTN.Components["id1"].ToString()] = key;
                                        }
                                    }
                                }
                            }
                        }
                        else if (for_type.Contains("range"))
                        {
                            foreach (string key in variable_type_dictionary.Keys)
                            {
                                //LogParameter.ActualValue.LogMessage("reached here!???");
                                if (!key.Contains("in") && !key.Contains("res"))// can not map input or output varibale with unseen variable
                                {
                                    //LogParameter.ActualValue.LogMessage("reached here!");
                                    if (variable_type_dictionary[key].Equals("int"))//when the type of additional variable matches unknwon variable type
                                    {
                                        if (!variable_name_mapping_dictionary.ContainsValue(key))//when the additonal variable is not mapped yet
                                        {
                                            variable_name_mapping_dictionary[for_targetCTN.Components["id1"].ToString()] = key;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            LogParameter.ActualValue.LogMessage("case for: unable to map unseen variable :" + for_type);
                            return false;
                        }
                    }

                    if (!BuildSymbolicExpressionTree(random, for_targetCTN, for_node3, grammar, 1, 0))
                    {
                        if(!BuildDummyValue(random, for_node3, grammar, 0))
                        {
                            LogParameter.ActualValue.LogMessage("Case for: unable to assign variable to for loop first <variable>");
                            return false;
                        }
                    }

                    //right variable
                    if (!BuildSymbolicExpressionTree(random, for_iterCTN, for_node3, grammar, 1, 1))
                    {
                        LogParameter.ActualValue.LogMessage("Case for: unable to assign variable to for loop second <variable>");
                        return false;
                    }
                    
                    //<code>
                    var loopBodyTreeNode = (CustomTreeNode)customRoot.Components["body" + lineNumber.ToString()];
                    List<ISymbolicExpressionTreeNode> lineSymbolicTreeNode_list = new List<ISymbolicExpressionTreeNode>();
                    allowedChildSymbol_for = grammar.GetAllowedChildSymbols(for_node3.Symbol, for_code_index);
                    for (int i = 0; i < loopBodyTreeNode.numberOfLines-1; i++)
                    {
                        lineSymbolicTreeNode_list.Add(allowedChildSymbol_for.ElementAt(1).CreateTreeNode());
                    }
                    lineSymbolicTreeNode_list.Add(allowedChildSymbol_for.Last().CreateTreeNode());
                    foreach (var tempNode in lineSymbolicTreeNode_list)
                    {
                        if (tempNode.HasLocalParameters) tempNode.ResetLocalParameters(random);
                    }
                    //LogParameter.ActualValue.LogMessage("for node 3 : " + for_node3.Symbol.Name);
                    for_node3.AddSubtree(lineSymbolicTreeNode_list[0]);
                    if (!BuildSymbolicExpressionTree(random, loopBodyTreeNode, lineSymbolicTreeNode_list[0], grammar, 1))
                    {
                        BuildDummyLine(random, lineSymbolicTreeNode_list[0], grammar);
                    }
                    for (int i = 1; i < loopBodyTreeNode.numberOfLines; i++)
                    {
                        lineSymbolicTreeNode_list[i - 1].AddSubtree(lineSymbolicTreeNode_list[i]);
                        if (!BuildSymbolicExpressionTree(random, loopBodyTreeNode, lineSymbolicTreeNode_list[i], grammar, 1))
                        {
                            BuildDummyLine(random, lineSymbolicTreeNode_list[i], grammar);
                        }
                    }

                    symbolicRoot.AddSubtree(for_node1);
                    return true;
                #endregion

                case "BinOp":
                    #region case BinOp
                    // when it is "BinOp" it is always connected with either <int> or <float> or other variable type
                    // handle <int> or <float>
                    //LogParameter.ActualValue.LogMessage("symbolicRoot:" + symbolicRoot.Symbol.Name + " childindex:" + childSubtreeIndex);//test
                    if (symbolicRoot.Symbol.Name.Contains("string"))
                    {
                        string binOP_string_type = ((CustomTreeNode)customRoot.Components["op1"]).Components["_type1"].ToString();
                        if (binOP_string_type.Contains("Mult"))
                        {
                            int number_BinOp = Int32.Parse(((CustomTreeNode)customRoot.Components["right1"]).Components["value1"].ToString());
                            List<ISymbolicExpressionTreeNode> binOP_mult_TreeNode = new List<ISymbolicExpressionTreeNode>();

                            var allowedChildSymbol_BinOp = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                            foreach (var symbol in allowedChildSymbol_BinOp)
                            {
                                if (symbol.Name.Contains("'('<string>' + '<string>')'"))
                                {
                                    for(int i=0; i< number_BinOp-1; i++)
                                    {
                                        binOP_mult_TreeNode.Add(symbol.CreateTreeNode());
                                    }
                                }
                            }
                            symbolicRoot.AddSubtree(binOP_mult_TreeNode[0]);
                            for (int i = 1; i < binOP_mult_TreeNode.Count; i++)
                            {
                                if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["left" + lineNumber.ToString()], binOP_mult_TreeNode[i - 1], grammar, 1, 0))
                                {
                                    BuildDummyValue(random, binOP_mult_TreeNode[i - 1], grammar, 0);
                                }
                                binOP_mult_TreeNode[i - 1].AddSubtree(binOP_mult_TreeNode[i]);
                            }
                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["left" + lineNumber.ToString()], binOP_mult_TreeNode.Last(), grammar, 1, 0))
                            {
                                BuildDummyValue(random, binOP_mult_TreeNode.Last(), grammar, 0);
                            }
                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["left" + lineNumber.ToString()], binOP_mult_TreeNode.Last(), grammar, 1, 1))
                            {
                                BuildDummyValue(random, binOP_mult_TreeNode.Last(), grammar, 1);
                            }

                        }
                        else if (binOP_string_type.Contains("Add"))
                        {
                            var allowedChildSymbol_BinOp = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                            var binOP_node1  = allowedChildSymbol_BinOp.First().CreateTreeNode();
                            foreach (var symbol in allowedChildSymbol_BinOp)
                            {
                                if (symbol.Name.Contains("'('<string>' + '<string>')'"))
                                {
                                    binOP_node1 = symbol.CreateTreeNode();
                                    symbolicRoot.AddSubtree(binOP_node1);
                                }
                            }
                            BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["left" + lineNumber.ToString()], binOP_node1, grammar, 1, 0);
                            BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["right" + lineNumber.ToString()], binOP_node1, grammar, 1, 1);

                        }
                        else
                        {
                            throw new Exception("BinOP string: type not seen!");
                        }
                    }
                    else
                    {
                        var allowedChildSymbol_BinOp = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var binOp_node1 = allowedChildSymbol_BinOp.First().CreateTreeNode();//initialize random for avoid error

                        string operatorType = ((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString();
                        if (symbolicRoot.Symbol.ToString().Contains("int"))// handle <int> <-- '( '<int>' '<arith_ops>' '<int>' )'
                        {
                            // handle <int> <-- '( '<int>' '<arith_ops>' '<int>' )'
                            // when it is connected from <int>
                            // left side can not be another expression
                            foreach (var symbol in allowedChildSymbol_BinOp)
                            {

                                if (operatorType.Contains("Add") || operatorType.Contains("Mult") || operatorType.Contains("Sub"))
                                {
                                    //normal arith_ops
                                    if (symbol.ToString().Contains("'<int>' '<arith_ops>' '<int>'"))
                                    {
                                        binOp_node1 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node1.HasLocalParameters) binOp_node1.ResetLocalParameters(random);
                                        symbolicRoot.AddSubtree(binOp_node1);
                                        break;
                                    }
                                }
                                else
                                {
                                    //arith_ops_protected
                                    if (symbol.ToString().Contains("<int_arith_ops_protected>'('<int>','<int>')'"))
                                    {
                                        // <compound_stmt> node
                                        binOp_node1 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node1.HasLocalParameters) binOp_node1.ResetLocalParameters(random);
                                        symbolicRoot.AddSubtree(binOp_node1);
                                        break;
                                    }
                                }

                            }
                        }
                        // handle <float> <-- '( '<float>' '<arith_ops>' '<float>' )' OR '( '<int>' '<arith_ops>' '<float>' )' OR '( '<float>' '<arith_ops>' '<int>' )'
                        else if (symbolicRoot.Symbol.ToString().Contains("float"))
                        {
                            foreach (var symbol in allowedChildSymbol_BinOp)
                            {
                                if (operatorType.Contains("Add") || operatorType.Contains("Mult") || operatorType.Contains("Sub"))
                                {
                                    //normal arith_ops
                                    if (symbol.ToString().Contains("<float>' '<arith_ops>' '<float>"))
                                    {
                                        binOp_node1 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node1.HasLocalParameters) binOp_node1.ResetLocalParameters(random);
                                        symbolicRoot.AddSubtree(binOp_node1);
                                        break;
                                    }
                                }
                                else
                                {
                                    //arith_ops_protected
                                    if (symbol.ToString().Contains("<float_arith_ops_protected>'('<float>','<float>')'"))
                                    {
                                        // <compound_stmt> node
                                        binOp_node1 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node1.HasLocalParameters) binOp_node1.ResetLocalParameters(random);
                                        symbolicRoot.AddSubtree(binOp_node1);
                                        break;
                                    }
                                }
                            }
                        }

                        //LogParameter.ActualValue.LogMessage("Reached here! " + operatorType);//test
                        // handle '( '<float>' '<arith_ops>' '<float>' )' OR others
                        if (operatorType.Contains("Add") || operatorType.Contains("Mult") || operatorType.Contains("Sub"))
                        {
                            //normal arith_ops
                            //handle <float>

                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["left" + lineNumber.ToString()], binOp_node1, grammar, 1, 0))
                            {
                                BuildDummyValue(random, binOp_node1, grammar, 0);
                            }

                            //handle <arith_ops> 
                            allowedChildSymbol_BinOp = grammar.GetAllowedChildSymbols(binOp_node1.Symbol, 1);
                            //LogParameter.ActualValue.LogMessage("binOp_node1:" + binOp_node1.Symbol.Name + " childindex:" + childSubtreeIndex);//test
                            var binOp_node2_2 = allowedChildSymbol_BinOp.First().CreateTreeNode();//initialize random for avoid error
                            if (operatorType.Contains("Add"))
                            {
                                foreach (var symbol in allowedChildSymbol_BinOp)
                                {
                                    if (symbol.ToString().Contains("+"))
                                    {
                                        // handle <float_var> <-- acutal varialbe name
                                        binOp_node2_2 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node2_2.HasLocalParameters) binOp_node2_2.ResetLocalParameters(random);
                                        binOp_node1.AddSubtree(binOp_node2_2);
                                        break;
                                    }
                                }
                            }
                            else if (operatorType.Contains("Mult"))
                            {
                                foreach (var symbol in allowedChildSymbol_BinOp)
                                {
                                    if (symbol.ToString().Contains("*"))
                                    {
                                        // handle <float_var> <-- acutal varialbe name
                                        binOp_node2_2 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node2_2.HasLocalParameters) binOp_node2_2.ResetLocalParameters(random);
                                        binOp_node1.AddSubtree(binOp_node2_2);
                                        break;
                                    }
                                }
                            }
                            else if (operatorType.Contains("Sub"))
                            {
                                foreach (var symbol in allowedChildSymbol_BinOp)
                                {
                                    if (symbol.ToString().Contains("-"))
                                    {
                                        // handle <float_var> <-- acutal varialbe name
                                        binOp_node2_2 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node2_2.HasLocalParameters) binOp_node2_2.ResetLocalParameters(random);
                                        binOp_node1.AddSubtree(binOp_node2_2);
                                        break;
                                    }
                                }
                            }

                            //handle <float>
                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["right" + lineNumber.ToString()], binOp_node1, grammar, 1, 2))
                            {
                                BuildDummyValue(random, binOp_node1, grammar, 2);
                            }

                        }
                        else
                        {
                            //arith_ops_protected: <float_arith_ops_protected>'('<float>','<float>')'
                            //handle <float_arith_ops_protected>
                            allowedChildSymbol_BinOp = grammar.GetAllowedChildSymbols(binOp_node1.Symbol, 0);
                            var binOp_node2_1 = allowedChildSymbol_BinOp.First().CreateTreeNode();//initialize random for avoid error
                            foreach (var symbol in allowedChildSymbol_BinOp)
                            {
                                if (operatorType.Equals("Div"))
                                {
                                    if (symbol.ToString().Contains("'div'"))
                                    {
                                        binOp_node2_1 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node2_1.HasLocalParameters) binOp_node2_1.ResetLocalParameters(random);
                                        binOp_node1.AddSubtree(binOp_node2_1);
                                        break;
                                    }
                                }
                                else if (operatorType.Contains("Mod"))
                                {
                                    if (symbol.ToString().Contains("'mod'"))
                                    {
                                        binOp_node2_1 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node2_1.HasLocalParameters) binOp_node2_1.ResetLocalParameters(random);
                                        binOp_node1.AddSubtree(binOp_node2_1);
                                        break;
                                    }
                                }
                                else if (operatorType.Contains("FloorDiv"))
                                {
                                    if (symbol.ToString().Contains("'divInt'"))
                                    {
                                        binOp_node2_1 = symbol.CreateTreeNode();//update node to correct one
                                        if (binOp_node2_1.HasLocalParameters) binOp_node2_1.ResetLocalParameters(random);
                                        binOp_node1.AddSubtree(binOp_node2_1);
                                        break;
                                    }
                                }
                                else
                                {
                                    throw new Exception("BinOP unseen operator");
                                }
                            }
                            //handle fisrt <float>
                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["left" + lineNumber.ToString()], binOp_node1, grammar, 1, 1))
                            {
                                BuildDummyValue(random, binOp_node1, grammar, 1);
                            }
                            //handle second <float>
                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["right" + lineNumber.ToString()], binOp_node1, grammar, 1, 2))
                            {
                                BuildDummyValue(random, binOp_node1, grammar, 2);
                            }
                        }
                    }
                    break;
                #endregion

                case "Name":
                    #region case Name
                    // when it is "Name" it is always connected from either <int> or <float> or other variable type
                    // handle <int> or <float>
                    string variable_name = customRoot.Components["id" + lineNumber.ToString()].ToString();
                    //LogParameter.ActualValue.LogMessage(variable_name);
                    var name_type = GetVariableType(variable_name);
                    if (name_type == "unknown")
                    {
                        if (!variable_name_mapping_dictionary.ContainsKey(variable_name))
                        {
                            LogParameter.ActualValue.LogMessage("Case Name: the name does not mapped!");
                            return false;
                        }
                        variable_name = variable_name_mapping_dictionary[variable_name];
                        name_type = GetVariableType(variable_name);
                    }
                    var allowedChildSymbol_Name = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                    List<ISymbolicExpressionTreeNode> Name_SymbolicExpressionTreeNodeList = new List<ISymbolicExpressionTreeNode>
                    {
                        symbolicRoot
                    };

                    if (!symbolicRoot.Symbol.ToString().Contains(name_type))
                    {
                        //LogParameter.ActualValue.LogMessage("Reached here!");//test
                        if (symbolicRoot.Symbol.ToString().Contains("int"))
                        {
                            foreach (var symbol in allowedChildSymbol_Name)
                            {
                                if (symbol.ToString().Contains("'int('<float>')'"))
                                {
                                    Name_SymbolicExpressionTreeNodeList.Add(symbol.CreateTreeNode());
                                    if (Name_SymbolicExpressionTreeNodeList.Last().HasLocalParameters) Name_SymbolicExpressionTreeNodeList.Last().ResetLocalParameters(random);
                                    symbolicRoot.AddSubtree(Name_SymbolicExpressionTreeNodeList.Last());
                                    break;
                                }
                            }
                        }
                        else if (symbolicRoot.Symbol.ToString().Contains("float"))
                        {
                            foreach (var symbol in allowedChildSymbol_Name)
                            {
                                if (symbol.ToString().Contains("<int>"))
                                {
                                    Name_SymbolicExpressionTreeNodeList.Add(symbol.CreateTreeNode());
                                    if (Name_SymbolicExpressionTreeNodeList.Last().HasLocalParameters) Name_SymbolicExpressionTreeNodeList.Last().ResetLocalParameters(random);
                                    symbolicRoot.AddSubtree(Name_SymbolicExpressionTreeNodeList.Last());
                                    break;
                                }
                            }
                        }
                        else if (symbolicRoot.Symbol.ToString().Contains("string"))
                        {
                            foreach (var symbol in allowedChildSymbol_Name)
                            {
                                if (symbol.ToString().Contains("<char>"))
                                {
                                    Name_SymbolicExpressionTreeNodeList.Add(symbol.CreateTreeNode());
                                    if (Name_SymbolicExpressionTreeNodeList.Last().HasLocalParameters) Name_SymbolicExpressionTreeNodeList.Last().ResetLocalParameters(random);
                                    symbolicRoot.AddSubtree(Name_SymbolicExpressionTreeNodeList.Last());
                                    break;
                                }
                            }
                        }
                        childSubtreeIndex = 0;


                    }

                    //LogParameter.ActualValue.LogMessage("this is name called" + Name_SymbolicExpressionTreeNodeList.Last().Symbol.ToString());//test
                    //LogParameter.ActualValue.LogMessage("this is name called" +  " -- " +name_type);//test

                    //handle <int> or <float> <-- <int_var> or <float_var>
                    allowedChildSymbol_Name = grammar.GetAllowedChildSymbols(Name_SymbolicExpressionTreeNodeList.Last().Symbol, childSubtreeIndex);
                    foreach (var symbol in allowedChildSymbol_Name)
                    {
                        if (symbol.ToString().Contains(name_type + "_var"))
                        {
                            //LogParameter.ActualValue.LogMessage("reached here!!!" + " -- " + variable_name);//test
                            Name_SymbolicExpressionTreeNodeList.Add(symbol.CreateTreeNode());
                            if (Name_SymbolicExpressionTreeNodeList.Last().HasLocalParameters) Name_SymbolicExpressionTreeNodeList.Last().ResetLocalParameters(random);
                            Name_SymbolicExpressionTreeNodeList[Name_SymbolicExpressionTreeNodeList.Count - 2].AddSubtree(Name_SymbolicExpressionTreeNodeList.Last());
                            break;
                        }
                    }


                    //assign actual variable name to <int_Var> or <float_Var>
                    allowedChildSymbol_Name = grammar.GetAllowedChildSymbols(Name_SymbolicExpressionTreeNodeList.Last().Symbol, 0);
                    foreach (var symbol in allowedChildSymbol_Name)
                    {
                        if (symbol.ToString().Contains(variable_name))
                        {
                            Name_SymbolicExpressionTreeNodeList.Add(symbol.CreateTreeNode());
                            if (Name_SymbolicExpressionTreeNodeList.Last().HasLocalParameters) Name_SymbolicExpressionTreeNodeList.Last().ResetLocalParameters(random);
                            Name_SymbolicExpressionTreeNodeList[Name_SymbolicExpressionTreeNodeList.Count - 2].AddSubtree(Name_SymbolicExpressionTreeNodeList.Last());
                            break;
                        }
                    }
                    
                    //LogParameter.ActualValue.LogMessage("this is name called" + Name_SymbolicExpressionTreeNodeList[Name_SymbolicExpressionTreeNodeList.Count - 2].Symbol.ToString());//test
                    

                    break;
                #endregion

                case "Constant":
                    #region case Constant
                    // when it is "Constant" it is always connected from either <int> or <float> or other variable type
                    // handle <int> or <float>
                    //LogParameter.ActualValue.LogMessage("this is constant called" + symbolicRoot.Symbol.Name);//test

                    if (symbolicRoot.Symbol.ToString().Contains("int"))
                    {
                        //node1: handle <int> <-- 'int('<number>'.0)'
                        var allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var constant_node1 = allowedChildSymbol_Constant.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Constant)
                        {
                            if (symbol.ToString().Contains("'int('<number>'.0)'"))
                            {
                                constant_node1 = symbol.CreateTreeNode();//update node to correct one
                                if (constant_node1.HasLocalParameters) constant_node1.ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(constant_node1);
                                break;
                            }
                        }

                        string number = customRoot.Components["value1"].ToString();
                        // node <number> list: it will create children node for <number> 
                        allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(constant_node1.Symbol, 0);
                        List<ISymbolicExpressionTreeNode> leftNumber_node_list = new List<ISymbolicExpressionTreeNode>();
                        for (int i = 0; i < number.Length - 1; i++)
                        {
                            //<num><number>
                            leftNumber_node_list.Add(allowedChildSymbol_Constant.ElementAt(1).CreateTreeNode());
                        }
                        leftNumber_node_list.Add(allowedChildSymbol_Constant.Last().CreateTreeNode());
                        foreach (var tempNode in leftNumber_node_list)
                        {
                            if (tempNode.HasLocalParameters) tempNode.ResetLocalParameters(random);
                        }
                        // <num> OR <num><number>
                        constant_node1.AddSubtree(leftNumber_node_list[0]);
                        allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(leftNumber_node_list[0].Symbol, 0);
                        List<ISymbolicExpressionTreeNode> leftNumber_num_node_list = new List<ISymbolicExpressionTreeNode>();
                        int j = 0;
                        bool numstatus = false;
                        foreach (var symbol in allowedChildSymbol_Constant)
                        {
                            if (symbol.ToString().Contains(number[j]))
                            {
                                leftNumber_num_node_list.Add(symbol.CreateTreeNode());
                                if (leftNumber_num_node_list.Last().HasLocalParameters) leftNumber_num_node_list.Last().ResetLocalParameters(random);
                                leftNumber_node_list[j].AddSubtree(leftNumber_num_node_list.Last());
                                numstatus = true;
                                break;
                            }
                        }
                        if(numstatus == false)
                        {
                            leftNumber_node_list[j].AddSubtree(allowedChildSymbol_Constant.First().CreateTreeNode());
                        }
                        j++;
                        while (j < number.Length)
                        {
                            leftNumber_node_list[j - 1].AddSubtree(leftNumber_node_list[j]);
                            numstatus = false;
                            foreach (var symbol in allowedChildSymbol_Constant)
                            {
                                if (symbol.ToString().Contains(number[j]))
                                {
                                    leftNumber_num_node_list.Add(symbol.CreateTreeNode());
                                    if (leftNumber_num_node_list.Last().HasLocalParameters) leftNumber_num_node_list.Last().ResetLocalParameters(random);
                                    leftNumber_node_list[j].AddSubtree(leftNumber_num_node_list.Last());
                                    numstatus = true;
                                    break;
                                }
                            }
                            if (numstatus == false)
                            {
                                leftNumber_node_list[j].AddSubtree(allowedChildSymbol_Constant.First().CreateTreeNode());
                            }
                            j++;
                        }
                    }
                    else if (symbolicRoot.Symbol.ToString().Contains("float"))
                    {

                        //node1: handle <float> <-- <number>'.'<number>
                        var allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var constant_node1 = allowedChildSymbol_Constant.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Constant)
                        {
                            if (symbol.ToString().Contains("<number>'.'<number>"))
                            {
                                constant_node1 = symbol.CreateTreeNode();//update node to correct one
                                if (constant_node1.HasLocalParameters) constant_node1.ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(constant_node1);
                                break;
                            }
                        }

                        // process the constant
                        string[] number = customRoot.Components["value1"].ToString().Split('.');
                        if (!customRoot.Components["value1"].ToString().Contains('.'))
                        {
                            Array.Resize(ref number, 2);
                            number[1] = "0";
                        }

                        // node left <number> list: it will create children node for left <number> in <number>.<number>
                        allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(constant_node1.Symbol, 0);
                        List<ISymbolicExpressionTreeNode> leftNumber_node_list = new List<ISymbolicExpressionTreeNode>();
                        for (int i = 0; i < number[0].Length - 1; i++)
                        {
                            //<num><number>
                            leftNumber_node_list.Add(allowedChildSymbol_Constant.ElementAt(1).CreateTreeNode());
                        }
                        leftNumber_node_list.Add(allowedChildSymbol_Constant.Last().CreateTreeNode());
                        foreach (var tempNode in leftNumber_node_list)
                        {
                            if (tempNode.HasLocalParameters) tempNode.ResetLocalParameters(random);
                        }
                        // <num> OR <num><number>
                        constant_node1.AddSubtree(leftNumber_node_list[0]);
                        allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(leftNumber_node_list[0].Symbol, 0);
                        List<ISymbolicExpressionTreeNode> leftNumber_num_node_list = new List<ISymbolicExpressionTreeNode>();
                        int j = 0;
                        foreach (var symbol in allowedChildSymbol_Constant)
                        {
                            if (symbol.ToString().Contains(number[0][j]))
                            {
                                leftNumber_num_node_list.Add(symbol.CreateTreeNode());
                                if (leftNumber_num_node_list.Last().HasLocalParameters) leftNumber_num_node_list.Last().ResetLocalParameters(random);
                                leftNumber_node_list[j].AddSubtree(leftNumber_num_node_list.Last());
                                break;
                            }
                        }
                        j++;
                        while (j < number[0].Length)
                        {
                            leftNumber_node_list[j - 1].AddSubtree(leftNumber_node_list[j]);
                            foreach (var symbol in allowedChildSymbol_Constant)
                            {
                                if (symbol.ToString().Contains(number[0][j]))
                                {
                                    leftNumber_num_node_list.Add(symbol.CreateTreeNode());
                                    if (leftNumber_num_node_list.Last().HasLocalParameters) leftNumber_num_node_list.Last().ResetLocalParameters(random);
                                    leftNumber_node_list[j].AddSubtree(leftNumber_num_node_list.Last());
                                    break;
                                }
                            }
                            j++;
                        }

                        // node right <number> list : it will create children node for right <number> in <number>.<number>
                        allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(constant_node1.Symbol, 1);
                        List<ISymbolicExpressionTreeNode> rightNumber_node_list = new List<ISymbolicExpressionTreeNode>();
                        for (int i = 0; i < number[1].Length - 1; i++)
                        {
                            //<num><number>
                            rightNumber_node_list.Add(allowedChildSymbol_Constant.ElementAt(1).CreateTreeNode());
                        }
                        rightNumber_node_list.Add(allowedChildSymbol_Constant.Last().CreateTreeNode());
                        foreach (var tempNode in rightNumber_node_list)
                        {
                            if (tempNode.HasLocalParameters) tempNode.ResetLocalParameters(random);
                        }
                        // <num> OR <num><number>
                        constant_node1.AddSubtree(rightNumber_node_list[0]);
                        allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(rightNumber_node_list[0].Symbol, 0);
                        List<ISymbolicExpressionTreeNode> rightNumber_num_node_list = new List<ISymbolicExpressionTreeNode>();
                        j = 0;
                        foreach (var symbol in allowedChildSymbol_Constant)
                        {
                            if (symbol.ToString().Contains(number[1][j]))
                            {
                                rightNumber_num_node_list.Add(symbol.CreateTreeNode());
                                if (rightNumber_num_node_list.Last().HasLocalParameters) rightNumber_num_node_list.Last().ResetLocalParameters(random);
                                rightNumber_node_list[j].AddSubtree(rightNumber_num_node_list.Last());
                                break;
                            }
                        }
                        j++;
                        while (j < number[1].Length)
                        {
                            rightNumber_node_list[j - 1].AddSubtree(rightNumber_node_list[j]);
                            foreach (var symbol in allowedChildSymbol_Constant)
                            {
                                if (symbol.ToString().Contains(number[1][j]))
                                {
                                    rightNumber_num_node_list.Add(symbol.CreateTreeNode());
                                    if (rightNumber_num_node_list.Last().HasLocalParameters) rightNumber_num_node_list.Last().ResetLocalParameters(random);
                                    rightNumber_node_list[j].AddSubtree(rightNumber_num_node_list.Last());
                                    break;
                                }
                            }
                            j++;
                        }
                    }
                    else if (symbolicRoot.Symbol.ToString().Contains("string"))
                    {
                        var allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        List<ISymbolicExpressionTreeNode> constant_string_SymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>
                        {
                            symbolicRoot
                        };

                        foreach(var symbol in allowedChildSymbol_Constant)
                        {
                            if(symbol.Name.Contains("<string_const>"))
                            {
                                //LogParameter.ActualValue.LogMessage("reached here!2");//test
                                constant_string_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                if (constant_string_SymbolicTreeNodeList.Last().HasLocalParameters) constant_string_SymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(constant_string_SymbolicTreeNodeList.Last());
                                childSubtreeIndex = 0;
                            }
                        }

                        //LogParameter.ActualValue.LogMessage(constant_string_SymbolicTreeNodeList.Last().Symbol.Name);
                        allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(constant_string_SymbolicTreeNodeList.Last().Symbol, childSubtreeIndex);

                        //used for "small" or "large" here (fixed string constant)
                        bool success_constant = false;
                        if (!customRoot.Components["value1"].ToString().Equals("") && constant_string_SymbolicTreeNodeList.Last().Symbol.Name.Contains("<string_const>"))
                        {
                            //LogParameter.ActualValue.LogMessage("here--->" + customRoot.Components["value1"].ToString());
                            foreach (var symbol in allowedChildSymbol_Constant)
                            {
                                if (symbol.Name.Contains(customRoot.Components["value"+lineNumber.ToString()].ToString()))
                                {
                                    constant_string_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                    if (constant_string_SymbolicTreeNodeList.Last().HasLocalParameters) constant_string_SymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                    constant_string_SymbolicTreeNodeList[constant_string_SymbolicTreeNodeList.Count - 2].AddSubtree(constant_string_SymbolicTreeNodeList.Last());
                                    success_constant = true;
                                    break;
                                }
                            }
                        }
                        
                        // if no given constant match
                        if (!success_constant)
                        {
                            //improve here
                            foreach (var symbol in allowedChildSymbol_Constant)
                            {
                                if (symbol.Name.Contains("<string_const_part>"))
                                {
                                    constant_string_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                    if (constant_string_SymbolicTreeNodeList.Last().HasLocalParameters) constant_string_SymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                    constant_string_SymbolicTreeNodeList[constant_string_SymbolicTreeNodeList.Count - 2].AddSubtree(constant_string_SymbolicTreeNodeList.Last());
                                    childSubtreeIndex = 0;
                                }
                            }
                            
                            allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(constant_string_SymbolicTreeNodeList.Last().Symbol, childSubtreeIndex);
                            foreach (var symbol in allowedChildSymbol_Constant)
                            {
                                if (symbol.Name.Equals("<string_literal>"))
                                {
                                    constant_string_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                    if (constant_string_SymbolicTreeNodeList.Last().HasLocalParameters) constant_string_SymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                    constant_string_SymbolicTreeNodeList[constant_string_SymbolicTreeNodeList.Count - 2].AddSubtree(constant_string_SymbolicTreeNodeList.Last());
                                }
                            }

                            allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(constant_string_SymbolicTreeNodeList.Last().Symbol, 0);
                            //LogParameter.ActualValue.LogMessage(constant_node3.Symbol.Name);
                            var constant_node4 = allowedChildSymbol_Constant.First().CreateTreeNode();
                            foreach(var symbol in allowedChildSymbol_Constant)
                            {
                                //LogParameter.ActualValue.LogMessage(symbol.Name);//test
                                string temp_string_constant = "";
                                if (customRoot.Components["value" + lineNumber.ToString()].ToString().Contains("\\n")){
                                    temp_string_constant = "'\\" + customRoot.Components["value" + lineNumber.ToString()].ToString() + "'";
                                }
                                else
                                {
                                    temp_string_constant = "'" + customRoot.Components["value" + lineNumber.ToString()].ToString() + "'";
                                }
                                
                                //LogParameter.ActualValue.LogMessage(temp_string_constant + "symbol name:" + symbol.Name);//test
                                if (symbol.Name.Equals(temp_string_constant))
                                {
                                    constant_node4 = symbol.CreateTreeNode();
                                    if (constant_node4.HasLocalParameters) constant_node4.ResetLocalParameters(random);
                                    constant_string_SymbolicTreeNodeList.Last().AddSubtree(constant_node4);
                                    break;
                                }
                            }
                            
                        }

                    }
                    else if (symbolicRoot.Symbol.ToString().Contains("bool"))
                    {
                        var allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var constant_node1 = allowedChildSymbol_Constant.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Constant)
                        {
                            if (symbol.Name.Contains("<bool_const>"))
                            {
                                constant_node1 = symbol.CreateTreeNode();
                                if (constant_node1.HasLocalParameters) constant_node1.ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(constant_node1);
                            }
                        }

                        allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(constant_node1.Symbol, 0);
                        var constant_node2 = allowedChildSymbol_Constant.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Constant)
                        {
                            if (symbol.Name.Contains(customRoot.Components["value1"].ToString()))
                            {
                                constant_node2 = symbol.CreateTreeNode();
                                if (constant_node2.HasLocalParameters) constant_node2.ResetLocalParameters(random);
                                constant_node1.AddSubtree(constant_node2);
                            }
                        }
                    }
                    else if (symbolicRoot.Symbol.ToString().Contains("char"))
                    {
                        var allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        List<ISymbolicExpressionTreeNode> constant_string_SymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>
                        {
                            symbolicRoot
                        };

                        foreach (var symbol in allowedChildSymbol_Constant)
                        {
                            if (symbol.Name.Contains("<char_literal>"))
                            {
                                //LogParameter.ActualValue.LogMessage("reached here!2");//test
                                constant_string_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                if (constant_string_SymbolicTreeNodeList.Last().HasLocalParameters) constant_string_SymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(constant_string_SymbolicTreeNodeList.Last());
                                childSubtreeIndex = 0;
                            }
                        }

                        //LogParameter.ActualValue.LogMessage(constant_string_SymbolicTreeNodeList.Last().Symbol.Name);
                        allowedChildSymbol_Constant = grammar.GetAllowedChildSymbols(constant_string_SymbolicTreeNodeList.Last().Symbol, childSubtreeIndex);
                        var constant_node4 = allowedChildSymbol_Constant.First().CreateTreeNode();
                        foreach (var symbol in allowedChildSymbol_Constant)
                        {
                            if (symbol.Name.Equals("'" + customRoot.Components["value" + lineNumber.ToString()].ToString() + "'"))
                            {
                                
                                constant_node4 = symbol.CreateTreeNode();
                                if (constant_node4.HasLocalParameters) constant_node4.ResetLocalParameters(random);
                                constant_string_SymbolicTreeNodeList.Last().AddSubtree(constant_node4);
                                break;
                            }
                        }

                    }
                    else
                    {
                        LogParameter.ActualValue.LogMessage("case constant: Constant type not seen! root: " + symbolicRoot.Symbol.ToString());
                        return false;
                    }
                    break;
                #endregion

                case "Call":
                    #region case Call
                    // when it is "Call" it is always connected from either <int> or <float> or other variable type
                    // it also can connected from <statement>
                    string function = "";
                    if (((CustomTreeNode)customRoot.Components["func1"]).Components.ContainsKey("id1"))
                    {
                        function = ((CustomTreeNode)customRoot.Components["func1"]).Components["id1"].ToString();
                    }else if (((CustomTreeNode)customRoot.Components["func1"]).Components.ContainsKey("attr1"))
                    {
                        function = ((CustomTreeNode)customRoot.Components["func1"]).Components["attr1"].ToString();
                    }
                    else
                    {
                        LogParameter.ActualValue.LogMessage("Can't find the funtion name");
                        return false;
                    }
                    
                    if (function.Contains("round"))
                    {
                        var allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var call_node1 = allowedChildSymbol_Call.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            if (symbol.ToString().Contains("round"))
                            {
                                call_node1 = symbol.CreateTreeNode();
                                if (call_node1.HasLocalParameters) call_node1.ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(call_node1);
                                break;
                            }
                        }
                        //handle 'round('<float>')'
                        BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["args1"], call_node1, grammar, 1);
                    }
                    else if (function.Contains("append"))
                    {
                        // <simple_stmt>
                        var allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var call_node1 = allowedChildSymbol_Call.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            if (symbol.ToString().Contains("<simple_stmt>"))
                            {
                                call_node1 = symbol.CreateTreeNode();
                                if (call_node1.HasLocalParameters) call_node1.ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(call_node1);
                                break;
                            }
                        }
                        // <call>
                        allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(call_node1.Symbol, 0);
                        var call_node2 = allowedChildSymbol_Call.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            if (symbol.ToString().Contains("<call>"))
                            {
                                call_node2 = symbol.CreateTreeNode();
                                if (call_node2.HasLocalParameters) call_node2.ResetLocalParameters(random);
                                call_node1.AddSubtree(call_node2);
                                break;
                            }
                        }
                        // <append>
                        allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(call_node2.Symbol, 0);
                        var call_node3 = allowedChildSymbol_Call.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            if (symbol.ToString().Contains("append"))
                            {
                                call_node3 = symbol.CreateTreeNode();
                                if (call_node3.HasLocalParameters) call_node3.ResetLocalParameters(random);
                                call_node2.AddSubtree(call_node3);
                                break;
                            }
                        }
                        //left variable
                        if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)((CustomTreeNode)customRoot.Components["func1"]).Components["value1"], call_node3, grammar, 1, 0)) {
                            // replace unmappable part with dummy variable
                            if (!BuildDummyValue(random, call_node3, grammar, 0))
                            {
                                return false;
                            }
                        }
                        //right variable
                        if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["args1"], call_node3, grammar, 1, 1))
                        {
                            // replace unmappable part with dummy variable
                            if (!BuildDummyValue(random, call_node3, grammar, 1))
                            {
                                return false;
                            }
                        }
                    }
                    else if (function.Contains("len"))
                    {
                        var allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var call_node1 = allowedChildSymbol_Call.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            if (symbol.ToString().Contains("'len('<"))
                            {
                                call_node1 = symbol.CreateTreeNode();
                                if (call_node1.HasLocalParameters) call_node1.ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(call_node1);
                                break;
                            }
                        }
                        //handle variable
                        if(!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["args" + lineNumber.ToString()], call_node1, grammar, 1))
                        {
                            /*// generate grammar: for arguments that unable to map
                            var gg_len = (CustomTreeNode)customRoot.Components["args" + lineNumber.ToString()];
                            StringBuilder sb_len = new StringBuilder("<int> ::= 'len(");
                            if (gg_len.Components["_type1"].ToString().Contains("ListComp"))
                            {
                                sb_len.Append("[");
                                sb_len.Append(((CustomTreeNode)gg_len.Components["elt1"]).Components["id1"].ToString());
                                sb_len.Append(" for ");
                                sb_len.Append(((CustomTreeNode)((CustomTreeNode)gg_len.Components["generators1"]).Components["target1"]).Components["id1"].ToString());
                                sb_len.Append(" in ");
                                sb_len.Append(((CustomTreeNode)((CustomTreeNode)gg_len.Components["generators1"]).Components["iter1"]).Components["id1"].ToString());
                                sb_len.Append(" if ");
                                sb_len.Append(((CustomTreeNode)((CustomTreeNode)((CustomTreeNode)gg_len.Components["generators1"]).Components["ifs1"]).Components["left1"]).Components["id1"].ToString());
                                if (((CustomTreeNode)((CustomTreeNode)((CustomTreeNode)gg_len.Components["generators1"]).Components["ifs1"]).Components["ops1"]).Components["_type1"].ToString().Contains("NotEq")){
                                    sb_len.Append(" != ");
                                }
                                if (((CustomTreeNode)((CustomTreeNode)((CustomTreeNode)gg_len.Components["generators1"]).Components["ifs1"]).Components["comparators1"]).Components["_type1"].ToString().Contains("Constant"))
                                {
                                    sb_len.Append(" \"");
                                    sb_len.Append(((CustomTreeNode)((CustomTreeNode)((CustomTreeNode)gg_len.Components["generators1"]).Components["ifs1"]).Components["comparators1"]).Components["value1"].ToString());
                                    sb_len.Append("\"");
                                }
                                sb_len.Append("]");
                            }
                            sb_len.Append(")'");
                            LogParameter.ActualValue.LogMessage("Generated Grammar:" + sb_len.ToString());*/

                            // replace unmappable part with dummy variable
                            if (!BuildDummyValue(random, call_node1, grammar, 0))
                            {
                                return false;
                            }
                        }
                    }
                    else if (function.Contains("isalpha"))
                    {
                        var allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var call_node1 = allowedChildSymbol_Call.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            if (symbol.ToString().Contains("isalpha"))
                            {
                                call_node1 = symbol.CreateTreeNode();
                                if (call_node1.HasLocalParameters) call_node1.ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(call_node1);
                                break;
                            }
                        }
                        //handle <char>
                        BuildSymbolicExpressionTree(random, (CustomTreeNode)((CustomTreeNode)customRoot.Components["func" + lineNumber.ToString()]).Components["value1"], call_node1, grammar, 1);
                    }
                    else if (function.Contains("ord"))
                    {
                        var allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var call_node1 = allowedChildSymbol_Call.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            if (symbol.ToString().Contains("ord"))
                            {
                                call_node1 = symbol.CreateTreeNode();
                                if (call_node1.HasLocalParameters) call_node1.ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(call_node1);
                                break;
                            }
                        }
                        //handle <char>
                        if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["args" + lineNumber.ToString()], call_node1, grammar, 1))
                        {
                            // replace unmappable part with dummy variable
                            if (!BuildDummyValue(random, call_node1, grammar, 0))
                            {
                                return false;
                            }
                        }
                            
                    }
                    else if (function.Contains("min"))
                    {
                        var allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        //number of arguments
                        var call_min_args_CTN = (CustomTreeNode)customRoot.Components["args1"];
                        List<ISymbolicExpressionTreeNode> min_SymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>();
                        int min_NumberOfArgs = 2;
                        while (call_min_args_CTN.Components.ContainsKey("_type" + (min_NumberOfArgs+1).ToString()))
                        {
                            min_NumberOfArgs += 1;
                        }
                        
                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            //LogParameter.ActualValue.LogMessage(symbol.Name);
                            if (symbol.ToString().Contains("'min('<int>', '<int>')'"))
                            {
                                for (int i = 0; i < min_NumberOfArgs - 1; i++)
                                {
                                    //LogParameter.ActualValue.LogMessage("in here!");
                                    min_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                }
                                break;
                            }
                            
                        }
                        
                        symbolicRoot.AddSubtree(min_SymbolicTreeNodeList[0]);
                        BuildSymbolicExpressionTree(random, call_min_args_CTN, min_SymbolicTreeNodeList[0], grammar, 1, 0);
                        int min_index=1;
                        for (min_index = 1; min_index < min_NumberOfArgs - 1; min_index++)
                        {
                            min_SymbolicTreeNodeList[min_index - 1].AddSubtree(min_SymbolicTreeNodeList[min_index]);
                            BuildSymbolicExpressionTree(random, call_min_args_CTN, min_SymbolicTreeNodeList[min_index], grammar, min_index + 1, 0);
                        }
                        BuildSymbolicExpressionTree(random, call_min_args_CTN, min_SymbolicTreeNodeList[min_index-1], grammar, min_index + 1, 1);
                    }
                    else if (function.Contains("lower"))
                    {
                        var allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        //number of arguments
                        var call_min_args_CTN = (CustomTreeNode)customRoot.Components["args1"];
                        List<ISymbolicExpressionTreeNode> min_SymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>();
                        int min_NumberOfArgs = 2;
                        while (call_min_args_CTN.Components.ContainsKey("_type" + (min_NumberOfArgs + 1).ToString()))
                        {
                            min_NumberOfArgs += 1;
                        }

                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            //LogParameter.ActualValue.LogMessage(symbol.Name);
                            if (symbol.ToString().Contains("'min('<int>', '<int>')'"))
                            {
                                for (int i = 0; i < min_NumberOfArgs - 1; i++)
                                {
                                    //LogParameter.ActualValue.LogMessage("in here!");
                                    min_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                }
                                break;
                            }

                        }

                        symbolicRoot.AddSubtree(min_SymbolicTreeNodeList[0]);
                        BuildSymbolicExpressionTree(random, call_min_args_CTN, min_SymbolicTreeNodeList[0], grammar, 1, 0);
                        int min_index = 1;
                        for (min_index = 1; min_index < min_NumberOfArgs - 1; min_index++)
                        {
                            min_SymbolicTreeNodeList[min_index - 1].AddSubtree(min_SymbolicTreeNodeList[min_index]);
                            BuildSymbolicExpressionTree(random, call_min_args_CTN, min_SymbolicTreeNodeList[min_index], grammar, min_index + 1, 0);
                        }
                        BuildSymbolicExpressionTree(random, call_min_args_CTN, min_SymbolicTreeNodeList[min_index - 1], grammar, min_index + 1, 1);
                    }
                    else if (function.Contains("range"))
                    {
                        if(((CustomTreeNode)customRoot.Components["args" + lineNumber.ToString()]).Components.ContainsKey("_type2"))
                        {
                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["args" + lineNumber.ToString()], symbolicRoot, grammar, 1, childSubtreeIndex))
                            {
                                if (!BuildDummyValue(random, symbolicRoot, grammar, childSubtreeIndex))
                                {
                                    LogParameter.ActualValue.LogMessage("Case Call: range unable to assign variable");
                                    return false;
                                }

                            }
                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["args" + lineNumber.ToString()], symbolicRoot, grammar, 2, childSubtreeIndex + 1))
                            {
                                if (!BuildDummyValue(random, symbolicRoot, grammar, childSubtreeIndex + 1))
                                {
                                    LogParameter.ActualValue.LogMessage("Case Call: range unable to assign variable");
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            CustomTreeNode call_range_temp_customNote = new CustomTreeNode();
                            call_range_temp_customNote.Components["_type1"] = "Constant";
                            call_range_temp_customNote.Components["value1"] = "0";
                            BuildSymbolicExpressionTree(random, call_range_temp_customNote, symbolicRoot, grammar, 1, childSubtreeIndex);

                            if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["args" + lineNumber.ToString()], symbolicRoot, grammar, 1, childSubtreeIndex + 1))
                            {
                                if (!BuildDummyValue(random, symbolicRoot, grammar, childSubtreeIndex + 1))
                                {
                                    LogParameter.ActualValue.LogMessage("Case Call: range unable to assign variable");
                                    return false;
                                }
                            }
                        }

                        
                    }
                    else if (function.Contains("replace"))
                    {
                        var allowedChildSymbol_Call = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var call_node1 = allowedChildSymbol_Call.First().CreateTreeNode();//initialize random for avoid error
                        foreach (var symbol in allowedChildSymbol_Call)
                        {
                            if (symbol.ToString().Contains("<string>'.replace('<string>','<string>')'"))
                            {
                                call_node1 = symbol.CreateTreeNode();
                                if (call_node1.HasLocalParameters) call_node1.ResetLocalParameters(random);
                                symbolicRoot.AddSubtree(call_node1);
                                break;
                            }
                        }
                        if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)((CustomTreeNode)customRoot.Components["func" + lineNumber.ToString()]).Components["value1"], call_node1, grammar, 1, 0))
                        {
                            if (!BuildDummyValue(random, call_node1, grammar, 0))
                            {
                                LogParameter.ActualValue.LogMessage("Case Call: replace unable to assign variable0");
                                return false;
                            }
                        }
                        if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["args" + lineNumber.ToString()], call_node1, grammar, 1, 1))
                        {
                            if (!BuildDummyValue(random, call_node1, grammar, 1))
                            {
                                LogParameter.ActualValue.LogMessage("Case Call: replace unable to assign variable1");
                                return false;
                            }
                        }
                        if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["args" + lineNumber.ToString()], call_node1, grammar, 2, 2))
                        {
                            if (!BuildDummyValue(random, call_node1, grammar, 2))
                            {
                                LogParameter.ActualValue.LogMessage("Case Call: replace unable to assign variable2");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        LogParameter.ActualValue.LogMessage("Called function unseen:" + function);

                        // generate grammar: for functions that unable to map
                        StringBuilder sb_newFunction = new StringBuilder(GetSubtreeType(symbolicRoot, childSubtreeIndex) + " ::= ");
                        LogParameter.ActualValue.LogMessage("Generated Grammar: " + sb_newFunction.ToString());
                        /*if (customRoot.parentNode.Components["_type1"].ToString().Contains("If")){
                            //gg_newFunction.Append("<bool> ::= '");
                            //this one is build for handle isInteger // attribute type fucntion
                            if (((CustomTreeNode)((CustomTreeNode)customRoot.Components["func1"]).Components["value1"]).Components["_type1"].ToString().Contains("BinOp"))
                            {
                                if (((CustomTreeNode)((CustomTreeNode)((CustomTreeNode)customRoot.Components["func1"]).Components["value1"]).Components["op1"]).Components["_type1"].ToString().Contains("Pow")) { }
                            }
                        }*/

                        return false;
                    }
                    break;
                #endregion

                case "While":
                    #region case While
                    var allowedChildSymbol_While = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                    List<ISymbolicExpressionTreeNode> whileSymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>
                    {
                        symbolicRoot
                    };

                    //<compound_stmt>
                    foreach (var symbol in allowedChildSymbol_While)
                    {
                        //LogParameter.ActualValue.LogMessage(symbol.Name);
                        if (symbol.Name.Contains("<compound_stmt>"))
                        {
                            var While_node1 = symbol.CreateTreeNode();
                            whileSymbolicTreeNodeList.Add(While_node1);
                            whileSymbolicTreeNodeList[whileSymbolicTreeNodeList.Count - 2].AddSubtree(While_node1);
                            break;
                        }
                    }

                    //<loop>
                    allowedChildSymbol_While = grammar.GetAllowedChildSymbols(whileSymbolicTreeNodeList.Last().Symbol, 0);
                    whileSymbolicTreeNodeList.Add(allowedChildSymbol_While.Last().CreateTreeNode());
                    whileSymbolicTreeNodeList[whileSymbolicTreeNodeList.Count - 2].AddSubtree(whileSymbolicTreeNodeList.Last());
                    int parentIndex = whileSymbolicTreeNodeList.Count - 1;

                    //handle <bool>
                    BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["test" + lineNumber.ToString()], whileSymbolicTreeNodeList[parentIndex], grammar, 1);

                    //handle <code>
                    var whileloopBodyTreeNode = (CustomTreeNode)customRoot.Components["body" + lineNumber.ToString()];
                    List<ISymbolicExpressionTreeNode> whilelineSymbolicTreeNode_list = new List<ISymbolicExpressionTreeNode>();
                    allowedChildSymbol_While = grammar.GetAllowedChildSymbols(whileSymbolicTreeNodeList[parentIndex].Symbol, 1);
                    for (int i = 0; i < whileloopBodyTreeNode.numberOfLines-1; i++)
                    {
                        whilelineSymbolicTreeNode_list.Add(allowedChildSymbol_While.ElementAt(1).CreateTreeNode());
                    }
                    whilelineSymbolicTreeNode_list.Add(allowedChildSymbol_While.Last().CreateTreeNode());
                    foreach (var tempNode in whilelineSymbolicTreeNode_list)
                    {
                        if (tempNode.HasLocalParameters) tempNode.ResetLocalParameters(random);
                    }
                    whileSymbolicTreeNodeList[parentIndex].AddSubtree(whilelineSymbolicTreeNode_list[0]);
                    BuildSymbolicExpressionTree(random, whileloopBodyTreeNode, whilelineSymbolicTreeNode_list[0], grammar, 1);
                    for (int i = 1; i < whileloopBodyTreeNode.numberOfLines; i++)
                    {
                        whilelineSymbolicTreeNode_list[i - 1].AddSubtree(whilelineSymbolicTreeNode_list[i]);
                        BuildSymbolicExpressionTree(random, whileloopBodyTreeNode, whilelineSymbolicTreeNode_list[i], grammar, i+1);
                    }

                    break;
                #endregion

                case "Compare":
                    #region case Compare
                    //connected from <bool>
                    List<ISymbolicExpressionTreeNode> compare_SymbolicTreeNodeList = new List<ISymbolicExpressionTreeNode>
                    {
                        symbolicRoot
                    };

                    //determine number of comparators
                    var allowedChildSymbol_Compare = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                    var compare_ops_CTN = (CustomTreeNode)customRoot.Components["ops" + lineNumber.ToString()];
                    var compare_right_comparators = (CustomTreeNode)customRoot.Components["comparators" + lineNumber.ToString()];
                    var compare_left_CTN = (CustomTreeNode)customRoot.Components["left" + lineNumber.ToString()];

                    string compare_variable_type;
                    var variableTypes = GetCompareType(customRoot);
                    if (variableTypes.Contains("len"))
                    {
                        compare_variable_type = "int";
                    }
                    else if(variableTypes.Contains("string"))
                    {
                        compare_variable_type = "string";
                    }
                    else if (variableTypes.Contains("char"))
                    {
                        compare_variable_type = "char";
                    }
                    else if (variableTypes.Contains("float"))
                    {
                        compare_variable_type = "float";
                    }
                    else if (variableTypes.Contains("int"))
                    {
                        compare_variable_type = "int";
                    }
                    else
                    {
                        throw new Exception("Compare: compare type not seen!");
                    }

                    if(compare_variable_type == "float" || compare_variable_type == "int")
                    {
                        int numberOfComparators = 1;
                        if (compare_ops_CTN.Components.ContainsKey("_type2"))//when number of comparators > 2
                        {
                            while (compare_ops_CTN.Components.ContainsKey("_type" + (numberOfComparators + 1).ToString()))
                            {
                                foreach (var symbol in allowedChildSymbol_Compare)
                                {
                                    //LogParameter.ActualValue.LogMessage(symbol.Name);
                                    if (symbol.Name.Contains("'('<bool>' '<bool_op>' '<bool>')'"))
                                    {
                                        //LogParameter.ActualValue.LogMessage("reached here");
                                        compare_SymbolicTreeNodeList.Add(symbol.CreateTreeNode());
                                        if (compare_SymbolicTreeNodeList.Last().HasLocalParameters) compare_SymbolicTreeNodeList.Last().ResetLocalParameters(random);
                                    }
                                }
                                numberOfComparators++;
                            }
                        }

                        // connect symbolic tree for number of comparators

                        int compare_index;
                        string tempType_Compare = "";
                        for (compare_index = 1; compare_index < numberOfComparators; compare_index++)
                        {
                            compare_SymbolicTreeNodeList[compare_index - 1].AddSubtree(compare_SymbolicTreeNodeList[compare_index]);
                            #region left <bool>
                            //left <bool>
                            //check type of comparison symbolic tree node: <int>' '<comp_op>' '<int> or <float>' '<comp_op>' '<float>

                            /*if (compare_index == 1)
                            {
                                if (compare_left_CTN.Components["_type1"].ToString().Contains("Call"))// when left comparator is a function
                                {
                                    var functionType = GetFunctionType(((CustomTreeNode)compare_left_CTN.Components["func1"]).Components["id1"].ToString());
                                    if (functionType.Contains("int"))
                                    {
                                        tempType_Compare = "<int>' '<comp_op>' '<int>";
                                    }
                                    else if (functionType.Contains("float"))
                                    {
                                        tempType_Compare = "<float>' '<comp_op>' '<float>";
                                    }
                                    else
                                    {
                                        throw new Exception("Compare function type not seen");
                                    }
                                }
                                else if (compare_right_comparators.Components["_type1"].ToString().Contains("Call"))// when right comparator is a function
                                {
                                    var functionType = GetFunctionType(((CustomTreeNode)compare_right_comparators.Components["func1"]).Components["id1"].ToString());
                                    if (functionType.Contains("int"))
                                    {
                                        tempType_Compare = "<int>' '<comp_op>' '<int>";
                                    }
                                    else if (functionType.Contains("float"))
                                    {
                                        tempType_Compare = "<float>' '<comp_op>' '<float>";
                                    }
                                    else
                                    {
                                        throw new Exception("Compare function type not seen");
                                    }
                                }
                                else// when comparation object is constant or variable
                                {
                                    tempType_Compare = "<float>' '<comp_op>' '<float>";
                                }
                            }
                            else
                            {
                                if (compare_right_comparators.Components["_type" + (compare_index - 1).ToString()].ToString().Contains("Call"))// when left comparator is a function
                                {
                                    var functionType = GetFunctionType(((CustomTreeNode)compare_right_comparators.Components["func" + (compare_index - 1).ToString()]).Components["id1"].ToString());
                                    if (functionType.Contains("int"))
                                    {
                                        tempType_Compare = "<int>' '<comp_op>' '<int>";
                                    }
                                    else if (functionType.Contains("float"))
                                    {
                                        tempType_Compare = "<float>' '<comp_op>' '<float>";
                                    }
                                    else
                                    {
                                        throw new Exception("Compare function type not seen");
                                    }
                                }
                                else if (compare_right_comparators.Components["_type" + compare_index.ToString()].ToString().Contains("Call"))// when right comparator is a function
                                {
                                    var functionType = GetFunctionType(((CustomTreeNode)compare_right_comparators.Components["func" + compare_index.ToString()]).Components["id1"].ToString());
                                    if (functionType.Contains("int"))
                                    {
                                        tempType_Compare = "<int>' '<comp_op>' '<int>";
                                    }
                                    else if (functionType.Contains("float"))
                                    {
                                        tempType_Compare = "<float>' '<comp_op>' '<float>";
                                    }
                                    else
                                    {
                                        throw new Exception("Compare function type not seen");
                                    }
                                }
                                else// when comparation object is constant or variable
                                {
                                    tempType_Compare = "<float>' '<comp_op>' '<float>";
                                }
                            }*/

                            if(compare_variable_type == "int") 
                            {
                                tempType_Compare = "<int>' '<comp_op>' '<int>";
                            }
                            else if (compare_variable_type == "float")
                            {
                                tempType_Compare = "<float>' '<comp_op>' '<float>";
                            }

                            //LogParameter.ActualValue.LogMessage("--- type:" + tempType_Compare);

                            //assign determined symbolic tree node: <int>' '<comp_op>' '<int> or <float>' '<comp_op>' '<float>
                            allowedChildSymbol_Compare = grammar.GetAllowedChildSymbols(compare_SymbolicTreeNodeList[compare_index].Symbol, childSubtreeIndex);
                            var compare_node1 = allowedChildSymbol_Compare.First().CreateTreeNode();
                            foreach (var symbol in allowedChildSymbol_Compare)
                            {
                                if (symbol.Name.Contains(tempType_Compare))
                                {
                                    compare_node1 = symbol.CreateTreeNode();
                                    if (compare_node1.HasLocalParameters) compare_node1.ResetLocalParameters(random);
                                    compare_SymbolicTreeNodeList[compare_index].AddSubtree(compare_node1);
                                    break;
                                }
                            }
                            if (compare_index == 1)
                            {
                                //left <variable>
                                BuildSymbolicExpressionTree(random, compare_left_CTN, compare_node1, grammar, 1, 0);
                                //<comp_op>
                                allowedChildSymbol_Compare = grammar.GetAllowedChildSymbols(compare_node1.Symbol, 1);
                                //LogParameter.ActualValue.LogMessage("Testing this one:" + compare_node1.Symbol.Name);
                                var compare_node2 = allowedChildSymbol_Compare.First().CreateTreeNode();
                                string compare_type = ((CustomTreeNode)customRoot.Components["ops1"]).Components["_type" + compare_index.ToString()].ToString();
                                string compare_symbol = "";
                                if (compare_type.Equals("NotEq"))
                                {
                                    compare_symbol = "!=";
                                }
                                else if (compare_type.Equals("Eq"))
                                {
                                    compare_symbol = "==";
                                }
                                else if (compare_type.Equals("GtE"))
                                {
                                    compare_symbol = ">=";
                                }
                                else if (compare_type.Equals("LtE"))
                                {
                                    compare_symbol = "<=";
                                }
                                else if (compare_type.Equals("Lt"))
                                {
                                    compare_symbol = "<";
                                }
                                else if (compare_type.Equals("Gt"))
                                {
                                    compare_symbol = ">";
                                }
                                else
                                {
                                    throw new Exception("the compare symbol not seen!");
                                }
                                foreach (var symbol in allowedChildSymbol_Compare)
                                {
                                    if (symbol.Name.Contains(compare_symbol))
                                    {
                                        compare_node2 = symbol.CreateTreeNode();
                                        if (compare_node2.HasLocalParameters) compare_node2.ResetLocalParameters(random);
                                        compare_node1.AddSubtree(compare_node2);
                                        break;
                                    }
                                }
                                //right <variable>
                                BuildSymbolicExpressionTree(random, compare_right_comparators, compare_node1, grammar, 1, 2);
                            }
                            else
                            {
                                //left <variable>
                                BuildSymbolicExpressionTree(random, compare_right_comparators, compare_node1, grammar, compare_index - 1, 0);
                                //<comp_op>
                                allowedChildSymbol_Compare = grammar.GetAllowedChildSymbols(compare_node1.Symbol, 1);
                                var compare_node2 = allowedChildSymbol_Compare.First().CreateTreeNode();
                                string compare_type = ((CustomTreeNode)customRoot.Components["ops1"]).Components["_type" + compare_index.ToString()].ToString();
                                string compare_symbol = "";
                                if (compare_type.Equals("NotEq"))
                                {
                                    compare_symbol = "!=";
                                }
                                else if (compare_type.Equals("Eq"))
                                {
                                    compare_symbol = "==";
                                }
                                else if (compare_type.Equals("GtE"))
                                {
                                    compare_symbol = ">=";
                                }
                                else if (compare_type.Equals("LtE"))
                                {
                                    compare_symbol = "<=";
                                }
                                else if (compare_type.Equals("Lt"))
                                {
                                    compare_symbol = "<";
                                }
                                else if (compare_type.Equals("Gt"))
                                {
                                    compare_symbol = ">";
                                }
                                else
                                {
                                    throw new Exception("the compare symbol not seen!");
                                }
                                foreach (var symbol in allowedChildSymbol_Compare)
                                {
                                    if (symbol.Name.Contains(compare_symbol))
                                    {
                                        compare_node2 = symbol.CreateTreeNode();
                                        if (compare_node2.HasLocalParameters) compare_node2.ResetLocalParameters(random);
                                        compare_node1.AddSubtree(compare_node2);
                                        break;
                                    }
                                }
                                //right <variable>
                                BuildSymbolicExpressionTree(random, compare_right_comparators, compare_node1, grammar, compare_index, 2);
                            }
                            #endregion
                            //<bool_op>
                            allowedChildSymbol_Compare = grammar.GetAllowedChildSymbols(compare_SymbolicTreeNodeList[compare_index].Symbol, 1);
                            var compare_node3 = allowedChildSymbol_Compare.First().CreateTreeNode();//'and'
                            foreach (var symbol in allowedChildSymbol_Compare)
                            {
                                if (symbol.Name.Contains("'and'"))
                                {
                                    compare_node3 = symbol.CreateTreeNode();
                                    if (compare_node3.HasLocalParameters) compare_node3.ResetLocalParameters(random);
                                    compare_SymbolicTreeNodeList[compare_index].AddSubtree(compare_node3);
                                    break;
                                }
                            }
                            childSubtreeIndex = 2;
                        }

                        if (compare_index == 1)
                        {
                            /*if (compare_left_CTN.Components["_type1"].ToString().Contains("Call"))// when left comparator is a function
                            {
                                var functionType = GetFunctionType(((CustomTreeNode)compare_left_CTN.Components["func1"]).Components["id1"].ToString());
                                if (functionType.Contains("int"))
                                {
                                    tempType_Compare = "<int>' '<comp_op>' '<int>";
                                }
                                else if (functionType.Contains("float"))
                                {
                                    tempType_Compare = "<float>' '<comp_op>' '<float>";
                                }
                                else
                                {
                                    throw new Exception("Compare function type not seen");
                                }
                            }
                            else if (compare_right_comparators.Components["_type1"].ToString().Contains("Call"))// when right comparator is a function
                            {
                                var functionType = GetFunctionType(((CustomTreeNode)compare_right_comparators.Components["func1"]).Components["id1"].ToString());
                                if (functionType.Contains("int"))
                                {
                                    tempType_Compare = "<int>' '<comp_op>' '<int>";
                                }
                                else if (functionType.Contains("float"))
                                {
                                    tempType_Compare = "<float>' '<comp_op>' '<float>";
                                }
                                else
                                {
                                    throw new Exception("Compare function type not seen");
                                }
                            }
                            else// when comparation object is constant or variable
                            {
                                tempType_Compare = "<float>' '<comp_op>' '<float>";
                            }*/

                            if (compare_variable_type == "int")
                            {
                                tempType_Compare = "<int>' '<comp_op>' '<int>";
                            }
                            else if (compare_variable_type == "float")
                            {
                                tempType_Compare = "<float>' '<comp_op>' '<float>";
                            }

                            allowedChildSymbol_Compare = grammar.GetAllowedChildSymbols(compare_SymbolicTreeNodeList.Last().Symbol, childSubtreeIndex);
                            var compare_node4 = allowedChildSymbol_Compare.First().CreateTreeNode();
                            foreach (var symbol in allowedChildSymbol_Compare)
                            {
                                if (symbol.Name.Contains(tempType_Compare))
                                {
                                    compare_node4 = symbol.CreateTreeNode();
                                    if (compare_node4.HasLocalParameters) compare_node4.ResetLocalParameters(random);
                                    compare_SymbolicTreeNodeList.Last().AddSubtree(compare_node4);
                                    break;
                                }
                            }

                            //left <variable>
                            BuildSymbolicExpressionTree(random, compare_left_CTN, compare_node4, grammar, 1, 0);
                            //<comp_op>
                            allowedChildSymbol_Compare = grammar.GetAllowedChildSymbols(compare_node4.Symbol, 1);
                            //LogParameter.ActualValue.LogMessage(compare_node4.Symbol.Name + "--- type:" + tempType_Compare);
                            var compare_node5 = allowedChildSymbol_Compare.First().CreateTreeNode();
                            string compare_type = ((CustomTreeNode)customRoot.Components["ops1"]).Components["_type" + compare_index.ToString()].ToString();
                            string compare_symbol = "";
                            if (compare_type.Equals("NotEq"))
                            {
                                compare_symbol = "!=";
                            }
                            else if (compare_type.Equals("Eq"))
                            {
                                compare_symbol = "==";
                            }
                            else if (compare_type.Equals("GtE"))
                            {
                                compare_symbol = ">=";
                            }
                            else if (compare_type.Equals("LtE"))
                            {
                                compare_symbol = "<=";
                            }
                            else if (compare_type.Equals("Lt"))
                            {
                                compare_symbol = "<";
                            }
                            else if (compare_type.Equals("Gt"))
                            {
                                compare_symbol = ">";
                            }
                            else
                            {
                                throw new Exception("the compare symbol not seen!");
                            }
                            foreach (var symbol in allowedChildSymbol_Compare)
                            {
                                if (symbol.Name.Contains(compare_symbol))
                                {
                                    compare_node5 = symbol.CreateTreeNode();
                                    if (compare_node5.HasLocalParameters) compare_node5.ResetLocalParameters(random);
                                    compare_node4.AddSubtree(compare_node5);
                                    break;
                                }
                            }
                            //right <variable>
                            BuildSymbolicExpressionTree(random, compare_right_comparators, compare_node4, grammar, 1, 2);

                        }
                        else
                        {
                            /*if (compare_right_comparators.Components["_type" + (compare_index - 1).ToString()].ToString().Contains("Call"))// when left comparator is a function
                            {
                                var functionType = GetFunctionType(((CustomTreeNode)compare_right_comparators.Components["func" + (compare_index - 1).ToString()]).Components["id1"].ToString());
                                if (functionType.Contains("int"))
                                {
                                    tempType_Compare = "<int>' '<comp_op>' '<int>";
                                }
                                else if (functionType.Contains("float"))
                                {
                                    tempType_Compare = "<float>' '<comp_op>' '<float>";
                                }
                                else
                                {
                                    throw new Exception("Compare function type not seen");
                                }
                            }
                            else if (compare_right_comparators.Components["_type" + compare_index.ToString()].ToString().Contains("Call"))// when right comparator is a function
                            {
                                var functionType = GetFunctionType(((CustomTreeNode)compare_right_comparators.Components["func" + compare_index.ToString()]).Components["id1"].ToString());
                                if (functionType.Contains("int"))
                                {
                                    tempType_Compare = "<int>' '<comp_op>' '<int>";
                                }
                                else if (functionType.Contains("float"))
                                {
                                    tempType_Compare = "<float>' '<comp_op>' '<float>";
                                }
                                else
                                {
                                    throw new Exception("Compare function type not seen");
                                }
                            }
                            else// when comparation object is constant or variable
                            {
                                tempType_Compare = "<float>' '<comp_op>' '<float>";
                            }*/

                            allowedChildSymbol_Compare = grammar.GetAllowedChildSymbols(compare_SymbolicTreeNodeList.Last().Symbol, childSubtreeIndex);
                            var compare_node4 = allowedChildSymbol_Compare.First().CreateTreeNode();
                            foreach (var symbol in allowedChildSymbol_Compare)
                            {
                                if (symbol.Name.Contains(tempType_Compare))
                                {
                                    compare_node4 = symbol.CreateTreeNode();
                                    if (compare_node4.HasLocalParameters) compare_node4.ResetLocalParameters(random);
                                    compare_SymbolicTreeNodeList.Last().AddSubtree(compare_node4);
                                    break;
                                }
                            }

                            //left <variable>
                            BuildSymbolicExpressionTree(random, compare_right_comparators, compare_node4, grammar, compare_index - 1, 0);
                            //<comp_op>
                            allowedChildSymbol_Compare = grammar.GetAllowedChildSymbols(compare_node4.Symbol, 1);
                            var compare_node5 = allowedChildSymbol_Compare.First().CreateTreeNode();
                            string compare_type = ((CustomTreeNode)customRoot.Components["ops1"]).Components["_type" + compare_index.ToString()].ToString();
                            string compare_symbol = "";
                            if (compare_type.Equals("NotEq"))
                            {
                                compare_symbol = "!=";
                            }
                            else if (compare_type.Equals("Eq"))
                            {
                                compare_symbol = "==";
                            }
                            else if (compare_type.Equals("GtE"))
                            {
                                compare_symbol = ">=";
                            }
                            else if (compare_type.Equals("LtE"))
                            {
                                compare_symbol = "<=";
                            }
                            else if (compare_type.Equals("Lt"))
                            {
                                compare_symbol = "<";
                            }
                            else if (compare_type.Equals("Gt"))
                            {
                                compare_symbol = ">";
                            }
                            else
                            {
                                throw new Exception("the compare symbol not seen!");
                            }
                            foreach (var symbol in allowedChildSymbol_Compare)
                            {
                                if (symbol.Name.Contains(compare_symbol))
                                {
                                    compare_node5 = symbol.CreateTreeNode();
                                    if (compare_node5.HasLocalParameters) compare_node5.ResetLocalParameters(random);
                                    compare_node4.AddSubtree(compare_node5);
                                    break;
                                }
                            }
                            //right <variable>
                            BuildSymbolicExpressionTree(random, compare_right_comparators, compare_node4, grammar, compare_index, 2);
                        }
                    }
                    else if (compare_variable_type == "char" || compare_variable_type == "string")
                    {
                        var compare_node1 = allowedChildSymbol_Compare.First().CreateTreeNode();
                        foreach(var symbol in allowedChildSymbol_Compare)
                        {
                            if (compare_ops_CTN.Components["_type1"].Equals("Eq"))
                            {
                                if (symbol.Name.Contains("'('<string>' == '<string>')'"))
                                {
                                    compare_node1 = symbol.CreateTreeNode();
                                    symbolicRoot.AddSubtree(compare_node1);
                                }
                            }
                            else if (compare_ops_CTN.Components["_type1"].Equals("In"))
                            {
                                if (symbol.Name.Contains("'('<string>' in '<string>')'"))
                                {
                                    compare_node1 = symbol.CreateTreeNode();
                                    symbolicRoot.AddSubtree(compare_node1);
                                }
                            }
                            else
                            {
                                LogParameter.ActualValue.LogMessage("String Compare: ops not seen --> " + compare_ops_CTN.Components["_type1"].ToString());
                                return false;
                            }
                        }
                        //left variable
                        BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["left" + lineNumber.ToString()], compare_node1, grammar, 1, 0);
                        //right
                        BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["comparators" + lineNumber.ToString()], compare_node1, grammar, 1, 1);
                        
                    }
                    else
                    {
                        LogParameter.ActualValue.LogMessage("Compare: compare type not implemented!");
                        return false;
                    }
                    break;
                #endregion

                case "Expr":
                    #region case Expr
                    BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["value1"], symbolicRoot, grammar, 1, childSubtreeIndex);
                    break;
                #endregion

                case "BoolOp":
                    #region case BoolOp
                    //connected from <bool>
                    List<ISymbolicExpressionTreeNode> boolOp_SETNList = new List<ISymbolicExpressionTreeNode>
                    {
                        symbolicRoot
                    };
                    var allowedChildSymbol_boolOp = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                    foreach (var symbol in allowedChildSymbol_boolOp)
                    {
                        if (symbol.Name.Contains("'('<bool>' '<bool_op>' '<bool>')'"))
                        {
                            boolOp_SETNList.Add(symbol.CreateTreeNode());
                            symbolicRoot.AddSubtree(boolOp_SETNList.Last());
                            break;
                        }
                    }

                    //left
                    BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["values1"], boolOp_SETNList.Last(), grammar, 1, 0);

                    //<bool_op>
                    allowedChildSymbol_boolOp = grammar.GetAllowedChildSymbols(boolOp_SETNList.Last().Symbol, 1);
                    var boolOp_Node1 = allowedChildSymbol_boolOp.First().CreateTreeNode();
                    foreach (var symbol in allowedChildSymbol_boolOp)
                    {

                        if (((CustomTreeNode)(customRoot.Components["op1"])).Components["_type1"].ToString().Contains("Or"))
                        {
                            if (symbol.Name.Contains("'or'"))
                            {
                                boolOp_Node1 = symbol.CreateTreeNode();
                                boolOp_SETNList.Last().AddSubtree(boolOp_Node1);
                                break;
                            }
                        }
                        else if (((CustomTreeNode)(customRoot.Components["op1"])).Components["_type1"].ToString().Contains("And"))
                        {
                            if (symbol.Name.Contains("'and'"))
                            {
                                boolOp_Node1 = symbol.CreateTreeNode();
                                boolOp_SETNList.Last().AddSubtree(boolOp_Node1);
                                break;
                            }
                        }
                        else
                        {
                            throw new Exception("boolOp: type not seen! -->" + ((CustomTreeNode)(customRoot.Components["op1"])).Components["_type1"].ToString());
                        }
                    }

                    //right
                    BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["values1"], boolOp_SETNList.Last(), grammar, 2, 2);

                    break;
                #endregion

                case "UnaryOp":
                    #region case UnaryOp:
                    if (symbolicRoot.Symbol.Name.Contains("int"))
                    {
                        var allowedChildSymbol_UnaryOp = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                        var unaryOp_node1 = allowedChildSymbol_UnaryOp.First().CreateTreeNode();
                        foreach (var symbol in allowedChildSymbol_UnaryOp)
                        {
                            if (symbol.Name.Contains("<arith_prefix><int>"))
                            {
                                unaryOp_node1 = symbol.CreateTreeNode();
                                symbolicRoot.AddSubtree(unaryOp_node1);
                                break;
                            }
                        }

                        //<arith_prefix>
                        allowedChildSymbol_UnaryOp = grammar.GetAllowedChildSymbols(unaryOp_node1.Symbol, 0);
                        var unaryOp_node2 = allowedChildSymbol_UnaryOp.First().CreateTreeNode();
                        foreach (var symbol in allowedChildSymbol_UnaryOp)
                        {
                            if (((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString().Contains("Sub"))
                            {
                                if (symbol.Name.Contains("-"))
                                {
                                    unaryOp_node2 = symbol.CreateTreeNode();
                                    unaryOp_node1.AddSubtree(unaryOp_node2);
                                    break;
                                }
                            }
                            else
                            {
                                LogParameter.ActualValue.LogMessage("case UnaryOp: unable to handle <arith_prefix> " + ((CustomTreeNode)customRoot.Components["op" + lineNumber.ToString()]).Components["_type1"].ToString());
                                return false;
                            }
                        }

                        //<int>
                        if(!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["operand" + lineNumber.ToString()], unaryOp_node1, grammar, 1, 1))
                        {
                            LogParameter.ActualValue.LogMessage("case UnaryOp: unable to handle <int>");
                            return false;
                        }
                    }
                    else
                    {
                        LogParameter.ActualValue.LogMessage("case UnaryOp: root type not implemented! root: " + symbolicRoot.Symbol.Name);
                        return false;
                    }
                    break;
                #endregion

                case "Subscript":
                    #region Subscript
                    var allowedChildSymbol_Subscript = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
                    var subscript_node1 = allowedChildSymbol_Subscript.First().CreateTreeNode();
                    foreach (var symbol in allowedChildSymbol_Subscript)
                    {
                        if (symbol.Name.Contains("'getIndexIntList('<list_int>', '<int>')'"))
                        {
                            subscript_node1 = symbol.CreateTreeNode();
                            symbolicRoot.AddSubtree(subscript_node1);
                            break;
                        }
                    }


                    //<list_int>
                    if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["value" + lineNumber.ToString()], subscript_node1, grammar, 1, 0))
                    {
                        BuildDummyValue(random, subscript_node1, grammar, 0);
                    }


                    //<int>
                    if (!BuildSymbolicExpressionTree(random, (CustomTreeNode)customRoot.Components["slice" + lineNumber.ToString()], subscript_node1, grammar, 1, 1))
                    {
                        BuildDummyValue(random, subscript_node1, grammar, 1);
                    }
                    break;
                    #endregion

                default:
                    LogParameter.ActualValue.LogMessage("switch default: component type not seen! type:" + type);

                    #region generate grammar based on unsuccessful mapping part
                    StringBuilder sb_default = new StringBuilder();
                    //generate header for grammar
                    if (customRoot.parentNode.Components["_type1"].ToString().Contains("Call"))
                    {
                        //need to get the group symbol of the type
                        string functionName = "";
                        if (((CustomTreeNode)customRoot.parentNode.Components["func1"]).Components.ContainsKey("id1"))
                        {
                            functionName = ((CustomTreeNode)customRoot.parentNode.Components["func1"]).Components["id1"].ToString();
                        }
                        else if (((CustomTreeNode)customRoot.parentNode.Components["func1"]).Components.ContainsKey("attr1"))
                        {
                            functionName = ((CustomTreeNode)customRoot.parentNode.Components["func1"]).Components["attr1"].ToString();
                        }
                        else
                        {
                            LogParameter.ActualValue.LogMessage("Can't find the funtion name, grammar generation failed!");
                        }
                        var functionType = GetFunctionType(functionName);
                        sb_default.Append("<" + functionType.ToString() + "> ::= '" + functionName + "(");
                    }

                    //generate body for grammar
                    switch (type)
                    {
                        case "ListComp":
                            sb_default.Append("[");
                            sb_default.Append(((CustomTreeNode)customRoot.Components["elt1"]).Components["id1"].ToString());
                            sb_default.Append(" for ");
                            sb_default.Append(((CustomTreeNode)((CustomTreeNode)customRoot.Components["generators1"]).Components["target1"]).Components["id1"].ToString());
                            sb_default.Append(" in ");
                            sb_default.Append(((CustomTreeNode)((CustomTreeNode)customRoot.Components["generators1"]).Components["iter1"]).Components["id1"].ToString());
                            sb_default.Append(" if ");
                            sb_default.Append(((CustomTreeNode)((CustomTreeNode)((CustomTreeNode)customRoot.Components["generators1"]).Components["ifs1"]).Components["left1"]).Components["id1"].ToString());
                            if (((CustomTreeNode)((CustomTreeNode)((CustomTreeNode)customRoot.Components["generators1"]).Components["ifs1"]).Components["ops1"]).Components["_type1"].ToString().Contains("NotEq"))
                            {
                                sb_default.Append(" != ");
                            }
                            if (((CustomTreeNode)((CustomTreeNode)((CustomTreeNode)customRoot.Components["generators1"]).Components["ifs1"]).Components["comparators1"]).Components["_type1"].ToString().Contains("Constant"))
                            {
                                sb_default.Append(" \"");
                                sb_default.Append(((CustomTreeNode)((CustomTreeNode)((CustomTreeNode)customRoot.Components["generators1"]).Components["ifs1"]).Components["comparators1"]).Components["value1"].ToString());
                                sb_default.Append("\"");
                            }
                            sb_default.Append("]");
                            break;

                        default:
                            LogParameter.ActualValue.LogMessage("grammar generation switch default: component type not seen! type:" + type);
                            break;
                    }

                    //generate footer for grammar
                    if (customRoot.parentNode.Components["_type1"].ToString().Contains("Call"))
                    {
                        sb_default.Append(")'");
                    }

                    using (StreamWriter writer = new StreamWriter("./" + ProblemName + "_generatedGrammar.txt"))
                    {
                        writer.WriteLine(sb_default.ToString());
                    }
                    LogParameter.ActualValue.LogMessage("Generated Grammar:" + sb_default.ToString());
                    #endregion

                    return false;
            }
            return true;
        }

        private bool BuildDummyValue(IRandom random, ISymbolicExpressionTreeNode symbolicRoot, ISymbolicExpressionGrammar grammar, int childSubtreeIndex = 0)
        {
            //LogParameter.ActualValue.LogMessage("reached here --> BuildDummyValue root synbol:" + symbolicRoot.Symbol.Name);//tes
            /*string symbolicRootType = "";
            if (symbolicRoot.Symbol.Name.Contains("int"))
            {
                symbolicRootType = "int";
            }
            else if (symbolicRoot.Symbol.Name.Contains("float"))
            {
                symbolicRootType = "float";
            }
            else if (symbolicRoot.Symbol.Name.Contains("string"))
            {
                symbolicRootType = "string";
            }
            else if (symbolicRoot.Symbol.Name.Contains("bool"))
            {
                symbolicRootType = "bool";
            }
            else
            {
                return false;
            }*/

            var allowedChildSymbol_DummyValue = grammar.GetAllowedChildSymbols(symbolicRoot.Symbol, childSubtreeIndex);
            var dummyValue_node1 = allowedChildSymbol_DummyValue.First().CreateTreeNode();
            foreach (var symbol in allowedChildSymbol_DummyValue)
            {
                //LogParameter.ActualValue.LogMessage("contain -->" + symbolicRootType + "_var");//test
                if (symbol.ToString().Contains("_var"))
                {
                    //LogParameter.ActualValue.LogMessage("reached here!!!" + " -- " + variable_name);//test
                    dummyValue_node1 = symbol.CreateTreeNode();
                    if (dummyValue_node1.HasLocalParameters) dummyValue_node1.ResetLocalParameters(random);
                    symbolicRoot.AddSubtree(dummyValue_node1);
                    break;
                }
            }

            allowedChildSymbol_DummyValue = grammar.GetAllowedChildSymbols(dummyValue_node1.Symbol, 0);
            var dummyValue_node2 = allowedChildSymbol_DummyValue.First().CreateTreeNode();
            foreach (var symbol in allowedChildSymbol_DummyValue)
            {
                if (!symbol.ToString().Contains("in") && !symbol.ToString().Contains("res") && !variable_name_mapping_dictionary.ContainsValue(symbol.Name))
                {
                    
                    dummyValue_node2 = symbol.CreateTreeNode();
                    if (dummyValue_node2.HasLocalParameters) dummyValue_node2.ResetLocalParameters(random);
                    dummyValue_node1.AddSubtree(dummyValue_node2);
                    LogParameter.ActualValue.LogMessage("Dummy variable created:" + " -- " + symbol.Name);
                    break;
                }
            }
            return true;
        }

        string GetVariableType(string variableName)
        {
            if (variable_type_dictionary.ContainsKey(variableName))
            {
                return variable_type_dictionary[variableName];
            }
            else
            {
                return "unknown";
            }
        }

        string GetFunctionType(string functionName)
        {
            foreach (var key in function_return_type_dictionary.Keys)
            {
                if (key.Contains(functionName))
                {
                    return function_return_type_dictionary[key];
                }
            }
            return "unknown";
        }

        /// <summary>
        /// iterate through given CTN and return all the variable types in the CTN as a list
        /// </summary>
        /// <param name="root"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        List<string> GetCompareType(CustomTreeNode root, List<string> returnType = null)
        {
            if (returnType == null)
            {
                returnType = new List<string>();
            }
            foreach (var key in root.Components.Keys)
            {
                if(root.Components[key] is CustomTreeNode)
                {
                    GetCompareType((CustomTreeNode)root.Components[key], returnType);
                }
                else if (key.Contains("id"))
                {
                    string temp = GetVariableType(root.Components[key].ToString());
                    if (temp == "unknown")
                    {
                        if (variable_name_mapping_dictionary.ContainsKey(root.Components[key].ToString()))
                        {
                            temp = GetVariableType(variable_name_mapping_dictionary[root.Components[key].ToString()]);
                        }
                        else
                        {
                            temp = root.Components[key].ToString();
                        }
                        
                    }
                    returnType.Add(temp);
                }
            }
            return returnType;
        }

        string GetConstantType(string constant)
        {
            foreach (char c in constant)
            {
                if (Char.IsLetter(c))
                {
                    return "string";
                }else if(c == '.')
                {
                    return "float";
                }
            }
            return "int";
        }

        string GetSubtreeType(ISymbolicExpressionTreeNode symbolicRoot, int subtreeIndex)
        {
            string s = symbolicRoot.Symbol.Name;


            var foundIndexes = new List<int>();
            for (int i = s.IndexOf('<'); i > -1; i = s.IndexOf('<', i + 1))
            {
                // for loop end when i=-1 ('a' not found)
                foundIndexes.Add(i);
            }

            var foundIndexes1 = new List<int>();
            for (int i = s.IndexOf('>'); i > -1; i = s.IndexOf('>', i + 1))
            {
                // for loop end when i=-1 ('a' not found)
                foundIndexes1.Add(i);
            }

            var types = new List<String>();
            for (int i = 0; i < foundIndexes1.Count; i++)
            {
                types.Add(s.Substring(foundIndexes[i], foundIndexes1[i] - foundIndexes[i] + 1));
            }

            return types[subtreeIndex];
        }

        void CollectCTNwithUknownVariableName(CustomTreeNode root, List<CustomTreeNode> CTNwithUknownVariableName, List<string> parentNameList, CustomTreeNode parentOfRoot = null, string parentName = null)
        {

            foreach (var key in root.Components.Keys)
            {
                if (root.Components[key] is CustomTreeNode)
                {
                    CollectCTNwithUknownVariableName((CustomTreeNode)root.Components[key], CTNwithUknownVariableName, parentNameList, root, key);
                }
                else if(key.Contains("id") && !variable_type_dictionary.Keys.Contains(root.Components[key].ToString()) && !variable_name_mapping_dictionary.Keys.Contains(root.Components[key].ToString()))
                {
                    //LogParameter.ActualValue.LogMessage("-------entered here!--------");
                    if (parentOfRoot != null)
                    {
                        CTNwithUknownVariableName.Add(parentOfRoot);
                        parentNameList.Add(parentName);
                    }
                }
            }
        }

    }
}
