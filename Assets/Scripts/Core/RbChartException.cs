using System;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Thrown when a chart file is malformed, unsupported, or violates the spec.</summary>
    public sealed class RbChartException : Exception
    {
        public RbChartException(string message) : base(message) { }
        public RbChartException(string message, Exception inner) : base(message, inner) { }
    }
}
