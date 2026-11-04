namespace api.DTOs;

public class PersonDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}

public class CreatePersonRequestDto
{
    public required string Name { get; set; }
}
