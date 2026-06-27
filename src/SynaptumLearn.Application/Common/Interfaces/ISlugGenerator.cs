namespace SynaptumLearn.Application.Common.Interfaces;

public interface ISlugGenerator
{
    Task<string> GenerateSchoolSlugAsync(string schoolName);
}