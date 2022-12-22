using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CA;

public partial class CAContractTests : CAContractTestBase
{
    [Fact]
    public async Task CreateHolderTest()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = User2Address
        });
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
                ManagerAddress = User1Address,
                DeviceString = "123"
            }
        });
    }

    [Fact]
    public async Task CreateHolderTest_Fail_GuardianApproved_Null()
    {
        var executionResult = await CaContractStub.CreateCAHolder.SendWithExceptionAsync(new CreateCAHolderInput
        {
            GuardianApproved = null,
            Manager = new Manager
            {
                ManagerAddress = User1Address,
                DeviceString = "123"
            }
        });
        executionResult.TransactionResult.Error.ShouldContain("invalid input guardian type");
    }
    
    [Fact]
    public async Task CreateHolderTest_Fail_GuardianType_Null()
    {
        var executionResult = await CaContractStub.CreateCAHolder.SendWithExceptionAsync(new CreateCAHolderInput
        {
            GuardianApproved = new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = "",
                    Type = 0
                }
            },
            Manager = new Manager
            {
                ManagerAddress = User1Address,
                DeviceString = "123"
            }
        });
        executionResult.TransactionResult.Error.ShouldContain("invalid input guardian type");
    }
    
    [Fact]
    public async Task CreateHolderTest_Fail_Manager_Null()
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
            Manager = null
        });
        executionResult.TransactionResult.Error.ShouldContain("invalid input manager");
    }
}