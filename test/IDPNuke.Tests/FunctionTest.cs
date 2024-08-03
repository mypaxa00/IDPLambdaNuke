using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

namespace IDPNuke.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestResponseElementsCount()
    {
        Function function = new();
        ILambdaContext context = new TestLambdaContext();
        IEnumerable<int> result = await function.FunctionHandler(context);
        Assert.Equal(25, result.Count());
    }
}
