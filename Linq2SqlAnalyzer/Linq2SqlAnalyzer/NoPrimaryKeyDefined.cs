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
            foreach (var classAttrib in classDec.AttributeLists.SelectMany(x => x.Attributes))
            {
                if (c.SemanticModel.GetSymbolInfo(classAttrib).Symbol.ToString().StartsWith("System.Data.Linq.Mapping.TableAttribute"))
                {
                    isTable = true;
                    break;
                }
            }

            if (!isTable)
                return;

            foreach (var member in classDec.Members)
            {
                // System.Data.Linq.Mapping.ColumnAttribute can only be applied to Properties and Fields
                if (!(member.Kind() == SyntaxKind.PropertyDeclaration || member.Kind() == SyntaxKind.FieldDeclaration))
                    continue;

                SyntaxList<AttributeListSyntax> attributes;

                if (member.Kind() == SyntaxKind.PropertyDeclaration) attributes = ((PropertyDeclarationSyntax)member).AttributeLists;
                if (member.Kind() == SyntaxKind.FieldDeclaration) attributes = ((FieldDeclarationSyntax)member).AttributeLists;
                
                foreach (var attrib in attributes.SelectMany(x => x.Attributes))
                {
                    if (!c.SemanticModel.GetSymbolInfo(attrib).Symbol.ToString().StartsWith("System.Data.Linq.Mapping.ColumnAttribute"))
                        continue;
                    
                    var primKeyAttrib = attrib.ArgumentList?.Arguments.FirstOrDefault(x => x.NameEquals?.Name?.ToString() == "IsPrimaryKey");
                    if (primKeyAttrib == null)
                        continue;

                    var primKeyExpression = primKeyAttrib.Expression;
                    var expressionKind = primKeyExpression.Kind();

                    if (expressionKind == SyntaxKind.TrueLiteralExpression)
                        return;
                }
            }
            
            c.ReportDiagnostic(Diagnostic.Create(Rule, classDec.Identifier.GetLocation(), classDec.Identifier.ToString() ));
        }
    }
}