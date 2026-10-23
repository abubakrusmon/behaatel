using BehaAtel.Application.Features.Residents.Common;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Residents;

public class ResidentBalanceQueriesTests
{
    [Theory]
    [InlineData(20, 100, -80)]
    [InlineData(100, 20, 80)]
    [InlineData(50, 50, 0)]
    public void Calculate_ReturnsPaidMinusCharged(decimal paid, decimal charged, decimal expected)
    {
        var balance = ResidentBalanceQueries.Calculate(paid, charged);

        balance.Should().Be(expected);
    }
}
