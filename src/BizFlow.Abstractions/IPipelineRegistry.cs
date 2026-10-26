using BizFlow.Abstractions.Model;

namespace BizFlow.Abstractions
{
    public interface IPipelineRegistry
    {
        IReadOnlyList<Pipeline> GetAll();
        void Create(Pipeline pipeline);
        bool Remove(string pipelineName);
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
