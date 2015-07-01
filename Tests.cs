using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BlackBox
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestPrg()
        {
            // 2 tests for integer interval and 2 tests for unit interval.
            var prgTests = new ITestConfiguration[4];
            for (uint i = 0; i < prgTests.Length; i++)
            {
                prgTests[i] = new PrgTestConfiguration 
                {
                    Seed = i,
                    Iterations = 1000,
                    UniformInterval = i < 2 ? new Tuple<uint, uint>(i * 10, (i + 10) * 10) : null,
                };
            }
            Run(outputFilePatternExpected, outputFilePatternActual, outputJsonConfigFile, prgTests);

            for (uint i = 0; i < 2; i++)
            {
                // integer content so should be exact match
                CompareExactMatch(FormatPath(outputFilePatternExpected, i), FormatPath(outputFilePatternActual, i));
            }

            for (uint i = 2; i < 4; i++)
            {
                float[] expected = File.ReadAllLines(FormatPath(outputFilePatternExpected, i)).Select(l => Convert.ToSingle(l)).ToArray();
                float[] actual = File.ReadAllLines(FormatPath(outputFilePatternActual, i)).Select(l => Convert.ToSingle(l)).ToArray();

                Assert.AreEqual(expected.Length, actual.Length);
                for (int j = 0; j < expected.Length; j++)
                {
                    // allow slightly different float precision
                    Assert.IsTrue(Math.Abs(expected[j] - actual[j]) < PrecisionOffset);
                }
            }
        }

        [TestMethod]
        public void TestHash()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";

            var rand = new Random();

            var hashTests = new ITestConfiguration[5];
            for (uint i = 0; i < hashTests.Length; i++)
            {
                int numValues = rand.Next(10, 20);
                var values = new string[numValues];

                for (int v = 0; v < numValues; v++)
                {
                    if (rand.NextDouble() < 0.5)
                    {
                        // generate string of numbers only 
                        values[v] = rand.Next(1000000).ToString("000000");
                    }
                    else
                    {
                        // generate random string
                        int length = rand.Next(10, 20); // random length (in characters)

                        values[v] = new string(Enumerable
                            .Repeat(chars, length)
                            .Select(s => s[rand.Next(s.Length)])
                            .ToArray());
                    }
                }

                hashTests[i] = new HashTestConfiguration
                {
                    Values = values.ToList()
                };
            }

            Run(outputFilePatternExpected, outputFilePatternActual, outputJsonConfigFile, hashTests);

            for (uint i = 0; i < hashTests.Length; i++)
            {
                // integer content so should be exact match
                CompareExactMatch(FormatPath(outputFilePatternExpected, i), FormatPath(outputFilePatternActual, i));
            }
        }

        [TestMethod]
        public void TestEpsilonGreedy()
        {
            var epsilonGreedyTests = new EpsilonGreedyTestConfiguration[]
            {
                // No exploration
                new EpsilonGreedyTestConfiguration
                {
                    AppId = TestContext.TestName + "NoExplorationFixedActionContext",
                    ContextType = ContextType.FixedAction, // test fixed-action context
                    Epsilon = 0f,
                    NumberOfActions = 20,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 10 }
                },
                new EpsilonGreedyTestConfiguration
                {
                    AppId = TestContext.TestName + "NoExplorationVariableActionContext",
                    ContextType = ContextType.VariableAction, // test variable-action context
                    Epsilon = 0f,
                    NumberOfActions = 20,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 10 }
                },

                // Regular exploration
                new EpsilonGreedyTestConfiguration
                {
                    AppId = TestContext.TestName + "RegularExplorationFixedActionContext",
                    ContextType = ContextType.FixedAction,
                    Epsilon = 0.2f,
                    NumberOfActions = 10,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 9 }
                },
                new EpsilonGreedyTestConfiguration
                {
                    AppId = TestContext.TestName + "RegularExplorationVariableActionContext",
                    ContextType = ContextType.VariableAction,
                    Epsilon = 0.2f,
                    NumberOfActions = 10,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 9 }
                },

                // Heavy exploration
                new EpsilonGreedyTestConfiguration
                {
                    AppId = TestContext.TestName + "HeavyExplorationFixedActionContext",
                    ContextType = ContextType.FixedAction,
                    Epsilon = 0.9f,
                    NumberOfActions = 90, // test many actions
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 81 }
                },
                new EpsilonGreedyTestConfiguration
                {
                    AppId = TestContext.TestName + "HeavyExplorationVariableActionContext",
                    ContextType = ContextType.VariableAction,
                    Epsilon = 0.9f,
                    NumberOfActions = 90,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 81 }
                }
            };

            Run(outputFilePatternExpected, outputFilePatternActual, outputJsonConfigFile, epsilonGreedyTests);

            for (uint i = 0; i < epsilonGreedyTests.Length; i++)
            {
                CompareExplorationData(FormatPath(outputFilePatternExpected, i), FormatPath(outputFilePatternActual, i));
            }
        }

        [TestMethod]
        public void TestTauFirst()
        {
            var tauFirstTests = new TauFirstTestConfiguration[]
            {
                // No exploration
                new TauFirstTestConfiguration
                {
                    AppId = TestContext.TestName + "NoExplorationFixedActionContext",
                    ContextType = ContextType.FixedAction, // test fixed-action context
                    Tau = 0,
                    NumberOfActions = 20,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 10 }
                },
                new TauFirstTestConfiguration
                {
                    AppId = TestContext.TestName + "NoExplorationVariableActionContext",
                    ContextType = ContextType.VariableAction, // test variable-action context
                    Tau = 5,
                    NumberOfActions = 20,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 10 }
                },

                // Heavy exploration
                new TauFirstTestConfiguration
                {
                    AppId = TestContext.TestName + "HeavyExplorationFixedActionContext",
                    ContextType = ContextType.FixedAction,
                    Tau = 100,
                    NumberOfActions = 10,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 9 }
                },
                new TauFirstTestConfiguration
                {
                    AppId = TestContext.TestName + "HeavyExplorationVariableActionContext",
                    ContextType = ContextType.VariableAction,
                    Tau = 100,
                    NumberOfActions = 10,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 9 }
                }
            };

            Run(outputFilePatternExpected, outputFilePatternActual, outputJsonConfigFile, tauFirstTests);

            for (uint i = 0; i < tauFirstTests.Length; i++)
            {
                CompareExplorationData(FormatPath(outputFilePatternExpected, i), FormatPath(outputFilePatternActual, i));
            }
        }

        [TestMethod]
        public void TestSoftmax()
        {
            var softmaxTests = new SoftmaxTestConfiguration[]
            {
                new SoftmaxTestConfiguration
                {
                    AppId = TestContext.TestName + "LowLambdaFixedActionContext",
                    ContextType = ContextType.FixedAction, // test fixed-action context
                    Lambda = 0.1f,
                    NumberOfActions = 20,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    ScorerConfiguration = new FixedScorerConfiguration { Score = 1 }
                },
                new SoftmaxTestConfiguration
                {
                    AppId = TestContext.TestName + "LowLambdaVariableActionContext",
                    ContextType = ContextType.VariableAction, // test variable-action context
                    Lambda = 0.1f,
                    NumberOfActions = 20,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    ScorerConfiguration = new FixedScorerConfiguration { Score = 5 }
                },

                new SoftmaxTestConfiguration
                {
                    AppId = TestContext.TestName + "HighLambdaFixedActionContext",
                    ContextType = ContextType.FixedAction,
                    Lambda = 0.9f,
                    NumberOfActions = 10,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    ScorerConfiguration = new IntegerProgressionScorerConfiguration { Start = 1 }
                },
                new SoftmaxTestConfiguration
                {
                    AppId = TestContext.TestName + "HighLambdaVariableActionContext",
                    ContextType = ContextType.VariableAction,
                    Lambda = 0.9f,
                    NumberOfActions = 10,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    ScorerConfiguration = new IntegerProgressionScorerConfiguration { Start = 5 }
                }
            };

            Run(outputFilePatternExpected, outputFilePatternActual, outputJsonConfigFile, softmaxTests);

            for (uint i = 0; i < softmaxTests.Length; i++)
            {
                CompareExplorationData(FormatPath(outputFilePatternExpected, i), FormatPath(outputFilePatternActual, i));
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            if (Directory.Exists(WorkingDir))
            {
                Directory.Delete(WorkingDir, recursive: true);
            }
            Directory.CreateDirectory(WorkingDir);

            // {0} is test config id since multiple configs can run within one test method.
            outputFilePatternExpected = TestContext.TestName + "_result_{0}_expected.txt";
            outputFilePatternActual = TestContext.TestName + "_result_{0}_actual.txt";
            outputJsonConfigFile = TestContext.TestName + ".json";
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(WorkingDir))
            {
                Directory.Delete(WorkingDir, recursive: true);
            }
        }

        private void CompareExactMatch(string file1, string file2)
        {
            string[] lines1 = File.ReadAllLines(file1).Select(l => l.Trim()).Where(l => !String.IsNullOrEmpty(l)).ToArray();
            string[] lines2 = File.ReadAllLines(file2).Select(l => l.Trim()).Where(l => !String.IsNullOrEmpty(l)).ToArray();

            Assert.IsTrue(Enumerable.SequenceEqual(lines1, lines2));
        }

        private void CompareExplorationData(string file1, string file2)
        {
            var extractExploration = (Func<string, Tuple<uint, string, float, string>>)((line) => 
            {
                string[] barData = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string[] spaceData = barData[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new Tuple<uint, string, float, string>(
                    Convert.ToUInt32(spaceData[0]), // action
                    spaceData[1].Trim(), // key
                    Convert.ToSingle(spaceData[2]), // prob
                    barData[1].Trim()); // context
            });

            string[] lines1 = File.ReadAllLines(file1).Select(l => l.Trim()).Where(l => !String.IsNullOrEmpty(l)).ToArray();
            string[] lines2 = File.ReadAllLines(file2).Select(l => l.Trim()).Where(l => !String.IsNullOrEmpty(l)).ToArray();

            Assert.AreEqual(lines1.Length, lines2.Length);

            for (int i = 0; i < lines1.Length; i++)
            {
                Tuple<uint, string, float, string> data1 = extractExploration(lines1[i]);
                Tuple<uint, string, float, string> data2 = extractExploration(lines2[i]);

                Assert.AreEqual(data1.Item1, data2.Item1);
                Assert.AreEqual(data1.Item2, data2.Item2);
                Assert.IsTrue(Math.Abs(data1.Item3 - data2.Item3) <= PrecisionOffset);
                Assert.AreEqual(data1.Item4, data2.Item4);
            }
        }

        private void Run(string outputFilePatternExpected, string outputFilePatternActual, string outputJsonConfigFile, ITestConfiguration[] tests)
        {
            for (uint i = 0; i < tests.Length; i++)
            {
                tests[i].OutputFile = FormatPath(outputFilePatternExpected, i);
            }

            string jsonConfigFile = Path.Combine(WorkingDir, outputJsonConfigFile);
            File.WriteAllText(jsonConfigFile, JsonConvert.SerializeObject(tests));

            var psi = new ProcessStartInfo();
            psi.FileName = CppExePath;
            psi.Arguments = jsonConfigFile;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;

            Process.Start(psi).WaitForExit();

            for (uint i = 0; i < tests.Length; i++)
            {
                tests[i].OutputFile = FormatPath(outputFilePatternActual, i);
            }
            File.WriteAllText(jsonConfigFile, JsonConvert.SerializeObject(tests));

            psi.FileName = CsharpExePath;
            Process.Start(psi).WaitForExit();
        }

        private string FormatPath(string pattern, uint iteration)
        {
            return Path.Combine(WorkingDir, string.Format(pattern, iteration));
        }

        ///// <summary>
        ///// Gets or sets the test context which provides
        ///// information about and functionality for the current test run.
        ///// </summary>
        public TestContext TestContext { get; set; }

        private string outputFilePatternExpected;
        private string outputFilePatternActual;
        private string outputJsonConfigFile;

        private readonly string CppExePath = @"..\..\..\explore-cpp\bin\x64\Release\black_box_tests.exe";
        private readonly string CsharpExePath = @"..\..\..\explore-csharp\bin\AnyCPU\Release\BlackBoxTests.exe";
        private readonly string WorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "Test");
        private readonly float PrecisionOffset = 1e-4f;
    }
}
