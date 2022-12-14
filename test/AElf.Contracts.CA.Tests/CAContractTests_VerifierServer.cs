using System.Threading.Tasks;
using Google.Protobuf;
using Xunit;

namespace AElf.Contracts.CA;

public partial class CAContractTests : CAContractTestBase
{
    [Fact]
    public async Task AddVerifierServerEndPointsTest()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        await CaContractStub.AddVerifierServerEndPoints.SendAsync(new AddVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });
    }
}