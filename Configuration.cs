using System;
using System.Collections.Generic;

namespace BlackBox
{
    public interface ITestConfiguration
    {
        TestType Type { get; }
        string OutputFile { get; set; }
    }

    public abstract class BaseExploreTestConfiguration : ITestConfiguration
    {
        /// <summary>
        /// The type of the test to run.
        /// </summary>
        public abstract TestType Type { get; }

        /// <summary>
        /// The application Id.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// The type of context, e.g. fixed-action or variable-action context.
        /// </summary>
        /// <remarks>
        /// Black-box interface for each language should implement a corresponding Context based on this type.
        /// </remarks>
        public ContextType ContextType { get; set; }

        /// <summary>
        /// Number of actions to explore.
        /// </summary>
        public uint NumberOfActions { get; set; }

        /// <summary>
        /// List of experimental unit Ids to run exploration over.
        /// </summary>
        public List<string> ExperimentalUnitIdList { get; set; }

        /// <summary>
        /// The output file to write to.
        /// </summary>
        public string OutputFile { get; set; }
    }

    public interface IPolicyConfiguration
    {
        /// <summary>
        /// The type of default policy used within the exploration algorithm. For now only fixed-action policy is tested.
        /// </summary>
        /// <remarks>
        /// Black-box interface for each language should implement a corresponding IPolicy based on this type and returning the action specified in the test configuration.
        /// </remarks>
        PolicyType PolicyType { get; }
    }

    /// <summary>
    /// Black-box interface needs to implement an IPolicy class which always returns the same value as the Action property below.
    /// </summary>
    public class FixedPolicyConfiguration : IPolicyConfiguration
    {
        public PolicyType PolicyType
        {
            get { return PolicyType.Fixed; }
        }

        public uint Action { get; set; }
    }

    public class PrgTestConfiguration : ITestConfiguration
    {
        public ulong Seed { get; set; }
        public int Iterations { get; set; }
        public Tuple<uint, uint> UniformInterval { get; set; }
        public TestType Type { get { return TestType.Prg; } }
        public string OutputFile { get; set; }
    }

    public class HashTestConfiguration : ITestConfiguration
    {
        public List<string> Values { get; set; }
        public TestType Type { get { return TestType.Hash; } }
        public string OutputFile { get; set; }
    }

    public class EpsilonGreedyTestConfiguration : BaseExploreTestConfiguration
    {
        public override TestType Type { get { return TestType.EpsilonGreedy; } }
        public float Epsilon { get; set; }
        public IPolicyConfiguration PolicyConfiguration { get; set; }
    }

    public class TauFirstTestConfiguration : BaseExploreTestConfiguration
    {
        public override TestType Type { get { return TestType.TauFirst; } }
        public uint Tau { get; set; }
        public IPolicyConfiguration PolicyConfiguration { get; set; }
    }

    public enum TestType
    { 
        Prg = 0,
        Hash,
        EpsilonGreedy,
        TauFirst
    }

    public enum PolicyType
    { 
        Fixed = 0
    }

    public enum ContextType
    { 
        FixedAction = 0,
        VariableAction
    }
}
