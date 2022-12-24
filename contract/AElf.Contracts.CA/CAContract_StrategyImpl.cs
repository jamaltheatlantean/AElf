// using System;
// using System.Collections.Generic;
// using System.Threading;
// using AElf.Sdk.CSharp;
// using Google.Protobuf;
// using Google.Protobuf.Collections;
//
// namespace AElf.Contracts.CA;
//
//
// public class AddStrategy : BinaryBooleanStrategy
// {
//     public override string StrategyName { get => CAContractConstants.And; }
//
//     public override object Validate(StrategyContext context)
//     {
//         return (bool)One.Validate(context) && (bool)Two.Validate(context);
//     }
//     
// }
//
// public class OrStrategy : BinaryBooleanStrategy
// {
//     public override string StrategyName { get => CAContractConstants.Or; }
//
//     public override object Validate(StrategyContext context)
//     {
//         return (bool)One.Validate(context) || (bool)Two.Validate(context);
//     }
// }
//
// public class NotStrategy : UnaryBooleanStrategy
// {
//     public override string StrategyName { get => CAContractConstants.Not; }
//
//     public override object Validate(StrategyContext context)
//     {
//         return !(bool)One.Validate(context);
//     }
//     
// }
//
// public class IfElseStrategy : Strategy
// {
//     public Strategy IfCondition { get; set; }
//     public Strategy Than { get; set; }
//     public Strategy Else { get; set; }
//
//     public override string StrategyName { get => CAContractConstants.IfElse; }
//
//     public override object Validate(StrategyContext context)
//     {
//         return ((bool)IfCondition.Validate(context)) ? (bool)Than.Validate(context) : (bool)Else.Validate(context);
//     }
//
//     public override Strategy Parse(StrategyNode node)
//     {
//         throw new NotImplementedException();
//     }
//
//     public override StrategyNode ToStrategyNode()
//     {
//         return new StrategyNode()
//         {
//             Name = StrategyName,
//             Value =
//             {
//                 IfCondition.ToStrategyNode().ToByteString(),
//                 Than.ToStrategyNode().ToByteString(),
//                 Else.ToStrategyNode().ToByteString()
//             }
//         };
//     }
// }
//
// public class LargerThanStrategy : BinaryNumericCompareStrategy
// {
//     public override string StrategyName { get => CAContractConstants.LargerThan; }
//     protected override Func<long, long, bool> Compare { get => (one, two) => one > two; }
// }
//
// public class NotLargerThanStrategy : BinaryNumericCompareStrategy
// {
//     public override string StrategyName { get => CAContractConstants.NotLargerThan; }
//     protected override Func<long, long, bool> Compare { get => (one, two) => one <= two; }
// }
//
// public class LessThanStrategy : BinaryNumericCompareStrategy
// {
//     public override string StrategyName { get => CAContractConstants.LessThan; }
//     protected override Func<long, long, bool> Compare { get => (one, two) => one < two; }
// }
//
// public class NotLessThanStrategy : BinaryNumericCompareStrategy
// {
//     public override string StrategyName { get => CAContractConstants.NotLessThan; }
//     protected override Func<long, long, bool> Compare { get => (one, two) => one >= two; }
// }
//
// public class EqualStrategy : BinaryNumericCompareStrategy
// {
//     public override string StrategyName { get => CAContractConstants.Equal; }
//     protected override Func<long, long, bool> Compare { get => (one, two) => one == two; }
// }
//
// public class NotEqualStrategy : BinaryNumericCompareStrategy
// {
//     public override string StrategyName { get => CAContractConstants.NotEqual; }
//     protected override Func<long, long, bool> Compare { get => (one, two) => one != two; }
// }
//
// public class RatioByTenThousand : BinaryNumericCompareStrategy
// {
//     public override string StrategyName { get => CAContractConstants.RatioByTenThousand; }
//     protected override Func<long, long, bool> Compare { get => (one, two) => one != two; }
// }
//
//
//
// /*public class BiggerThanStrategy : Strategy
// {
//     private BiggerThanParameters Parameters { get; set; }
//
//     public override bool Validate(StrategyContext context)
//     {
//         return true;
//     }
//
//     public override Strategy Parse(StrategyNode node)
//     {
//         Parameters.MergeFrom(node.Value);
//         return this;
//     }
//
//     public override StrategyNode ToStrategyNode()
//     {
//         return new StrategyNode()
//         {
//             Name = nameof(BiggerThanStrategy),
//             Value = Parameters.ToByteString()
//         };
//     }
// }
//
// public class AndStrategy : Strategy
// {
//     public Strategy One { get; set; }
//     public Strategy Two { get; set; }
//
//     public override bool Validate(StrategyContext context)
//     {
//         return One.Validate(context) && Two.Validate(context);
//     }
//
//     public override Strategy Parse(StrategyNode node)
//     {
//         One = StrategyFactory.Create(node.Value[0]);
//         Two = StrategyFactory.Create(node.Value[0]);
//         return this;
//     }
//
//     public override StrategyNode ToStrategyNode()
//     {
//         return new StrategyNode()
//         {
//             Name = nameof(AndStrategy),
//             Value = { One.ToStrategyNode(), Two.ToStrategyNode() }
//         };
//     }
//
//     public class StrategyFactory
//     {
//         public static Strategy Create(StrategyNode node)
//         {
//             switch (node.Name)
//             {
//                 case "And":
//                     return new AndStrategy().Parse(node);
//
//                 case "BiggerThan":
//                     return new BiggerThanStrategy().Parse(node);
//             }
//
//             throw new NotImplementedException();
//         }
//     }
// }*/
