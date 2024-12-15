namespace TaskBoard.BlazorServer.Services
{
    public interface IReadmeService
    {
        Task<string> GetReadmeContentAsync();
    }
}