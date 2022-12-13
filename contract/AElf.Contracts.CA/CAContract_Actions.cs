using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract : CAContractContainer.CAContractBase
{
    /// <summary>
    ///     The Create method can only be executed in aelf MainChain.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public override Empty CreateCAHolder(CreateCAHolderInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "CA Holder can only be created at aelf mainchain.");
        Assert(input != null,"Invalid input.");
        Assert(input.GuardianApproved.GuardianType.GuardianType_ != null, "GuardianType should not be null");
        var guardianType = input.GuardianApproved.GuardianType;
        var holderId = State.LoginGuardianTypeMap[guardianType.GuardianType_];
        var holderInfo = holderId != null ? State.HolderInfoMap[holderId] : new HolderInfo();

        // If CAHolder doesn't exist
        if (holderId != null)
        {
            return new Empty();
        }
        holderId = HashHelper.ConcatAndCompute(Context.TransactionId, Context.PreviousBlockHash);

        holderInfo.CreatorAddress = Context.Sender;
        holderInfo.Managers.Add(input.Manager);
        holderInfo.GuardiansInfo = new GuardiansInfo()
        {
            Guardians = { input.GuardianApproved },
            LoginGuardianTypeIndexes = { 0 }
        };
            
        State.HolderInfoMap.Set(holderId, holderInfo);
        State.LoginGuardianTypeMap.Set(guardianType.GuardianType_, holderId);

        // Log Event
        Context.Fire(new CAHolderCreated()
        {
            Creator = Context.Sender,
            CaHash = holderId,
            Manager = input.Manager.ManagerAddresses,
            DeviceString = input.Manager.DeviceString
        });

        return new Empty();
    }
}