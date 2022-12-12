using AElf.Cryptography;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    // TODO
    // Add a Guardian, if already added, return true
    public override Empty AddGuardian(AddGuardianInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "Guardian can only be added at aelf mainchain.");
        Assert(State.HolderInfoMap[input.CaHash] != null,"CA holder does not exist.");
        if (State.HolderInfoMap[input.CaHash].GuardiansInfo == null)
        {
            throw new AssertionException("No guardians under the holder.");
        }
        if (State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians.Contains(input.GuardianToAdd))
        {
            return new Empty();
        }
        //TODO:Whether the approved guardians count is satisfied.
        foreach (var guardian in input.GuardiansApproved)
        {
            Assert(State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians.Contains(guardian),
                $"Guardian does not exist in the holder.Guardian type:{guardian.GuardianType}");
            //TODO：Verify the signature.
            //CryptoHelper.RecoverPublicKey(guardian.Verifier.Signature.ToByteArray(),)
        }
        
        return new Empty();
    }

    // TODO
    // Remove a Guardian, if already removed, return true
    public override Empty RemoveGuardian(RemoveGuardianInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "Guardian can only be added at aelf mainchain.");
        Assert(State.HolderInfoMap[input.CaHash] != null,"CA holder does not exist.");
        if (State.HolderInfoMap[input.CaHash].GuardiansInfo == null)
        {
            throw new AssertionException("No guardians under the holder.");
        }
        if (!State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians.Contains(input.GuardianToRemove))
        {
            return new Empty();
        }
        //TODO:Whether the approved guardians count is satisfied.
        foreach (var guardian in input.GuardiansApproved)
        {
            Assert(State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians.Contains(guardian),
                $"Guardian does not exist in the holder.Guardian type:{guardian.GuardianType}");
            //TODO：Verify the signature.
            //CryptoHelper.RecoverPublicKey(guardian.Verifier.Signature.ToByteArray(),)
        }
        return new Empty();
    }
    
}