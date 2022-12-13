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
        Assert(input == null);
        
        Assert(input.LoginGuardianType == null || string.IsNullOrEmpty(input.LoginGuardianType.GuardianType_));
        var loginGuardianType = input.LoginGuardianType;
        var caHash = State.LoginGuardianTypeMap[loginGuardianType.GuardianType_];

        if (caHash == null)
        {
            throw new AssertionException("CA Holder does not exist.");
        } 
        var holder = State.HolderInfoMap[caHash];
        var guardians = holder.GuardiansInfo.Guardians;

        // TODO Verify guardians with input.GuardiansApproved.
        if (!true)
        {
            throw new AssertionException("Verification error");
        }
            
        // Manager doesn't exist
        if (holder.Managers.Contains(input.Manager))
        {
            return new Empty();
        }
        State.HolderInfoMap[caHash].Managers.Add(input.Manager);

        Context.Fire(new ManagerSocialRecovered()
        {
            CaHash = caHash,
            Manager = input.Manager.ManagerAddresses,
            DeviceString = input.Manager.DeviceString
        });
        
        return new Empty();
    }

    public override Empty AddManager(AddManagerInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "Manager can only be added at aelf mainchain.");
        Assert(input == null);
        Assert(State.HolderInfoMap[input.CaHash] != null, "CA Holder does not exist");

        // Manager already exists
        if (State.HolderInfoMap[input.CaHash].Managers.Contains(input.Manager))
        {
            return new Empty();
        }
        
        State.HolderInfoMap[input.CaHash].Managers.Add(input.Manager);
        return new Empty();
    }

    public override Empty RemoveManager(RemoveManagerInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "Manager can only be removed at aelf mainchain.");
        Assert(input == null);
        Assert(State.HolderInfoMap[input.CaHash] != null, "CA Holder does not exist");
        
        // Manager does not exist
        if (!State.HolderInfoMap[input.CaHash].Managers.Contains(input.Manager))
        {
            return new Empty();
        }

        State.HolderInfoMap[input.CaHash].Managers.Remove(input.Manager);
        return new Empty();
    }
    
    public override Empty ManagerForwardCall(ManagerForwardCallInput input)
    {
        Assert(input.CaHash != null,"CA hash is null.");
        CheckManagerPermission(input.CaHash, Context.Sender);
        Context.SendVirtualInline(input.CaHash,input.ContractAddress,input.MethodName,input.Args);
        return new Empty();
    }

    public override Empty ManagerTransfer(ManagerTransferInput input)
    {
        Assert(input.CaHash != null, "CA hash is null.");
        CheckManagerPermission(input.CaHash, Context.Sender);
        Context.SendVirtualInline(input.CaHash, State.TokenContract.Value, nameof(State.TokenContract.Transfer),
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
        Context.SendVirtualInline(input.CaHash, State.TokenContract.Value, nameof(State.TokenContract.TransferFrom),
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
        Assert(State.HolderInfoMap[caHash] != null,"Invalid CA hash.");
        var managerList = State.HolderInfoMap[caHash].Managers.Select(manager => manager.ManagerAddresses).ToList();
        Assert(managerList.Contains(managerAddress),"No permission.");
    }
}