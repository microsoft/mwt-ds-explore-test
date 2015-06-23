using System;
using System.Collections.Generic;

namespace BlackBox
{
    public interface ITestConfiguration
    {
        TestType Type { get; }
        string OutputFile { get; set; }
    }

    public class PrgTestConfiguration : ITestConfiguration
    {
        public ulong Seed { get; set; }
        public int Iterations { get; set; }
        public Tuple<uint, uint> UniformInterval { get; set; }
        public TestType Type { get { return TestType.PRG; } }
        public string OutputFile { get; set; }
    }

    public class HashTestConfiguration : ITestConfiguration
    {
        public List<string> Values { get; set; }
        public TestType Type { get { return TestType.HASH; } }
        public string OutputFile { get; set; }
    }

    public enum TestType
    { 
        PRG = 0,
        HASH,
        EXPLORE
    }
}
