// ------------------------------------------------------------------------
// Copyright 2023 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Dapr.AspNetCore;
using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dapr.AspNetCore
{
    /// <summary>
    /// Provides extension methods for the <see cref="IDataProtectionBuilder"/>.
    /// </summary>
    public static class DaprDataProtectionExtensions
    {
        /// <summary>
        /// Configures how the data protection keys are persisted to the Dapr secret store.
        /// </summary>
        /// <returns></returns>
        public static IDataProtectionBuilder PersistKeysToDaprState(this IDataProtectionBuilder builder,
            Action<DaprDataProtectionOptions> setupAction)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(setupAction);

            builder.Services.AddDaprDataProtection(setupAction);
            return builder;
        }
    }

    /// <summary>
    /// Implements an XML repository for storing Data Protection data in a Dapr secret store.
    /// </summary>
    public sealed class DaprXmlRepository : IXmlRepository
    {
        private readonly string _repositoryKey;
        private readonly string _secretStoreName;
        private readonly DaprClient _daprClient;
        private const string KeyPrefix = "dapr-dataprotection";

        /// <summary>
        /// Instantiates a new instance of a <see cref="DaprXmlRepository"/>.
        /// </summary>
        /// <param name="daprClient"></param>
        /// <param name="options"></param>
        public DaprXmlRepository(DaprClient daprClient, IOptionsMonitor<DaprDataProtectionOptions> options)
        {
            _daprClient = daprClient;
            _secretStoreName = options.CurrentValue.SecretStoreName;
            _repositoryKey = options.CurrentValue.SecretName;
        }

        /// <summary>
        /// Gets all top-level XML elements in the repository.
        /// </summary>
        /// <remarks>All top-level elements in the repository.</remarks>
        public IReadOnlyCollection<XElement> GetAllElements() => GetElementListFromState();

        private byte[] ConvertElementListToByteArray(IEnumerable<XElement> elements)
        {
            using var memoryStream = new MemoryStream();
            new XmlSerializer(typeof(List<string>)).Serialize((Stream)memoryStream,
                (object)elements
                    .Select((Func<XElement, string>)(e => e.ToString(SaveOptions.DisableFormatting)))
                    .ToList());
            return memoryStream.GetBuffer();
        }

        private static List<XElement> GetElementList(string value)
        {
            var elementList = new List<XElement>();
            if (string.IsNullOrWhiteSpace(value)) return elementList;

            if (value.Contains("\0"))
                value = value.Replace("\0", "");
            var xmlSerializer = new XmlSerializer(typeof(List<string>));
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
            elementList.AddRange(from text in (List<string>)xmlSerializer.Deserialize((Stream)memoryStream) select XElement.Parse(text));
            return elementList;
        }

        /// <summary>
        /// Adds a top-level XML element to the repository.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <param name="friendlyName">An optional name to be associated with the XML element.
        /// For instance, if this repository stores XML files on disk, the friendly name may
        /// be used as part of the file name. Repository implementations are not required to
        /// observe this parameter even if it has been provided by the caller.</param>
        /// <remarks>
        /// The 'friendlyName' parameter must be unique if specified. For instance, it could
        /// be the id of the key being stored.
        /// </remarks>
        public void StoreElement(XElement element, string friendlyName)
        {
            var elementList = GetElementListFromState();
            elementList.Add(element);

            //Save the element list back to state
            var byteArray = ConvertElementListToByteArray(elementList);
            _daprClient.SaveStateAsync(_secretStoreName, $"{KeyPrefix}-{_repositoryKey}", byteArray).Wait();
        }

        /// <summary>
        /// Retrieves the element list from the state.
        /// </summary>
        /// <returns></returns>
        private List<XElement> GetElementListFromState()
        {
            var stateStr = _daprClient.GetStateAsync<byte[]>(_secretStoreName, $"{KeyPrefix}-{_repositoryKey}").ConfigureAwait(false).GetAwaiter()
                .GetResult() ?? Array.Empty<byte>();
            var state = Encoding.UTF8.GetString(stateStr);
            return GetElementList(state);
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides the implementation used to add data protection capabilities on Dapr.
    /// </summary>
    public static class DaprDataProtectionServiceExtensions
    {
        /// <summary>
        /// Implements data protection using the Dapr secret store.
        /// </summary>
        /// <param name="services">The service collection used in dependency injection.</param>
        /// <param name="setupAction">Provides the options used to configure the data protection functionality.</param>
        /// <returns></returns>
        public static IServiceCollection AddDaprDataProtection(this IServiceCollection services,
            Action<DaprDataProtectionOptions> setupAction)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(setupAction);

            services.AddDaprClient();
            services.AddOptions();
            services.Configure<DaprDataProtectionOptions>(setupAction);
            services.AddSingleton<IXmlRepository, DaprXmlRepository>();
            return services;
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Options providing the information necessary to configure the Dapr data protection behavior.
    /// </summary>
    public sealed class DaprDataProtectionOptions
    {
        /// <summary>
        /// The name of the secret store to save the data protection keys in.
        /// </summary>
        public string SecretStoreName { get; }

        /// <summary>
        /// The name of the secret storing the data protection keys.
        /// </summary>
        public string SecretName { get; }

        /// <summary>
        /// Instantiates the options with the necessary values for properly configuring the Data Protection functionality.
        /// </summary>
        /// <param name="secretStoreName">The name of the secret store to save the data protection keys to.</param>
        /// <param name="secretName">The name of the secret storing the data protection keys.</param>
        public DaprDataProtectionOptions(string secretStoreName, string secretName)
        {
            SecretStoreName = secretStoreName;
            SecretName = secretName;
        }
    }
}
