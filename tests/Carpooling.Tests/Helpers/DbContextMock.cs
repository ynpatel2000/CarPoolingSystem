public static class DbContextMock
{
    public static IQueryable<T> BuildMockDbSet<T>(List<T> data) where T : class
    {
        return data.AsQueryable();
    }
}
