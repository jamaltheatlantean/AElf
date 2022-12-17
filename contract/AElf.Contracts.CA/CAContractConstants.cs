using System.Collections.Generic;

namespace AElf.Contracts.CA;

public static class CAContractConstants
{
    public const int LoginGuardianTypeIsOccupiedByOthers = 0;
    // >1 fine, == 0 , conflict.
    public const int LoginGuardianTypeIsNotOccupied = 1;
    public const int LoginGuardianTypeIsYours = 2;
    public const int SecondsForOneDay = 86400; // 24*60*60
    public const string GeneralJsonGuardianRules = @"
    {
        ""||"": [{
            ""rule"": {
                ""minGuardianCount"": ""1"",
                ""maxGuardianCount"": ""4"",
                ""parameters"": [
                    ""largerThan"",
                    ""guardianApprovedCount"",
                    ""guardianCount""
                ]
            }
        }, {
            ""rule"": {
                ""minGuardianCount"": ""4"",
                ""maxGuardianCount"": ""int.Max"",
                ""parameters"": [
                    ""roundDown"",
                    ""guardianApprovedCount"",
                    ""guardianCount"",
                    ""6000"",
                    ""1""]
            }
        }]
    }";

    public const string Rule = "rule";
    public const string And = "&&";
    public const string Or = "||";
    public const string Not = "!";
    
    public const string MinGuardianCount = "minGuardianCount";
    public const string MaxGuardianCount = "maxGuardianCount";
    public const string IntMAX = "int.Max";
    public const string Parameters = "parameters";
    public const string LargerThan = "largerThan";
    public const string RoundDown = "roundDown";
    public const string GuardianApprovedCount = "guardianApprovedCount";
    public const string GuardianCount = "guardianCount";

    public const int TenThousand = 10000;

    public const string ELFTokenSymbol = "ELF";
    public const int CADelegationAmount = 100000000;
}