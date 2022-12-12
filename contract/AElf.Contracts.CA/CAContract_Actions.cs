using System;
using AElf.Sdk.CSharp;
using AElf.Types;
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
            "CA Protocol can only be created at aelf mainchain.");
        
        var guardianType = input.GuardianApproved.GuardianType;
        var holderId = State.LoginGuardianTypeMap[guardianType.GuardianType_];
        var holderInfo = holderId != null ? State.HolderInfoMap[holderId] : new HolderInfo();
        Address caAddress = new Address();

        // If CAHolder doesn't exist
        if (holderId == null)
        {
            holderId = HashHelper.ConcatAndCompute(Context.TransactionId, Context.PreviousBlockHash);

            holderInfo.CreatorAddress = Context.Sender;
            holderInfo.Managers.Add(input.Manager);
            caAddress = Context.ConvertVirtualAddressToContractAddress(holderId, Context.Self);
            
            State.HolderInfoMap.Set(holderId, holderInfo);
            State.LoginGuardianTypeMap.Set(guardianType.GuardianType_, holderId);
        }
        // CAHolder exists
        else
        {
            
        }

        // Log Event
        Context.Fire(new CAHolderCreated()
        {

        });

        return new Empty();
    }
}