using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class Paper
{
    public Guid PaperId { get; set; }

    public string Doi { get; set; }

    public string Title { get; set; }

    public string Abstract { get; set; }

    public int? PublicationYear { get; set; }

    public DateOnly? PublicationDate { get; set; }

    public string PaperType { get; set; }

    public string Language { get; set; }

    public int? CitedByCount { get; set; }

    public int? ReferenceCount { get; set; }

    public string Volume { get; set; }

    public string Issue { get; set; }

    public string Page { get; set; }

    public bool? IsOpenAccess { get; set; }

    public bool? IsRetracted { get; set; }

    public Guid? JournalId { get; set; }

    public string ReferencedWorks { get; set; }

    public string RelatedWorks { get; set; }

    public string AbstractInvertedIndex { get; set; }

    public string CountsByYear { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Journal Journal { get; set; }

    public virtual ICollection<PaperAuthor> PaperAuthors { get; set; } = new List<PaperAuthor>();

    public virtual ICollection<PaperKeyword> PaperKeywords { get; set; } = new List<PaperKeyword>();

    public virtual ICollection<PaperSourceMapping> PaperSourceMappings { get; set; } = new List<PaperSourceMapping>();

    public virtual ICollection<PaperTopic> PaperTopics { get; set; } = new List<PaperTopic>();

    public virtual ICollection<UserBookmark> UserBookmarks { get; set; } = new List<UserBookmark>();
}

