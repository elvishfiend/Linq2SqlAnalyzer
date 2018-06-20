using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace Linq2SqlAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    class WhereCharEqualsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "IQueryableCharEqualsCharAnalyzer";

        private static readonly string Title = "LinqToSql CharEqualsEqualsCharAnalyzer";
        private static readonly string MessageFormat = "<{0} {1} {2} can have bad Sql performance. Please use <{0}>.Equals(<{2}>) instead.";
        private static readonly string Description = "";
        private const string Category = "LinqToSql";

        // ignored methods
        private readonly string[] ignoredMethods = new[] { "Select" };

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression );
        }

        private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = context.Node as BinaryExpressionSyntax;

            // we're only interested in Char Literal or char/char?/Nullable<char> Property/Field

            var leftType = context.SemanticModel.GetTypeInfo(binaryExpression.Left);
            if (leftType.Type == null || !(leftType.Type.ToString() == "char" || leftType.Type.ToString() == "char?"))
                return;

            var rightType = context.SemanticModel.GetTypeInfo(binaryExpression.Right);
            if (rightType.Type == null || !(rightType.Type.ToString() == "char" || rightType.Type.ToString() == "char?"))
                return;

            // now we need to traverse up the syntax tree until we get to a method, or something else that makes us bailOut earlier
            SyntaxNode parent = binaryExpression.Parent;

            // todo: expand this list of symbols that will make us bail out early.
            var bailOut = new[] { SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.GlobalStatement };

            while (parent.Kind() != SyntaxKind.InvocationExpression)
            {
                // if we get too far up, we need to quit.
                if (bailOut.Contains(parent.Kind()))
                    return;
                
                parent = parent.Parent;

                if (parent == null)
                    return;
            }

            var invocationExpr = parent as InvocationExpressionSyntax;

            if (invocationExpr == null) return;

            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;

            var memberSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpr);

            var methodSymbol = memberSymbol.Symbol as IMethodSymbol;

            // if the method doesn't come from the System.Linq.Queryable namespace, we can ignore it.
            if (methodSymbol?.ContainingSymbol.ToString() != "System.Linq.Queryable")
                return;

            var baseType = methodSymbol.TypeArguments.FirstOrDefault();

            // check if we're dealing with a Linq-to-Sql data class
            if (!(baseType.GetAttributes().Any(x => x.AttributeClass.ToString() == "System.Data.Linq.Mapping.TableAttribute")))
                return;

            // complain about it!
            context.ReportDiagnostic(Diagnostic.Create(Rule, binaryExpression.GetLocation(), leftType.Type.ToString(), binaryExpression.OperatorToken.ToString(), rightType.Type.ToString()));

        }
            /*var invocationExpr = (InvocationExpressionSyntax)context.Node;

            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;

            // quick check to see if we should even continue checking - this reduces the overall impact of the check
            if (memberAccessExpr?.Name.ToString() != "Where") return;

            // check to see if this is a Where() on an System.Data.Linq.Table<> or IQueryable<>
            var memberSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpr);

            // make sure we're only checking Where calls from System.Linq.Queryable/Enumerable
            var methodSymbol = memberSymbol.Symbol as IMethodSymbol;
            if (methodSymbol?.ContainingSymbol.ToString() != "System.Linq.Queryable")
                
                return;

            // get the base type that the Extension Method is acting on
            var baseType = methodSymbol.TypeArguments.First();

            // check if the base type has a System.Data.Linq.Mapping.TableAttribute => definitely a LinqToSql query
            var baseAttribs = baseType.GetAttributes();

            var isTableType = baseAttribs.Any(x => x.AttributeClass.ToString() == "System.Data.Linq.Mapping.TableAttribute");
            
            // we're only interested in things that have a TableAttribute
            if (!isTableType)
                return;

            // get all the arguments
            var argumentList = invocationExpr.ArgumentList;

            // get the first argument - this should be the Lambda Expression used in the Method.
            var argument = argumentList.Arguments.First();

            var lambdaExpression = argument.Expression as LambdaExpressionSyntax;

            

            

            

        }*/
    }
}
