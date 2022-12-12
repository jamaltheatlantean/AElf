using System;
namespace AElf.Contracts.CA;

public partial class CAContract
{
    public override GetHolderInfoOutput GetHolderInfo(GetHolderInfoInput input)
    {
        Assert(input == null);
        // CaHash and loginGuardianType cannot be invalid at same time.
        Assert(input.CaHash == null && String.IsNullOrEmpty(input.LoginGuardianType));


        return base.GetHolderInfo(input);
    }
}