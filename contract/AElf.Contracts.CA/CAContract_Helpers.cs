using AElf.Types;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    private bool JsonExpressionCalculate(string jsonExpression)
    {
        return true;
    }

    private Address CalculateCaAddress(Hash virtualAddress, Address contractAddress)
    {
        return Context.ConvertVirtualAddressToContractAddress(virtualAddress, contractAddress);
    }
}