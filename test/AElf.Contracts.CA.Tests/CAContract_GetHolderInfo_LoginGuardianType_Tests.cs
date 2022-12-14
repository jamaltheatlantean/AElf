using System.Threading.Tasks;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CA;

public partial class CAContractTests : CAContractTestBase
{
    [Fact]
    public async Task GetHolderInfo_ByCaHash_Test()
    {
        await CreateAHolder_Helper();

        // cannot get caHash directly，for caHash is created internelly
        var getHolderInfoOutput = await CaContractStub.GetHolderInfo.SendAsync(new GetHolderInfoInput
        {
            CaHash = null,
            LoginGuardianType = "1@google.com"
        });

        var caHash = getHolderInfoOutput.Output.CaHash;
        
        getHolderInfoOutput = await CaContractStub.GetHolderInfo.SendAsync(new GetHolderInfoInput
        {
            CaHash = caHash,
            LoginGuardianType = "1@google.com"
        });

        var guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        var guardians = guardiansInfo.Guardians;
        guardians.Count.ShouldBe(1);
        guardians[0].GuardianType.Type.ShouldBe(GuardianTypeType.GuardianTypeOfEmail);
        guardians[0].GuardianType.GuardianType_.ShouldBe("1@google.com");
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(1);
        guardiansInfo.LoginGuardianTypeIndexes.Contains(0);
    }
    
    [Fact]
    public async Task GetHolderInfo_ByLoginGuardian_Test()
    {
        await CreateAHolder_Helper();

        var getHolderInfoOutput = await CaContractStub.GetHolderInfo.SendAsync(new GetHolderInfoInput
        {
            CaHash = null,
            LoginGuardianType = "1@google.com"
        });

        var guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        var guardians = guardiansInfo.Guardians;
        guardians.Count.ShouldBe(1);
        guardians[0].GuardianType.Type.ShouldBe(GuardianTypeType.GuardianTypeOfEmail);
        guardians[0].GuardianType.GuardianType_.ShouldBe("1@google.com");
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(1);
        guardiansInfo.LoginGuardianTypeIndexes.Contains(0);
    }
    
    [Fact]
    public async Task GetHolderInfo_ByNULL_Test()
    {
        await CreateAHolder_Helper();

        var executionResult = await CaContractStub.GetHolderInfo.CallWithExceptionAsync(new GetHolderInfoInput
        {
            CaHash = null,
            LoginGuardianType = ""
        });
        
        executionResult.Value.ShouldContain("CaHash is null, and loginGuardianType is empty");
    }
    
    [Fact]
    public async Task GetHolderInfo_ByInvalidCaHash_Test()
    {
        await CreateAHolder_Helper();

        var executionResult = await CaContractStub.GetHolderInfo.CallWithExceptionAsync(new GetHolderInfoInput
        {
            CaHash = new Hash(),
            LoginGuardianType = ""
        });
        
        executionResult.Value.ShouldContain("Bad ca_hash");
    }
    
    [Fact]
    public async Task GetHolderInfo_ByInvalidLoginGuardianType_Test()
    {
        await CreateAHolder_Helper();

        var executionResult = await CaContractStub.GetHolderInfo.CallWithExceptionAsync(new GetHolderInfoInput
        {
            CaHash = null,
            LoginGuardianType = "Invalid"
        });
        
        executionResult.Value.ShouldContain("Not found ca_hash by a the loginGuardianType");
    }

    private async Task CreateAHolder_Helper()
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

    // private async Task AddAGuardian_Helper()
    // {
    //     await CaContractStub.AddGuardian.SendAsync(new AddGuardianInput
    //     {
    //         GuardiansApproved = new Guardian
    //         {
    //             GuardianType = new GuardianType
    //             {
    //                 GuardianType_ = "1@google.com",
    //                 Type = 0
    //             }
    //         }
    //     });
    // }
}