﻿using System.Collections.Generic;
using sample.Domain.Entities;

namespace sample.Application.Features.Todo.Dtos
{
    public class TodoItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public ICollection<TagDto> Tags { get; set; } = [];
    }
}
