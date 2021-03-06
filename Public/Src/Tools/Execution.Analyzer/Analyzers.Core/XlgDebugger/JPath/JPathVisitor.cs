//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from JPath.g4 by ANTLR 4.7.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace BuildXL.Execution.Analyzer.JPath {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="JPathParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.2")]
[System.CLSCompliant(false)]
public interface IJPathVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="JPathParser.intBinaryOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIntBinaryOp([NotNull] JPathParser.IntBinaryOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="JPathParser.intUnaryOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIntUnaryOp([NotNull] JPathParser.IntUnaryOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="JPathParser.boolBinaryOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolBinaryOp([NotNull] JPathParser.BoolBinaryOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="JPathParser.logicBinaryOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicBinaryOp([NotNull] JPathParser.LogicBinaryOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="JPathParser.logicUnaryOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicUnaryOp([NotNull] JPathParser.LogicUnaryOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="JPathParser.setBinaryOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSetBinaryOp([NotNull] JPathParser.SetBinaryOpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="JPathParser.anyBinaryOp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAnyBinaryOp([NotNull] JPathParser.AnyBinaryOpContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>PropertyId</c>
	/// labeled alternative in <see cref="JPathParser.prop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPropertyId([NotNull] JPathParser.PropertyIdContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>EscId</c>
	/// labeled alternative in <see cref="JPathParser.prop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEscId([NotNull] JPathParser.EscIdContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>IdSelector</c>
	/// labeled alternative in <see cref="JPathParser.selector"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdSelector([NotNull] JPathParser.IdSelectorContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>StrLitExpr</c>
	/// labeled alternative in <see cref="JPathParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStrLitExpr([NotNull] JPathParser.StrLitExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>RegExLitExpr</c>
	/// labeled alternative in <see cref="JPathParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRegExLitExpr([NotNull] JPathParser.RegExLitExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>IntLitExpr</c>
	/// labeled alternative in <see cref="JPathParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIntLitExpr([NotNull] JPathParser.IntLitExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>PropertyValue</c>
	/// labeled alternative in <see cref="JPathParser.propVal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPropertyValue([NotNull] JPathParser.PropertyValueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ObjLitProps</c>
	/// labeled alternative in <see cref="JPathParser.objLit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObjLitProps([NotNull] JPathParser.ObjLitPropsContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>MapExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMapExpr([NotNull] JPathParser.MapExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>FuncOptExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncOptExpr([NotNull] JPathParser.FuncOptExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>CardinalityExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCardinalityExpr([NotNull] JPathParser.CardinalityExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>SaveToFileExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSaveToFileExpr([NotNull] JPathParser.SaveToFileExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>LetExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLetExpr([NotNull] JPathParser.LetExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>SubExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSubExpr([NotNull] JPathParser.SubExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>AppendToFileExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAppendToFileExpr([NotNull] JPathParser.AppendToFileExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>BinExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBinExpr([NotNull] JPathParser.BinExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>RangeExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRangeExpr([NotNull] JPathParser.RangeExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>AssignExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignExpr([NotNull] JPathParser.AssignExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>SelectorExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSelectorExpr([NotNull] JPathParser.SelectorExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>FilterExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFilterExpr([NotNull] JPathParser.FilterExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>RootExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRootExpr([NotNull] JPathParser.RootExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ObjLitExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitObjLitExpr([NotNull] JPathParser.ObjLitExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>PipeExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPipeExpr([NotNull] JPathParser.PipeExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>VarExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarExpr([NotNull] JPathParser.VarExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>LiteralExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLiteralExpr([NotNull] JPathParser.LiteralExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>FuncAppExprParen</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncAppExprParen([NotNull] JPathParser.FuncAppExprParenContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ThisExpr</c>
	/// labeled alternative in <see cref="JPathParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitThisExpr([NotNull] JPathParser.ThisExprContext context);
}
} // namespace BuildXL.Execution.Analyzer.JPath
