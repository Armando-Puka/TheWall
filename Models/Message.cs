#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
namespace TheWall.Models;

public class Message {
    [Key]
    public int MessageId { get; set; }

    public int? UserId { get; set; }
    public User? Commenter { get; set; }

    [Required(ErrorMessage = "Message field is required")]
    public string MessageText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<Comment>? AllComments { get; set; } = new List<Comment>(); 
}