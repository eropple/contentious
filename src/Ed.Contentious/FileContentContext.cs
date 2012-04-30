using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ed.Contentious.Exceptions;

namespace Ed.Contentious
{
    /// <summary>
    /// A ContentContext that uses the file system as its backing store.
    /// </summary>
    public class FileContentContext : ContentContext
    {
        public readonly String ContentRoot;

        protected readonly List<IDisposable> DisposalList = new List<IDisposable>(); 
        protected readonly Dictionary<Type, Dictionary<String, IDisposable>> LookupTable =
            new Dictionary<Type, Dictionary<String, IDisposable>>();

        public FileContentContext(String contentRoot)
            : base(null)
        {
            ContentRoot = contentRoot;
        }

        protected FileContentContext(FileContentContext parent) 
            : base(parent)
        {
            ContentRoot = parent.ContentRoot;
        }

        protected override ContentContext CreateChildContextReal()
        {
            return new FileContentContext(this);
        }

        protected override Boolean IsLoadedInContext<TLoadType>(String key)
        {
            Type t = typeof(TLoadType);
            ContentInfo info;
            if (ContentTypeInfo.TryGetValue(t, out info) == false)
            {
                throw new TypeNotRegisteredException("Type not registered: " + t);
            }

            String fullPath = Path.Combine(ContentRoot, info.SubRoot, key);
            if (fullPath.Contains(".."))
            {
                throw new ArgumentException("Invalid path for Contentious: " + fullPath);
            }

            Dictionary<String, IDisposable> table;
            if (LookupTable.TryGetValue(typeof(TLoadType), out table) == false)
            {
                return false;
            }

            return table.ContainsKey(fullPath);
        }

        protected override TLoadType LoadInContext<TLoadType>(string key)
        {
            // "Copy-paste exactly once; if you need to again, refactor."

            Type t = typeof (TLoadType);
            ContentInfo info;
            if (ContentTypeInfo.TryGetValue(t, out info) == false)
            {
                throw new TypeNotRegisteredException("Type not registered: " + t);
            }

            String fullPath = Path.Combine(ContentRoot, info.SubRoot, key);
            if (fullPath.Contains(".."))
            {
                throw new ArgumentException("Invalid path for Contentious: " + fullPath);
            }

            Dictionary<String, IDisposable> table;
            if (LookupTable.TryGetValue(t, out table) == false)
            {
                table = new Dictionary<String, IDisposable>();
                LookupTable.Add(t, table);
            }

            IDisposable o;
            TLoadType obj;
            if (table.TryGetValue(fullPath, out o) == false)
            {
                if (File.Exists(fullPath) == false)
                {
                    throw new FileNotFoundException("Missing file for type: " + t,
                        fullPath);
                }

                FileStream fs = File.OpenRead(fullPath);
                obj = (TLoadType)info.ParseMethod(this, fs);
                DisposalList.Add(obj);
            }
            else
            {
                obj = (TLoadType) o;
            }

            return obj;
        }

        protected override void DisposeContext()
        {
            throw new NotImplementedException();
        }
    }
}
