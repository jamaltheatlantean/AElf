using AElf.Contracts.MultiToken;
using AElf.Contracts.Parliament;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Cryptography.ECDSA;
using AElf.Types;

namespace AElf.Contracts.CA;

public class CAContractTestBase : ContractTestBase<CAContractTestAElfModule>
{
    internal ParliamentContractImplContainer.ParliamentContractImplStub ParliamentContractStub;
    internal CAContractContainer.CAContractStub CaContractStub { get; set; }
    internal TokenContractContainer.TokenContractStub TokenContractStub { get; set; }
    
    protected ECKeyPair DefaultKeyPair => Accounts[0].KeyPair;
    protected Address DefaultAddress => Accounts[0].Address;
    protected ECKeyPair User1KeyPair => Accounts[1].KeyPair;
    protected ECKeyPair User2KeyPair => Accounts[2].KeyPair;
    protected ECKeyPair User3KeyPair => Accounts[3].KeyPair;
    protected Address User1Address => Accounts[1].Address;
    protected Address User2Address => Accounts[2].Address;
    protected Address User3Address => Accounts[3].Address;

    protected Hash CaContractName => HashHelper.ComputeFrom("AElf.ContractNames.CA");
    protected Address CaContractAddress { get; set; }
    
    
    public CAContractTestBase()
    {
        CaContractAddress = SystemContractAddresses[CaContractName];
        CaContractStub = GetCaContractTester(DefaultKeyPair);
        ParliamentContractStub = GetParliamentContractTester(DefaultKeyPair);
        TokenContractStub = GetTokenContractTester(DefaultKeyPair);
    }

    
    internal CAContractContainer.CAContractStub GetCaContractTester(ECKeyPair keyPair)
    {
        return GetTester<CAContractContainer.CAContractStub>(CaContractAddress,
            keyPair);
    }
    internal ParliamentContractImplContainer.ParliamentContractImplStub GetParliamentContractTester(
        ECKeyPair keyPair)
    {
        return GetTester<ParliamentContractImplContainer.ParliamentContractImplStub>(ParliamentContractAddress,
            keyPair);
    }
    internal TokenContractContainer.TokenContractStub GetTokenContractTester(
        ECKeyPair keyPair)
    {
        return GetTester<TokenContractContainer.TokenContractStub>(TokenContractAddress,
            keyPair);
    }
    
}