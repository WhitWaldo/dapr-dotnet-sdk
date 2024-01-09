using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapr.Workflow.Analyzers.Test;

using VerifyCS =
    Tests.MsTest.Verifiers.CSharpAnalyzerVerifier<
        Analyzers.CSharpWorkflowActivityDependencyInjectionValidationAnalyzer>;

[TestClass]
public class WorkflowActivityDependencyInjectionValidationTests
{
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
            ReferenceAssemblies = refAssemblies, TestState = {Sources = {test}, ExpectedDiagnostics = { }}
        }.RunAsync();
    }
    
    [TestMethod]
    public async Task ShouldNotWarnIfThereAreNoWorkflowActivitiesAvailable()
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

public sealed class MyWorkflow : Workflow<string, bool> {}
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
            ReferenceAssemblies = refAssemblies, TestState = {Sources = {test}, ExpectedDiagnostics = { }}
        }.RunAsync();
    }

    [TestMethod]
    public async Task ShouldNotWarnWhenWorkflowActivitiesAreAlreadyRegistered()
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
                opt.RegisterActivity<MyActivity>();
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

    public void RegisterActivity<TActivity>() where TActivity : class, IWorkflowActivity 
    {
    }
} 

public interface IWorkflow {}

public interface IWorkflowActivity {}

public abstract class Workflow<TIn, TOut> : IWorkflow {}

public abstract class WorkflowActivity<TIn, TOut> : IWorkflowActivity {}

public class MyWorkflow : Workflow<string, bool> {}

public class MyActivity : WorkflowActivity<string, object> {}
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
            ReferenceAssemblies = refAssemblies, TestState = {Sources = {test}, ExpectedDiagnostics = { }}
        }.RunAsync();
    }

    [TestMethod]
    public async Task ShouldWarnOnMissingWorkflowActivity()
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

    public void RegisterActivity<TActivity>() where TActivity : class, IWorkflowActivity 
    {
    }
} 

public interface IWorkflow {}

public interface IWorkflowActivity {}

public abstract class Workflow<TIn, TOut> : IWorkflow {}

public abstract class WorkflowActivity<TIn, TOut> : IWorkflowActivity {}

public class MyWorkflow : Workflow<string, bool> {}

public class MyActivity : WorkflowActivity<string, object> {}
";
        var expected = VerifyCS.Diagnostic()
            .WithMessage("The type 'MyActivity' is not registered with the service provider")
            .WithSpan(53, 14, 53, 24)
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
            ReferenceAssemblies = refAssemblies, TestState = {Sources = {test}, ExpectedDiagnostics = { expected }}
        }.RunAsync();
    }

    [TestMethod]
    public async Task ShouldWarnOnMissingWorkflowActivities()
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
                opt.RegisterActivity<MyActivity>();
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

    public void RegisterActivity<TActivity>() where TActivity : class, IWorkflowActivity 
    {
    }
} 

public interface IWorkflow {}

public interface IWorkflowActivity {}

public abstract class Workflow<TIn, TOut> : IWorkflow {}

public abstract class WorkflowActivity<TIn, TOut> : IWorkflowActivity {}

public class MyWorkflow : Workflow<string, bool> {}

public class MyActivity : WorkflowActivity<string, object> {}
public class MyOtherActivity : WorkflowActivity<string, object> {}
public class AnotherActivity : WorkflowActivity<bool, long> {}
";
        var expectedMyOtherActivity = VerifyCS.Diagnostic()
            .WithMessage("The type 'MyOtherActivity' is not registered with the service provider")
            .WithSpan(55, 14, 55, 29)
            .WithSeverity(DiagnosticSeverity.Warning);

        var expectedAnotherActivity = VerifyCS.Diagnostic()
            .WithMessage("The type 'AnotherActivity' is not registered with the service provider")
            .WithSpan(56, 14, 56, 29)
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
            TestState = {Sources = {test}, ExpectedDiagnostics = {expectedMyOtherActivity, expectedAnotherActivity}}
        }.RunAsync();
    }
}
