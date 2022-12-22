using System;
using System.Collections.Generic;
using AElf.Contracts.MultiToken;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract : CAContractContainer.CAContractBase
{
    public override Empty Initialize(InitializeInput input)
    {
        
        
        Assert(!State.Initialized.Value, "Already initialized.");
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
        if (input == null)
        {
            throw new AssertionException("Invalid input.");
        }
        Assert(input.GuardianApproved != null 
               && input.GuardianApproved.GuardianType != null
               && !string.IsNullOrEmpty(input.GuardianApproved.GuardianType.GuardianType_),
            "invalid input guardian type");
        Assert(input.Manager != null, "invalid input manager");
        var guardianType = input.GuardianApproved?.GuardianType;
        var holderId = State.LoginGuardianTypeMap[guardianType?.GuardianType_];
        var holderInfo = holderId != null ? State.HolderInfoMap[holderId] : new HolderInfo();

        // if CAHolder does not exist
        if (holderId == null)
        {
            Assert(State.LoginGuardianTypeLockMap[guardianType.GuardianType_] == 0,"guardianType is Locked");
            holderId = HashHelper.ConcatAndCompute(Context.TransactionId, Context.PreviousBlockHash);

            holderInfo.CreatorAddress = Context.Sender;
            holderInfo.Managers.Add(input.Manager);
            SetDelegator(holderId, input.Manager);
            
            holderInfo.GuardiansInfo = new GuardiansInfo
            {
                Guardians = {input.GuardianApproved},
                LoginGuardianTypeIndexes = {0}
            };
            holderInfo.JsonExpression = string.IsNullOrEmpty(input.JsonGuardianRules)
                ? CAContractConstants.GeneralJsonGuardianRules
                : input.JsonGuardianRules;

            State.HolderInfoMap[holderId] = holderInfo;
            State.LoginGuardianTypeMap[guardianType?.GuardianType_] = holderId;
        }

        // Log Event
        Context.Fire(new CAHolderCreated
        {
            Creator = Context.Sender,
            CaHash = holderId,
            Manager = input.Manager?.ManagerAddress,
            DeviceString = input.Manager?.DeviceString
        });

        return new Empty();
    }

    private void SetDelegator(Hash holderId, Manager manager)
    {
        var delegations = new Dictionary<string, long>
        {
            [CAContractConstants.ELFTokenSymbol] = CAContractConstants.CADelegationAmount
        };
        
        Context.SendVirtualInline(holderId, State.TokenContract.Value, "SetTransactionFeeDelegations", new SetTransactionFeeDelegationsInput
        {
            DelegatorAddress = manager.ManagerAddress,
            Delegations =
            {
                delegations
            }
        });
    }

    private void SetDelegators(Hash holderId, RepeatedField<Manager> managers)
    {
        foreach (var manager in managers)
        {
            SetDelegator(holderId, manager);
        }
    }
    
    private void RemoveDelegator(Hash holderId, Manager manager)
    {
        Context.SendVirtualInline(holderId, State.TokenContract.Value, "RemoveTransactionFeeDelegator", new RemoveTransactionFeeDelegatorInput
        {
            DelegatorAddress = manager.ManagerAddress
        });
    }

    private void RemoveDelegators(Hash holderId, RepeatedField<Manager> managers)
    {
        foreach (var manager in managers)
        {
            RemoveDelegator(holderId, manager);
        }
    }
}