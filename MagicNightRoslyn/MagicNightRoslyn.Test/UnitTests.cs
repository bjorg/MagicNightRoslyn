﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace MagicNightRoslyn.Test {

    [TestClass]
    public class UnitTest : CodeFixVerifier {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1() {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2() {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            [StringFormat(formatParam: ""formatString"", argsParam: ""arguments"")]
            void Log(string formatString, params object[] arguments) { }

            void Test() {
                Log(""{0} {13}"", 123);
            }
        }
    }";
            var expected = new DiagnosticResult {
                Id = "MagicNightRoslyn",
                Message = "Type name \'TypeName\' contains lowercase letters",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new MagicNightRoslynCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new MagicNightRoslynAnalyzer();
        }
    }
}