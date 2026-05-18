namespace BusinessToolbox.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "📁"; // Default icon
    
    // Navigation property
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
