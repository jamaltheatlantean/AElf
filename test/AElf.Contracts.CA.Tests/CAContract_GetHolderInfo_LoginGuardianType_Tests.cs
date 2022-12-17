using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AElf.CSharp.Core;
using AElf.Kernel;
using AElf.Types;
using Google.Protobuf.Collections;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CA;

public partial class CAContractTests : CAContractTestBase
{
    [Fact]
    public async Task GetHolderInfo_ByCaHash_Test()
    {
        var caHash = await CreateHolder();

        var getHolderInfoOutput = await CaContractStub.GetHolderInfo.SendAsync(new GetHolderInfoInput
        {
            CaHash = caHash,
            LoginGuardianType = ""
        });

        var guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        var guardians = guardiansInfo.Guardians;
        guardians.Count.ShouldBe(1);
        guardians[0].GuardianType.Type.ShouldBe(GuardianTypeType.GuardianTypeOfEmail);
        guardians[0].GuardianType.GuardianType_.ShouldBe(GuardianType);
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(1);
        guardiansInfo.LoginGuardianTypeIndexes.Contains(0);
    }
    
    [Fact]
    public async Task GetHolderInfo_ByLoginGuardian_Test()
    {
        await CreateHolder();

        var getHolderInfoOutput = await CaContractStub.GetHolderInfo.SendAsync(new GetHolderInfoInput
        {
            CaHash = null,
            LoginGuardianType = GuardianType
        });

        var guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        var guardians = guardiansInfo.Guardians;
        guardians.Count.ShouldBe(1);
        guardians[0].GuardianType.Type.ShouldBe(GuardianTypeType.GuardianTypeOfEmail);
        guardians[0].GuardianType.GuardianType_.ShouldBe(GuardianType);
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(1);
        guardiansInfo.LoginGuardianTypeIndexes.Contains(0);
    }
    
    [Fact]
    public async Task GetHolderInfo_ByNULL_Test()
    {
        await CreateHolder();

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
        await CreateHolder();

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
        await CreateHolder();

        var executionResult = await CaContractStub.GetHolderInfo.CallWithExceptionAsync(new GetHolderInfoInput
        {
            CaHash = null,
            LoginGuardianType = "Invalid"
        });
        
        executionResult.Value.ShouldContain("Not found ca_hash by a the loginGuardianType");
    }
    
    [Fact]
    public async Task SetLoginGuardianType_Succeed_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();
        
        var getHolderInfoOutput = await CaContractStub.GetHolderInfo.SendAsync(new GetHolderInfoInput
        {
            CaHash = caHash,
            LoginGuardianType = ""
        });
        
        var guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        var guardians = guardiansInfo.Guardians;
        guardians.Count.ShouldBe(2);
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(1);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldNotContain(1);

        getHolderInfoOutput = await SetGuardianTypeForLogin_AndGetHolderInfo_Helper(caHash, null);
        
        guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(2);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(1);
    }
    
    [Fact]
    public async Task SetLoginGuardianType_Again_Succeed_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();
        
        var getHolderInfoOutput = await CaContractStub.GetHolderInfo.SendAsync(new GetHolderInfoInput
        {
            CaHash = caHash,
            LoginGuardianType = ""
        });
        
        var guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        var guardians = guardiansInfo.Guardians;
        guardians.Count.ShouldBe(2);
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(1);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldNotContain(1);

        getHolderInfoOutput = await SetGuardianTypeForLogin_AndGetHolderInfo_Helper(caHash, null);
        
        guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(2);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(1);
        
        getHolderInfoOutput = await SetGuardianTypeForLogin_AndGetHolderInfo_Helper(caHash, null);
        
        guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(2);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(1);
    }
    
    [Fact]
    public async Task SetLoginGuardianType_CashNull_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();


        var executionResult = await CaContractStub.SetGuardianTypeForLogin.SendWithExceptionAsync(
            new SetGuardianTypeForLoginInput
            {
                CaHash = null,
                GuardianType = new GuardianType
                {
                    Type = GuardianTypeType.GuardianTypeOfEmail,
                    GuardianType_ = GuardianType1
                }
            });

        executionResult.TransactionResult.Error.ShouldNotBeNull();

    }
    
    [Fact]
    public async Task SetLoginGuardianType_GuardianTypeNull_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();


        var executionResult = await CaContractStub.SetGuardianTypeForLogin.SendWithExceptionAsync(
            new SetGuardianTypeForLoginInput
            {
                CaHash = caHash,
                GuardianType = null
            });

        executionResult.TransactionResult.Error.ShouldNotBeNull();

    }
    
    [Fact]
    public async Task SetLoginGuardianType_GuardianTypeEmpty_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();


        var executionResult = await CaContractStub.SetGuardianTypeForLogin.SendWithExceptionAsync(
            new SetGuardianTypeForLoginInput
            {
                CaHash = caHash,
                GuardianType = new GuardianType
                {
                    Type = GuardianTypeType.GuardianTypeOfEmail,
                    GuardianType_ = ""
                }
            });

        executionResult.TransactionResult.Error.ShouldNotBeNull();

    }
    
    [Fact]
    public async Task UnsetLoginGuardianType_Succeed_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();

        var getHolderInfoOutput = await SetGuardianTypeForLogin_AndGetHolderInfo_Helper(caHash, null);
        
        var guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(2);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(1);
        
        getHolderInfoOutput = await UnsetGuardianTypeForLogin_AndGetHolderInfo_Helper(caHash, null);
        
        guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(1);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldNotContain(1);
    }
    
    [Fact]
    public async Task UnsetLoginGuardianType_Again_Succeed_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();

        var getHolderInfoOutput = await SetGuardianTypeForLogin_AndGetHolderInfo_Helper(caHash, null);
        
        var guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(2);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(1);
        
        getHolderInfoOutput = await UnsetGuardianTypeForLogin_AndGetHolderInfo_Helper(caHash, null);
        
        guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(1);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldNotContain(1);
        
        getHolderInfoOutput = await UnsetGuardianTypeForLogin_AndGetHolderInfo_Helper(caHash, null);
        
        guardiansInfo = getHolderInfoOutput.Output.GuardiansInfo;
        
        guardiansInfo.LoginGuardianTypeIndexes.Count.ShouldBe(1);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldContain(0);
        guardiansInfo.LoginGuardianTypeIndexes.ShouldNotContain(1);
    }
    
    [Fact]
    public async Task UnsetLoginGuardianType_CashNull_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();


        var executionResult = await CaContractStub.UnsetGuardianTypeForLogin.SendWithExceptionAsync(
            new UnsetGuardianTypeForLoginInput
            {
                CaHash = null,
                GuardianType = new GuardianType
                {
                    Type = GuardianTypeType.GuardianTypeOfEmail,
                    GuardianType_ = GuardianType1
                }
            });

        executionResult.TransactionResult.Error.ShouldNotBeNull();

    }
    
    [Fact]
    public async Task UnsetLoginGuardianType_GuardianTypeNull_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();


        var executionResult = await CaContractStub.UnsetGuardianTypeForLogin.SendWithExceptionAsync(
            new UnsetGuardianTypeForLoginInput
            {
                CaHash = caHash,
                GuardianType = null
            });

        executionResult.TransactionResult.Error.ShouldNotBeNull();

    }
    
    [Fact]
    public async Task UnsetLoginGuardianType_GuardianTypeEmpty_Test()
    {
        var caHash = await CreateAHolder_AndGetCash_Helper();


        var executionResult = await CaContractStub.UnsetGuardianTypeForLogin.SendWithExceptionAsync(
            new UnsetGuardianTypeForLoginInput
            {
                CaHash = caHash,
                GuardianType = new GuardianType
                {
                    Type = GuardianTypeType.GuardianTypeOfEmail,
                    GuardianType_ = ""
                }
            });

        executionResult.TransactionResult.Error.ShouldNotBeNull();

    }

    private async Task<IExecutionResult<GetHolderInfoOutput>> SetGuardianTypeForLogin_AndGetHolderInfo_Helper(Hash caHash, GuardianType guardianType)
    {
        await CaContractStub.SetGuardianTypeForLogin.SendAsync(new SetGuardianTypeForLoginInput
        {
            CaHash = caHash,
            GuardianType = guardianType ?? new GuardianType
            {
                Type = GuardianTypeType.GuardianTypeOfEmail,
                GuardianType_ = GuardianType1
            }
        });
        
        var getHolderInfoOutput = await CaContractStub.GetHolderInfo.SendAsync(new GetHolderInfoInput
        {
            CaHash = caHash,
            LoginGuardianType = ""
        });

        return getHolderInfoOutput;
    }
    
    private async Task<IExecutionResult<GetHolderInfoOutput>> UnsetGuardianTypeForLogin_AndGetHolderInfo_Helper(Hash caHash, GuardianType guardianType)
    {
        await CaContractStub.UnsetGuardianTypeForLogin.SendAsync(new UnsetGuardianTypeForLoginInput
        {
            CaHash = caHash,
            GuardianType = guardianType ?? new GuardianType
            {
                Type = GuardianTypeType.GuardianTypeOfEmail,
                GuardianType_ = GuardianType1
            }
        });
        
        var getHolderInfoOutput = await CaContractStub.GetHolderInfo.SendAsync(new GetHolderInfoInput
        {
            CaHash = caHash,
            LoginGuardianType = ""
        });

        return getHolderInfoOutput;
    }
    

    private async Task<Hash> CreateAHolder_AndGetCash_Helper()
    {
        var caHash = await CreateHolder();

        await AddAGuardian_Helper(caHash);

        return caHash;
    }

    private async Task AddAGuardian_Helper(Hash caHash)
    {
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair,VerifierAddress,verificationTime, GuardianType,0);
        var signature1 = await GenerateSignature(VerifierKeyPair1,VerifierAddress1,verificationTime, GuardianType1,0);
        var guardianApprove = new List<Guardian>
        {
            new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName,
                    Signature = signature,
                    VerifierAddress = VerifierAddress,
                    VerificationTime = verificationTime
                }
            }
        };
        var input = new AddGuardianInput
        {
            CaHash = caHash,
            GuardianToAdd = new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType1,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName1,
                    Signature = signature1,
                    VerifierAddress = VerifierAddress1,
                    VerificationTime = verificationTime
                }
            },
            GuardiansApproved = {guardianApprove}
        };
        await CaContractStub.AddGuardian.SendAsync(input);
        {
            var holderInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
            {
                CaHash = caHash
            });
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType1);
        }
    }
}