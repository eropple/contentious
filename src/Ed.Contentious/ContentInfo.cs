using System;
using System.IO;

namespace Ed.Contentious
{
    /// <summary>
    /// Container class for describing content types for ContentContexts.
    /// </summary>
    public class ContentInfo
    {
        /// <summary>
        /// The .NET type under which to classify this ContentInfo. It must
        /// be a type that implements IDisposable or a runtime exception will
        /// be thrown.
        /// </summary>
        public readonly Type Type;
        /// <summary>
        /// The path, attached to your given ContentContext.ContentRoot via
        /// a simple System.IO.Path.Combine, under which all data files of this
        /// type shall be stored.
        /// </summary>
        /// <remarks>
        /// For example, if your ContentRoot is at "C:\Game\Content", and your
        /// SubRoot is "Textures\Lossless", the SubRoot under which all 
        /// instances of this type's files should be placed would be found at
        /// "C:\Game\Content\Textures\Lossless".
        /// </remarks>
        public readonly String SubRoot;
        /// <summary>
        /// The method that should be invoked to convert a loaded file stream
        /// into the requested type.
        /// </summary>
        /// <remarks>
        /// Due to one of the more braindead typing failures in .NET (the lack
        /// of existential types, a la SomeClass[?] in Java), this delegate
        /// must always return Object. I know this sucks, and I apologize. A
        /// somewhat-okay patch is to define your parsing methods as returning
        /// the correct type anyway; methods matching delegate signatures takes
        /// into account upcasting so a method that returns Foo can still be
        /// passed in as a ContentLoadDelegate.
        /// 
        /// The constructors do check for assignability, however, so there's at
        /// least that.
        /// </remarks>
        public readonly ContentLoadDelegate ParseMethod;

        /// <summary>
        /// Whether or not objects created from this type are idempotent.
        /// </summary>
        /// <remarks>
        /// Normally (when Idempotent is false), for every call to a
        /// ContentContext's Load[T] method, a new object is created. This is
        /// good for mutable objects that are being created from a base
        /// blueprint of some sort. However, there are a lot of (almost always
        /// immutable) objects that only need one representation in memory,
        /// like texture data or a bitmap font. When specifying these, setting
        /// Idempotent to true results in any request to the same string key
        /// always returning the same file.
        /// </remarks>
        public readonly Boolean Idempotent;

        /// <summary>
        /// The most specific constructor for a ContentInfo. Allows explicit
        /// setting of all fields.
        /// </summary>
        /// <param name="type">
        /// The .NET type of this ContentInfo. Must implement IDisposable.
        /// </param>
        /// <param name="subRoot">
        /// The sub-root, off of the context's ContentRoot, in which all files
        /// of this type may be found. A null or empty string will make all
        /// paths relative to the ContentRoot and ignore this field.
        /// </param>
        /// <param name="parseMethod">
        /// The parser method for this ContentInfo. Validity will be checked
        /// at runtime such that the return type of this method is assignable
        /// to the type of this ContentInfo.
        /// </param>
        /// <param name="idempotent">
        /// Whether or not a new object is created on every call of Load[T] for
        /// a given key.
        /// </param>
        public ContentInfo(Type type, String subRoot, 
            ContentLoadDelegate parseMethod, Boolean idempotent)
        {
            Idempotent = idempotent;

            if (typeof(IDisposable).IsAssignableFrom(type) == false)
            {
                throw new ArgumentException(String.Format("Type '{0}' does not " +
                    "implement IDisposable.", type));
            }
            Type = type;

            if (subRoot == String.Empty)
            {
                subRoot = null;
            }
            SubRoot = subRoot;

            if (type.IsAssignableFrom(parseMethod.Method.ReturnType) == false)
            {
                throw new ArgumentException(String.Format("ParseMethod for type '{0}' " +
                    "doesn't return something assignable to that type. Returns '{1}' instead.",
                    type,
                    parseMethod.Method.ReturnType));
            }
            ParseMethod = parseMethod;
        }

        /// <summary>
        /// A simplified constructor for ContentInfo.
        /// </summary>
        /// <param name="type">
        /// The .NET type of this ContentInfo. Must implement IDisposable.
        /// </param>
        /// <param name="parseMethod">
        /// The parser method for this ContentInfo. Validity will be checked
        /// at runtime such that the return type of this method is assignable
        /// to the type of this ContentInfo.
        /// </param>
        /// <param name="idempotent">
        /// Whether or not a new object is created on every call of Load[T] for
        /// a given key.
        /// </param>
        public ContentInfo(Type type, ContentLoadDelegate parseMethod, Boolean idempotent)
            : this(type, null, parseMethod, idempotent)
        {
        }
    }

    public delegate Object ContentLoadDelegate(ContentContext context, Stream input);
}
