namespace Deveel.Filters {
    /// <summary>
    /// An exception thrown when the evaluation of
    /// a filter fails
    /// </summary>
    public class FilterEvaluationException : FilterException {
        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        public FilterEvaluationException() {
        }

        /// <summary>
        /// Initializes a new instance of the exception with a
        /// message that describes the error.
        /// </summary>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        public FilterEvaluationException(string message) : base(message) {
        }

        /// <summary>
        /// Initializes a new instance of the exception with a
        /// message that describes the error and the exception
        /// that caused it.
        /// </summary>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        /// <param name="innerException">
        /// The exception that caused the error.
        /// </param>
        public FilterEvaluationException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
