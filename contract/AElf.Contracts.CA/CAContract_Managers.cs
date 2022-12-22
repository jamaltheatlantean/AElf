using System;
using System.Linq;
using AElf.Contracts.MultiToken;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    // For SocialRecovery
    public override Empty SocialRecovery(SocialRecoveryInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "Social Recovery can only be acted at aelf mainchain.");
        Assert(input != null, "invalid input");
        
        Assert(input.LoginGuardianType != null && !string.IsNullOrEmpty(input.LoginGuardianType.GuardianType_), 
            "invalid input login guardian type");
        var loginGuardianType = input.LoginGuardianType;
        var caHash = State.LoginGuardianTypeMap[loginGuardianType.GuardianType_];

        if (caHash == null)
        {
            throw new AssertionException("CA Holder does not exist.");
        } 
        var holder = State.HolderInfoMap[caHash];
        var guardians = holder.GuardiansInfo.Guardians;
        
        Assert(input.GuardiansApproved != null, "invalid input Guardians Approved");
        var count = 0;
        foreach (var guardian in input.GuardiansApproved)
        {
            if (guardians.Contains(guardian))
            {
                count++;
            }
        }
        
        // Assert(IsRuleSatisfied(guardians.Count, count, holder.JsonExpression), 
        //     "invalid input Guardians Approved");

        // Manager does not exists
        if (!holder.Managers.Contains(input.Manager))
        {
            State.HolderInfoMap[caHash].Managers.Add(input.Manager);
            SetDelegator(caHash, input.Manager);
        }
        
        Context.Fire(new ManagerSocialRecovered()
        {
            CaHash = caHash,
            Manager = input.Manager.ManagerAddress,
            DeviceString = input.Manager.DeviceString
        });
        
        return new Empty();
    }

    public override Empty AddManager(AddManagerInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "Manager can only be added at aelf mainchain.");
        Assert(input != null, "invalid input");
        CheckManagerInput(input.CaHash, input.Manager);

        // Manager does not exists
        if (!State.HolderInfoMap[input.CaHash].Managers.Contains(input.Manager))
        {
            State.HolderInfoMap[input.CaHash].Managers.Add(input.Manager);
            SetDelegator(input.CaHash, input.Manager);
        }
        
        Context.Fire(new ManagerAdded
        {
            CaHash = input.CaHash,
            Manager = input.Manager.ManagerAddress,
            DeviceString = input.Manager.DeviceString
        });
        
        return new Empty();
    }

    public override Empty RemoveManager(RemoveManagerInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "Manager can only be removed at aelf mainchain.");
        Assert(input != null, "invalid input");
        CheckManagerInput(input.CaHash, input.Manager);

        // Manager exists
        if (State.HolderInfoMap[input.CaHash].Managers.Contains(input.Manager))
        {
            Assert(Context.Sender.Equals(input.Manager.ManagerAddress), "No permission to remove");
            State.HolderInfoMap[input.CaHash].Managers.Remove(input.Manager);
            RemoveDelegator(input.CaHash, input.Manager);
        }
        
        Context.Fire(new ManagerRemoved
        {
            CaHash = input.CaHash,
            Manager = input.Manager.ManagerAddress,
            DeviceString = input.Manager.DeviceString
        });

        return new Empty();
    }

    private void CheckManagerInput(Hash hash, Manager manager)
    {
        Assert(hash != null, "invalid input CaHash");
        CheckManagerPermission(hash, Context.Sender);
        Assert(manager != null, "invalid input manager");
        Assert(!String.IsNullOrEmpty(manager.DeviceString) && manager.ManagerAddress != null, "invalid input manager");
    }
    
    public override Empty ManagerForwardCall(ManagerForwardCallInput input)
    {
        Assert(input.CaHash != null, "CA hash is null.");
        Assert(input.ContractAddress != null && !String.IsNullOrEmpty(input.MethodName) && !input.Args.IsEmpty,
            "Invalid input.");
        CheckManagerPermission(input.CaHash, Context.Sender);
        Context.SendVirtualInline(input.CaHash, input.ContractAddress, input.MethodName, input.Args);
        return new Empty();
    }

    public override Empty ManagerTransfer(ManagerTransferInput input)
    {
        Assert(input.CaHash != null, "CA hash is null.");
        CheckManagerPermission(input.CaHash, Context.Sender);
        Assert(input.To != null && !String.IsNullOrEmpty(input.Symbol), "Invalid input.");
        Context.SendVirtualInline(input.CaHash, State.TokenContract.Value,
            nameof(State.TokenContract.Transfer),
            new TransferInput
            {
                To = input.To,
                Amount = input.Amount,
                Symbol = input.Symbol,
                Memo = input.Memo
            }.ToByteString());
        return new Empty();
    }

    public override Empty ManagerTransferFrom(ManagerTransferFromInput input)
    {
        Assert(input.CaHash != null, "CA hash is null.");
        CheckManagerPermission(input.CaHash, Context.Sender);
        Assert(input.From != null && input.To != null && !String.IsNullOrEmpty(input.Symbol), 
            "Invalid input.");
        Context.SendVirtualInline(input.CaHash, State.TokenContract.Value, 
            nameof(State.TokenContract.TransferFrom),
            new TransferFromInput
            {
                From = input.From,
                To = input.To,
                Amount = input.Amount,
                Symbol = input.Symbol,
                Memo = input.Memo
            }.ToByteString());
        return new Empty();
    }

    private void CheckManagerPermission(Hash caHash, Address managerAddress)
    {
        Assert(State.HolderInfoMap[caHash] != null, $"CA holder is null.CA hash:{caHash}");
        var managerList = State.HolderInfoMap[caHash].Managers.Select(manager => manager.ManagerAddress).ToList();
        Assert(managerList.Contains(managerAddress), "No permission.");
    }
}