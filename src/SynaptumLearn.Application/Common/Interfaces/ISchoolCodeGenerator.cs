namespace SynaptumLearn.Application.Common.Interfaces;

public interface ISchoolCodeGenerator
{
    Task<string> GenerateAsync();
}