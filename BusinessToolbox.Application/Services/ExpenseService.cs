using BusinessToolbox.Application.Interfaces;
using BusinessToolbox.Domain.Entities;

namespace BusinessToolbox.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repository;

    public ExpenseService(IExpenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Expense>> GetUserExpensesAsync(string userId)
    {
        return await _repository.GetByUserIdAsync(userId);
    }

    public async Task<Expense?> GetExpenseByIdAsync(Guid id, string userId)
    {
        var expense = await _repository.GetByIdAsync(id);
        if (expense == null || expense.UserId != userId)
        {
            return null;
        }
        return expense;
    }

    public async Task<Expense> CreateExpenseAsync(Expense expense)
    {
        expense.Id = Guid.NewGuid();
        expense.Date = expense.Date == default ? DateTime.UtcNow : expense.Date;
        await _repository.AddAsync(expense);
        return expense;
    }

    public async Task UpdateExpenseAsync(Expense expense, string userId)
    {
        var existingExpense = await _repository.GetByIdAsync(expense.Id);
        if (existingExpense == null || existingExpense.UserId != userId)
        {
            throw new UnauthorizedAccessException("No tienes permiso para editar este gasto.");
        }

        existingExpense.Amount = expense.Amount;
        existingExpense.Description = expense.Description;
        existingExpense.Date = expense.Date;
        existingExpense.CategoryId = expense.CategoryId;

        await _repository.UpdateAsync(existingExpense);
    }

    public async Task DeleteExpenseAsync(Guid id, string userId)
    {
        var expense = await _repository.GetByIdAsync(id);
        if (expense == null || expense.UserId != userId)
        {
            throw new UnauthorizedAccessException("No tienes permiso para eliminar este gasto.");
        }

        await _repository.DeleteAsync(expense);
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return await _repository.GetCategoriesAsync();
    }
}
