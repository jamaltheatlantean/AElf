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
    private const string GuardianType2 = "test2@google.com";
    private const string VerifierName = "HuoBi";
    private const string VerifierName1 = "PortKey";
    private const string VerifierName2 = "Binance";

    private async Task<ByteString> GenerateSignature(ECKeyPair verifier, Address verifierAddress,
        Timestamp verificationTime, string guardianType, int type)
    {
        var guardianData =
            HashHelper.ConcatAndCompute(HashHelper.ComputeFrom(type), HashHelper.ComputeFrom(guardianType));
        var verifierData = HashHelper.ConcatAndCompute(HashHelper.ComputeFrom(verificationTime),
            HashHelper.ComputeFrom(verifierAddress));
        var data = HashHelper.ConcatAndCompute(guardianData, verifierData);
        var signature = CryptoHelper.SignWithPrivateKey(verifier.PrivateKey, data.ToByteArray());
        return ByteStringHelper.FromHexString(signature.ToHex());
    }

    private async Task<Hash> CreateHolder()
    {
        var verificationTime = TimestampHelper.GetUtcNow();
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
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
                    Signature = signature,
                    VerifierAddress = VerifierAddress,
                    VerificationTime = verificationTime
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
    public async Task<Hash> AddGuardianTest()
    {
        var verificationTime = TimestampHelper.GetUtcNow();
        var caHash = await CreateHolder();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
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
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(2);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType1);
        }
        return caHash;
    }

    [Fact]
    public async Task<Hash> AddGuardianTest_RepeatedGuardianType_DifferentVerifier()
    {
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
        var signature2 =
            await GenerateSignature(VerifierKeyPair2, VerifierAddress2, verificationTime, GuardianType1, 0);
        var caHash = await AddGuardianTest();
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
            },
            new Guardian
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
                    Name = VerifierName2,
                    Signature = signature2,
                    VerifierAddress = VerifierAddress2,
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
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(3);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType1);
            holderInfo.GuardiansInfo.Guardians.Last().Verifier.Name.ShouldBe(VerifierName2);
        }
        return caHash;
    }

    [Fact]
    public async Task<Hash> AddGuardianTest_Success_GuardianCount4_Approve3()
    {
        var caHash = await AddGuardian();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
        var signature2 =
            await GenerateSignature(VerifierKeyPair2, VerifierAddress2, verificationTime, GuardianType1, 0);
        var signature3 = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType2, 0);
        var signature4 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType2, 0);
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
            },
            new Guardian
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
            new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType2,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName,
                    Signature = signature3,
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
                    GuardianType_ = GuardianType2,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName1,
                    Signature = signature4,
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
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(5);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType2);
            holderInfo.GuardiansInfo.Guardians.Last().Verifier.Name.ShouldBe(VerifierName1);
        }
        return caHash;
    }

    private async Task<Hash> AddGuardian()
    {
        var caHash = await AddGuardianTest_RepeatedGuardianType_DifferentVerifier();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
        var signature2 =
            await GenerateSignature(VerifierKeyPair2, VerifierAddress2, verificationTime, GuardianType1, 0);
        var signature3 = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType2, 0);
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
            },
            new Guardian
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
            new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType1,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName2,
                    Signature = signature2,
                    VerifierAddress = VerifierAddress2,
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
                    GuardianType_ = GuardianType2,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName,
                    Signature = signature3,
                    VerifierAddress = VerifierAddress,
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
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(4);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType2);
            holderInfo.GuardiansInfo.Guardians.Last().Verifier.Name.ShouldBe(VerifierName);
        }
        return caHash;
    }

    [Fact]
    public async Task AddGuardianTest_Failed_ApproveCountNotEnough_CountLessThan4()
    {
        var caHash = await AddGuardian();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
        var signature2 =
            await GenerateSignature(VerifierKeyPair2, VerifierAddress2, verificationTime, GuardianType1, 0);
        var signature4 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType2, 0);
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
            },
            new Guardian
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
            }
        };
        var input = new AddGuardianInput
        {
            CaHash = caHash,
            GuardianToAdd = new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType2,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName1,
                    Signature = signature4,
                    VerifierAddress = VerifierAddress1,
                    VerificationTime = verificationTime
                }
            },
            GuardiansApproved = {guardianApprove}
        };
        var executionResult = await CaContractStub.AddGuardian.SendWithExceptionAsync(input);
        executionResult.TransactionResult.Error.ShouldContain("The number of approved guardians does not satisfy the rules.");
    }

    [Fact]
    public async Task AddGuardianTest_Failed_IncorrectData()
    {
        var caHash = await CreateHolder();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType1, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
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
        var executionResult = await CaContractStub.AddGuardian.SendWithExceptionAsync(input);
        executionResult.TransactionResult.Error.ShouldContain("Verification failed.");
    }

    [Fact]
    public async Task AddGuardianTest_Failed_IncorrectAddress()
    {
        var caHash = await CreateHolder();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType1, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
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
                    VerifierAddress = VerifierAddress,
                    VerificationTime = verificationTime
                }
            },
            GuardiansApproved = {guardianApprove}
        };
        var executionResult = await CaContractStub.AddGuardian.SendWithExceptionAsync(input);
        executionResult.TransactionResult.Error.ShouldContain("Verification failed.");
    }

    [Fact]
    public async Task AddGuardianTest_AlreadyExist()
    {
        var caHash = await AddGuardianTest();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType1, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
        {
            var holderInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
            {
                CaHash = caHash
            });
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(2);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType1);
        }
        {
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
                        VerifierAddress = VerifierAddress,
                        VerificationTime = verificationTime
                    }
                },
                GuardiansApproved = {guardianApprove}
            };
            await CaContractStub.AddGuardian.SendAsync(input);
        }
        {
            var holderInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
            {
                CaHash = caHash
            });
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(2);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType1);
        }
    }

    [Fact]
    public async Task AddGuardianTest_Failed_HolderNotExist()
    {
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType1, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
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
            GuardiansApproved =
            {
                guardianApprove
            }
        });
        executionResult.TransactionResult.Error.ShouldContain("CA holder does not exist.");
    }

    [Fact]
    public async Task AddGuardianTest_Failed_InvalidInput()
    {
        var caHash = await CreateHolder();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType1, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
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
        {
            var executionResult = await CaContractStub.AddGuardian.SendWithExceptionAsync(new AddGuardianInput
            {
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
                GuardiansApproved =
                {
                    guardianApprove
                }
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
        {
            var executionResult = await CaContractStub.AddGuardian.SendWithExceptionAsync(new AddGuardianInput
            {
                CaHash = caHash,
                GuardiansApproved =
                {
                    guardianApprove
                }
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
        {
            var executionResult = await CaContractStub.AddGuardian.SendWithExceptionAsync(new AddGuardianInput
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
                }
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
    }

    [Fact]
    public async Task<Hash> RemoveGuardianTest()
    {
        var caHash = await AddGuardianTest();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
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
            },
            new Guardian
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
            }
        };
        {
            var holderInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
            {
                CaHash = caHash
            });
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(2);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType1);
        }
        await CaContractStub.RemoveGuardian.SendAsync(new RemoveGuardianInput
        {
            CaHash = caHash,
            GuardianToRemove = new Guardian
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
        });
        {
            var holderInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
            {
                CaHash = caHash
            });
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(1);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType);
        }
        return caHash;
    }

    [Fact]
    public async Task RemoveGuardianTest_AlreadyRemoved()
    {
        var caHash = await RemoveGuardianTest();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType1, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
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
        {
            var holderInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
            {
                CaHash = caHash
            });
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(1);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType);
        }
        await CaContractStub.RemoveGuardian.SendAsync(new RemoveGuardianInput
        {
            CaHash = caHash,
            GuardianToRemove = new Guardian
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
        });
        {
            var holderInfo = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
            {
                CaHash = caHash
            });
            holderInfo.GuardiansInfo.Guardians.Count.ShouldBe(1);
            holderInfo.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType);
        }
    }

    [Fact]
    public async Task RemoveGuardianTest_Failed_HolderNotExist()
    {
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType1, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
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
                    VerifierAddress = VerifierAddress1,
                    VerificationTime = verificationTime
                }
            }
        };
        var executionResult = await CaContractStub.RemoveGuardian.SendWithExceptionAsync(new RemoveGuardianInput
        {
            CaHash = HashHelper.ComputeFrom("aaa"),
            GuardianToRemove = new Guardian
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
            GuardiansApproved =
            {
                guardianApprove
            }
        });
        executionResult.TransactionResult.Error.ShouldContain("CA holder does not exist.");
    }

    [Fact]
    public async Task RemoveGuardianTest_Failed_InvalidInput()
    {
        var caHash = await CreateHolder();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType1, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
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
        {
            var executionResult = await CaContractStub.RemoveGuardian.SendWithExceptionAsync(new RemoveGuardianInput
            {
                GuardianToRemove = new Guardian
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
                GuardiansApproved =
                {
                    guardianApprove
                }
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
        {
            var executionResult = await CaContractStub.RemoveGuardian.SendWithExceptionAsync(new RemoveGuardianInput
            {
                CaHash = caHash,
                GuardiansApproved =
                {
                    guardianApprove
                }
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
        {
            var executionResult = await CaContractStub.RemoveGuardian.SendWithExceptionAsync(new RemoveGuardianInput
            {
                CaHash = caHash,
                GuardianToRemove = new Guardian
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
                }
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
    }

    [Fact]
    public async Task UpdateGuardianTest()
    {
        var caHash = await AddGuardianTest();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
        var signature2 =
            await GenerateSignature(VerifierKeyPair2, VerifierAddress2, verificationTime, GuardianType1, 0);

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
            },
            new Guardian
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
            }
        };
        await CaContractStub.UpdateGuardian.SendAsync(new UpdateGuardianInput
        {
            CaHash = caHash,
            GuardianToUpdatePre = new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType1,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName1
                }
            },
            GuardianToUpdateNew = new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType1,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName2,
                    Signature = signature2,
                    VerifierAddress = VerifierAddress2,
                    VerificationTime = verificationTime
                }
            },
            GuardiansApproved = { guardianApprove }
        });
        {
            var guardian = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
            {
                CaHash = caHash
            });
            guardian.GuardiansInfo.Guardians.Count.ShouldBe(2);
            guardian.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType1);
            guardian.GuardiansInfo.Guardians.Last().Verifier.Name.ShouldBe(VerifierName2);
        }
    }

    [Fact]
    public async Task UpdateGuardianTest_AlreadyExist()
    {
        var caHash = await AddGuardianTest_RepeatedGuardianType_DifferentVerifier();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
        var signature2 =
            await GenerateSignature(VerifierKeyPair2, VerifierAddress2, verificationTime, GuardianType1, 0);

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
            },
            new Guardian
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
            new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType1,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName2,
                    Signature = signature2,
                    VerifierAddress = VerifierAddress2,
                    VerificationTime = verificationTime
                }
            }
        };
        await CaContractStub.UpdateGuardian.SendAsync(new UpdateGuardianInput
        {
            CaHash = caHash,
            GuardianToUpdatePre = new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType1,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName1
                }
            },
            GuardianToUpdateNew = new Guardian
            {
                GuardianType = new GuardianType
                {
                    GuardianType_ = GuardianType1,
                    Type = 0
                },
                Verifier = new Verifier
                {
                    Name = VerifierName2,
                    Signature = signature2,
                    VerifierAddress = VerifierAddress2,
                    VerificationTime = verificationTime
                }
            },
            GuardiansApproved = { guardianApprove }
        });
        {
            var guardian = await CaContractStub.GetHolderInfo.CallAsync(new GetHolderInfoInput
            {
                CaHash = caHash
            });
            guardian.GuardiansInfo.Guardians.Count.ShouldBe(3);
            guardian.GuardiansInfo.Guardians[1].GuardianType.GuardianType_.ShouldBe(GuardianType1);
            guardian.GuardiansInfo.Guardians[1].Verifier.Name.ShouldBe(VerifierName1);
            guardian.GuardiansInfo.Guardians.Last().GuardianType.GuardianType_.ShouldBe(GuardianType1);
            guardian.GuardiansInfo.Guardians.Last().Verifier.Name.ShouldBe(VerifierName2);
        }
    }

    [Fact]
    public async Task UpdateGuardianTest_Failed_InvalidInput()
    {
        var caHash = await AddGuardianTest();
        var verificationTime = TimestampHelper.GetUtcNow();
        var signature = await GenerateSignature(VerifierKeyPair, VerifierAddress, verificationTime, GuardianType, 0);
        var signature1 =
            await GenerateSignature(VerifierKeyPair1, VerifierAddress1, verificationTime, GuardianType1, 0);
        var signature2 =
            await GenerateSignature(VerifierKeyPair2, VerifierAddress2, verificationTime, GuardianType1, 0);

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
            },
            new Guardian
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
            }
        };
        {
            var executionResult = await CaContractStub.UpdateGuardian.SendWithExceptionAsync(new UpdateGuardianInput
            {
                GuardianToUpdatePre = new Guardian
                {
                    GuardianType = new GuardianType
                    {
                        GuardianType_ = GuardianType1,
                        Type = 0
                    },
                    Verifier = new Verifier
                    {
                        Name = VerifierName1
                    }
                },
                GuardianToUpdateNew = new Guardian
                {
                    GuardianType = new GuardianType
                    {
                        GuardianType_ = GuardianType1,
                        Type = 0
                    },
                    Verifier = new Verifier
                    {
                        Name = VerifierName2,
                        Signature = signature2,
                        VerifierAddress = VerifierAddress2,
                        VerificationTime = verificationTime
                    }
                },
                GuardiansApproved = {guardianApprove}
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
        {
            var executionResult = await CaContractStub.UpdateGuardian.SendWithExceptionAsync(new UpdateGuardianInput
            {
                CaHash = caHash,
                GuardianToUpdateNew = new Guardian
                {
                    GuardianType = new GuardianType
                    {
                        GuardianType_ = GuardianType1,
                        Type = 0
                    },
                    Verifier = new Verifier
                    {
                        Name = VerifierName2,
                        Signature = signature2,
                        VerifierAddress = VerifierAddress2,
                        VerificationTime = verificationTime
                    }
                },
                GuardiansApproved = {guardianApprove}
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
        {
            var executionResult = await CaContractStub.UpdateGuardian.SendWithExceptionAsync(new UpdateGuardianInput
            {
                CaHash = caHash,
                GuardianToUpdatePre = new Guardian
                {
                    GuardianType = new GuardianType
                    {
                        GuardianType_ = GuardianType1,
                        Type = 0
                    },
                    Verifier = new Verifier
                    {
                        Name = VerifierName1
                    }
                },
                GuardianToUpdateNew = new Guardian
                {
                    GuardianType = new GuardianType
                    {
                        GuardianType_ = GuardianType1,
                        Type = 0
                    },
                    Verifier = new Verifier
                    {
                        Name = VerifierName2,
                        Signature = signature2,
                        VerifierAddress = VerifierAddress2,
                        VerificationTime = verificationTime
                    }
                },
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
        {
            var executionResult = await CaContractStub.UpdateGuardian.SendWithExceptionAsync(new UpdateGuardianInput
            {
                CaHash = caHash,
                GuardianToUpdatePre = new Guardian
                {
                    GuardianType = new GuardianType
                    {
                        GuardianType_ = GuardianType1,
                        Type = 0
                    },
                    Verifier = new Verifier
                    {
                        Name = VerifierName1
                    }
                },
                GuardianToUpdateNew = new Guardian
                {
                    GuardianType = new GuardianType
                    {
                        GuardianType_ = GuardianType,
                        Type = 0
                    },
                    Verifier = new Verifier
                    {
                        Name = VerifierName2,
                        Signature = signature2,
                        VerifierAddress = VerifierAddress2,
                        VerificationTime = verificationTime
                    }
                },
                GuardiansApproved = {guardianApprove}
            });
            executionResult.TransactionResult.Error.ShouldContain("Inconsistent guardian type.");
        }
    }
}