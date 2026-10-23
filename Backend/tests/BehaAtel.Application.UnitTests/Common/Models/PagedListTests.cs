using BehaAtel.Application.Common.Models;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Common.Models;

public class PagedListTests
{
    [Fact]
    public void Create_ReturnsRequestedPageAndMetadata()
    {
        var result = PagedList<int>.Create([1, 2, 3, 4, 5], page: 2, pageSize: 2);

        result.Items.Should().Equal(3, 4);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.HasNext.Should().BeTrue();
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void Create_CapsPageSizeAtOneHundred()
    {
        var result = PagedList<int>.Create(Enumerable.Range(1, 150), page: 1, pageSize: 150);

        result.Items.Should().HaveCount(100);
        result.PageSize.Should().Be(100);
        result.TotalCount.Should().Be(150);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public void Create_RejectsInvalidPage()
    {
        var action = () => PagedList<int>.Create([1, 2], page: 0, pageSize: 20);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
