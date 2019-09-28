using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Linq2SqlAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ToUpperLower : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ToUpperLower";
        internal static readonly LocalizableString Title = "ToUpperLower Title";
        internal static readonly LocalizableString MessageFormat = "ToUpperLower '{0}'";
        internal const string Category = "ToUpperLower Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // todo: not complete yet.
            //context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;

            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;

            var memberAccessExprName = memberAccessExpr?.Name.ToString();

            // quick check to see if we should even continue checking - this reduces the overall impact of the check
            if (!((memberAccessExprName?.StartsWith("ToUpper") ?? false) || (memberAccessExprName?.StartsWith("ToLower") ?? false)))
                return;

            // check that the method is in the class that we're interested in
            var memberSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpr).Symbol as IMethodSymbol;
            if (!memberSymbol?.ToString().StartsWith("System.String.") ?? true) return;


        }
    }
}