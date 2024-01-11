using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapr.Workflow.Analyzers.Test;
using VerifyCS =
    Tests.MsTest.Verifiers.CSharpAnalyzerVerifier<
        Analyzers.DAPR1001WorkflowDependencyInjectionValidationAnalyzer>;

[TestClass]
public class WorkflowDependencyInjectionValidationTests
{
    [TestMethod]
    public async Task ShouldNotWarnIfThereAreNoWorkflowsAvailable()
    {
        var test =
            @"
using System;
using System.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
	public void Run(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
		{
			services.AddDaprWorkflow(opt => { });
		});

		using var host = builder.Build();
		host.Start();
	}
}

public static class DaprExtensions
{
    public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
    {
        return serviceCollection;
    }    
}

public sealed class WorkflowRuntimeOptions
{
    public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
    {
    }
} 

public interface IWorkflow {}

public abstract class Workflow<TIn, TOut> : IWorkflow {}
";
        var refAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
            .AddPackages(ImmutableArray.Create(
                new PackageIdentity("Microsoft.Extensions.Hosting", "8.0.0"),
                new PackageIdentity("Microsoft.Extensions.DependencyInjection", "8.0.0")))
            .AddAssemblies(
                ImmutableArray.Create(
                    "Microsoft.Extensions.DependencyInjection.Abstractions",
                    "Microsoft.Extensions.DependencyInjection.Abstractions",
                    "Microsoft.Extensions.Hosting",
                    "Microsoft.Extensions.Hosting.Abstractions",
                    "Microsoft.Extensions.Logging.Abstractions"));

        await new VerifyCS.Test
        {
            ReferenceAssemblies = refAssemblies, TestState = {Sources = {test}, ExpectedDiagnostics = {}}
        }.RunAsync();
    }

    [TestMethod]
    public async Task ShouldNotWarnWhenWorkflowAlreadyRegistered()
    {
        var test =
            @"
using System;
using System.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
	public void Run(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
		{
			services.AddDaprWorkflow(opt => 
            {
                opt.RegisterWorkflow<MyWorkflow>(); 
            });
		});

		using var host = builder.Build();
		host.Start();
	}
}

public static class DaprExtensions
{
    public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
    {
        return serviceCollection;
    }    
}

public sealed class WorkflowRuntimeOptions
{
    public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
    {
    }
} 

public interface IWorkflow {}

public abstract class Workflow<TIn, TOut> : IWorkflow {}

public class MyWorkflow : Workflow<string, bool> {}
";

        var refAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
            .AddPackages(ImmutableArray.Create(
                new PackageIdentity("Microsoft.Extensions.Hosting", "8.0.0"),
                new PackageIdentity("Microsoft.Extensions.DependencyInjection", "8.0.0")))
            .AddAssemblies(
                ImmutableArray.Create(
                    "Microsoft.Extensions.DependencyInjection.Abstractions",
                    "Microsoft.Extensions.DependencyInjection.Abstractions",
                    "Microsoft.Extensions.Hosting",
                    "Microsoft.Extensions.Hosting.Abstractions",
                    "Microsoft.Extensions.Logging.Abstractions"));

        await new VerifyCS.Test
        {
            ReferenceAssemblies = refAssemblies, TestState = {Sources = {test}, ExpectedDiagnostics = {}}
        }.RunAsync();
    }
    
    [TestMethod]
    public async Task ShouldWarnOnMissingWorkflow()
    {
        var test = 
@"
using System;
using System.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
	public void Run(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
		{
			services.AddDaprWorkflow(opt => { });
		});

		using var host = builder.Build();
		host.Start();
	}
}

public static class DaprExtensions
{
    public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
    {
        return serviceCollection;
    }    
}

public sealed class WorkflowRuntimeOptions
{
    public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
    {
    }
} 

public interface IWorkflow {}

public abstract class Workflow<TIn, TOut> : IWorkflow {}

public class MyWorkflow : Workflow<string, bool> {}
";

		var expected = VerifyCS.Diagnostic()
			.WithMessage(
				$"The workflow type 'MyWorkflow' is not registered with the service provider")
            .WithSpan(40, 14, 40, 24)
			.WithSeverity(DiagnosticSeverity.Warning);

		var refAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
			.AddPackages(ImmutableArray.Create(
				new PackageIdentity("Microsoft.Extensions.Hosting", "8.0.0"),
				new PackageIdentity("Microsoft.Extensions.DependencyInjection", "8.0.0")))
			.AddAssemblies(
				ImmutableArray.Create(
					"Microsoft.Extensions.DependencyInjection.Abstractions",
					"Microsoft.Extensions.DependencyInjection.Abstractions",
					"Microsoft.Extensions.Hosting",
					"Microsoft.Extensions.Hosting.Abstractions",
					"Microsoft.Extensions.Logging.Abstractions"));

		await new VerifyCS.Test
		{
			ReferenceAssemblies = refAssemblies,
			TestState =
			{
				Sources = {test},
				ExpectedDiagnostics = {expected}
			}
		}.RunAsync();
    }
    
    [TestMethod]
    public async Task ShouldWarnOnMultipleMissingWorkflows()
    {
        var test =
            @"
using System;
using System.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
	public void Run(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
		{
			services.AddDaprWorkflow(options => {});
		});

		using var host = builder.Build();
		host.Start();
	}
}

public static class DaprExtensions
{
    public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
    {
        return serviceCollection;
    }
}

public sealed class WorkflowRuntimeOptions
{
    public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
    {
    }
} 

public interface IWorkflow {}

public abstract class Workflow<TIn, TOut> : IWorkflow {}

public class MyWorkflow : Workflow<string, bool> {}

public class MyOtherWorkflow : Workflow<string, bool> {};
";

        var expectMyWorkflow = VerifyCS.Diagnostic()
            .WithMessage(
                $"The workflow type 'MyWorkflow' is not registered with the service provider")
            .WithSpan(40, 14, 40, 24)
            .WithSeverity(DiagnosticSeverity.Warning);

        var expectMyOtherWorkflow = VerifyCS.Diagnostic()
            .WithMessage($"The workflow type 'MyOtherWorkflow' is not registered with the service provider")
            .WithSpan(42, 14, 42, 29)
            .WithSeverity(DiagnosticSeverity.Warning);

        var refAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
            .AddPackages(ImmutableArray.Create(
                new PackageIdentity("Microsoft.Extensions.Hosting", "8.0.0"),
                new PackageIdentity("Microsoft.Extensions.DependencyInjection", "8.0.0")))
            .AddAssemblies(
                ImmutableArray.Create(
                    "Microsoft.Extensions.DependencyInjection.Abstractions",
                    "Microsoft.Extensions.DependencyInjection.Abstractions",
                    "Microsoft.Extensions.Hosting",
                    "Microsoft.Extensions.Hosting.Abstractions",
                    "Microsoft.Extensions.Logging.Abstractions"));

        await new VerifyCS.Test
        {
            ReferenceAssemblies = refAssemblies,
            TestState = {Sources = {test}, ExpectedDiagnostics = {expectMyWorkflow, expectMyOtherWorkflow}}
        }.RunAsync();
    }

    [TestMethod]
    public async Task ShouldNotReportOnExistingRegistrationButShouldOnMissingRegistration()
    {
        var test =
            @"
using System;
using System.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
	public void Run(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
		{
			services.AddDaprWorkflow(opt => 
            {
                opt.RegisterWorkflow<MyWorkflow>(); 
            });
		});

		using var host = builder.Build();
		host.Start();
	}
}

public static class DaprExtensions
{
    public static IServiceCollection AddDaprWorkflow(this IServiceCollection serviceCollection, Action<WorkflowRuntimeOptions> configure) 
    {
        return serviceCollection;
    }    
}

public sealed class WorkflowRuntimeOptions
{
    public void RegisterWorkflow<TWorkflow>() where TWorkflow : class, IWorkflow, new()
    {
    }
} 

public interface IWorkflow {}

public abstract class Workflow<TIn, TOut> : IWorkflow {}

public class MyWorkflow : Workflow<string, bool> {}

public class MyUnregisteredWorkflow : Workflow<string, bool> {};
";

        var expected = VerifyCS.Diagnostic()
            .WithMessage(
                $"The workflow type 'MyUnregisteredWorkflow' is not registered with the service provider")
            .WithSpan(45, 14, 45, 36)
            .WithSeverity(DiagnosticSeverity.Warning);

        var refAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
            .AddPackages(ImmutableArray.Create(
                new PackageIdentity("Microsoft.Extensions.Hosting", "8.0.0"),
                new PackageIdentity("Microsoft.Extensions.DependencyInjection", "8.0.0")))
            .AddAssemblies(
                ImmutableArray.Create(
                    "Microsoft.Extensions.DependencyInjection.Abstractions",
                    "Microsoft.Extensions.DependencyInjection.Abstractions",
                    "Microsoft.Extensions.Hosting",
                    "Microsoft.Extensions.Hosting.Abstractions",
                    "Microsoft.Extensions.Logging.Abstractions"));

        await new VerifyCS.Test
        {
            ReferenceAssemblies = refAssemblies, TestState = {Sources = {test}, ExpectedDiagnostics = {expected}}
        }.RunAsync();
    }
}
