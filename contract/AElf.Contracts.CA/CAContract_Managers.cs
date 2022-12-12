using System.Xml;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.Crmf;
using Virgil.Crypto.Pfs;

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
        Assert(input != null, "input should not be null");
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
        Assert(input != null, "input should not be null");
        Assert(State.HolderInfoMap[input.CaHash] != null, "CA Holder does not exist");
        
        // Manager does not exist
        if (!State.HolderInfoMap[input.CaHash].Managers.Contains(input.Manager))
        {
            return new Empty();
        }

        State.HolderInfoMap[input.CaHash].Managers.Remove(input.Manager);
        return new Empty();
    }
}