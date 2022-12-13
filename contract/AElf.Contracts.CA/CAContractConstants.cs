namespace AElf.Contracts.CA;

public static class CAContractConstants
{
    public const int LoginGuardianTypeIsOccupiedByOthers = 0;
    // >1 fine, == 0 , conflict.
    public const int LoginGuardianTypeIsNotOccupied = 1;
    public const int LoginGuardianTypeIsYours = 2;
}