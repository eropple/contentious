using System;
using System.Collections.Generic;

namespace Ed.Contentious
{
    /// <summary>
    /// The abstract base type for all content contexts.
    /// </summary>
    public abstract class ContentContext : IDisposable
    {
        protected readonly Dictionary<Type, ContentInfo> ContentTypeInfo;
        protected readonly List<ContentContext> ChildContexts = new List<ContentContext>(); 

        public readonly ContentContext Parent;

        protected ContentContext(ContentContext parent)
        {
            Parent = parent;

            // this is a readonly rather than a property for perf reasons; JIT
            // on XNA/.NETCF is spotty at best
            ContentTypeInfo = (parent == null) ? new Dictionary<Type, ContentInfo>() 
                : parent.ContentTypeInfo;
        }

        /// <summary>
        /// Determines whether this is a root context (has no parent).
        /// </summary>
        public Boolean IsRootContext { get { return Parent == null; } }

        /// <summary>
        /// Registers a new ContentInfo to this context. Note that this must be
        /// called upon a root context or will throw an exception.
        /// </summary>
        /// <param name="info"></param>
        public void Register(ContentInfo info)
        {
            if (IsRootContext == false)
            {
                throw new InvalidOperationException("Register() can " +
                    "only be called on a root context.");
            }

            ContentTypeInfo.Add(info.Type, info);
        }

        /// <summary>
        /// Creates a child context that will present already-loaded assets of
        /// its parent as its own.
        /// </summary>
        /// <returns>A new context that is the child of this one.</returns>
        public ContentContext CreateChildContext()
        {
            ContentContext childContext = CreateChildContextReal();
            ChildContexts.Add(childContext);
            return childContext;
        }

        protected abstract ContentContext CreateChildContextReal();

        /// <summary>
        /// Determines whether this context, or a parent context, has already
        /// loaded this type.
        /// </summary>
        /// <typeparam name="TLoadType"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public Boolean IsLoaded<TLoadType>(String key)
            where TLoadType : IDisposable
        {
            if (IsRootContext == false)
            {
                if (Parent.IsLoaded<TLoadType>(key)) return true;
            }

            return IsLoadedInContext<TLoadType>(key);
        }

        /// <summary>
        /// Internal method for determining whether or not this context (as
        /// opposed to the chain of contexts in which this context exists)
        /// has the desired object.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <typeparam name="TLoadType"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected abstract Boolean IsLoadedInContext<TLoadType>(String key)
            where TLoadType : IDisposable;


        /// <summary>
        /// Loads the specified filename (within the context's ContentRoot) and
        /// parses it according to the requested type's ContentInfo.
        /// </summary>
        /// <typeparam name="TLoadType"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TLoadType Load<TLoadType>(String key)
            where TLoadType : IDisposable
        {
            if (IsRootContext == false)
            {
                if (Parent.IsLoaded<TLoadType>(key))
                {
                    return Parent.Load<TLoadType>(key);
                }
            }

            return LoadInContext<TLoadType>(key);
        }

        /// <summary>
        /// Internal method for loading a content item into this context.
        /// </summary>
        /// <typeparam name="TLoadType"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected abstract TLoadType LoadInContext<TLoadType>(String key)
            where TLoadType : IDisposable;


        public Boolean IsDisposed { get; protected set; }
        /// <summary>
        /// Disposes of all content and child contexts.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            foreach (ContentContext child in ChildContexts)
            {
                child.Dispose();
            }

            DisposeContext();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Invoked when Dispose() is called. Dispose() itself just adds a
        /// wrapper to prevent multiple calls from doing anything.
        /// </summary>
        protected abstract void DisposeContext();
    }
}
