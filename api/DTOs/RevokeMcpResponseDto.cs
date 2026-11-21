namespace api.DTOs;

public class RevokeMcpResponseDto
{
    // Clients that had an active connection and were disconnected.
    public required int Clients { get; set; }
}
