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

            Func<uint, string, string> formatPath = (i, pattern) => { return Path.Combine(WorkingDir, string.Format(pattern, i)); };

            // 2 tests for integer interval and 2 tests for unit interval.
            var prgTests = new PrgTestConfiguration[4];
            for (uint i = 0; i < prgTests.Length; i++)
            {
                prgTests[i] = new PrgTestConfiguration 
                {
                    Seed = i,
                    Iterations = 1000,
                    UniformInterval = i < 2 ? new Tuple<uint, uint>(i * 10, (i + 10) * 10) : null,
                    OutputFile = formatPath(i, outputFilePatternExpected)
                };
            }
            string jsonConfigFile = Path.Combine(WorkingDir, outputJsonConfigFile);
            File.WriteAllText(jsonConfigFile, JsonConvert.SerializeObject(prgTests));

            var psi = new ProcessStartInfo();
            psi.FileName = CppExePath;
            psi.Arguments = jsonConfigFile;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;

            Process.Start(psi).WaitForExit();

            for (uint i = 0; i < prgTests.Length; i++)
            {
                prgTests[i].OutputFile = formatPath(i, outputFilePatternActual);
            }
            File.WriteAllText(jsonConfigFile, JsonConvert.SerializeObject(prgTests));

            psi.FileName = CsharpExePath;
            Process.Start(psi).WaitForExit();

            for (uint i = 0; i < 2; i++)
            {
                // integer content so should be exact match
                Assert.AreEqual(File.ReadAllText(formatPath(i, outputFilePatternExpected)), File.ReadAllText(formatPath(i, outputFilePatternActual)));
            }

            for (uint i = 2; i < 4; i++)
            {
                float[] expected = File.ReadAllLines(formatPath(i, outputFilePatternExpected)).Select(l => Convert.ToSingle(l)).ToArray();
                float[] actual = File.ReadAllLines(formatPath(i, outputFilePatternActual)).Select(l => Convert.ToSingle(l)).ToArray();

                Assert.AreEqual(expected.Length, actual.Length);
                for (int j = 0; j < expected.Length; j++)
                {
                    // allow slightly different float precision
                    Assert.IsTrue(Math.Abs(expected[j] - actual[j]) < 1e-6);
                }
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
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(WorkingDir))
            {
                Directory.Delete(WorkingDir, recursive: true);
            }
        }

        private readonly string CppExePath = @"..\..\..\explore-cpp\bin\Win32\Debug\black_box_tests.exe";
        private readonly string CsharpExePath = @"..\..\..\explore-csharp\bin\AnyCPU\Debug\BlackBoxTests.exe";
        private readonly string WorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "Test");
    }
}
