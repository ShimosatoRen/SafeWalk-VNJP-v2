namespace DotNetStarterProject.Models;

public sealed class SampleItem
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}
