using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Linq2SqlAnalyzer.Test
{
    [TestClass]
    class NoPrimaryKeyTest : CodeFixVerifier
    {

        public void HasPrimaryKeyDefined()
        {

            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data.Linq;

namespace ConsoleApplication1
{
    [System.Data.Linq.Mapping.TableAttribute(Name = ""dbo.Table"")]
    public class TestTable
    {
        //[System.Data.Linq.Mapping.Column(IsPrimaryKey = true)]
        public int Id {get;set;}
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "",
                Message = String.Format("", ""),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs",12, 0)
                }
            };

            VerifyCSharpDiagnostic(test, expected);


        }

    }
}
