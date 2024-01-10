// using System.Collections.Immutable;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.Testing;
// using Microsoft.VisualStudio.TestTools.UnitTesting;
//
// namespace Dapr.Workflow.Analyzers.Test;
//
// using VerifyCS = Tests.MsTest.Verifiers.CSharpCodeFixVerifier<
//     Analyzers.CSharpWorkflowDependencyInjectionValidationAnalyzer, 
//     CodeFixes.CSharpWorkflowDependencyInjectionAnalyzerCodeFixProvider>;
//
// [TestClass]
// public class WorkflowDependencyInjectionValidationCodeFixTests
// {
//     [TestMethod]
//     public async Task ShouldRemainUnchangedIfNoWorkflowsFound()
//     {
//         var test =
//             @"
// using System;
// using System.Runtime;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
//
// public class Program
// {
// 	public void Run(string[] args)
// 	{
// 		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
// 		{
// 			services.AddDaprWorkflow(opt => { });
// 		});
//
// 		using var host = builder.Build();
// 		host.Start();
// 	}
// }
//
// public static class DaprExtensions
// {
//     public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
//     {
//         return serviceCollection;
//     }    
// }
//
// public sealed class WorkflowRuntimeOptions
// {
//     public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
//     {
//     }
// } 
//
// public interface IWorkflow {}
//
// public abstract class Workflow<TIn, TOut> : IWorkflow {}
// ";
//         var refAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
//             .AddPackages(ImmutableArray.Create(
//                 new PackageIdentity("Microsoft.Extensions.Hosting", "8.0.0"),
//                 new PackageIdentity("Microsoft.Extensions.DependencyInjection", "8.0.0")))
//             .AddAssemblies(
//                 ImmutableArray.Create(
//                     "Microsoft.Extensions.DependencyInjection.Abstractions",
//                     "Microsoft.Extensions.DependencyInjection.Abstractions",
//                     "Microsoft.Extensions.Hosting",
//                     "Microsoft.Extensions.Hosting.Abstractions",
//                     "Microsoft.Extensions.Logging.Abstractions"));
//
//         await new VerifyCS.Test
//         {
//             ReferenceAssemblies = refAssemblies,
//             TestState = {Sources = {test}},
//             FixedCode = test,
//             ExpectedDiagnostics = { }
//         }.RunAsync();
//     }
//
//     [TestMethod]
//     public async Task ShouldRemainUnchangedIfWorkflowAlreadyRegistered()
//     {
//         var test =
//             @"
// using System;
// using System.Runtime;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
//
// public class Program
// {
// 	public void Run(string[] args)
// 	{
// 		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
// 		{
// 			services.AddDaprWorkflow(opt => 
//             {
//                 opt.RegisterWorkflow<MyWorkflow>(); 
//             });
// 		});
//
// 		using var host = builder.Build();
// 		host.Start();
// 	}
// }
//
// public static class DaprExtensions
// {
//     public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
//     {
//         return serviceCollection;
//     }    
// }
//
// public sealed class WorkflowRuntimeOptions
// {
//     public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
//     {
//     }
// } 
//
// public interface IWorkflow {}
//
// public abstract class Workflow<TIn, TOut> : IWorkflow {}
//
// public sealed class MyWorkflow : Workflow<string, object> {};
// ";
//         var refAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
//             .AddPackages(ImmutableArray.Create(
//                 new PackageIdentity("Microsoft.Extensions.Hosting", "8.0.0"),
//                 new PackageIdentity("Microsoft.Extensions.DependencyInjection", "8.0.0")))
//             .AddAssemblies(
//                 ImmutableArray.Create(
//                     "Microsoft.Extensions.DependencyInjection.Abstractions",
//                     "Microsoft.Extensions.DependencyInjection.Abstractions",
//                     "Microsoft.Extensions.Hosting",
//                     "Microsoft.Extensions.Hosting.Abstractions",
//                     "Microsoft.Extensions.Logging.Abstractions"));
//
//         await new VerifyCS.Test
//         {
//             ReferenceAssemblies = refAssemblies,
//             TestState = {Sources = {test}},
//             FixedCode = test,
//             ExpectedDiagnostics = { }
//         }.RunAsync();
//     }
//
//     [TestMethod]
//     public async Task ShouldCorrectMissingWorkflow()
//     {
//         var test = @"
// using System;
// using System.Runtime;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
//
// public class Program
// {
// 	public void Run(string[] args)
// 	{
// 		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
// 		{
// 			services.AddDaprWorkflow(opt => 
//             { 
//             });
// 		});
//
// 		using var host = builder.Build();
// 		host.Start();
// 	}
// }
//
// public static class DaprExtensions
// {
//     public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
//     {
//         return serviceCollection;
//     }    
// }
//
// public sealed class WorkflowRuntimeOptions
// {
//     public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
//     {
//     }
// } 
//
// public interface IWorkflow {}
//
// public abstract class Workflow<TIn, TOut> : IWorkflow {}
//
// public sealed class MyWorkflow : Workflow<string, object> {};
// ";
//         
//         var expected = @"
// using System;
// using System.Runtime;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
//
// public class Program
// {
// 	public void Run(string[] args)
// 	{
// 		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
// 		{
// 			services.AddDaprWorkflow(opt => opt.RegisterWorkflow<MyWorkflow>());
// 		});
//
// 		using var host = builder.Build();
// 		host.Start();
// 	}
// }
//
// public static class DaprExtensions
// {
//     public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
//     {
//         return serviceCollection;
//     }    
// }
//
// public sealed class WorkflowRuntimeOptions
// {
//     public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
//     {
//     }
// } 
//
// public interface IWorkflow {}
//
// public abstract class Workflow<TIn, TOut> : IWorkflow {}
//
// public sealed class MyWorkflow : Workflow<string, object> {}";
//         
//         var expectedDiagnostic = VerifyCS.Diagnostic()
//             .WithMessage(
//                 $"The workflow type 'MyWorkflow' is not registered with the service provider")
//             .WithSpan(42, 21, 42, 31) //Doesn't seem to like something about this line
//             .WithArguments("MyWorkflow")
//             .WithSeverity(DiagnosticSeverity.Warning);
//         
//         var refAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
//             .AddPackages(ImmutableArray.Create(
//                 new PackageIdentity("Microsoft.Extensions.Hosting", "8.0.0"),
//                 new PackageIdentity("Microsoft.Extensions.DependencyInjection", "8.0.0")))
//             .AddAssemblies(
//                 ImmutableArray.Create(
//                     "Microsoft.Extensions.DependencyInjection.Abstractions",
//                     "Microsoft.Extensions.DependencyInjection.Abstractions",
//                     "Microsoft.Extensions.Hosting",
//                     "Microsoft.Extensions.Hosting.Abstractions",
//                     "Microsoft.Extensions.Logging.Abstractions"));
//
//         await new VerifyCS.Test
//         {
//             ReferenceAssemblies = refAssemblies,
//             TestState = {Sources = {test}},
//             FixedCode = expected,
//             ExpectedDiagnostics = { expectedDiagnostic }
//         }.RunAsync();
//     }
//
//     [TestMethod]
//     public async Task ShouldRegisterMultipleWorkflowsAsNecessary()
//     {
//         var test = @"
// using System;
// using System.Runtime;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
//
// public class Program
// {
// 	public void Run(string[] args)
// 	{
// 		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
// 		{
// 			services.AddDaprWorkflow(opt => 
//             { 
//             });
// 		});
//
// 		using var host = builder.Build();
// 		host.Start();
// 	}
// }
//
// public static class DaprExtensions
// {
//     public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
//     {
//         return serviceCollection;
//     }    
// }
//
// public sealed class WorkflowRuntimeOptions
// {
//     public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
//     {
//     }
// } 
//
// public interface IWorkflow {}
//
// public abstract class Workflow<TIn, TOut> : IWorkflow {}
//
// public sealed class MyWorkflow : Workflow<string, object> {}
// public sealed class MyOtherWorkflow : Workflow<bool, long> {}";
//         
//         var expected = @"
// using System;
// using System.Runtime;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;
//
// public class Program
// {
// 	public void Run(string[] args)
// 	{
// 		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
// 		{
// 			services.AddDaprWorkflow(opt => 
//             {
//                 opt.RegisterWorkflow<MyWorkflow>();
//                 opt.RegisterWorkflow<MyOtherWorkflow>();
//             });
// 		});
//
// 		using var host = builder.Build();
// 		host.Start();
// 	}
// }
//
// public static class DaprExtensions
// {
//     public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
//     {
//         return serviceCollection;
//     }    
// }
//
// public sealed class WorkflowRuntimeOptions
// {
//     public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
//     {
//     }
// } 
//
// public interface IWorkflow {}
//
// public abstract class Workflow<TIn, TOut> : IWorkflow {}
//
// public sealed class MyWorkflow : Workflow<string, object> {}
// public sealed class MyOtherWorkflow : Workflow<bool, long> {}";
//         
//         var expectedDiagnostic = VerifyCS.Diagnostic()
//             .WithMessage(
//                 $"The workflow type 'MyWorkflow' is not registered with the service provider")
//             .WithSpan(42, 21, 42, 31) //Doesn't seem to like something about this line
//             .WithArguments("MyWorkflow")
//             .WithSeverity(DiagnosticSeverity.Warning);
//
//         var expectedDiagnostic2 = VerifyCS.Diagnostic()
//             .WithMessage(
//                 $"The workflow type 'MyOtherWorkflow' is not registered with the service provider")
//             .WithSpan(43, 21, 43, 36) //Doesn't seem to like something about this line
//             .WithArguments("MyWorkflow")
//             .WithSeverity(DiagnosticSeverity.Warning);
//         
//         var refAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
//             .AddPackages(ImmutableArray.Create(
//                 new PackageIdentity("Microsoft.Extensions.Hosting", "8.0.0"),
//                 new PackageIdentity("Microsoft.Extensions.DependencyInjection", "8.0.0")))
//             .AddAssemblies(
//                 ImmutableArray.Create(
//                     "Microsoft.Extensions.DependencyInjection.Abstractions",
//                     "Microsoft.Extensions.DependencyInjection.Abstractions",
//                     "Microsoft.Extensions.Hosting",
//                     "Microsoft.Extensions.Hosting.Abstractions",
//                     "Microsoft.Extensions.Logging.Abstractions"));
//
//         await new VerifyCS.Test
//         {
//             ReferenceAssemblies = refAssemblies,
//             TestState = {Sources = {test}},
//             FixedCode = expected,
//             ExpectedDiagnostics = { expectedDiagnostic, expectedDiagnostic2 }
//         }.RunAsync();
//     }
// }
