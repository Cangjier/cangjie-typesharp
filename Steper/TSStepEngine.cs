using Cangjie.Core.Steper;
using Cangjie.Dawn.Steper;
using Cangjie.Dawn.Steper.ControllerSteps;
using Cangjie.Dawn.Steper.JsonSteps;
using Cangjie.Dawn.Steper.StringSteps;
using Cangjie.Dawn.Steper.ValueSteps;

namespace Cangjie.TypeSharp.Steper;

public class TSStepEngine : StepParserEngine<char>
{
    public TSStepEngine()
    {

    }

    public override void Initial()
    {
        AddSingleStepParser(NumberStep.Parser.Instance, BooleanStep.Parser.Instance,NullStep.Parser.Instance, MethodStep.Parser.Instance, Prior.Parser.Instance,FieldStep.FieldStepParser.Instance, LetStep.Parser.Instance,RegexStep.Parser.Instance)
            .Add(StringStep.Parsers)
            .Add(NewStep.Parser.Instance)
            .Add(JsonArrayStep.Parser.Instance, JsonObjectStep.Parser.Instance)
            .Add(ImportStep.Parser.Instance)
            .Add(LamdaStep.Parser.Instance)
            .Add(IfStep.Parser.Instance, ForStep.Parser.Instance, ForeachStep.Parser.Instance, WhileStep.Parser.Instance, TryStep.Parser.Instance)
            .Add(StatementStep.Parser.Instance);
        CreateRank().Add(WrapStep.Parser.Instance,DebuggerStep.Parser.Instance);
        CreateRank().Add(TypeStep.DotParser.Instance);
        CreateRank().Add(MemberStep.MemberStepParser.Instance, IndexStep.Parser.Instance, MemberMethod.Parser.Instance,NullConditionalOperatorStep.Parser.Instance, MemberMethod.PreviousIsInstanceParser.Instance);
        CreateRank().Add(UnaryOperatorStep.Parser.UnaryPlus, UnaryOperatorStep.Parser.UnaryNegation,UnaryOperatorStep.Parser.Increment, UnaryOperatorStep.Parser.Decrement, UnaryOperatorStep.Parser.LogicalNot);
        CreateRank().Add(AwaitStep.Parser.Instance);
        CreateRank().Add(AssignStep.HyperAssignParser.Instance);
        CreateRank().Add(BinaryOperator.Parser.MultiplyOperators);
        CreateRank().Add(BinaryOperator.Parser.AdditionOperators);
        CreateRank().Add(BinaryOperator.Parser.EqualityOperators);
        CreateRank().Add(BinaryOperator.Parser.GreaterThanOperators);
        CreateRank().Add(LogicalOperator.Parser.AndOperator);
        CreateRank().Add(LogicalOperator.Parser.OrOperator);
        CreateRank().Add(AsStep.Parser.Instance);
        CreateRank().Add(NullCoalescingOperatorStep.Parser.Instance);
        CreateRank().Add(TernaryOperatorStep.Parser.Instance);
        CreateRank().Add(TypeDefineStep.Parser.Instance);
        CreateRank().Add(AssignStep.Parser.Instance);
        CreateRank().Add(ReturnStep.Parser.Instance, ContinueStep.Parser.Instance, BreakStep.Parser.Instance, ThrowStep.Parser.Instance);
    }
}
