using System.Linq;
using System.Threading.Tasks;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CA;

public partial class CAContractTests : CAContractTestBase
{
    private async Task CreateHolderDefault()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
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
    public async Task SocialRecoveryTest()
    {
        await CreateHolderDefault();
        await CaContractStub.SocialRecovery.SendAsync(new SocialRecoveryInput()
        {
            Manager = new Manager
            {
                ManagerAddress = User2Address,
                DeviceString = "567"
            },
            LoginGuardianType = new GuardianType
            {
                GuardianType_ = "1@google.com",
                Type = 0
            }
        });
    }

    [Fact]
    public async Task AddManagerTest()
    {
        await CreateHolderDefault();
        var caInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput()
        {
            LoginGuardianType = "1@google.com"
        });
      
        //success
        var manager = new Manager()
        {
            ManagerAddress = User2Address,
            DeviceString = "iphone14-2022"
        };
        await CaContractUser1Stub.AddManager.SendAsync(new AddManagerInput()
        {
            CaHash = caInfo.CaHash,
            Manager = manager
        });
        caInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput()
        {
            LoginGuardianType = "1@google.com"
        });
        caInfo.Managers.ShouldContain(manager);
        //manager already existed
        var txResult =  await CaContractUser1Stub.AddManager.SendAsync(new AddManagerInput()
        {
            CaHash = caInfo.CaHash,
            Manager = new Manager()
            {
                ManagerAddress = User2Address,
                DeviceString = "iphone14-2022"
            }
        });
        txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        
        //caHolder not exist
        var notExistedCash = HashHelper.ComputeFrom("Invalid CaHash");
        var txExecutionResult =  await CaContractUser1Stub.AddManager.SendWithExceptionAsync(new AddManagerInput()
        {
            CaHash = notExistedCash,
            Manager = new Manager()
            {
                ManagerAddress = User2Address,
                DeviceString = "iphone14-2022"
            }
        });
        txExecutionResult.TransactionResult.Error.ShouldContain("CA holder is null");
        
        //input caHash is null
        txExecutionResult =  await CaContractUser1Stub.AddManager.SendWithExceptionAsync(new AddManagerInput()
        {
            Manager = new Manager()
            {
                ManagerAddress = User2Address,
                DeviceString = "iphone14-2022"
            }
        });
        txExecutionResult.TransactionResult.Error.ShouldContain("invalid input CaHash");
        
        //input manager is null
        txExecutionResult =  await CaContractUser1Stub.AddManager.SendWithExceptionAsync(new AddManagerInput()
        {
            CaHash = caInfo.CaHash
        });
        txExecutionResult.TransactionResult.Error.ShouldContain("invalid input manager");
    }

    [Fact]
    public async Task RemoveManagerTest()
    {
        await CreateHolderDefault();
        var caInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput()
        {
            LoginGuardianType = "1@google.com"
        });
        //caHolder not existed
        var notExistedCash = HashHelper.ComputeFrom("Invalid CaHash");
        var txExecutionResult =  await CaContractUser1Stub.RemoveManager.SendWithExceptionAsync(new RemoveManagerInput()
        {
            CaHash = notExistedCash,
            Manager = new Manager()
            {
                ManagerAddress = User2Address,
                DeviceString = "iphone14-2022"
            }
        });
        txExecutionResult.TransactionResult.Error.ShouldContain("CA holder is null.");
        
        //input caHash is null
        txExecutionResult =  await CaContractUser1Stub.RemoveManager.SendWithExceptionAsync(new RemoveManagerInput()
        {
            Manager = new Manager()
            {
                ManagerAddress = User2Address,
                DeviceString = "iphone14-2022"
            }
        });
        txExecutionResult.TransactionResult.Error.ShouldContain("invalid input CaHash");
        
        //input manager is null
        txExecutionResult =  await CaContractUser1Stub.RemoveManager.SendWithExceptionAsync(new RemoveManagerInput()
        {
            CaHash = caInfo.CaHash
        });
        txExecutionResult.TransactionResult.Error.ShouldContain("invalid input manager");

        //manager not exist
        var txResult =  await CaContractUser1Stub.RemoveManager.SendAsync(new RemoveManagerInput()
        {
            CaHash = caInfo.CaHash,
            Manager = new Manager()
            {
                ManagerAddress = User2Address,
                DeviceString = "iphone14-2022"
            }
        });
        txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        
        //success
        var manager = new Manager
        {
            ManagerAddress = User1Address,
            DeviceString = "123"
        };
        await CaContractUser1Stub.RemoveManager.SendAsync(new RemoveManagerInput()
        {
            CaHash = caInfo.CaHash,
            Manager = manager
        });
       
        caInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput()
        {
            LoginGuardianType = "1@google.com"
        });
        caInfo.Managers.ShouldNotContain(manager);
    }
}