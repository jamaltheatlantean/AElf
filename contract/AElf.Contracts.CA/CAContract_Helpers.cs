using System.Collections.Generic;
using AElf.Types;

namespace AElf.Contracts.CA;

public partial class CAContract
{ 
    private void JsonExpressionCalculate(string jsonExpression)
    {
        // Dictionary<string, object> jsonObj = Context.DeserializeJsonToDictionary(jsonExpression);
        // if (jsonObj["opr"] == null) return;
        // Assert(jsonObj["opr"].ToString().Equals("?:"), "out opr fail");
        // Assert(Context.DeserializeJsonToDictionary(jsonObj["left"].ToString())["opr"].ToString().Equals("?:"), "inner opr fail");
        // // switch (jsonObj["opr"])
        // {
        //     case "?:":
        //         Assert(1 == 1, "fail");
        //         break;
        //     case "&&":
        //         break;
        //     case "||":
        //         break;
        //     case "==":
        //         break;
        //     case "!=":
        //         break;
        //     case ">":
        //         break;
        //     case "<":
        //         break;
        //     case ">=":
        //         break;
        //     case "<=":
        //         break;
        //     case "+":
        //         break;
        //     case "-":
        //         break;
        //     case "*":
        //         break;
        //     case "/":
        //         break;
        //     case "%":
        //         break;
        //     case "!":
        //         break;
        //     case "round":
        //         break;
        //     case "roundDown":
        //         break;
        //     case "roundUp":
        //         break;
        //     case "++":
        //         break;
        //     case "--":
        //         break;
        // }
    }
    private Address CalculateCaAddress(Hash virtualAddress, Address contractAddress)
    {
        return Context.ConvertVirtualAddressToContractAddress(virtualAddress, contractAddress);
    }
}