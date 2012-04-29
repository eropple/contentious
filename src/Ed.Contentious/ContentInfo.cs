using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

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
        /// All valid extensions for this content type. These extensions need
        /// not be only file extensions; they are matched against the file name
        /// (though not any attached directories) with String.EndsWith. As
        /// such, an "extension" such as ".map.xml" would be accepted.
        /// </summary>
        public readonly ReadOnlyCollection<String> ValidExtensions;
        /// <summary>
        /// The method that should be invoked to convert a loaded file stream
        /// into the requested type.
        /// </summary>
        /// <remarks>
        /// Due to one of the more braindead typing failures in .NET (the lack
        /// of existential types, a la SomeClass<?> in Java), this delegate
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
        /// <param name="validExtensions">
        /// A collection of valid extensions for this type.
        /// </param>
        /// <param name="parseMethod">
        /// The parser method for this ContentInfo. Validity will be checked
        /// at runtime such that the return type of this method is assignable
        /// to the type of this ContentInfo.
        /// </param>
        public ContentInfo(Type type, String subRoot, 
            IList<String> validExtensions, ContentLoadDelegate parseMethod)
        {
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

            if (validExtensions is ReadOnlyCollection<String>)
            {
                ValidExtensions = (ReadOnlyCollection<String>) validExtensions;
            }
            else
            {
                ValidExtensions = new ReadOnlyCollection<String>(validExtensions);
            }

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
        /// A simplified constructor for ContentInfo. Does not allow specifying
        /// extensions for file checking or subRoots.
        /// </summary>
        /// <param name="type">
        /// The .NET type of this ContentInfo. Must implement IDisposable.
        /// </param>
        /// <param name="parseMethod">
        /// The parser method for this ContentInfo. Validity will be checked
        /// at runtime such that the return type of this method is assignable
        /// to the type of this ContentInfo.
        /// </param>
        public ContentInfo(Type type, ContentLoadDelegate parseMethod)
            : this(type, null, new String[] { }, parseMethod)
        {
        }
    }

    public delegate Object ContentLoadDelegate(ContentContext context, Stream input);
}
