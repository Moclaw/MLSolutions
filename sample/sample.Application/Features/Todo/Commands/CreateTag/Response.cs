using sample.Application.Features.Todo.Dtos;

namespace sample.Application.Features.Todo.Commands.CreateTag;

public class Response : TagDto
{
    public static Response FromTagDto(TagDto dto)
    {
        return new Response
        {
            Id = dto.Id,
            Name = dto.Name,
            Color = dto.Color
        };
    }
}
