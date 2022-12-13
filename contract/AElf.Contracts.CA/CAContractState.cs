using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace AElf.Contracts.CA;

public partial class CAContractState : ContractState
{
    /// <summary>
    /// Login Guardian Type -> HolderInfo Hash
    /// multiple Login Guardian Type to one HolderInfo Hash
    /// only on MainChain
    /// </summary>
    public MappedState<string, Hash> LoginGuardianTypeMap { get; set; }
    
    /// <summary>
    /// HolderInfo Hash -> HolderInfo
    /// All CA contracts
    /// </summary>
    public MappedState<Hash, HolderInfo> HolderInfoMap { get; set; }
    public SingletonState<Address> Admin { get; set; }

    /// <summary>
    ///  Verifier list
    /// only on MainChain
    /// </summary>
    public SingletonState<VerifierServerList> VerifiersServerList { get; set; }

    /// <summary>
    ///  CAServer list
    /// only on MainChain
    /// </summary>
    public SingletonState<CAServerList> CaServerList { get; set; }

}