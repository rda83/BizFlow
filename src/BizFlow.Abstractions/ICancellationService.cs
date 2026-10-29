
namespace BizFlow.Abstractions
{
    /// <summary>
    /// Provides functionality to request the cancellation of a currently running pipeline.
    /// </summary>
    public interface ICancellationService
    {
        /// <summary>
        /// Requests cancellation of the specified pipeline.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline to cancel.</param>
        /// <returns>
        /// <see langword="true"/> if the cancellation request was successfully issued;
        /// <see langword="false"/> if the pipeline does not exist, is not currently running,
        /// or has already completed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pipelineName"/> is <see langword="null"/>.
        /// </exception>
        bool Cancel(string pipelineName);
    }
}
