# Contentious #

Contentious is a content management library for games written in .NET and Mono.
I originally wrote what's now becoming Contentious (this is the third rewrite)
because the XNA Content Pipeline just isn't very good for PC games; it might be
a solid tool for 360 and WP7 development, but personally I don't really care
about either. Contentious is designed for Windows, Mac, and Linux, with the
appropriate variants for MonoTouch and maybe MonoDroid when I get to that with
the project for which I'm writing it.

Contentious is released under the ISC license, packaged in this repo as
LICENSE.md.


One quick note: I've gotten a couple of questions as of late about why I'm so
many pushing small libraries like this, Stateful, etc. for public release. It's
mostly for my own purposes; loosely coupled code is in most cases better code
and releasing modular libraries, even if they look (or are!) simple, out as
open source is a decent way to make sure that independent and reusable systems
stay independent and reusable. If it's of use to you, awesome - use it in good
health.

## How It Works ##
Contentious is based around the **ContentContext**. At its core, ContentContext
objects define a location (by default on disk, but at some point adapters for
Android's weird file storage and a layer that can sit on top of XNA's
ContentManager stuff) and a set of content types. Each content type contains a
Type (what a given file resource will be turned into), a list of valid file
extensions for this type, and an an optional sub-path within your content root
where all files pertaining to this type will live (for example, you might have
all Texture2D objects as .png and stored within $CONTENTROOT/Textures).

Once you've created a ContentContext, you load in a resource via the
ContentContext.Load<T>() method. Your path should be relative to the content
subroot for that type; providing a file extension is optional but recommended;
automatic extension appending is provided mostly for future XNA friendliness
but I don't use it in my own projects. All content types that are loaded into
a ContentContext must implement IDisposable, because when a ContentContext is
disposed it will dispose of all objects created through it.

ContentContexts also have the notion of parent contexts. In your application,
you'll generally only explicitly create one global context via a constructor
and the rest via ContentContext.CreateChildContext(). Child contexts inherit
all of the parent's configuration values (even if they are changed after the
child is created). They also check with their parent contexts before attempting
to load a piece of content from disk, which can reduce multiple loads of the
same content. For example, you might load a font file into your global context,
then request the same font file from a child context; it would be stupid to
load the font file into the child context a second time. Of course, the child
context won't try to Dispose() of resources it doesn't own, and disposing of a
parent context disposes of all child contexts.

## Variants ##
**FileContentContext** is probably the one most people want: it's a layer of
abstraction over the file system and nothing else. It should be noted that
it's not super-secure; while it disallows ".." in keys, it shouldn't be
considered a very effective way of disallowing access outside of the content
root. (Future idea: use my Filotic library to sandbox this. Not for now,
though.)