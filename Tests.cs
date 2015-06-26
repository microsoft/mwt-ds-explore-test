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
            string outputFilePatternExpected = "prg_result_{0}_expected.txt";
            string outputFilePatternActual = "prg_result_{0}_actual.txt";
            string outputJsonConfigFile = "prg.json";

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
                Assert.AreEqual(File.ReadAllText(FormatPath(outputFilePatternExpected, i)), File.ReadAllText(FormatPath(outputFilePatternActual, i)));
            }

            for (uint i = 2; i < 4; i++)
            {
                float[] expected = File.ReadAllLines(FormatPath(outputFilePatternExpected, i)).Select(l => Convert.ToSingle(l)).ToArray();
                float[] actual = File.ReadAllLines(FormatPath(outputFilePatternActual, i)).Select(l => Convert.ToSingle(l)).ToArray();

                Assert.AreEqual(expected.Length, actual.Length);
                for (int j = 0; j < expected.Length; j++)
                {
                    // allow slightly different float precision
                    Assert.IsTrue(Math.Abs(expected[j] - actual[j]) < 1e-6);
                }
            }
        }

        [TestMethod]
        public void TestHash()
        {
            string outputFilePatternExpected = "hash_result_{0}_expected.txt";
            string outputFilePatternActual = "hash_result_{0}_actual.txt";
            string outputJsonConfigFile = "hash.json";

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
                Assert.AreEqual(File.ReadAllText(FormatPath(outputFilePatternExpected, i)), File.ReadAllText(FormatPath(outputFilePatternActual, i)));
            }
        }

        [TestMethod]
        public void TestEpsilonGreedy()
        {
            string outputFilePatternExpected = "epsilon_greedy_result_{0}_expected.txt";
            string outputFilePatternActual = "epsilon_greedy_result_{0}_actual.txt";
            string outputJsonConfigFile = "epsilon_greedy.json";

            var epsilonGreedyTests = new EpsilonGreedyTestConfiguration[]
            {
                // No exploration
                new EpsilonGreedyTestConfiguration
                {
                    AppId = "EpsilonGreedyNoExplorationFixedActionContext",
                    ContextType = ContextType.FixedAction, // test fixed-action context
                    Epsilon = 0f,
                    NumberOfActions = 20,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 10 }
                },
                new EpsilonGreedyTestConfiguration
                {
                    AppId = "EpsilonGreedyNoExplorationVariableActionContext",
                    ContextType = ContextType.VariableAction, // test variable-action context
                    Epsilon = 0f,
                    NumberOfActions = 20,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 10 }
                },

                // Regular exploration
                new EpsilonGreedyTestConfiguration
                {
                    AppId = "EpsilonGreedyRegularExplorationFixedActionContext",
                    ContextType = ContextType.FixedAction,
                    Epsilon = 0.2f,
                    NumberOfActions = 10,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 9 }
                },
                new EpsilonGreedyTestConfiguration
                {
                    AppId = "EpsilonGreedyRegularExplorationVariableActionContext",
                    ContextType = ContextType.VariableAction,
                    Epsilon = 0.2f,
                    NumberOfActions = 10,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 9 }
                },

                // Heavy exploration
                new EpsilonGreedyTestConfiguration
                {
                    AppId = "EpsilonGreedyHeavyExplorationFixedActionContext",
                    ContextType = ContextType.FixedAction,
                    Epsilon = 0.9f,
                    NumberOfActions = 90, // test many actions
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 81 }
                },
                new EpsilonGreedyTestConfiguration
                {
                    AppId = "EpsilonGreedyHeavyExplorationVariableActionContext",
                    ContextType = ContextType.VariableAction,
                    Epsilon = 0.9f,
                    NumberOfActions = 90,
                    ExperimentalUnitIdList = Enumerable.Range(1, 100).Select(i => i.ToString()).ToList(),
                    PolicyConfiguration = new FixedPolicyConfiguration { Action = 81 }
                }
            };

            Run(outputFilePatternExpected, outputFilePatternActual, outputJsonConfigFile, epsilonGreedyTests);

            //for (uint i = 0; i < hashTests.Length; i++)
            //{
            //    // integer content so should be exact match
            //    Assert.AreEqual(File.ReadAllText(FormatPath(outputFilePatternExpected, i)), File.ReadAllText(FormatPath(outputFilePatternActual, i)));
            //}
        }

        [TestInitialize]
        public void Initialize()
        {
            if (Directory.Exists(WorkingDir))
            {
                Directory.Delete(WorkingDir, recursive: true);
            }
            Directory.CreateDirectory(WorkingDir);
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(WorkingDir))
            {
                Directory.Delete(WorkingDir, recursive: true);
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

        private readonly string CppExePath = @"..\..\..\explore-cpp\bin\x64\Release\black_box_tests.exe";
        private readonly string CsharpExePath = @"..\..\..\explore-csharp\bin\AnyCPU\Release\BlackBoxTests.exe";
        private readonly string WorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "Test");
    }
}
