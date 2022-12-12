using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    // TODO
    // Add a Guardian, if already added, return true
    public Empty AddGuardian(AddGuardianInput input)
    {
        
        return new Empty();
    }

    // TODO
    // Remove a Guardian, if already removed, return true
    public Empty RemoveGuardian(RemoveGuardianInput input)
    {
        return new Empty();
    }

    // TODO
    // Set a GuardianType for login, if already set, return ture
    public Empty SetGuardianTypeForLogin(SetGuardianTypeForLoginInput input)
    {
        return new Empty();
    }

    // TODO
    // Set a GuardianType for login, if already unset, return ture
    public Empty UnsetGuardianTypeForLogin(UnsetGuardianTypeForLoginInput input)
    {
        return new Empty();
    }
}