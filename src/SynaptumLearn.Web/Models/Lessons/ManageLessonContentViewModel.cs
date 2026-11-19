
using SynaptumLearn.Application.Curriculum.Lessons.ListLessonContents;
using SynaptumLearn.Domain.Content;

namespace SynaptumLearn.Web.Models.Lessons;

public sealed class ManageLessonContentViewModel
{
    public Guid LessonId { get; set; }

    public string LessonTitle { get; set; } = string.Empty;

    public LessonStatus Status { get; set; }

    public IReadOnlyList<LessonContentItemDto> Contents { get; set; }
        = Array.Empty<LessonContentItemDto>();
}
