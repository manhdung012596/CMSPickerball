using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public class ArticleService : IArticleService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ArticleService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // ===================== ARTICLE =====================

    public async Task<List<ArticleListDto>> GetAllArticlesAsync(string? searchTerm = null, int? categoryId = null, int? status = null)
    {
        var query = _context.Articles
            .Include(a => a.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(a => a.Title.Contains(searchTerm) || (a.AuthorName != null && a.AuthorName.Contains(searchTerm)));

        if (categoryId.HasValue)
            query = query.Where(a => a.CategoryId == categoryId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status);

        var articles = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return _mapper.Map<List<ArticleListDto>>(articles);
    }

    public async Task<Article?> GetArticleByIdAsync(int id)
    {
        return await _context.Articles
            .Include(a => a.Category)
            .Include(a => a.Comments)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<UpdateArticleDto?> GetArticleForEditAsync(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null) return null;
        return _mapper.Map<UpdateArticleDto>(article);
    }

    public async Task<(bool Success, string Message)> CreateArticleAsync(CreateArticleDto dto)
    {
        try
        {
            var article = _mapper.Map<Article>(dto);
            article.CreatedAt = DateTime.Now;
            article.Slug = GenerateSlug(dto.Title);

            if (dto.Status == 1)
                article.PublishedAt = DateTime.Now;

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return (true, "Thêm bài viết thành công!");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateArticleAsync(UpdateArticleDto dto)
    {
        try
        {
            var article = await _context.Articles.FindAsync(dto.Id);
            if (article == null) return (false, "Bài viết không tồn tại.");

            var wasPublished = article.Status == 1;
            _mapper.Map(dto, article);
            article.UpdatedAt = DateTime.Now;
            article.Slug = GenerateSlug(dto.Title);

            if (dto.Status == 1 && !wasPublished)
                article.PublishedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "Cập nhật bài viết thành công!");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteArticleAsync(int id)
    {
        try
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return (false, "Bài viết không tồn tại.");

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return (true, "Xóa bài viết thành công!");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> TogglePublishAsync(int id)
    {
        try
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return (false, "Bài viết không tồn tại.");

            if (article.Status == 1)
            {
                article.Status = 0;
            }
            else
            {
                article.Status = 1;
                article.PublishedAt = DateTime.Now;
            }
            article.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            var msg = article.Status == 1 ? "Đã xuất bản bài viết!" : "Đã đưa về trạng thái Nháp!";
            return (true, msg);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    // ===================== CATEGORY =====================

    public async Task<List<ArticleCategoryListDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.ArticleCategories
            .Include(c => c.Parent)
            .Include(c => c.Articles)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync();
        return _mapper.Map<List<ArticleCategoryListDto>>(categories);
    }

    public async Task<List<ArticleCategory>> GetCategorySelectListAsync()
    {
        return await _context.ArticleCategories
            .Where(c => c.Status == 1)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ArticleCategory?> GetCategoryByIdAsync(int id)
    {
        return await _context.ArticleCategories
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<UpdateArticleCategoryDto?> GetCategoryForEditAsync(int id)
    {
        var cat = await _context.ArticleCategories.FindAsync(id);
        if (cat == null) return null;
        return _mapper.Map<UpdateArticleCategoryDto>(cat);
    }

    public async Task<(bool Success, string Message)> CreateCategoryAsync(CreateArticleCategoryDto dto)
    {
        try
        {
            var exists = await _context.ArticleCategories.AnyAsync(c => c.Name == dto.Name);
            if (exists) return (false, "Danh mục này đã tồn tại!");

            var category = _mapper.Map<ArticleCategory>(dto);
            category.Slug = GenerateSlug(dto.Name);

            _context.ArticleCategories.Add(category);
            await _context.SaveChangesAsync();
            return (true, "Thêm danh mục thành công!");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(UpdateArticleCategoryDto dto)
    {
        try
        {
            var category = await _context.ArticleCategories.FindAsync(dto.Id);
            if (category == null) return (false, "Danh mục không tồn tại.");

            var exists = await _context.ArticleCategories.AnyAsync(c => c.Name == dto.Name && c.Id != dto.Id);
            if (exists) return (false, "Tên danh mục này đã được sử dụng!");

            _mapper.Map(dto, category);
            category.Slug = GenerateSlug(dto.Name);

            await _context.SaveChangesAsync();
            return (true, "Cập nhật danh mục thành công!");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
    {
        try
        {
            var category = await _context.ArticleCategories.FindAsync(id);
            if (category == null) return (false, "Danh mục không tồn tại.");

            var hasArticles = await _context.Articles.AnyAsync(a => a.CategoryId == id);
            if (hasArticles) return (false, "Không thể xóa! Danh mục này đang có bài viết liên kết.");

            _context.ArticleCategories.Remove(category);
            await _context.SaveChangesAsync();
            return (true, "Xóa danh mục thành công!");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    // ===================== COMMENT =====================

    public async Task<List<ArticleCommentListDto>> GetAllCommentsAsync(int? status = null, string? searchTerm = null)
    {
        var query = _context.ArticleComments
            .Include(c => c.Article)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(c => c.AuthorName.Contains(searchTerm) || c.Content.Contains(searchTerm));

        var comments = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return _mapper.Map<List<ArticleCommentListDto>>(comments);
    }

    public async Task<(bool Success, string Message)> ApproveCommentAsync(int id)
    {
        try
        {
            var comment = await _context.ArticleComments.FindAsync(id);
            if (comment == null) return (false, "Bình luận không tồn tại.");

            comment.Status = 1;
            await _context.SaveChangesAsync();
            return (true, "Đã duyệt bình luận!");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> RejectCommentAsync(int id)
    {
        try
        {
            var comment = await _context.ArticleComments.FindAsync(id);
            if (comment == null) return (false, "Bình luận không tồn tại.");

            comment.Status = 2;
            await _context.SaveChangesAsync();
            return (true, "Đã từ chối/đánh dấu spam bình luận!");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteCommentAsync(int id)
    {
        try
        {
            var comment = await _context.ArticleComments.FindAsync(id);
            if (comment == null) return (false, "Bình luận không tồn tại.");

            _context.ArticleComments.Remove(comment);
            await _context.SaveChangesAsync();
            return (true, "Xóa bình luận thành công!");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<int> GetPendingCommentsCountAsync()
    {
        return await _context.ArticleComments.CountAsync(c => c.Status == 0);
    }

    // ===================== HELPERS =====================

    private static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return string.Empty;

        // Basic Vietnamese slug
        var slug = title.ToLower();
        slug = slug.Replace("đ", "d").Replace("Đ", "d");
        slug = Regex.Replace(slug, @"[àáâãäåạăắặấầẩẫ]", "a");
        slug = Regex.Replace(slug, @"[èéêëẹếềệểễ]", "e");
        slug = Regex.Replace(slug, @"[ìíîïị]", "i");
        slug = Regex.Replace(slug, @"[òóôõöọốồổỗộớờởỡợ]", "o");
        slug = Regex.Replace(slug, @"[ùúûüụứừửữự]", "u");
        slug = Regex.Replace(slug, @"[ýỳỵỷỹ]", "y");
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}
