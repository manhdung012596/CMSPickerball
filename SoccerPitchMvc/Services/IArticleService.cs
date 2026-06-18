using System.Collections.Generic;
using System.Threading.Tasks;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public interface IArticleService
{
    // === ARTICLE ===
    Task<List<ArticleListDto>> GetAllArticlesAsync(string? searchTerm = null, int? categoryId = null, int? status = null);
    Task<Article?> GetArticleByIdAsync(int id);
    Task<UpdateArticleDto?> GetArticleForEditAsync(int id);
    Task<(bool Success, string Message)> CreateArticleAsync(CreateArticleDto dto);
    Task<(bool Success, string Message)> UpdateArticleAsync(UpdateArticleDto dto);
    Task<(bool Success, string Message)> DeleteArticleAsync(int id);
    Task<(bool Success, string Message)> TogglePublishAsync(int id);

    // === CATEGORY ===
    Task<List<ArticleCategoryListDto>> GetAllCategoriesAsync();
    Task<List<ArticleCategory>> GetCategorySelectListAsync();
    Task<ArticleCategory?> GetCategoryByIdAsync(int id);
    Task<UpdateArticleCategoryDto?> GetCategoryForEditAsync(int id);
    Task<(bool Success, string Message)> CreateCategoryAsync(CreateArticleCategoryDto dto);
    Task<(bool Success, string Message)> UpdateCategoryAsync(UpdateArticleCategoryDto dto);
    Task<(bool Success, string Message)> DeleteCategoryAsync(int id);

    // === COMMENT ===
    Task<List<ArticleCommentListDto>> GetAllCommentsAsync(int? status = null, string? searchTerm = null);
    Task<(bool Success, string Message)> ApproveCommentAsync(int id);
    Task<(bool Success, string Message)> RejectCommentAsync(int id);
    Task<(bool Success, string Message)> DeleteCommentAsync(int id);
    Task<int> GetPendingCommentsCountAsync();
}
