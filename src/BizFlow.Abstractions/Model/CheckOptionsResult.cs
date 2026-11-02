namespace BizFlow.Abstractions.Model
{
    /// <summary>
    /// Represents the result of validating the configuration options for a worker.
    /// </summary>
    public class CheckOptionsResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the provided options are valid.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the options are valid; otherwise, <see langword="false"/>.
        /// </value>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message providing additional details about the validation result.
        /// Typically contains error information if <see cref="Success"/> is <see langword="false"/>.
        /// </summary>
        /// <value>
        /// A detailed message. Defaults to <see cref="string.Empty"/>.
        /// </value>
        public string Message { get; set; } = string.Empty;
    }
}
