using System;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Bình luận bài viết
/// </summary>
public class ArticleComment
{
    public int Id { get; set; }

    public int ArticleId { get; set; }

    [Required]
    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AuthorEmail { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái: 0=Chờ duyệt, 1=Đã duyệt, 2=Từ chối/Spam
    /// </summary>
    public int Status { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? ParentId { get; set; }

    // Navigation properties
    public virtual Article? Article { get; set; }
    public virtual ArticleComment? Parent { get; set; }
}
