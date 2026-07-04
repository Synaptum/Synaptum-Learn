namespace SynaptumLearn.Application.Common.Interfaces
{
    public interface ISequenceGenerator
    {
        Task<string> GenerateAsync(string entityName, string prefix, CancellationToken cancellationToken = default);
    }
}