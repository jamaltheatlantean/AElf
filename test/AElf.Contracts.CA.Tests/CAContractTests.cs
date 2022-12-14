using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CA;

public partial class CAContractTests : CAContractTestBase
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
        {
            var executionResult = await CaContractStub.CreateCAHolder.SendWithExceptionAsync(new CreateCAHolderInput
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
            executionResult.TransactionResult.Error.ShouldContain("...");
        }
    }
}