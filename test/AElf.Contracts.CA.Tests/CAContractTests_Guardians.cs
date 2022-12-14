using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Cryptography;
using AElf.Cryptography.ECDSA;
using AElf.Kernel;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CA;

public partial class CAContractTests
{
    private const string GuardianType = "test@google.com";
    private const string GuardianType1 = "test1@google.com";
    private const string VerifierName = "HuoBi";
    private const string VerifierName1 = "PortKey";

    private async Task<ByteString> GenerateSignature(ECKeyPair verifier,string guardianType)
    {
        var data = HashHelper.ConcatAndCompute(HashHelper.ComputeFrom(guardianType),
            HashHelper.ComputeFrom(TimestampHelper.GetUtcNow()));
        var signature = CryptoHelper.SignWithPrivateKey(verifier.PrivateKey, data.ToByteArray());
        return ByteStringHelper.FromHexString(signature.ToHex());
    }
    
    private async Task<Hash> CreateHolder()
    {
        var signature = await GenerateSignature(VerifierKeyPair,GuardianType);
        await CaContractStub.CreateCAHolder.SendAsync(new CreateCAHolderInput
        {
            GuardianApproved = new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName,
                    Signature = signature
                }
            },
            Manager = new Manager
            {
                ManagerAddresses = User1Address,
                DeviceString = "123"
            }
        });
        var holderInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
        {
            LoginGuardianType = GuardianType
        });
        holderInfo.GuardiansInfo.Guardians.First().GuardianType.GuardianType_.ShouldBe(GuardianType);
        return holderInfo.CaHash;
    }

    [Fact]
    public async Task AddGuardianTest()
    {
        var caHash = await CreateHolder();
        var signature = await GenerateSignature(VerifierKeyPair, GuardianType1);
        var signature1 = await GenerateSignature(VerifierKeyPair1, GuardianType1);
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
                    Signature = signature
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
                    Signature = signature1
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

    [Fact]
    public async Task AddGuardianTest_Failed_HolderNotExist()
    {
        var signature = await GenerateSignature(VerifierKeyPair,GuardianType1);
        var signature1 = await GenerateSignature(VerifierKeyPair1,GuardianType1);
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
                    Signature = signature
                }
            }
        };
        var executionResult = await CaContractStub.AddGuardian.SendWithExceptionAsync(new AddGuardianInput
        {
            CaHash = HashHelper.ComputeFrom("aaa"),
            GuardianToAdd =new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType1,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName1,
                    Signature = signature1
                }
            },
            GuardiansApproved =
            {
                guardianApprove
            }
        });
        executionResult.TransactionResult.Error.ShouldContain("CA holder does not exist.");
    }
}