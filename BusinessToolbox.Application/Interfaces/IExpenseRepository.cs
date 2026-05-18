using BusinessToolbox.Domain.Entities;

namespace BusinessToolbox.Application.Interfaces;

public interface IExpenseRepository
{
    Task<IEnumerable<Expense>> GetByUserIdAsync(string userId);
    Task<Expense?> GetByIdAsync(Guid id);
    Task AddAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(Expense expense);
    Task<IEnumerable<Category>> GetCategoriesAsync();
}
