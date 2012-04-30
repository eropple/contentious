using System;
using System.Collections.Generic;
using System.IO;

namespace Ed.Contentious
{
    /// <summary>
    /// A ContentContext that uses the file system as its backing store.
    /// </summary>
    public class FileContentContext : ContentContext
    {
        public readonly String ContentRoot;

        protected readonly List<IDisposable> DisposalList = new List<IDisposable>(); 
        protected readonly Dictionary<Type, Dictionary<String, IDisposable>> IdempotentLookupTable =
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

        protected override Boolean IsLoaded<TLoadType>(string key)
        {
            Type t = typeof(TLoadType);
            ContentInfo info = GetInfo(t);
            if (info.Idempotent == false)
            {
                throw new InvalidOperationException("IsLoaded<T> fails because " + 
                    t + " is not idempotent.");
            }

            String fullPath = BuildFullPath(info, key);

            Dictionary<String, IDisposable> table;
            if (IdempotentLookupTable.TryGetValue(t, out table) == false)
            {
                return false;
            }

            return table.ContainsKey(fullPath);
        }

        protected override TLoadType LoadInContext<TLoadType>(string key)
        {
            Type t = typeof (TLoadType);
            ContentInfo info = GetInfo(t);

            String fullPath = BuildFullPath(info, key);
            if (fullPath.Contains(".."))
            {
                throw new ArgumentException("Invalid path for Contentious: " + fullPath);
            }

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
                    FileStream fs = File.OpenRead(fullPath);
                    obj = (TLoadType)info.ParseMethod(this, fs);
                    table.Add(fullPath, obj);
                    DisposalList.Add(obj);
                }
                else
                {
                    obj = (TLoadType)o;
                }
            }
            else // not idempotent objects
            {
                obj = (TLoadType)info.ParseMethod(this, File.OpenRead(fullPath));
                DisposalList.Add(obj);
            }

            return obj;
        }

        protected override void DisposeContext()
        {
            foreach (IDisposable obj in DisposalList)
            {
                obj.Dispose();
            }
        }

        protected String BuildFullPath(ContentInfo info, String key)
        {
            return (info.SubRoot != null)
                ? Path.Combine(ContentRoot, info.SubRoot, key)
                : Path.Combine(ContentRoot, key);
        }
    }
}
