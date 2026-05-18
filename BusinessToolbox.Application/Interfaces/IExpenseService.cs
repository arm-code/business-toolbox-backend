using BusinessToolbox.Domain.Entities;

namespace BusinessToolbox.Application.Interfaces;

public interface IExpenseService
{
    Task<IEnumerable<Expense>> GetUserExpensesAsync(string userId);
    Task<Expense?> GetExpenseByIdAsync(Guid id, string userId);
    Task<Expense> CreateExpenseAsync(Expense expense);
    Task UpdateExpenseAsync(Expense expense, string userId);
    Task DeleteExpenseAsync(Guid id, string userId);
    Task<IEnumerable<Category>> GetCategoriesAsync();
}
