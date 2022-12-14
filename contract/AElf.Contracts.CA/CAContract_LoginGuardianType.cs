using System;
using System.Runtime.CompilerServices;
using AElf.Types;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Virgil.CryptoAPI;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    // Set a GuardianType for login, if already set, return ture
    public override Empty SetGuardianTypeForLogin(SetGuardianTypeForLoginInput input)
    {
        Assert(input != null);
        Assert(input.CaHash != null); 
        // GuardianType should be valid, not null, and be with non-null GuardianType_
        Assert(input.GuardianType != null); 
        Assert(!String.IsNullOrEmpty(input.GuardianType.GuardianType_));
        
        HolderInfo holderInfo = State.HolderInfoMap[input.CaHash];
        string loginGuardianType = input.GuardianType.GuardianType_;

        var isOccupied = CheckLoginGuardianIsNotOccupied(loginGuardianType, input.CaHash);
        
        Assert(isOccupied != CAContractConstants.LoginGuardianTypeIsOccupiedByOthers, 
            $"The login guardian type --{loginGuardianType}-- is occupied by others!");

        // for idempotent
        if (isOccupied == CAContractConstants.LoginGuardianTypeIsYours)
        {
            return new Empty();
        }
        
        Assert(isOccupied == CAContractConstants.LoginGuardianTypeIsNotOccupied,
            "Internal error, how can it be?");
        
        FindGuardianTypeAndSet(holderInfo.GuardiansInfo, input.GuardianType);

        return new Empty();
    }

    // Unset a GuardianType for login, if already unset, return ture
    public override Empty UnsetGuardianTypeForLogin(UnsetGuardianTypeForLoginInput input)
    {
        Assert(input != null);
        Assert(input.CaHash != null); 
        // GuardianType should be valid, not null, and be with non-null GuardianType_
        Assert(input.GuardianType != null);
        Assert(!String.IsNullOrEmpty(input.GuardianType.GuardianType_));
        
        HolderInfo holderInfo = State.HolderInfoMap[input.CaHash];
        string loginGuardianType = input.GuardianType.GuardianType_;

        // Try to find the index of the GuardianType
        var guardians = holderInfo.GuardiansInfo.Guardians;
        var index = FindGuardianType(guardians, input.GuardianType);

        // not found, quit to be idempotent
        if (index >= guardians.Count)
        {
            return new Empty();
        }

        // Remove index from LoginGuradianTpyeIndexes set.
        holderInfo.GuardiansInfo.LoginGuardianTypeIndexes.Remove(index);

        return new Empty();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CheckLoginGuardianIsNotOccupied(string loginGuardianType, Hash caHash)
    {
        Hash result = State.LoginGuardianTypeMap[loginGuardianType];
        if (result == null)
        {
            return CAContractConstants.LoginGuardianTypeIsNotOccupied;
        }

        return result == caHash
            ? CAContractConstants.LoginGuardianTypeIsYours
            : CAContractConstants.LoginGuardianTypeIsOccupiedByOthers;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FindGuardianTypeAndSet(GuardiansInfo guardiansInfo, GuardianType guardianType)
    {
        var guardians = guardiansInfo.Guardians;

        var index = FindGuardianType(guardians, guardianType);
        
        // if index == guardians.Count, shows that it is not found and be out of bounds.
        if (index < guardians.Count)
        {
            // Add the index in array.
            // To be idempotent.
            if (!guardiansInfo.LoginGuardianTypeIndexes.Contains(index))
            {
                guardiansInfo.LoginGuardianTypeIndexes.Add(index);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindGuardianType(RepeatedField<Guardian> guardians, GuardianType guardianType)
    {
        // Find the same guardian in guardians
        int index = 0;
        foreach (var guardian in guardians)
        {
            if (guardian.GuardianType.Type == guardianType.Type
                && guardian.GuardianType.GuardianType_ == guardianType.GuardianType_)
            {
                break;
            }

            index++;
        }

        return index;
    }
}