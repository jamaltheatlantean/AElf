using System.Threading.Tasks;
using Xunit;

namespace AElf.Contracts.CA;

public partial class CAContractTests : CAContractTestBase
{
    private async Task CreateHolderDefault()
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
    
    [Fact]
    public async Task SocialRecoveryTest()
    {
        await CreateHolderDefault();
        await CaContractStub.SocialRecovery.SendAsync(new SocialRecoveryInput()
        {
            Manager = new Manager
            {
                ManagerAddresses = User2Address,
                DeviceString = "567"
            },
            LoginGuardianType = new GuardianType
            {
                GuardianType_ = "1@google.com",
                Type = 0
            }
        });
    }
}