using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GL.CMS
{
    public class Entry : IEntry
    {
        public IContentIdent Ident { get; set; }
        public IStructure Structure { get; set; }
        public IFragment Title { get; set; }
        public IFragment Body { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
        public IReadOnlyCollection<IContentProp> Props { get; set; }
        public IAudited Audit { get; set; }
        public IPublished Published { get; set; }
    }

    public class EntryFlat : IEntry, IContentIdent, IStructure, IFragment, IAudited, IPublished
    {
        private string title;

        public IContentIdent Ident => this;
        public IStructure Structure => this;
        public IFragment Title => this;
        public IFragment Body { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
        public IReadOnlyCollection<IContentProp> Props { get; set; }
        public IAudited Audit => this;
        public IPublished Published => this;

        // IContentIdent
        public int PermId { get; }
        public int ContentId { get; set; }
        public string Code { get; set; }
        public Uri Uri { get; set; }

        //IStructure
        public int ParentRef { get; set; }
        public int CategoryRef { get; set; }

        //IFragment Title
        public string Format { get; set; }
        public FragmentType Type { get; set; }
        public string GetContent() => this.title;
        public Task<Stream> GetContentStreamAsync() => Task.FromResult((Stream)CMSHelper.ToMemoryStream(this.title));

        // Audit
        public int Version { get; set; }
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public DateTime Modified { get; set; }
        public int ModifiedBy { get; set; }

        // Published
        public PublishedState Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int PublishedBy { get; set; }
    }
}