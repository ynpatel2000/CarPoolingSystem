namespace Carpooling.Application.Common.Querying;

public class SortRequest
{
    public string SortBy { get; set; } = "CreatedAt";

    public bool Descending { get; set; } = true;
}
