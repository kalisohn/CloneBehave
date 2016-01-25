namespace CloneBehave
{
    /// <summary>
    /// Provides extension methods for cloning objects
    /// </summary>
    public static class CloningExtensions
    {
        /// <summary>
        /// Recursively deep clones an object
        /// </summary>
        public static T Clone<T>(this T original)
        {
            CloneEngine engine = new CloneEngine();
            return engine.Clone(original);
        }
    }
}