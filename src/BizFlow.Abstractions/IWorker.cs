
using BizFlow.Abstractions.Model;
using System.Text.Json;

namespace BizFlow.Abstractions
{
    /// <summary>
    /// Defines a contract for a worker that executes the logic for a specific pipeline item.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are typically resolved using keyed dependency injection
    /// based on their unique operation type identifier.
    /// </remarks>
    public interface IWorker 
    {
        /// <summary>
        /// Asynchronously executes the worker's operation using the provided context.
        /// </summary>
        /// <param name="ctx">The execution context containing pipeline metadata and operation options.</param>
        /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous execution operation.</returns>
        Task ExecuteAsync(WorkerContext ctx, CancellationToken ct);

        /// <summary>
        /// Validates the provided JSON options to ensure they are suitable for this worker.
        /// </summary>
        /// <param name="options">The JSON options to validate.</param>
        /// <returns>
        /// A <see cref="CheckOptionsResult"/> indicating whether the options are valid, 
        /// and an optional message.
        /// </returns>
        /// <remarks>
        /// The default implementation always returns a successful result. 
        /// Override this method if the worker requires specific option validation.
        /// </remarks>
        Task<CheckOptionsResult> CheckOptions(JsonElement options)
        {
            return Task.FromResult(new CheckOptionsResult() { Success = true });
        }

        /// <summary>
        /// Deserializes the provided JSON options into a strongly-typed object.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize the options into.</typeparam>
        /// <param name="options">The JSON options to deserialize. Can be <see langword="null"/>.</param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="T"/>, or <see langword="null"/> if 
        /// <paramref name="options"/> is <see langword="null"/> or empty.
        /// </returns>
        T? GetOptions<T>(JsonElement? options) where T : class;
    }
}
