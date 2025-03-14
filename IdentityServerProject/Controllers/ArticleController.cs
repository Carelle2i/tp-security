using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ArticlesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ArticlesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // 1. Endpoint public: Liste des articles publiés
    [HttpGet("public")]
    public async Task<ActionResult> GetPublicArticles()
    {
        var articles = await _context.Articles
            .Where(a => a.IsPublished)
            .ToListAsync();

        return Ok(articles);
    }

    // 2. Endpoint pour utilisateurs authentifiés: Créer un article
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateArticle([FromBody] Article article)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        article.AuthorId = user.Id;
        article.PublishedDate = DateTime.UtcNow;
        article.IsPublished = false;  
        article.LikeCount = 0;

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
    }

    // 3. Endpoint pour utilisateurs authentifiés: Liker un article
    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<ActionResult> LikeArticle(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null || !article.IsPublished)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        // Vérifier si l'utilisateur a déjà liké un article
        var alreadyLiked = await _context.Articles
            .Where(a => a.AuthorId == user.Id && a.IsPublished)
            .AnyAsync(a => a.LikeCount > 0);

        if (alreadyLiked)
        {
            return BadRequest("Vous ne pouvez liker qu'un seul article.");
        }

        article.LikeCount++;
        await _context.SaveChangesAsync();

        return Ok(article);
    }

    // 4. Endpoint pour utilisateurs authentifiés: Modifier un article
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateArticle(int id, [FromBody] Article article)
    {
        var existingArticle = await _context.Articles.FindAsync(id);
        if (existingArticle == null || existingArticle.AuthorId != article.AuthorId)
        {
            return NotFound();
        }

        existingArticle.Title = article.Title;
        existingArticle.Summary = article.Summary;
        existingArticle.IsPublished = article.IsPublished;
        existingArticle.IsDraft = article.IsDraft;

        await _context.SaveChangesAsync();

        return Ok(existingArticle);
    }

    // 5. Endpoint pour utilisateurs authentifiés: Supprimer un article
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteArticle(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 6. Endpoint pour Administrateurs: Voir tous les articles, y compris brouillon
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetAllArticlesForAdmin()
    {
        var articles = await _context.Articles.ToListAsync();
        return Ok(articles);
    }

    // 7. Endpoint pour Administrateurs: Modifier ou supprimer des articles de tous les utilisateurs
    [HttpPut("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AdminUpdateArticle(int id, [FromBody] Article article)
    {
        var existingArticle = await _context.Articles.FindAsync(id);
        if (existingArticle == null)
        {
            return NotFound();
        }

        existingArticle.Title = article.Title;
        existingArticle.Summary = article.Summary;
        existingArticle.IsPublished = article.IsPublished;

        await _context.SaveChangesAsync();

        return Ok(existingArticle);
    }

    // 8. Récupérer un article par ID
    [HttpGet("{id}")]
    public async Task<ActionResult> GetArticle(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        return Ok(article);
    }
}
