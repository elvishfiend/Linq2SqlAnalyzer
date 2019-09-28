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
    public class NoPrimaryKeyDefined : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LinqToSqlTableMissingPrimaryKey";
        internal static readonly string Title = "";
        internal static readonly LocalizableString MessageFormat = "LinqToSql Object {0} is missing a Primary Key field and will not be able to be updated.";
        internal const string Category = "Analyzer1 Category";

        public const string TableAttributeName = "System.Data.Linq.Mapping.TableAttribute";
        public const string ColumnAttributeName = "System.Data.Linq.Mapping.ColumnAttribute";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(c => AnalyzeNode(c), SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext c)
        {
            var classDec = (ClassDeclarationSyntax)c.Node;
            
            var isTable = false;
            // a member can have many AttributeLists and an AttributeList can have many Attributes. e.g. [Attr1(), Attr2()]

            var classAttributes = classDec.AttributeLists.SelectMany(x => x.Attributes);
            
            // if we don't have a TableAttribute, we can skip this.
            if (!classAttributes.Any(x => IsLinqTableAttribute(c, x)))
                return;

            foreach (var member in classDec.Members)
            {
                // System.Data.Linq.Mapping.ColumnAttribute can only be applied to Properties and Fields
                //if (!(member.Kind() == SyntaxKind.PropertyDeclaration || member.Kind() == SyntaxKind.FieldDeclaration))

                // the default code generator only uses Properties, so we can ignore fields for now.
                if (member.Kind() != SyntaxKind.PropertyDeclaration)
                    continue;

                SyntaxList<AttributeListSyntax> attributes;

                if (member.Kind() == SyntaxKind.PropertyDeclaration) attributes = ((PropertyDeclarationSyntax)member).AttributeLists;
                //if (member.Kind() == SyntaxKind.FieldDeclaration) attributes = ((FieldDeclarationSyntax)member).AttributeLists;

                if (attributes.SelectMany(x => x.Attributes)
                    .Where(x => IsLinqColumnAttribute(c, x))
                    .Any(IsPrimaryKey))
                    return;
            }
            
            c.ReportDiagnostic(Diagnostic.Create(Rule, classDec.Identifier.GetLocation(), classDec.Identifier.ToString() ));
        }

        private bool IsLinqTableAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attribute)
        {
            return context.SemanticModel.GetSymbolInfo(attribute).Symbol.ToString().StartsWith(TableAttributeName);
        }

        private bool IsLinqColumnAttribute(SyntaxNodeAnalysisContext context, AttributeSyntax attribute)
        {
            return context.SemanticModel.GetSymbolInfo(attribute).Symbol.ToString().StartsWith(ColumnAttributeName);
        }

        private bool IsPrimaryKey(AttributeSyntax attribute)
        {
            var primKeyArg = attribute.ArgumentList?.Arguments.FirstOrDefault(arg => arg.NameEquals?.Name?.ToString() == "IsPrimaryKey");
            var primKeyExpression = primKeyArg.Expression;
            var expressionKind = primKeyExpression.Kind();

            return expressionKind == SyntaxKind.TrueLiteralExpression;
        }

    }
}