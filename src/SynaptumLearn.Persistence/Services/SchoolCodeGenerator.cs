using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Persistence.Contexts ;

namespace SynaptumLearn.Persistence.Services;

public class SchoolCodeGenerator : ISchoolCodeGenerator
{
    private readonly ApplicationDbContext _context;

    public SchoolCodeGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync()
    {
        var count = await _context.Schools.CountAsync();

        return $"SCH{(count + 1):D6}";
    }
}