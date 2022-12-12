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
    public Empty CreateCAHolder(CreateCAHolderInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "CA Protocol can only be created at aelf mainchain.");
        
        var guardianType = input.GuardianApproved.GuardianType;
        var holderId = State.LoginGuardianTypeMap[guardianType.GuardianType_];
        var holderInfo = holderId != null ? State.HolderInfoMap[holderId] : new HolderInfo();

        // If CAHolder doesn't exist
        if (holderId == null)
        {
            holderId = HashHelper.ConcatAndCompute(Context.TransactionId, Context.PreviousBlockHash);

            holderInfo.CreatorAddress = Context.Sender;
            holderInfo.Managers.Add(input.Manager);
            holderInfo.GuardiansInfo = new GuardiansInfo()
            {
                Guardians = { input.GuardianApproved },
                LoginGuradianTpyeIndexes = { 0 }
            };
            
            State.HolderInfoMap.Set(holderId, holderInfo);
            State.LoginGuardianTypeMap.Set(guardianType.GuardianType_, holderId);
        }

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