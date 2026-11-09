using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using SynaptumLearn.Application.Common.Interfaces;
using SynaptumLearn.Domain.Users;
using SynaptumLearn.Domain.Schools;
using SynaptumLearn.Domain.Enums;

namespace SynaptumLearn.Application.Users.CreateSchoolUser;

public sealed class CreateSchoolUserCommandHandler
    : IRequestHandler<CreateSchoolUserCommand, Guid>
{
    private readonly ISchoolAdminAccessService _schoolAdminAccessService;
    private readonly IApplicationDbContext _context;
    private readonly IIdentityUserProvisioningService _identityService;

    public CreateSchoolUserCommandHandler(
    ISchoolAdminAccessService schoolAdminAccessService,
    IApplicationDbContext context,
    IIdentityUserProvisioningService identityService)
    {
        _schoolAdminAccessService = schoolAdminAccessService;
        _context = context;
        _identityService = identityService;
    }

    public async Task<Guid> Handle(
        CreateSchoolUserCommand request,
        CancellationToken cancellationToken)
    {
        // Verify the caller is an authorized School Administrator.
        var schoolId = await _schoolAdminAccessService
            .GetAuthorizedSchoolIdAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new InvalidOperationException(
                "A first name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new InvalidOperationException(
                "A last name is required.");
        }

        if (request.FirstName.Trim().Length > 100)
        {
            throw new InvalidOperationException(
                "A first name cannot exceed 100 characters.");
        }

        if (request.LastName.Trim().Length > 100)
        {
            throw new InvalidOperationException(
                "A last name cannot exceed 100 characters.");
        }

        // Only Teacher and Learner accounts are permitted.
        if (!Enum.IsDefined(request.AccountType))
        {
            throw new InvalidOperationException(
                "Invalid school user account type.");
        }

        if (request.AccountType == SchoolUserAccountType.Teacher)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeNumber))
            {
                throw new InvalidOperationException(
                    "An employee number is required for teachers.");
            }

            if (request.EmployeeNumber.Trim().Length > 50)
            {
                throw new InvalidOperationException(
                    "An employee number cannot exceed 50 characters.");
            }

            if (!string.IsNullOrWhiteSpace(request.LearnerNumber) ||
                request.GradeId.HasValue)
            {
                throw new InvalidOperationException(
                    "Teacher accounts cannot contain learner information.");
            }
        }
        else if (request.AccountType == SchoolUserAccountType.Learner)
        {
            if (string.IsNullOrWhiteSpace(request.LearnerNumber))
            {
                throw new InvalidOperationException(
                    "A learner number is required for learners.");
            }

            if (request.LearnerNumber.Trim().Length > 50)
            {
                throw new InvalidOperationException(
                    "A learner number cannot exceed 50 characters.");
            }

            if (!request.GradeId.HasValue ||
                request.GradeId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "A valid grade is required for learners.");
            }

            if (!string.IsNullOrWhiteSpace(request.EmployeeNumber) ||
                request.IsClassTeacher)
            {
                throw new InvalidOperationException(
                    "Learner accounts cannot contain teacher information.");
            }
        }

        if (request.AccountType == SchoolUserAccountType.Learner)
        {
            var gradeExists = await _context.Grades
                .AsNoTracking()
                .AnyAsync(
                    g => g.Id == request.GradeId!.Value,
                    cancellationToken);

            if (!gradeExists)
            {
                throw new InvalidOperationException(
                    "The selected grade does not exist.");
            }
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException(
                "An email address is required.");
        }

        var email = request.Email.Trim();

        if (email.Length > 150)
        {
            throw new InvalidOperationException(
                "An email address cannot exceed 150 characters.");
        }

        if (!new EmailAddressAttribute().IsValid(email) ||
            email.Any(char.IsWhiteSpace))
        {
            throw new InvalidOperationException(
                "A valid email address is required.");
        }

        var existingUser = await _context.BusinessUsers
            .AnyAsync(
                u => u.Email == email,
                cancellationToken);

        if (existingUser)
        {
            throw new InvalidOperationException(
                "A domain user with this email already exists.");
        }

        if (request.AccountType == SchoolUserAccountType.Teacher)
        {
            var employeeNumber = request.EmployeeNumber!.Trim();

            var employeeNumberExists = await _context.TeacherProfiles
                .AsNoTracking()
                .AnyAsync(
                    t => t.SchoolId == schoolId &&
                         t.EmployeeNumber == employeeNumber,
                    cancellationToken);

            if (employeeNumberExists)
            {
                throw new InvalidOperationException(
                    "A teacher with this employee number already exists at this school.");
            }
        }
        else if (request.AccountType == SchoolUserAccountType.Learner)
        {
            var learnerNumber = request.LearnerNumber!.Trim();

            var learnerNumberExists = await _context.LearnerProfiles
                .AsNoTracking()
                .AnyAsync(
                    l => l.SchoolId == schoolId &&
                         l.LearnerNumber == learnerNumber,
                    cancellationToken);

            if (learnerNumberExists)
            {
                throw new InvalidOperationException(
                    "A learner with this learner number already exists at this school.");
            }
        }

        // Account creation
        Guid domainUserId = Guid.Empty;

        await _context.ExecuteInTransactionAsync(
            async ct =>
            {
                var identityUserId =
                    await _identityService.CreateUserAsync(
                        email,
                        request.Password,
                        ct);

                var user = new User
                {
                    IdentityUserId = identityUserId,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = email,
                    IsActive = true
                };

                _context.BusinessUsers.Add(user);
                await _context.SaveChangesAsync(ct);

                var schoolRole = request.AccountType switch
                {
                    SchoolUserAccountType.Teacher => SchoolRole.Teacher,
                    SchoolUserAccountType.Learner => SchoolRole.Learner,
                    _ => throw new InvalidOperationException(
                        "Invalid school user account type.")
                };

                var membership = new SchoolMembership
                {
                    SchoolId = schoolId,
                    UserId = user.Id,
                    Role = schoolRole,
                    IsActive = true
                };

                _context.SchoolMemberships.Add(membership);
                await _context.SaveChangesAsync(ct);

                if (request.AccountType == SchoolUserAccountType.Teacher)
                {
                    var teacherProfile = new TeacherProfile
                    {
                        SchoolId = schoolId,
                        UserId = user.Id,
                        EmployeeNumber = request.EmployeeNumber!.Trim(),
                        IsClassTeacher = request.IsClassTeacher
                    };

                    _context.TeacherProfiles.Add(teacherProfile);
                }
                else if (request.AccountType == SchoolUserAccountType.Learner)
                {
                    var learnerProfile = new LearnerProfile
                    {
                        SchoolId = schoolId,
                        UserId = user.Id,
                        LearnerNumber = request.LearnerNumber!.Trim(),
                        GradeId = request.GradeId!.Value
                    };

                    _context.LearnerProfiles.Add(learnerProfile);
                }

                await _context.SaveChangesAsync(ct);

                domainUserId = user.Id;
            },
            cancellationToken);

        return domainUserId;

    }
}
