using System.Linq;
using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    // Add a guardian, if already added, return 
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

        var holderInfo = State.HolderInfoMap[input.CaHash];
        
        //Whether the guardian type to be added has already in the holder info.
        //Filter: guardianType.type && guardianType.guardianType && Verifier.name
        var toAddGuardian = holderInfo.GuardiansInfo.Guardians.FirstOrDefault(g =>
            g.GuardianType.Type == input.GuardianToAdd.GuardianType.Type &&
            g.GuardianType.GuardianType_ == input.GuardianToAdd.GuardianType.GuardianType_ &&
            g.Verifier.Name == input.GuardianToAdd.Verifier.Name);

        if (toAddGuardian != null)
        {
            return new Empty();
        }

        //Check the verifier signature and data of the guardian to be added.
        CheckVerifierSignatureAndData(input.GuardianToAdd);

        //Whether the approved guardians count is satisfied.
        /*Assert(AreRulesSatisfied(holderInfo.GuardiansInfo.Guardians.Count, input.GuardiansApproved.Count,
            holderInfo.JsonExpression), "The number of approved guardians does not satisfy the rules.");*/

        foreach (var guardian in input.GuardiansApproved)
        {
            //Whether the guardian exists in the holder info.
            Assert(
                IsGuardianExist(input.CaHash, guardian),
                $"Guardian does not exist in the holder.Guardian type:{guardian.GuardianType}");
            //Check the verifier signature and data of the guardian to be approved.
            CheckVerifierSignatureAndData(guardian);
        }

        State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians.Add(input.GuardianToAdd);

        Context.Fire(new GuardianAdded
        {
            CaHash = input.CaHash,
            CaAddress = CalculateCaAddress(input.CaHash,Context.Self),
            GuardianAdded_ = input.GuardianToAdd
        });
        return new Empty();
    }

    // Remove a Guardian, if already removed, return 
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

        var holderInfo = State.HolderInfoMap[input.CaHash];
        //Select satisfied guardian to remove.
        //Filter: guardianType.type && guardianType.guardianType && Verifier.name
        var toRemoveGuardian = holderInfo.GuardiansInfo.Guardians
            .FirstOrDefault(g =>
                g.GuardianType.Type == input.GuardianToRemove.GuardianType.Type &&
                g.GuardianType.GuardianType_ == input.GuardianToRemove.GuardianType.GuardianType_ &&
                g.Verifier.Name == input.GuardianToRemove.Verifier.Name);

        if (toRemoveGuardian == null)
        {
            return new Empty();
        }

        //Check the verifier signature and data of the guardian to be removed.
        CheckVerifierSignatureAndData(toRemoveGuardian);

        //Whether the approved guardians count is satisfied.
        /*Assert(AreRulesSatisfied(holderInfo.GuardiansInfo.Guardians.Count, input.GuardiansApproved.Count,
            holderInfo.JsonExpression), "The number of approved guardians does not satisfy the rules.");*/

        foreach (var guardian in input.GuardiansApproved)
        {
            Assert(IsGuardianExist(input.CaHash, guardian),
                $"Guardian does not exist in the holder.Guardian type:{guardian.GuardianType}");
            //Check the verifier signature and data of the guardian to be approved.
            CheckVerifierSignatureAndData(guardian);
        }

        State.HolderInfoMap[input.CaHash].GuardiansInfo.Guardians.Remove(toRemoveGuardian);
        Context.Fire(new GuardianRemoved
        {
            CaHash = input.CaHash,
            CaAddress = CalculateCaAddress(input.CaHash,Context.Self),
            GuardianRemoved_ = toRemoveGuardian
        });

        return new Empty();
    }

    public override Empty UpdateGuardian(UpdateGuardianInput input)
    {
        Assert(input.CaHash != null && input.GuardianToUpdatePre != null
                                    && input.GuardianToUpdateNew != null && input.GuardiansApproved.Count != 0,
            "Invalid input.");
        Assert(State.HolderInfoMap[input.CaHash] != null, "CA holder does not exist.");
        Assert(input.GuardianToUpdatePre?.GuardianType.Type == input.GuardianToUpdateNew?.GuardianType.Type &&
               input.GuardianToUpdatePre?.GuardianType.GuardianType_ ==
               input.GuardianToUpdateNew?.GuardianType.GuardianType_, "Inconsistent guardian type.");
        if (State.HolderInfoMap[input.CaHash].GuardiansInfo == null)
        {
            throw new AssertionException("No guardians under the holder.");
        }

        var holderInfo = State.HolderInfoMap[input.CaHash];

        //Whether the guardian type to be updated in the holder info.
        //Filter: guardianType.type && guardianType.guardianType && Verifier.name
        var existPreGuardian = holderInfo.GuardiansInfo.Guardians.FirstOrDefault(g =>
            g.GuardianType.Type == input.GuardianToUpdatePre.GuardianType.Type &&
            g.GuardianType.GuardianType_ == input.GuardianToUpdatePre.GuardianType.GuardianType_ &&
            g.Verifier.Name == input.GuardianToUpdatePre.Verifier.Name);

        var toUpdateGuardian = holderInfo.GuardiansInfo.Guardians.FirstOrDefault(g =>
            g.GuardianType.Type == input.GuardianToUpdateNew.GuardianType.Type &&
            g.GuardianType.GuardianType_ == input.GuardianToUpdateNew.GuardianType.GuardianType_ &&
            g.Verifier.Name == input.GuardianToUpdateNew.Verifier.Name);

        if (existPreGuardian == null || toUpdateGuardian != null)
        {
            return new Empty();
        }

        //Check the verifier signature and data of the guardian to be removed.
        CheckVerifierSignatureAndData(input.GuardianToUpdateNew);

        //Whether the approved guardians count is satisfied.
        /*Assert(AreRulesSatisfied(holderInfo.GuardiansInfo.Guardians.Count, input.GuardiansApproved.Count,
            holderInfo.JsonExpression), "The number of approved guardians does not satisfy the rules.");*/

        foreach (var guardian in input.GuardiansApproved)
        {
            //Whether the guardian exists in the holder info.
            Assert(
                IsGuardianExist(input.CaHash, guardian),
                $"Guardian does not exist in the holder.Guardian type:{guardian.GuardianType}");
            //Check the verifier signature and data of the guardian to be approved.
            CheckVerifierSignatureAndData(guardian);
        }

        existPreGuardian.Verifier = input.GuardianToUpdateNew?.Verifier;
        
        Context.Fire(new GuardianUpdated
        {
            CaHash = input.CaHash,
            CaAddress = CalculateCaAddress(input.CaHash,Context.Self),
            GuardianUpdatedPre = existPreGuardian,
            GuardianUpdatedNew = input.GuardianToUpdateNew
        });

        return new Empty();
    }

    private void CheckVerifierSignatureAndData(Guardian guardian)
    {
        var verifier = guardian.Verifier;
        var guardianTypeData = HashHelper.ConcatAndCompute(HashHelper.ComputeFrom((int) guardian.GuardianType.Type),
            HashHelper.ComputeFrom(guardian.GuardianType.GuardianType_));
        var verifierData = HashHelper.ConcatAndCompute(HashHelper.ComputeFrom(guardian.Verifier.VerificationTime),
            HashHelper.ComputeFrom(guardian.Verifier.VerifierAddress));
        var data = HashHelper.ConcatAndCompute(guardianTypeData, verifierData);
        var publicKey = Context.RecoverPublicKey(verifier.Signature.ToByteArray(),
            data.ToByteArray());
        var verifierAddress = Address.FromPublicKey(publicKey);
        Assert( verifierAddress == verifier.VerifierAddress, "Verification failed.");
        Assert(guardian.Verifier.VerificationTime.AddMinutes(10) >= Context.CurrentBlockTime,
            "Verifier signature has expired.");
    }

    private bool IsGuardianExist(Hash caHash, Guardian guardian)
    {
        var satisfiedGuardians = State.HolderInfoMap[caHash].GuardiansInfo.Guardians.FirstOrDefault(
            g =>
            g.GuardianType.GuardianType_ == guardian.GuardianType.GuardianType_ &&
            g.Verifier.Name == guardian.Verifier.Name
        );
        return satisfiedGuardians != null;
    }
}