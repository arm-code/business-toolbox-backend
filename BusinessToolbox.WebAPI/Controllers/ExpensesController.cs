using System.Security.Claims;
using BusinessToolbox.Application.Interfaces;
using BusinessToolbox.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessToolbox.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? throw new UnauthorizedAccessException("ID de usuario no encontrado en el token.");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
    {
        var expenses = await _expenseService.GetUserExpensesAsync(UserId);
        return Ok(expenses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(Guid id)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(id, UserId);
        if (expense == null) return NotFound();
        return Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        expense.UserId = UserId;
        var createdExpense = await _expenseService.CreateExpenseAsync(expense);
        return CreatedAtAction(nameof(GetExpense), new { id = createdExpense.Id }, createdExpense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(Guid id, Expense expense)
    {
        if (id != expense.Id) return BadRequest();
        
        try
        {
            await _expenseService.UpdateExpenseAsync(expense, UserId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        try
        {
            await _expenseService.DeleteExpenseAsync(id, UserId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var categories = await _expenseService.GetCategoriesAsync();
        return Ok(categories);
    }
}
