using System.Collections.Generic;

namespace AElf.Contracts.CA;

public static class CAContractConstants
{
    public const int LoginGuardianTypeIsOccupiedByOthers = 0;
    // >1 fine, == 0 , conflict.
    public const int LoginGuardianTypeIsNotOccupied = 1;
    public const int LoginGuardianTypeIsYours = 2;
    public const int SecondsForOneDay = 86400; // 24*60*60

    // public static readonly CAContract.Strategy GeneralGuardianStrategy = new CAContract.IfElseStrategy()
    // {
    //     IfCondition = new CAContract.LargerThanStrategy()
    //     {
    //         One = GuardianCount,
    //         Two = 4
    //     },
    //     Than = new CAContract.NotLessThanStrategy()
    //     {
    //         One = GuardianApprovedCount,
    //         Two = GuardianCount
    //     },
    //     Else = new CAContract.NotLessThanStrategy()
    //     {
    //         One = GuardianApprovedCount,
    //         Two = new CAContract.RatioOfCountCalculationStrategy()
    //         {
    //             One = GuardianCount,
    //             Two = 6000
    //         }
    //     }
    // };

    public const string Rule = "rule";
    public const string And = "&&";
    public const string Or = "||";
    public const string Not = "!";
    public const string IfElse = "ifElse";
    public const string LargerThan = ">";
    public const string NotLargerThan = "<=";
    public const string LessThan = "<";
    public const string NotLessThan = ">=";
    public const string Equal = "==";
    public const string NotEqual = "!=";
    public const string RatioByTenThousand = "ratioByTenThousand";
    
    public const string IntMAX = "int.Max";
    public const string Parameters = "parameters";

    public const string GuardianApprovedCount = "guardianApprovedCount";
    public const string GuardianCount = "guardianCount";

    public const long TenThousand = 10000;

    public const string ELFTokenSymbol = "ELF";
    public const int CADelegationAmount = 100000000;
}