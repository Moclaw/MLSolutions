using sample.Shared.DTOs;

namespace sample.Shared.Interfaces
{
    public interface ITodoService
    {
        Task<IEnumerable<TodoItemDto>> GetAllAsync();
        Task<TodoItemDto?> GetByIdAsync(Guid id);
        Task<TodoItemDto> CreateAsync(CreateTodoItemDto createDto);
        Task<TodoItemDto> UpdateAsync(Guid id, UpdateTodoItemDto updateDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
