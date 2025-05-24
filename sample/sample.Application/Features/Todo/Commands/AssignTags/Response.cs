namespace sample.Application.Features.Todo.Commands.AssignTags;

public class Response
{
    public int TodoId { get; set; }
    public List<int> AssignedTagIds { get; set; } = new();
}
