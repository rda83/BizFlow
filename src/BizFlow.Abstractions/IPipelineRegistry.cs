using BizFlow.Abstractions.Model;

namespace BizFlow.Abstractions
{
    /// <summary>
    /// Provides access to the set of registered pipelines and allows
    /// adding, removing, and querying them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementations are expected to be thread-safe: <see cref="Create"/>,
    /// <see cref="Remove"/>, <see cref="GetAll"/>, and <see cref="Exist"/>
    /// may be called concurrently from different threads.
    /// </para>
    /// <para>
    /// Changes made through <see cref="Create"/> and <see cref="Remove"/> are
    /// communicated to observers via <see cref="OnAdded"/> and <see cref="OnRemoved"/>.
    /// See those events for important caveats regarding invocation order and locking.
    /// </para>
    /// </remarks>
    public interface IPipelineRegistry
    {
        /// <summary>
        /// Returns a snapshot of all currently registered pipelines.
        /// </summary>
        /// <returns>
        /// A read-only list containing every pipeline registered at the moment
        /// the method was called. The returned collection is a snapshot and is
        /// not affected by subsequent <see cref="Create"/> or <see cref="Remove"/>
        /// calls. Never <see langword="null"/>; may be empty.
        /// </returns>
        IReadOnlyList<Pipeline> GetAll();

        /// <summary>
        /// Registers a new pipeline with the registry.
        /// </summary>
        /// <param name="pipeline">
        /// The pipeline to register. Must not be <see langword="null"/> and must
        /// have a non-empty, non-whitespace <see cref="Pipeline.Name"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pipeline"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="pipeline"/> has an empty or whitespace-only name.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// A pipeline with the same name is already registered, or a runtime state
        /// with the same name already exists in the pipeline state store.
        /// </exception>
        /// <remarks>
        /// On success, <see cref="OnAdded"/> is raised for the newly registered
        /// pipeline. Subscribers to <see cref="OnAdded"/> are invoked outside the
        /// registry lock; see <see cref="OnAdded"/> for details.
        /// </remarks>
        void Create(Pipeline pipeline);

        /// <summary>
        /// Removes a pipeline from the registry.
        /// </summary>
        /// <param name="pipelineName">
        /// The name of the pipeline to remove. Must not be <see langword="null"/>,
        /// empty, or whitespace-only.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the pipeline was removed; <see langword="false"/>
        /// if no pipeline with the given name is registered, or if the pipeline is
        /// currently being executed (only pipelines in the <c>Waiting</c> state can
        /// be removed).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="pipelineName"/> is empty or whitespace-only.
        /// </exception>
        /// <remarks>
        /// On success, <see cref="OnRemoved"/> is raised for the removed pipeline
        /// name. Subscribers to <see cref="OnRemoved"/> are invoked outside the
        /// registry lock; see <see cref="OnAdded"/> for the ordering caveat.
        /// </remarks>
        bool Remove(string pipelineName);

        /// <summary>
        /// Determines whether a pipeline with the specified name is registered.
        /// </summary>
        /// <param name="pipelineName">
        /// The name of the pipeline to check. Must not be <see langword="null"/>,
        /// empty, or whitespace-only.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a pipeline with the given name is registered;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="pipelineName"/> is empty or whitespace-only.
        /// </exception>
        bool Exist(string pipelineName);

        /// <summary>
        /// Raised after a pipeline is successfully registered (both the definition
        /// and the runtime state are in place).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Handlers are invoked OUTSIDE the registry lock, so that:
        /// <list type="bullet">
        ///   <item>a slow or blocking handler cannot stall Create/Remove;</item>
        ///   <item>a handler that re-enters the registry (Create/Remove) cannot deadlock.</item>
        /// </list>
        /// </para>
        /// <para>
        /// Consequence: under concurrent Create/Remove of the same pipeline name,
        /// the order of <see cref="OnAdded"/> and <see cref="OnRemoved"/> is not guaranteed.
        /// A subscriber may observe OnRemoved before a previously-queued OnAdded.
        /// Handlers must be tolerant of this and should not rely on ordering;
        /// always re-read the current registry state via <see cref="GetAll"/> if needed.
        /// </para>
        /// </remarks>
        event EventHandler<Pipeline>? OnAdded;

        /// <summary>
        /// Raised after a pipeline is successfully removed.
        /// </summary>
        /// <remarks>
        /// See <see cref="OnAdded"/> for the ordering caveat: events are raised
        /// outside the registry lock, so OnRemoved may be observed before a
        /// previously-queued OnAdded for the same name.
        /// </remarks>
        event EventHandler<string>? OnRemoved;
    }
}
