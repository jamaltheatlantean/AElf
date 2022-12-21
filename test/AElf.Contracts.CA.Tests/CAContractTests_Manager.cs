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
    public async Task SocialRecoveryTest_GuardianType()
    {
        await CreateHolderDefault();
        // GuardianType_ is "";
        var executionResult = await CaContractStub.SocialRecovery.SendAsync(new SocialRecoveryInput()
        {
            Manager = new Manager
            {
                ManagerAddress = User2Address,
                DeviceString = "567"
            },
            LoginGuardianType = new GuardianType
            {
                GuardianType_ = "",
                Type = 0
            }
        });
        executionResult.TransactionResult.Error.ShouldContain("invalid input login guardian type");
        
        //GuardianType_ is null
        var result = await CaContractStub.SocialRecovery.SendAsync(new SocialRecoveryInput()
        {
            Manager = new Manager
            {
                ManagerAddress = User2Address,
                DeviceString = "567"
            },
            LoginGuardianType = new GuardianType
            {
                Type = 0
            }
        });
        result.TransactionResult.Error.ShouldContain("invalid input login guardian type");
        
        //type is null
        var taskResult = await CaContractStub.SocialRecovery.SendAsync(new SocialRecoveryInput()
        {
            Manager = new Manager
            {
                ManagerAddress = User2Address,
                DeviceString = "567"
            },
            LoginGuardianType = new GuardianType
            {
                GuardianType_ = "1@google.com"
            }
        });
        taskResult.TransactionResult.Error.ShouldContain("invalid input login guardian type");
    }

    [Fact]
    public async Task SocialRecoveryTest_Manager()
    {
        await CreateHolderDefault();
        //managerAddress  is  null;
        var taskResult = await CaContractStub.SocialRecovery.SendAsync(new SocialRecoveryInput()
        {
            Manager = new Manager
            {
                ManagerAddress = null,
                DeviceString = "567"
            },
            LoginGuardianType = new GuardianType
            {
                GuardianType_ = "1@google.com",
                Type = 0
            }
        });
        taskResult.TransactionResult.Error.ShouldContain("invalid input ManagerAddress is null ");
        //DeviceString is null
        var result = await CaContractStub.SocialRecovery.SendAsync(new SocialRecoveryInput()
        {
            Manager = new Manager
            {
                ManagerAddress = User2Address,
                DeviceString = null
            },
            LoginGuardianType = new GuardianType
            {
                GuardianType_ = "1@google.com",
                Type = 0
            }
        });
        result.TransactionResult.Error.ShouldContain("invalid input DeviceString is null ");
        //DeviceString is "";
        var executionResult = await CaContractStub.SocialRecovery.SendAsync(new SocialRecoveryInput()
        {
            Manager = new Manager
            {
                ManagerAddress = User2Address,
                DeviceString = ""
            },
            LoginGuardianType = new GuardianType
            {
                GuardianType_ = "1@google.com",
                Type = 0
            }
        });
        executionResult.TransactionResult.Error.ShouldContain("invalid input DeviceString is '' ");

        
        

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
        
        //input ManagerAddress is null
        txExecutionResult =  await CaContractUser1Stub.AddManager.SendWithExceptionAsync(new AddManagerInput()
        {
            Manager = new Manager()
            {
                ManagerAddress = null,
                DeviceString = "iphone14-2022"
            }
        });
        txExecutionResult.TransactionResult.Error.ShouldContain("invalid input manager");
          
        //inout deviceString is null
        txExecutionResult =  await CaContractUser1Stub.AddManager.SendWithExceptionAsync(new AddManagerInput()
        {
            Manager = new Manager()
            {
                ManagerAddress = User2Address,
                DeviceString = null
            }
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