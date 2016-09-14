using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MagicNightRoslyn {

    public static class LX {
        public static int FindIndex<T>(this IEnumerable<T> items, Predicate<T> predicate) {
            var result = 0;
            foreach(var item in items) {
                if(predicate(item)) {
                    return result;
                }
                ++result;
            }
            return -1;
        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MagicNightRoslynAnalyzer : DiagnosticAnalyzer {
        public const string DiagnosticId = "MagicNightRoslyn";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor WarningRule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description
        );

        private static DiagnosticDescriptor ErrorRule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(WarningRule); } }

        public override void Initialize(AnalysisContext context) {

            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context) {

            //TODO: implement your analysis!

            // Rough guideline:
            // 1) Find out what method is being invoked
            // 2) Find out if that method is marked with out special StringFormat attribute
            // 3) Figure out which parameters correspond to the format string and arguments
            // 4) Validate the format string and arguments
            var invocation = context.Node as InvocationExpressionSyntax;
            if(invocation == null) {
                return;
            }
            var model = context.SemanticModel;
            var methodDeclaration = model.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;
            if(methodDeclaration == null) {
                return;
            }
            var attributes = methodDeclaration.GetAttributes();
            var attribute = attributes[0];
            if(attribute.AttributeClass.Name != "StringFormat") {
                return;
            }
            var args = attribute.ApplicationSyntaxReference.GetSyntax().DescendantNodes().OfType<AttributeArgumentSyntax>().ToDictionary(value => value.NameColon.Name.Identifier.Text);
            var formatParam = args["formatParam"].Expression as LiteralExpressionSyntax;
            var formatValue = formatParam.Token.Text;
            var formatIndex = methodDeclaration.Parameters.FindIndex(p => "\"" + p.Name + "\"" == formatValue);
            if(formatIndex < 0) {
                return;
            }

            var argsParam = args["argsParam"].Expression as LiteralExpressionSyntax;
            var argsValue = argsParam.Token.Text;
            var argsIndex = methodDeclaration.Parameters.FindIndex(p => "\"" + p.Name + "\"" == argsValue);
            if(argsIndex < 0) {
                return;
            }

            var format = invocation.ArgumentList.Arguments[formatIndex].Expression as LiteralExpressionSyntax;
            if(format == null) {
                context.ReportDiagnostic(Diagnostic.Create(WarningRule, context.Node.GetLocation()));
            }

            var formatString = format.Token.Text;
            var formatArgCount = 0;
            for(var match = Regex.Match(formatString, @"{\d+}"); match.Success; match = match.NextMatch()) {
                formatArgCount = Math.Max(0, int.Parse(match.Value.Substring(1, match.Value.Length - 2)) + 1);
            }

            var arguments = invocation.ArgumentList.Arguments[argsIndex];
        }
    }
}
