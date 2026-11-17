namespace BizFlow.Abstractions.Model
{
    /// <summary>
    /// Specifies the unique operation type identifier for an <see cref="IWorker"/> implementation.
    /// </summary>
    /// <remarks>
    /// This attribute is used to bind an <see cref="IWorker"/> class to a specific string key. 
    /// During pipeline execution, the <c>PipelineExecutor</c> uses this key (matched with 
    /// <see cref="PipelineItem.TypeOperationId"/>) to resolve the correct worker 
    /// from the dependency injection container.
    /// </remarks>
    /// <example>
    /// <code>
    /// [TypeOperationId("delay-3min")]
    /// public class Delay3MinWorker : IWorker { ... }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TypeOperationIdAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the unique identifier used to resolve this worker via dependency injection.
        /// </summary>
        /// <value>
        /// The operation type identifier string. Defaults to <see cref="string.Empty"/>.
        /// </value>
        public string TypeOperationId { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeOperationIdAttribute"/> class.
        /// </summary>
        /// <param name="typeOperationId">The unique string identifier for the operation type.</param>
        public TypeOperationIdAttribute(string typeOperationId)
        {
            TypeOperationId = typeOperationId;
        }
    }
}
