using System;
using System.Runtime.CompilerServices;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    public override Empty SetGuardianTypeForLogin(SetGuardianTypeForLoginInput input)
    {
        Assert(input == null);
        Assert(input.CaHash == null); 
        // GuardianType should be valid, not null, and be with non-null GuardianType_
        Assert(input.GuardianType == null
            || input.GuardianType != null && String.IsNullOrEmpty(input.GuardianType.GuardianType_));

        HolderInfo holderInfo = State.HolderInfoMap[input.CaHash];
        string loginGuardianType = input.GuardianType.GuardianType_;
        
        
        

        return new Empty();
    }

    public override Empty UnsetGuardianTypeForLogin(UnsetGuardianTypeForLoginInput input)
    {
        
        return new Empty();
    }
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // private bool Check

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // private void SetGuardianTypeAsLogin(GuardiansInfo guardiansInfo, GuardianType)
    // {
    //     
    // }
}