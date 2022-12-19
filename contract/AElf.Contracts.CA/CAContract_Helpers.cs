using System.Collections.Generic;
using AElf.Types;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    private Address CalculateCaAddress(Hash virtualAddress, Address contractAddress)
    {
        return Context.ConvertVirtualAddressToContractAddress(virtualAddress, contractAddress);
    }
}