using System;
using System.Linq;
using AElf.Types;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    public override GetHolderInfoOutput GetHolderInfo(GetHolderInfoInput input)
    {
        Assert(input != null, "input cannot be null!");
        // CaHash and loginGuardianType cannot be invalid at same time.
        Assert(!(input.CaHash == null && String.IsNullOrEmpty(input.LoginGuardianType)), 
            $"CaHash is null, and loginGuardianType is empty: {input.CaHash}, {input.LoginGuardianType}");

        GetHolderInfoOutput output = new GetHolderInfoOutput();
        HolderInfo holderInfo = null;
        if (input.CaHash != null)
        {
            // use ca_hash to get holderInfo
            holderInfo = State.HolderInfoMap[input.CaHash];
            Assert(holderInfo != null, 
                $"Bad ca_hash, {input.CaHash}");

            output.CaHash = input.CaHash;
        }
        else
        {
            // use loginGuardianType to get holderInfo
            var caHash = State.LoginGuardianTypeMap[input.LoginGuardianType];
            Assert(caHash != null, 
                $"Not found ca_hash by a the loginGuardianType {input.LoginGuardianType}.");

            holderInfo = State.HolderInfoMap[caHash];
            Assert(holderInfo != null, 
                $"Bad ca_hash, {caHash}");

            output.CaHash = caHash;
        }

        output.CaAddress = Address.FromPublicKey(Context.Self.Value.Concat(output.CaHash.Value.ToByteArray().ComputeHash()).ToArray());
        output.GuardiansInfo = holderInfo.GuardiansInfo.Clone();

        return output;
    }
}