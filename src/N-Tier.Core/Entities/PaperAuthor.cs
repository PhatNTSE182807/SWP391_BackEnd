using System;
using System.Collections.Generic;

namespace N_Tier.Core.Entities;

public partial class PaperAuthor
{
    public Guid PaperAuthorId { get; set; }

    public Guid PaperId { get; set; }

    public Guid AuthorId { get; set; }

    public int? AuthorOrder { get; set; }

    public string AuthorPosition { get; set; }

    public string RawAuthorName { get; set; }

    public bool? IsCorresponding { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Author Author { get; set; }

    public virtual Paper Paper { get; set; }
}
