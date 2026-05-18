namespace BusinessToolbox.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty; // Supabase User ID
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    // Foreign Key
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
}
