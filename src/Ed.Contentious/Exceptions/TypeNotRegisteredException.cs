using System;

namespace Ed.Contentious.Exceptions
{
    /// <summary>
    /// Exception thrown when a type has not been registered to a context
    /// before it was requested.
    /// </summary>
    /// <remarks>
    /// This exception does not have any custom properties, 
    /// thus it does not implement ISerializable.
    /// </remarks>
    [Serializable]
    public class TypeNotRegisteredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotRegisteredException"/> class.
        /// </summary>
        public TypeNotRegisteredException() : base()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotRegisteredException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public TypeNotRegisteredException(string message) : base(message)
        {}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotRegisteredException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TypeNotRegisteredException(string message, Exception innerException) : base(message, innerException)
        {}
    }
}