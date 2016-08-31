using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GL.CMS
{
    // Universal Counting Id vs. Wiki-Code vs Fully Qualified URL
    public interface IContentIdent
    {
        // At least one must be present
        int PermId { get; }
        int ContentId { get; }
        string Code { get; }
        Uri Uri { get; }        // cms://local/root/suba/subb
    }

    // Categorization:
    //   ParentREF
    //   PathToRoot
    //   Category
    //   Tags
    public interface IStructure
    {
        int ParentRef { get; }
        int CategoryRef { get; }
    }


    // Extensions:
    //   Props Name=Value

    public enum FragmentType
    {
        Text, Image, Video, Binary
    }

    public interface IFragment
    {
        string Format { get; }
        FragmentType Type { get; }

        string GetContent();  // Simple but slow
        Task<Stream> GetContentStreamAsync(); // Fast but complex
    }

    // Should contain no references to other IEntry (like Children/Parent to keep a clean Lazy-load control)
    public interface IEntry
    {
        IContentIdent Ident { get; }
        IStructure Structure { get; }

        IFragment Title { get; }
        IFragment Body { get; }
        
        IReadOnlyCollection<string> Tags { get; }
        IReadOnlyCollection<IContentProp> Props { get; }

        IAudited Audit { get; }
        IPublished Published { get; }
    }

    public interface IContentProp
    {
        string Name { get; }
        string Value { get; }
    }

    public enum PublishedState
    {
        Draft,
        Published, 
        Archived,
        Deleted
    }

    public interface IPublished
    {
        PublishedState Status { get; }
        DateTime? PublishedAt { get; }
        int PublishedBy { get; }
    }

    public interface IAudited
    {
        int Version { get; }

        DateTime Created { get; }
        int CreatedBy { get; }

        DateTime Modified { get; }
        int ModifiedBy { get; }
    }

}
