using System.Threading.Tasks;
using Xunit;

namespace AElf.Contracts.CA;

public class CAContractTests : CAContractTestBase
{
    [Fact]
    public async Task CreateHolderTest()
    {
        await CaContractStub.CreateCAHolder.SendAsync(new CreateCAHolderInput
        {
            GuardianApproved = new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = "1@google.com",
                    Type = 0
                }
            },
            Manager = new Manager
            {
                ManagerAddresses = User1Address,
                DeviceString = "123"
            }
        });
    }
}