using System;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    private bool JsonExpressionCalculate(string jsonExpression)
    {
        jsonExpression = "{\"companyID\":\"15\",\"employees\":[{\"firstName\":\"Bill\",\"lastName\":\"Gates\"},{\"firstName\":\"George\",\"lastName\":\"Bush\"}],\"manager\":[{\"salary\":\"6000\",\"age\":\"23\"},{\"salary\":\"8000\",\"age\":\"26\"}]}  ";

        return true;
    }
}