// ------------------------------------------------------------------------
// Copyright 2026 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;

namespace Dapr.Workflow;

internal static class WorkflowVersioningIntegration
{
    private const string RegistryTypeName = "Dapr.Workflow.Versioning.GeneratedWorkflowVersionRegistry";
    private const string RegisterMethodName = "RegisterGeneratedWorkflows";
    private const string VersioningOptionsTypeName = "Dapr.Workflow.Versioning.WorkflowVersioningOptions";
    private const string VersioningResolverTypeName = "Dapr.Workflow.Versioning.IWorkflowVersionResolver";

    internal static void TryRegisterGeneratedWorkflows(WorkflowRuntimeOptions options, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(services);

        if (!IsVersioningEnabled(services))
            return;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
                continue;

            Type? registryType;
            try
            {
                registryType = assembly.GetType(RegistryTypeName, throwOnError: false, ignoreCase: false);
            }
            catch
            {
                continue;
            }

            var method = registryType?.GetMethod(RegisterMethodName, BindingFlags.Public | BindingFlags.Static);
            if (method is null)
            {
                continue;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 2 ||
                parameters[0].ParameterType != typeof(WorkflowRuntimeOptions) ||
                parameters[1].ParameterType != typeof(IServiceProvider))
            {
                continue;
            }

            method.Invoke(null, [options, services]);
        }
    }

    private static bool IsVersioningEnabled(IServiceProvider services)
    {
        return TryGetVersioningService(services, VersioningOptionsTypeName, out _)
               && TryGetVersioningService(services, VersioningResolverTypeName, out _);
    }

    private static bool TryGetVersioningService(IServiceProvider services, string typeName, out object? service)
    {
        service = null;
        var type = FindType(typeName);
        if (type is null)
        {
            return false;
        }

        service = services.GetService(type);
        return service is not null;
    }

    private static Type? FindType(string fullName) =>
        (from assembly in AppDomain.CurrentDomain.GetAssemblies()
            where !assembly.IsDynamic
            select assembly.GetType(fullName, throwOnError: false, ignoreCase: false)).OfType<Type>().FirstOrDefault();
}
