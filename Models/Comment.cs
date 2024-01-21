#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
namespace TheWall.Models;

public class Comment {
    [Key]
    public int CommentId { get; set; }

    public int? UserId { get; set; }

    public int? MessageId { get; set; }

    [Required(ErrorMessage = "Comment field is required")]
    public string CommentText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public User? UserCommenter { get; set; }
    public Message? MessageContent { get; set; }
}