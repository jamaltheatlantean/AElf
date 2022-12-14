using System.Threading.Tasks;
using Google.Protobuf;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CA;

public partial class CAContractTests : CAContractTestBase
{
    [Fact]
    public async Task AddVerifierServerEndPointsTest()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        await CaContractStub.AddVerifierServerEndPoints.SendAsync(new AddVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });
    }

    [Fact]
    public async Task AddVerifierServerEndPointsTest_Failed_NotAdmin()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = User1Address
        });
        var result = await CaContractStub.AddVerifierServerEndPoints.SendWithExceptionAsync(new AddVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });
        result.TransactionResult.Error.ShouldContain("Only Admin has permission to add VerifierServerEndPoints");
    }

    [Fact]
    public async Task AddVerifierServerEndPointsTest_Succeed()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        await CaContractStub.AddVerifierServerEndPoints.SendAsync(new AddVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });

        var inputWithSameName = new AddVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.2" }
        };
        await CaContractStub.AddVerifierServerEndPoints.SendAsync(inputWithSameName);

        var inputWithSameNameAndEndPoints = new AddVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        };
        await CaContractStub.AddVerifierServerEndPoints.SendAsync(inputWithSameNameAndEndPoints);
    }

    [Fact]
    public async Task AddVerifierServerEndPointsTest_Failed_InvalidInput()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        await CaContractStub.AddVerifierServerEndPoints.SendAsync(new AddVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });
        
        var inputWithNameNull = new AddVerifierServerEndPointsInput
        {
            EndPoints = { "127.0.0.2" }
        };
        var result = await CaContractStub.AddVerifierServerEndPoints.SendWithExceptionAsync(inputWithNameNull);
        result.TransactionResult.Error.ShouldContain("invalid input name");
        
        var inputWithEndPointsNull = new AddVerifierServerEndPointsInput
        {
            Name = "test1"
        };
        result = await CaContractStub.AddVerifierServerEndPoints.SendWithExceptionAsync(inputWithEndPointsNull);
        result.TransactionResult.Error.ShouldContain("invalid input EndPoints");
    }
    
    [Fact]
    public async Task RemoveVerifierServerEndPointsTest_Failed_NotAdmin()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = User1Address
        });
        var result = await CaContractStub.RemoveVerifierServerEndPoints.SendWithExceptionAsync(new RemoveVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });
        result.TransactionResult.Error.ShouldContain("Only Admin has permission to remove VerifierServerEndPoints");
    }

    [Fact]
    public async Task RemoveVerifierServerEndPointsTest_Succeed()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        await CaContractStub.RemoveVerifierServerEndPoints.SendAsync(new RemoveVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1", "127.0.0.2" }
        });
        
        var input = new RemoveVerifierServerEndPointsInput()
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        };
        await CaContractStub.RemoveVerifierServerEndPoints.SendAsync(input);
        
        var inputWithNameNotExist = new RemoveVerifierServerEndPointsInput
        {
            Name = "test1",
            EndPoints = { "127.0.0.2" }
        };
        await CaContractStub.RemoveVerifierServerEndPoints.SendAsync(inputWithNameNotExist);

        var inputWithEndPointsNotExist = new RemoveVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        };
        await CaContractStub.RemoveVerifierServerEndPoints.SendAsync(inputWithEndPointsNotExist);
    }

    [Fact]
    public async Task RemoveVerifierServerEndPointsTest_Failed_InvalidInput()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        await CaContractStub.RemoveVerifierServerEndPoints.SendAsync(new RemoveVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });
        
        var inputWithNameNull = new RemoveVerifierServerEndPointsInput
        {
            EndPoints = { "127.0.0.1" }
        };
        var result = await CaContractStub.RemoveVerifierServerEndPoints.SendWithExceptionAsync(inputWithNameNull);
        result.TransactionResult.Error.ShouldContain("invalid input name");
        
        var inputWithEndPointsNull = new RemoveVerifierServerEndPointsInput
        {
            Name = "test1"
        };
        result = await CaContractStub.RemoveVerifierServerEndPoints.SendWithExceptionAsync(inputWithEndPointsNull);
        result.TransactionResult.Error.ShouldContain("invalid input EndPoints");
    }
    
    [Fact]
    public async Task RemoveVerifierServerTest_Succeed()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        await CaContractStub.AddVerifierServerEndPoints.SendAsync(new AddVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });

        var input = new RemoveVerifierServerInput
        {
            Name = "test"
        };
        await CaContractStub.RemoveVerifierServer.SendAsync(input);

        var inputWithNameNotExist = new RemoveVerifierServerInput
        {
            Name = "test1"
        };
        await CaContractStub.RemoveVerifierServer.SendAsync(inputWithNameNotExist);
    }
    
    [Fact]
    public async Task RemoveVerifierServerTest_Failed_NotAdmin()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = User1Address
        });
        var result = await CaContractStub.RemoveVerifierServer.SendWithExceptionAsync(new RemoveVerifierServerInput
        {
            Name = "test"
        });
        result.TransactionResult.Error.ShouldContain("Only Admin has permission to remove VerifierServer");
    }
    
    [Fact]
    public async Task RemoveVerifierServerTest_Failed_InvalidInput()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        await CaContractStub.RemoveVerifierServerEndPoints.SendAsync(new RemoveVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });
        
        var inputWithNameNull = new RemoveVerifierServerEndPointsInput();
        var result = await CaContractStub.RemoveVerifierServerEndPoints.SendWithExceptionAsync(inputWithNameNull);
        result.TransactionResult.Error.ShouldContain("invalid input name");
    }
    
    [Fact]
    public async Task GetVerifierServersTest()
    {
        await CaContractStub.Initialize.SendAsync(new InitializeInput
        {
            ContractAdmin = DefaultAddress
        });
        await CaContractStub.AddVerifierServerEndPoints.SendAsync(new AddVerifierServerEndPointsInput
        {
            Name = "test",
            EndPoints = { "127.0.0.1" }
        });

        var result = CaContractStub.GetVerifierServers.CallAsync(new GetVerifierServersInput());
        result.Result.VerifierServers[0].Name.ShouldBe("test");
        result.Result.VerifierServers[0].EndPoints.ShouldContain("127.0.0.1");
    }
}