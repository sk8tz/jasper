﻿using System;
using System.Linq;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Xunit;

namespace Jasper.Testing.Codegen
{
    public class code_compilation_smoke_test
    {
        [Fact]
        public void can_codegen_something()
        {
            var code = @"
namespace Jasper.Core.Testing.Compilation
{
    public class Something
    {

    }
}
";

            var generator = new AssemblyGenerator();
            var assembly = generator.Generate(code);

            var somethingType = assembly.GetExportedTypes().Single();

            var something = Activator.CreateInstance(somethingType);

            Console.WriteLine(something);

        }
    }
}