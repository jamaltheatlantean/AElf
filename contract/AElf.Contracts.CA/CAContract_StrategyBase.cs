using System;
using System.Collections.Generic;
using System.Threading;
using AElf.Sdk.CSharp;
using Google.Protobuf;
using Google.Protobuf.Collections;

namespace AElf.Contracts.CA;

public partial class CAContract
{
    public abstract class Strategy
    {
        public abstract string StrategyName { get; }
        public abstract object Validate(StrategyContext context);
        public abstract Strategy Parse(StrategyNode node);
        public abstract StrategyNode ToStrategyNode();

        protected void Assert(bool asserted, string message = "Assertion failed!")
        {
            if (!asserted) throw new AssertionException(message);
        }

        public static Strategy DefaultStrategy()
        {
            return new CAContract.IfElseStrategy()
            {
                IfCondition = new CAContract.LargerThanStrategy()
                {
                    One = CAContractConstants.GuardianCount,
                    Two = 4
                },
                Than = new CAContract.NotLessThanStrategy()
                {
                    One = CAContractConstants.GuardianApprovedCount,
                    Two = CAContractConstants.GuardianCount
                },
                Else = new CAContract.NotLessThanStrategy()
                {
                    One = CAContractConstants.GuardianApprovedCount,
                    Two = new CAContract.RatioOfCountCalculationStrategy()
                    {
                        One = CAContractConstants.GuardianCount,
                        Two = 6000
                    }
                }
            };
        }
    }

    public abstract class UnaryBooleanStrategy : Strategy
    {
        public Strategy One { get; set; }

        public override Strategy Parse(StrategyNode node)
        {
            Assert(false, "not implement");
            return null;
        }

        public override StrategyNode ToStrategyNode()
        {
            return new StrategyNode()
            {
                Name = StrategyName,
                Value = { One.ToStrategyNode().ToByteString() }
            };
        }
    }

    public abstract class BinaryBooleanStrategy : Strategy
    {
        public Strategy One { get; set; }
        public Strategy Two { get; set; }

        public override Strategy Parse(StrategyNode node)
        {
            Assert(false, "not implement");
            return null;
        }

        public override StrategyNode ToStrategyNode()
        {
            return new StrategyNode()
            {
                Name = StrategyName,
                Value =
                {
                    One.ToStrategyNode().ToByteString(),
                    Two.ToStrategyNode().ToByteString()
                }
            };
        }
    }
    
    public abstract class BinaryNumericStrategy : Strategy
    {
        // parameters can be a long, a string for variable, or an instance of NumericStrategy for more calculation.
        public object One { get; set; }
        public object Two { get; set; }

        public override Strategy Parse(StrategyNode node)
        {
            return null;
        }

        public override StrategyNode ToStrategyNode()
        {
            return new StrategyNode()
            {
                Name = StrategyName,
                Value =
                {
                    ParameterToByteString(One),
                    ParameterToByteString(Two)
                }
            };
        }
        
        protected StrategyValueWrapper ParameterToStrategyValueWrapper(object obj)
        {
            var wrapper = new StrategyValueWrapper()
            {
                Value = new StrategyStringWrapper()
                {
                    Value = obj.ToString()
                }.ToByteString()
            };

            if (obj is long or int)
            {
                wrapper.Type = 0;
            }
            else
            {
                wrapper.Type = 1;
            }

            return wrapper;
        }

        protected ByteString ParameterToByteString(object obj)
        {
            if (obj is long or int or string)
            {
                return ParameterToStrategyValueWrapper(obj).ToByteString();
            }
            else if (obj is BinaryNumericStrategy)
            {
                return ((BinaryNumericStrategy)obj).ToStrategyNode().ToByteString();
            }

            Assert(false, "obj should be one of a long, a string, or an instance of NumericStrategy.");
            return null; // untouchable line, just for get rid of red underline masked by IDE.
        }

        protected object ValidateStrategyParameter(object obj, StrategyContext context)
        {
            if (obj is long or int or string)
            {
                return context.AssignVariableAndToLong(obj);
            }
            else if (obj is BinaryNumericStrategy)
            {
                return ((BinaryNumericStrategy)obj).Validate(context);
            }

            Assert(false, "obj should be one of a long, a string, or an instance of NumericStrategy.");
            return null; // untouchable line, just for get rid of red underline masked by IDE.
        }
    }

    public abstract class BinaryNumericCompareStrategy : BinaryNumericStrategy
    {
        protected abstract Func<long, long, bool> Compare { get; }

        public override object Validate(StrategyContext context)
        {
            return Compare((long)ValidateStrategyParameter(One, context),
                (long)ValidateStrategyParameter(Two, context));
        }
    }

    public abstract class BinaryNumericCalculateStrategy : BinaryNumericStrategy
    {
        protected abstract Func<long, long, long> Calculate { get; }

        public override object Validate(StrategyContext context)
        {
            return Calculate((long)ValidateStrategyParameter(One, context),
                (long)ValidateStrategyParameter(Two, context));
        }
    }

    public class StrategyContext
    {
        public int CurrentValidator { get; set; } = 1;
        public string Id { get; set; }

        public Dictionary<string, long> Variables { get; set; }

        public bool TryAssignVariable(string variableName, ref long value)
        {
            return Variables.TryGetValue(variableName, out value);
        }

        public bool TryParse(string valueString, ref long value)
        {
            return long.TryParse(valueString, out value);
        }

        public long AssignVariableAndToLong(object obj)
        {
            long value = 0;
            if (obj is int || obj is long)
            {
                return (long)obj;
            }
            else if (obj is string)
            {
                string str = (string)obj;
                if (TryAssignVariable(str, ref value))
                {
                    return value;
                }

                if (TryParse(str, ref value))
                {
                    return value;
                }
            }

            Assert(false, "A string here should be a variable name or a numeric string");
            return 0; // untouchable line, just for get rid of red underline masked by IDE.
        }

        protected void Assert(bool asserted, string message = "Assertion failed!")
        {
            if (!asserted) throw new AssertionException(message);
        }
    }
}