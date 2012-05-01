using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Ed.Contentious.Xna
{
    /// <summary>
    /// An adapter to the XnaContentContext for supporting the XNA content
    /// pipeline. Modifies the standard ContentContext calls to provide sane
    /// inputs to ContentManager. Make sure to read the IntelliSense docs for
    /// details.
    /// </summary>
    /// <remarks>
    /// Patching together Contentious and XNA is kind of boilerplate-y, but
    /// very doable. Since you've already defined all your ContentParsers, you
    /// already have the guts of the importer/processor logic already done.
    /// </remarks>
    public class XnaContentContext : ContentContext
    {
        protected readonly ContentManager XNAContent;
        protected readonly IServiceProvider Services;

        protected readonly Dictionary<Type, Dictionary<String, IDisposable>> IdempotentLookupTable =
            new Dictionary<Type, Dictionary<String, IDisposable>>();

        /// <summary>
        /// Constructor. Internally constructs the XNA ContentManager.
        /// </summary>
        /// <param name="rootDirectory">
        /// The value for ContentManager.RootDirectory. Usually but not always
        /// "Content".
        /// </param>
        /// <param name="services">
        /// The services container for your game. Usually accessible via the
        /// Services member of your Game class.
        /// </param>
        public XnaContentContext(String rootDirectory, IServiceProvider services)
            : base(null)
        {
            Services = services;
            XNAContent = new ContentManager(services, rootDirectory);
        }

        protected XnaContentContext(XnaContentContext parent) : base(parent)
        {
            Services = parent.Services;
            XNAContent = new ContentManager(parent.Services, parent.XNAContent.RootDirectory);
        }

        /// <summary>
        /// Creates a child context. This method works as it does in a desktop
        /// environment.
        /// </summary>
        /// <returns>The new child context.</returns>
        protected override ContentContext CreateChildContextReal()
        {
            return new XnaContentContext(this);
        }

        protected override bool IsLoaded<TLoadType>(string key)
        {
            Type t = typeof(TLoadType);
            ContentInfo info = GetInfo(t);
            if (info.Idempotent == false)
            {
                throw new InvalidOperationException("IsLoaded<T> fails because " +
                    t + " is not idempotent.");
            }

            String fullPath = BuildPath(info, key);

            Dictionary<String, IDisposable> table;
            if (IdempotentLookupTable.TryGetValue(t, out table) == false)
            {
                return false;
            }

            return table.ContainsKey(fullPath);
        }

        protected override TLoadType LoadInContext<TLoadType>(String key)
        {
            Type t = typeof (TLoadType);
            ContentInfo info = GetInfo(t);
            String fullPath = BuildPath(info, key);


            TLoadType obj;
            if (info.Idempotent)
            {
                Boolean checkLookupTable = true;
                Dictionary<String, IDisposable> table;
                if (IdempotentLookupTable.TryGetValue(t, out table) == false)
                {
                    table = new Dictionary<String, IDisposable>();
                    IdempotentLookupTable.Add(t, table);
                    checkLookupTable = false;
                }

                IDisposable o;
                if (checkLookupTable == false || table.TryGetValue(fullPath, out o) == false)
                {
                    obj = XNAContent.Load<TLoadType>(fullPath);
                    table.Add(fullPath, obj);
                }
                else
                {
                    obj = (TLoadType)o;
                }
            }
            else // not idempotent objects
            {
                obj = XNAContent.Load<TLoadType>(fullPath);
            }

            return obj;
        }

        protected override void DisposeContext()
        {
            XNAContent.Dispose();
        }

        protected String BuildPath(ContentInfo info, String key)
        {
            // because XNA's developers made the A+ decision to chop off
            // file extensions when adding content. That doesn't screw with
            // everyone else ever, right guys?
            return Path.Combine(Path.GetDirectoryName(key), Path.GetFileName(key));
        }
    }
}
