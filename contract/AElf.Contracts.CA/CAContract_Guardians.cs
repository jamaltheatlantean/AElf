namespace AElf.Contracts.CA.Protobuf;

public partial class CAContract
{
    // TODO
    // Add a Guardian, if already added, return true
    public AddGuardianOutput AddGuardian(AddGuardianInput input)
    {
        return new AddGuardianOutput();
    }

    // TODO
    // Remove a Guardian, if already removed, return true
    public RemoveGuardianOutput RemoveGuardian(RemoveGuardianInput input)
    {
        return new RemoveGuardianOutput();
    }

    // TODO
    // Set a GuardianType for login, if already set, return ture
    public SetGuardianTypeForLoginOutput SetGuardianTypeForLogin(SetGuardianTypeForLoginInput input)
    {
        return new SetGuardianTypeForLoginOutput();
    }

    // TODO
    // Set a GuardianType for login, if already unset, return ture
    public UnsetGuardianTypeForLoginOutput UnsetGuardianTypeForLogin(UnsetGuardianTypeForLoginInput input)
    {
        return new UnsetGuardianTypeForLoginOutput();
    }
}