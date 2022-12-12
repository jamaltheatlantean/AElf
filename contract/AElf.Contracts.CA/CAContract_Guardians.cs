using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    // TODO
    // Add a Guardian, if already added, return true
    public override Empty AddGuardian(AddGuardianInput input)
    {
        
        return new Empty();
    }

    // TODO
    // Remove a Guardian, if already removed, return true
    public override Empty RemoveGuardian(RemoveGuardianInput input)
    {
        return new Empty();
    }
}