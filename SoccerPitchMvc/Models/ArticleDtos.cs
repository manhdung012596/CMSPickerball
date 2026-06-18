using System;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

// ===== ARTICLE DTOs =====

public class ArticleListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? CategoryName { get; set; }
    public int Status { get; set; }
    public string StatusText => Status switch { 0 => "Nháp", 1 => "Đã xuất bản", 2 => "Ẩn", _ => "Không rõ" };
    public string StatusBadge => Status switch { 0 => "secondary", 1 => "success", 2 => "warning", _ => "light" };
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public string? AuthorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class CreateArticleDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(300, ErrorMessage = "Tiêu đề tối đa 300 ký tự")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Summary { get; set; }

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public int? CategoryId { get; set; }

    public int Status { get; set; } = 0;

    public bool IsFeatured { get; set; } = false;

    [MaxLength(100)]
    public string? AuthorName { get; set; }
}

public class UpdateArticleDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Summary { get; set; }

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public int? CategoryId { get; set; }

    public int Status { get; set; } = 0;

    public bool IsFeatured { get; set; } = false;

    [MaxLength(100)]
    public string? AuthorName { get; set; }
}

// ===== CATEGORY DTOs =====

public class ArticleCategoryListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentName { get; set; }
    public int SortOrder { get; set; }
    public int Status { get; set; }
    public string StatusText => Status == 1 ? "Hoạt động" : "Ẩn";
    public string StatusBadge => Status == 1 ? "success" : "secondary";
    public int ArticleCount { get; set; }
}

public class CreateArticleCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [MaxLength(150, ErrorMessage = "Tên danh mục tối đa 150 ký tự")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public int SortOrder { get; set; } = 0;

    public int Status { get; set; } = 1;
}

public class UpdateArticleCategoryDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public int SortOrder { get; set; } = 0;

    public int Status { get; set; } = 1;
}

// ===== COMMENT DTOs =====

public class ArticleCommentListDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public string ArticleTitle { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorEmail { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusText => Status switch { 0 => "Chờ duyệt", 1 => "Đã duyệt", 2 => "Spam", _ => "Không rõ" };
    public string StatusBadge => Status switch { 0 => "warning", 1 => "success", 2 => "danger", _ => "light" };
    public DateTime CreatedAt { get; set; }
}
