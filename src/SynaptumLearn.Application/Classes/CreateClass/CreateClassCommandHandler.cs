
using MediatR;
using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Schools;

namespace SynaptumLearn.Application.Classes.CreateClass;

public sealed class CreateClassCommandHandler
    : IRequestHandler<CreateClassCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchoolAdminAccessService _accessService;

    public CreateClassCommandHandler(
        IApplicationDbContext context,
        ISchoolAdminAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<Guid> Handle(
        CreateClassCommand request,
        CancellationToken cancellationToken)
    {
        var schoolId = await _accessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        var name = request.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
        {
            throw new InvalidOperationException(
                "Class name is required and must not exceed 50 characters.");
        }

        if (request.AcademicYear < 2000 || request.AcademicYear > 2100)
        {
            throw new InvalidOperationException(
                "Please provide a valid academic year.");
        }

        var gradeExists = await _context.Grades
            .AsNoTracking()
            .AnyAsync(g => g.Id == request.GradeId, cancellationToken);

        if (!gradeExists)
        {
            throw new InvalidOperationException(
                "The selected grade does not exist.");
        }

        var duplicateExists = await _context.Classes
            .AsNoTracking()
            .AnyAsync(c =>
                c.SchoolId == schoolId &&
                c.GradeId == request.GradeId &&
                c.AcademicYear == request.AcademicYear &&
                c.Name == name,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "A class with this name already exists for the selected grade and academic year.");
        }

        var schoolClass = new Class
        {
            SchoolId = schoolId,
            Name = name,
            GradeId = request.GradeId,
            AcademicYear = request.AcademicYear
        };

        _context.Classes.Add(schoolClass);

        await _context.SaveChangesAsync(cancellationToken);

        return schoolClass.Id;
    }
}
