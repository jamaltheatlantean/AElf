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
        var data = HashHelper.ComputeFrom(guardianType);
        var signature = CryptoHelper.SignWithPrivateKey(verifier.PrivateKey, data.ToByteArray());
        return ByteStringHelper.FromHexString(signature.ToHex());
    }
    
    private async Task<Hash> CreateHolder()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
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
                ManagerAddress = User1Address,
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
                    Signature = signature1,
                    Data = GuardianType1,
                    VerifierAddress = VerifierAddress1
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
    public async Task AddGuardianTest_Failed_IncorrectData()
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
                    Signature = signature1,
                    Data = GuardianType,
                    VerifierAddress = VerifierAddress1
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
                    Signature = signature1,
                    Data = GuardianType1,
                    VerifierAddress = VerifierAddress
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
        var signature = await GenerateSignature(VerifierKeyPair, GuardianType1);
        var signature1 = await GenerateSignature(VerifierKeyPair1, GuardianType1);
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
    
    [Fact]
    public async Task AddGuardianTest_Failed_InvalidInput()
    {
        var caHash = await CreateHolder();
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
                    Signature = signature1
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
                    Signature = signature1
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
    public async Task RemoveGuardianTest_Failed_InvalidInput()
    {
        var caHash = await CreateHolder();
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
                        Signature = signature1
                    }
                }
            });
            executionResult.TransactionResult.Error.ShouldContain("Invalid input.");
        }
    }
   
}