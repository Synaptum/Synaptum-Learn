using Microsoft.EntityFrameworkCore;
using SynaptumLearn.Persistence.Contexts;
using Xunit;

namespace SynaptumLearn.Tests;

public class UnitTest1
{
    [Fact]
    public void CanReferenceApplicationDbContext()
    {
        Assert.True(
            typeof(DbContext).IsAssignableFrom(
                typeof(ApplicationDbContext)));
    }
}