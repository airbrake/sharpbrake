namespace SharpBrake
{
    /// <summary>
    /// Builder interface
    /// </summary>
    /// <typeparam name="TIn">Input to build from</typeparam>
    /// <typeparam name="TOut">Type being built</typeparam>
    public interface IBuilder<in TIn, out TOut>
    {
        /// <summary>
        /// Build an instance of TOut from TIn
        /// </summary>
        /// <param name="input">Input to build from</param>
        /// <returns></returns>
        TOut Build(TIn input);
    }
}