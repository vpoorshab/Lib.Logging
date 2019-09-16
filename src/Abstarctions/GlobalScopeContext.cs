using System.Threading;

namespace Lib.Logging.Abstractions
{
    /// <summary>
    /// Global Shared Scope Context between threads 
    /// </summary>
    public static class GlobalScopeContext
    {
        private static readonly AsyncLocal<ScopeContext> Value = new AsyncLocal<ScopeContext>();

        /// <summary>
        /// return current global scope context
        /// </summary>
        public static ScopeContext Current => Value.Value ?? (Value.Value = ScopeContext.Instance);

        public static ScopeContext Initiate()
        {
            Value.Value = ScopeContext.Instance;
            return Current;
        }
    }
}