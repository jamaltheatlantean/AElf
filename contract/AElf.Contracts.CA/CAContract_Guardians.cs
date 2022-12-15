using System.Linq;
using AElf.Sdk.CSharp;
using AElf.Types;
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
        Assert(input.CaHash != null && input.GuardianToAdd != null && input.GuardiansApproved.Count != 0,
            "Invalid input.");
        Assert(State.HolderInfoMap[input.CaHash] != null, "CA holder does not exist.");
        if (State.HolderInfoMap[input.CaHash].GuardiansInfo == null)
        {
            throw new AssertionException("No guardians under the holder.");
        }

        var toAddGuardian = State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians.FirstOrDefault(g =>
            g.GuardianType.Type == input.GuardianToAdd.GuardianType.Type &&
            g.GuardianType.GuardianType_ == input.GuardianToAdd.GuardianType.GuardianType_);
        
        if (toAddGuardian != null)
        {
            return new Empty();
        }

        //TODO:Check verifier signature.
        var verifierInfo = input.GuardianToAdd?.Verifier;
        var pubkey = Context.RecoverPublicKeyWithArgs(verifierInfo?.Signature.ToByteArray(),
            HashHelper.ComputeFrom(verifierInfo?.Data).ToByteArray());
        var verifierAddress = Address.FromPublicKey(pubkey);
        Assert(verifierAddress == verifierInfo?.VerifierAddress,"Verification failed.");

        //TODO:Whether the approved guardians count is satisfied.

        foreach (var guardian in input.GuardiansApproved)
        {
            //Whether the guardian exists in the holder info.
            Assert(
                IsGuardianExist(input.CaHash, guardian),
                $"Guardian does not exist in the holder.Guardian type:{guardian.GuardianType}");
            //TODO：Verify the signature.
            // CryptoHelper.RecoverPublicKey(guardian.Verifier.Signature.ToByteArray(), HashHelper.ComputeFrom("aaa").ToByteArray(),
            //     out var pubkey);
        }

        State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians.Add(input.GuardianToAdd);

        return new Empty();
    }

    private bool IsGuardianExist(Hash caHash, Guardian guardian)
    {
        var satisfiedGuardians = State.HolderInfoMap[caHash].GuardiansInfo.Guardians.FirstOrDefault(g =>
            g.GuardianType.GuardianType_ == guardian.GuardianType.GuardianType_ &&
            g.Verifier.Name == guardian.Verifier.Name
        );
        return satisfiedGuardians != null;
    }

    // TODO
    // Remove a Guardian, if already removed, return true
    public override Empty RemoveGuardian(RemoveGuardianInput input)
    {
        Assert(Context.ChainId == ChainHelper.ConvertBase58ToChainId("AELF"),
            "Guardian can only be removed at aelf mainchain.");
        Assert(input.CaHash != null && input.GuardianToRemove != null && input.GuardiansApproved.Count != 0,
            "Invalid input.");
        Assert(State.HolderInfoMap[input.CaHash] != null, "CA holder does not exist.");
        if (State.HolderInfoMap[input.CaHash].GuardiansInfo == null)
        {
            throw new AssertionException("No guardians under the holder.");
        }

        //Select satisfied guardian to remove.
        //Filter: guardianType.type && guardianType.guardianType && Verifier.name
        var toRemoveGuardian = State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians
            .FirstOrDefault(g =>
                g.GuardianType.Type == input.GuardianToRemove.GuardianType.Type &&
                g.GuardianType.GuardianType_ == input.GuardianToRemove.GuardianType.GuardianType_ &&
                g.Verifier.Name == input.GuardianToRemove.Verifier.Name);

        if (toRemoveGuardian == null)
        {
            return new Empty();
        }
        //TODO:Check verifier signature.

        //TODO:Whether the approved guardians count is satisfied.

        foreach (var guardian in input.GuardiansApproved)
        {
            Assert(IsGuardianExist(input.CaHash, guardian),
                $"Guardian does not exist in the holder.Guardian type:{guardian.GuardianType}");
            //TODO：Verify the signature.
            //CryptoHelper.RecoverPublicKey(guardian.Verifier.Signature.ToByteArray(),)
        }
        
        State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians.Remove(toRemoveGuardian);

        return new Empty();
    }
}