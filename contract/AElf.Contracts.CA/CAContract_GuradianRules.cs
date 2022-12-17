using System;
using System.Collections.Generic;
using System.Threading;
using AElf.Sdk.CSharp;

namespace AElf.Contracts.CA;

// A CAHolder creator can set rules that should be satisfied when registering a CAHolder, or recovering it.
// And the rules are in forms of json, which can be parsed can verified easily.
public partial class CAContract
{
    private int ParseInt(string jsonText)
    {
        if (jsonText == CAContractConstants.IntMAX)
        {
            return Int32.MaxValue;
        }

        return int.Parse(jsonText);
    }

    private string GetParameters(object obj, int i)
    {
        if (obj is List<string>)
        {
            return ((List<string>)obj)[i];
        }

        return null;
    }

    private bool CalculationLargerThan(List<string> parameters)
    {
        var p1 = ParseInt(parameters[0]);
        var p2 = ParseInt(parameters[1]);

        return p1 >= p2;
    }

    private bool CalculationRoundDown(List<string> parameters)
    {
        var p1 = ParseInt(parameters[0]);
        var p2 = ParseInt(parameters[1]);
        var p3 = ParseInt(parameters[2]);
        var p4 = ParseInt(parameters[3]);

        return p1 >= p2 * p3 / CAContractConstants.TenThousand + p4;
    }

    private void Assignment(ref List<string> parameters, int guardianCount, int guardianApprovedCount)
    {
        for (int i = 0; i < parameters.Count; i++)
        {
            if (parameters[i] == CAContractConstants.GuardianCount)
            {
                parameters[i] = guardianCount.ToString();
            }
            else if(parameters[i] == CAContractConstants.GuardianApprovedCount)
            {
                parameters[i] = guardianApprovedCount.ToString();
            }
        }
    }

    private bool IsRuleSatisfied(int guardianCount, int guardianApprovedCount, string jsonGuardianRule)
    {
        bool result = false;
        
        var dict = Context.ParseJsonToPlainDictionary(jsonGuardianRule);
        var minGuardianCount = ParseInt((string)dict[CAContractConstants.MinGuardianCount]);
        var maxGuardianCount = ParseInt((string)dict[CAContractConstants.MaxGuardianCount]);
        // out of scope.
        if (guardianCount < minGuardianCount || guardianCount >= maxGuardianCount)
        {
            return false;
        }
        
        List<string> parameters = (List<string>)dict[CAContractConstants.Parameters];
        
        var operationType = GetParameters(parameters, 0);
        parameters.RemoveAt(0);
        Assignment(ref parameters, guardianCount, guardianApprovedCount);

        if (operationType == CAContractConstants.LargerThan)
        {
            result = CalculationLargerThan(parameters);
        }
        else if(operationType == CAContractConstants.RoundDown)
        {
            result = CalculationRoundDown(parameters);
        }

        return result;
    }


    private bool AndOperation(int guardianCount, int guardianApprovedCount, string jsonText1, string jsonText2)
    {
        var result1 = RecursionOperation(guardianCount, guardianApprovedCount, Context.ParseJsonToPlainDictionary(jsonText1));
        var result2 = RecursionOperation(guardianCount, guardianApprovedCount, Context.ParseJsonToPlainDictionary(jsonText2));

        return result1 && result2;
    }

    private bool OrOperation(int guardianCount, int guardianApprovedCount, string jsonText1, string jsonText2)
    {
        var result1 = RecursionOperation(guardianCount, guardianApprovedCount, Context.ParseJsonToPlainDictionary(jsonText1));
        var result2 = RecursionOperation(guardianCount, guardianApprovedCount, Context.ParseJsonToPlainDictionary(jsonText2));

        return result1 || result2;
    }

    private bool NotOperation(int guardianCount, int guardianApprovedCount, string jsonText)
    {
        return !RecursionOperation(guardianCount, guardianApprovedCount, Context.ParseJsonToPlainDictionary(jsonText));

    }
    
    private bool RecursionOperation(int guardianCount, int guardianApprovedCount, Dictionary<string, object> dict)
    {
        if (dict.ContainsKey(CAContractConstants.Rule))
        {
            return IsRuleSatisfied(guardianCount, guardianApprovedCount, (string)dict[CAContractConstants.Rule]);
        }
        else if (dict.ContainsKey(CAContractConstants.And))
        {
            List<string> jsonTexts = (List<string>)dict[CAContractConstants.And];
            return AndOperation(guardianCount, guardianApprovedCount, jsonTexts[0], jsonTexts[1]);
        }
        else if (dict.ContainsKey(CAContractConstants.Or))
        {
            List<string> jsonTexts = (List<string>)dict[CAContractConstants.Or];
            return OrOperation(guardianCount, guardianApprovedCount, jsonTexts[0], jsonTexts[1]);
        }
        else if (dict.ContainsKey(CAContractConstants.Not))
        {
            List<string> jsonTexts = (List<string>)dict[CAContractConstants.Or];
            return NotOperation(guardianCount, guardianApprovedCount, jsonTexts[0]);
        }

        return false;
    }

    private bool AreRulesSatisfied(int guardianCount, int guardianApprovedCount, string jsonGuardianRules)
    {
        var dict = Context.ParseJsonToPlainDictionary(jsonGuardianRules);

        return RecursionOperation(guardianCount, guardianApprovedCount, dict);
    }
    
    
    
}