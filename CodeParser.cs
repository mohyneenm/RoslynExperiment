using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.FindSymbols;

namespace SemanticsCS
{
    // var type = model.GetTypeInfo(identifierNameSyntax).Type;
    // var type = (ITypeSymbol)model.GetSymbolInfo(identifierNameSyntax).Symbol;
    // var extensionMethods = methods.Where(m => model.GetDeclaredSymbol(m).IsExtensionMethod);

    class CodeParser
    {
        static void Main(string[] args)
        {
            //GetAllIfStatements();
            GetAllProperties();
        }

        private static void GetAllIfStatements()
        {
            var text = File.ReadAllText(@"C:\Users\mohyneenm\Documents\Visual Studio 2017\Projects\VSIXTestProject\VSIXTestProject\Class1.cs");
            var syntxTree = CSharpSyntaxTree.ParseText(text);

            var root = (CompilationUnitSyntax)syntxTree.GetRoot();

            var compilation = CSharpCompilation.Create("HelloWorld")
                                               .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                                               .AddSyntaxTrees(syntxTree);
            var model = compilation.GetSemanticModel(syntxTree);
            var nameInfo = model.GetSymbolInfo(root.Usings[0].Name);
            var systemSymbol = (INamespaceSymbol)nameInfo.Symbol;

            //foreach (var ns in systemSymbol.GetNamespaceMembers())
            //{
            //  Console.WriteLine(ns.Name);
            //}

            var helloWorldString = root.DescendantNodes()
                                      .OfType<LiteralExpressionSyntax>()
                                      .First();
            var literalInfo = model.GetTypeInfo(helloWorldString);

            var stringTypeSymbol = (INamedTypeSymbol)literalInfo.Type;

            Console.Clear();
            foreach (var name in (from method in stringTypeSymbol.GetMembers()
                                                              .OfType<IMethodSymbol>()
                                  where method.ReturnType.Equals(stringTypeSymbol) &&
                                        method.DeclaredAccessibility == Accessibility.Public
                                  select method.Name).Distinct())
            {
                Console.WriteLine(name);
            }

            var ifStatements = root.DescendantNodes().OfType<IfStatementSyntax>();
        }

        private static void GetAllProperties()
        {
            /*string solutionPath = @"C:\Users\mohyneenm\Documents\Visual Studio 2017\Projects\VSIXTestProject\VSIXTestProject.sln";
            var msWorkspace = MSBuildWorkspace.Create();

            var solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;
            var createCommandList = new List<ISymbol>();
            var project = solution.Projects.Where(p => p.Name == "VSIXTestProject").First();
            var compilation = project.GetCompilationAsync().Result;
            var @class = compilation.GetTypeByMetadataName("VSIXTestProject.Class1");*/

            var text = File.ReadAllText(@"C:\Users\mohyneenm\Documents\Visual Studio 2017\Projects\VSIXTestProject\VSIXTestProject\Class1.cs");
            var syntaxTree = CSharpSyntaxTree.ParseText(text);
            var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
            var compilation = CSharpCompilation.Create("HelloWorld")
                                               .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                                               .AddSyntaxTrees(syntaxTree);

            var model = compilation.GetSemanticModel(syntaxTree);
            var @class = compilation.GetTypeByMetadataName("VSIXTestProject.Class1");

            var methodSyntax = syntaxTree
                            .GetRoot()
                            .DescendantNodes()
                            .OfType<MethodDeclarationSyntax>()
                            .First(m => m.Identifier.ValueText == "ChangeState");
            var methodSymbol = model.GetDeclaredSymbol(methodSyntax);

            var lst = new List<string>();
            GetStateChanges(model, methodSyntax, lst);
            

            //var left = ((AssignmentExpressionSyntax)exps.Expression).Left;
            //var id = (IdentifierNameSyntax)left;
            //var name = model.GetSymbolInfo(id);

            var method = @class.GetMembers("ChangeState")
                                .ToList()
                                .Where(s => s.Kind == SymbolKind.Method)
                                .Cast<IMethodSymbol>()
                                .FirstOrDefault();
            var returnType = method.ReturnType as ITypeSymbol;
            var returnTypeProperties = returnType.GetMembers()
                                                 .ToList()
                                                 .Where(s => s.Kind == SymbolKind.Property)
                                                 .Select(s => s.Name);/**/
            var classPropertiesAndFields = @class.GetMembers().Where(s => s.Kind == SymbolKind.Property || s.Kind == SymbolKind.Field).Select(s => s.Name);

            var symbol = @class.GetMembers("Age").First();

            //var result = SymbolFinder.FindReferencesAsync(symbol, solution).Result.ToList();
        }

        private static void GetStateChanges(SemanticModel model, MethodDeclarationSyntax methodSyntax, List<string> changes)
        {
            var statements = methodSyntax.Body.Statements; /*.Where(s => 
                                                            s is ExpressionStatementSyntax ||
                                                            s is IfStatementSyntax ||
                                                            s is LocalDeclarationStatementSyntax);*/
            GetStateChanges(model, statements, changes);
        }

        private static void GetStateChanges(SemanticModel model, IEnumerable<StatementSyntax> statements, List<string> changes) {
            var allInvocations = statements.SelectMany(s => s.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>());

            // MyArray[0] = 20;     // pick the lhs of this stmt
            var leftElementAccessExpressions = statements
                                .SelectMany(s => s.DescendantNodes()
                                            .Where(n => n is ElementAccessExpressionSyntax &&
                                                    n.Parent is AssignmentExpressionSyntax &&
                                                    ((AssignmentExpressionSyntax)(n.Parent)).Left == n))
                                .Cast<ElementAccessExpressionSyntax>()
                                .Select(n => (model.GetSymbolInfo((IdentifierNameSyntax)n.Expression)))
                                .Where(si => si.Symbol.Kind == SymbolKind.Property)
                                .Select(si => si.Symbol.Name);

            changes.AddRange(leftElementAccessExpressions);

            //  MyAge = 20;     // pick the lhs of this stmt
            var propertyAssignmentExpressions = statements
                                .SelectMany(s => s.DescendantNodes()
                                            .Where(n => n is IdentifierNameSyntax &&
                                                    n.Parent is AssignmentExpressionSyntax &&
                                                    ((AssignmentExpressionSyntax)(n.Parent)).Left == n))
                                .Cast<IdentifierNameSyntax>()
                                .Select(n => model.GetSymbolInfo(n))
                                .Where(si => si.Symbol.Kind == SymbolKind.Property)
                                .Select(si => si.Symbol.Name);

            changes.AddRange(propertyAssignmentExpressions);

            //  counter++;
            // --counter;
            var postfixAssignmentExpressions = statements
                                .SelectMany(s => s.DescendantNodes()
                                            .Where(n => n is PostfixUnaryExpressionSyntax || n is PrefixUnaryExpressionSyntax))
                                .Where(n => model.GetSymbolInfo(n).Symbol.Kind == SymbolKind.Method)
                                .Select(n => n is PostfixUnaryExpressionSyntax 
                                            ? ((IdentifierNameSyntax)((PostfixUnaryExpressionSyntax)n).Operand).Identifier.Text
                                            : ((IdentifierNameSyntax)((PrefixUnaryExpressionSyntax)n).Operand).Identifier.Text);

            changes.AddRange(postfixAssignmentExpressions);

            // SetAge();
            statements.SelectMany(s => s.DescendantNodes()
                                .Where(n => n is InvocationExpressionSyntax &&
                                            ((InvocationExpressionSyntax)n).Expression is IdentifierNameSyntax)
                    .Select(n => model.GetSymbolInfo(n)))
                    .Select(si => si.Symbol.DeclaringSyntaxReferences[0].GetSyntax())
                    .ToList()
                    .ForEach(n => GetStateChanges(model, (MethodDeclarationSyntax)n, changes));



            foreach (var stmt in statements) {
                if (stmt is ExpressionStatementSyntax) {
                    var expStmt = (ExpressionStatementSyntax)stmt;
                    if (expStmt.Expression is AssignmentExpressionSyntax) {
                        /*var exp = expStmt.Expression as AssignmentExpressionSyntax;
                        
                        ExpressionSyntax expSyntax;
                        if (exp.Left is ElementAccessExpressionSyntax) {    // MyArray[0] = 20;
                            var leftExp = (ElementAccessExpressionSyntax)exp.Left;
                            expSyntax = (IdentifierNameSyntax)leftExp.Expression;
                        }
                        else if (exp.Left is IdentifierNameSyntax) {        // MyAge = 20;
                            expSyntax = (IdentifierNameSyntax)exp.Left;
                        }
                        else
                            expSyntax = null;

                        var symbolInfo = model.GetSymbolInfo(expSyntax);
                        //var symbolInfo = model.GetSymbolInfo(exp.Left);
                        if (symbolInfo.Symbol.Kind == SymbolKind.Property) {
                            changes.Add(symbolInfo.Symbol.Name);
                        }*/
                    }
                    else if (expStmt.Expression is PostfixUnaryExpressionSyntax) {
                        /*var exp = expStmt.Expression as PostfixUnaryExpressionSyntax;
                        var symbolInfo = model.GetSymbolInfo(exp);
                        if (symbolInfo.Symbol.Kind == SymbolKind.Method)
                            changes.Add(((IdentifierNameSyntax)exp.Operand).Identifier.ValueText);*/
                    }
                    else if (expStmt.Expression is InvocationExpressionSyntax) {
                        var ienumerableType = model.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
                        //var ienumerableType = model.Compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
                        var intType = model.Compilation.GetTypeByMetadataName("System.Int32");
                        var type = ienumerableType.Construct(ienumerableType);
                        IdentifierNameSyntax identifierSyntax;

                        var invocationExpSyntax = expStmt.Expression as InvocationExpressionSyntax;

                        if (invocationExpSyntax.Expression is IdentifierNameSyntax) {    // SetAge();
                            /*var methodSymbolInfo = model.GetSymbolInfo(invocationExpSyntax);
                            var syntaxNode = methodSymbolInfo.Symbol.DeclaringSyntaxReferences[0].GetSyntax();
                            GetStateChanges(model, (MethodDeclarationSyntax)syntaxNode, changes);*/
                        }
                        else if (invocationExpSyntax.Expression is MemberAccessExpressionSyntax) {   // ((List<string>)MyEnumerable_clear).Clear()
                            var memberAccessExpSyntax = (MemberAccessExpressionSyntax)invocationExpSyntax.Expression;
                            if (memberAccessExpSyntax.Expression is ParenthesizedExpressionSyntax) {
                                var parenExpSyntax = memberAccessExpSyntax.Expression as ParenthesizedExpressionSyntax;
                                if (parenExpSyntax.Expression is CastExpressionSyntax) {
                                    var castExpSyntax = parenExpSyntax.Expression as CastExpressionSyntax;
                                    identifierSyntax = (IdentifierNameSyntax)castExpSyntax.Expression;
                                    changes.Add(identifierSyntax.Identifier.Text);
                                }
                            }


                            var lst = statements.SelectMany(s => s.DescendantNodes()
                                    .OfType<InvocationExpressionSyntax>()
                                    .SelectMany(n => n.DescendantNodes())
                                    .Where(n => n is IdentifierNameSyntax &&
                                            (((IdentifierNameSyntax)n).Parent is MemberAccessExpressionSyntax ||
                                            ((IdentifierNameSyntax)n).Parent is CastExpressionSyntax))
                                    .Select(n => model.GetSymbolInfo(n))
                                    .Where(si => si.Symbol != null && si.Symbol.Kind == SymbolKind.Property));  // why are Symbols for LINQ extension methods null???


                        }
                        else { // IEnumerable call => MyList_add.Add("hi")
                            var methodSymbolInfo = model.GetSymbolInfo(invocationExpSyntax);
                            var classTypeSymbol = methodSymbolInfo.Symbol.ContainingType;
                            var memberAccessExpSyntax = (MemberAccessExpressionSyntax)invocationExpSyntax.Expression;
                            if (classTypeSymbol.OriginalDefinition.Interfaces.Select(i => i.MetadataName).ToList().Contains(ienumerableType.MetadataName)) {
                                identifierSyntax = (IdentifierNameSyntax)memberAccessExpSyntax.Expression;
                                changes.Add(identifierSyntax.Identifier.Text);/**/
                            }
                        }

                    }
                }
                else if (stmt is IfStatementSyntax) {
                    //var ifStmt = (IfStatementSyntax)stmt;
                    var nodes = stmt
                            .DescendantNodes()
                            .Where(n => n is BlockSyntax && (n.Parent is IfStatementSyntax || n.Parent is ElseClauseSyntax))
                            .Cast<BlockSyntax>();
                    foreach(var node in nodes) {
                        GetStateChanges(model, node.Statements, changes);
                    }
                }
                else if (stmt is LocalDeclarationStatementSyntax) {
                    var localDeclarationStmt = (LocalDeclarationStatementSyntax)stmt;
                    // grab Remove from:
                    // var c = MyList_remove.Remove("hi");
                    // obj.Method()
                    // MemberAccessExpressionSyntax.Expression = object node
                    // MemberAccessExpressionSyntax.Name => member node
                    var invocationStmt = localDeclarationStmt
                                    .DescendantNodes()
                                    .Where(
                                        n => n is MemberAccessExpressionSyntax &&
                                        ((MemberAccessExpressionSyntax)n).Name.Identifier.Text == "Remove" )
                                    .Cast<MemberAccessExpressionSyntax>()
                                    .Select(n => (IdentifierNameSyntax)n.Expression)
                                    .FirstOrDefault();

                    if (invocationStmt != null) {
                        var symbolInfo = model.GetSymbolInfo(invocationStmt);
                        if (symbolInfo.Symbol.Kind == SymbolKind.Property) {
                            changes.Add(symbolInfo.Symbol.Name);
                        }
                    }
                }
            }
        }
    }
}