using System;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract : CAContractContainer.CAContractBase
{
    public override Empty Initialize(InitializeInput input)
    {
        Assert(!State.Initialized.Value,"Already initialized.");
        State.Admin.Value = input.ContractAdmin ?? Context.Sender;
        State.TokenContract.Value =
            Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
        State.Initialized.Value = true;
        return new Empty();
    }

    /// <summary>
    ///     The Create method can only be executed in aelf MainChain.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public override Empty CreateCAHolder(CreateCAHolderInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "CA Holder can only be created at aelf mainchain.");
        Assert(input != null, "invalid input");
        Assert(input.GuardianApproved  != null && input.GuardianApproved.GuardianType != null 
               && !String.IsNullOrEmpty(input.GuardianApproved.GuardianType.GuardianType_), 
            "invalid input guardian type");
        Assert(input.Manager != null, "invalid input manager");
        var guardianType = input.GuardianApproved.GuardianType;
        var holderId = State.LoginGuardianTypeMap[guardianType.GuardianType_];
        var holderInfo = holderId != null ? State.HolderInfoMap[holderId] : new HolderInfo();
        
        string json = "{\"opr\":\"?:\", \"left\":{\"opr\":\"?:\"}}";
        JsonExpressionCalculate(json);
        
        // if CAHolder does not exist
        if (holderId == null)
        {
            holderId = HashHelper.ConcatAndCompute(Context.TransactionId, Context.PreviousBlockHash);

            holderInfo.CreatorAddress = Context.Sender;
            holderInfo.Managers.Add(input.Manager);
            holderInfo.GuardiansInfo = new GuardiansInfo
            {
                Guardians = { input.GuardianApproved },
                LoginGuardianTypeIndexes = { 0 }
            };
            
            State.HolderInfoMap[holderId] = holderInfo;
            State.LoginGuardianTypeMap[guardianType.GuardianType_] = holderId;
        }
        // Log Event
        Context.Fire(new CAHolderCreated
        {
            Creator = Context.Sender,
            CaHash = holderId,
            Manager = input.Manager.ManagerAddresses,
            DeviceString = input.Manager.DeviceString
        });

        return new Empty();
    }
}