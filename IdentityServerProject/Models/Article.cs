using System;
using Microsoft.AspNetCore.Identity;

public class Article
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public DateTime PublishedDate { get; set; }
    public int LikeCount { get; set; }
    public bool IsPublished { get; set; }
    public required string AuthorId { get; set; }  // Correspond à l'ID de l'utilisateur (ApplicationUser)
    public required ApplicationUser Author { get; set; }  // Navigation vers l'utilisateur

    // Indique si l'article est en mode brouillon ou publié
    public bool IsDraft { get; set; }
}
