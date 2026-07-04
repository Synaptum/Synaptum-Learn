using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Persistence.Contexts;

namespace SynaptumLearn.Persistence.Common;
 
public class SlugGenerator : ISlugGenerator
{
    private readonly ApplicationDbContext _context;

    public SlugGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateSchoolSlugAsync(string schoolName, CancellationToken cancellationToken)
    {
        var slug = schoolName
            .Trim()
            .ToLowerInvariant();

        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");

        var originalSlug = slug;
        var counter = 2;

        while (await _context.Schools.AnyAsync(s => s.Slug == slug, cancellationToken))
        {
            slug = $"{originalSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}