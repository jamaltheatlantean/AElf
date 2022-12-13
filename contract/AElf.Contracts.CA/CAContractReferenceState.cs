using AElf.Contracts.MultiToken;

namespace AElf.Contracts.CA;

public partial class CAContractState
{
    internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }
}