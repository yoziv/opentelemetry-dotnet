// <copyright file="LoggerProviderBuilderExtensions.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

#nullable enable

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Internal;
using OpenTelemetry.Resources;

namespace OpenTelemetry.Logs;

/// <summary>
/// Contains extension methods for the <see cref="LoggerProviderBuilder"/> class.
/// </summary>
public static class LoggerProviderBuilderExtensions
{
    /// <summary>
    /// Sets the <see cref="ResourceBuilder"/> from which the Resource associated with
    /// this provider is built from. Overwrites currently set ResourceBuilder.
    /// You should usually use <see cref="ConfigureResource(LoggerProviderBuilder, Action{ResourceBuilder})"/> instead
    /// (call <see cref="ResourceBuilder.Clear"/> if desired).
    /// </summary>
    /// <param name="loggerProviderBuilder"><see cref="LoggerProviderBuilder"/>.</param>
    /// <param name="resourceBuilder"><see cref="ResourceBuilder"/> from which Resource will be built.</param>
    /// <returns>Returns <see cref="LoggerProviderBuilder"/> for chaining.</returns>
    public static LoggerProviderBuilder SetResourceBuilder(this LoggerProviderBuilder loggerProviderBuilder, ResourceBuilder resourceBuilder)
    {
        Guard.ThrowIfNull(resourceBuilder);

        loggerProviderBuilder.ConfigureBuilder((sp, builder) =>
        {
            if (builder is LoggerProviderBuilderSdk loggerProviderBuilderSdk)
            {
                loggerProviderBuilderSdk.SetResourceBuilder(resourceBuilder);
            }
        });

        return loggerProviderBuilder;
    }

    /// <summary>
    /// Modify the <see cref="ResourceBuilder"/> from which the Resource associated with
    /// this provider is built from in-place.
    /// </summary>
    /// <param name="loggerProviderBuilder"><see cref="LoggerProviderBuilder"/>.</param>
    /// <param name="configure">An action which modifies the provided <see cref="ResourceBuilder"/> in-place.</param>
    /// <returns>Returns <see cref="LoggerProviderBuilder"/> for chaining.</returns>
    public static LoggerProviderBuilder ConfigureResource(this LoggerProviderBuilder loggerProviderBuilder, Action<ResourceBuilder> configure)
    {
        Guard.ThrowIfNull(configure);

        loggerProviderBuilder.ConfigureBuilder((sp, builder) =>
        {
            if (builder is LoggerProviderBuilderSdk loggerProviderBuilderSdk)
            {
                loggerProviderBuilderSdk.ConfigureResource(configure);
            }
        });

        return loggerProviderBuilder;
    }

    /// <summary>
    /// Adds a processor to the provider.
    /// </summary>
    /// <param name="loggerProviderBuilder"><see cref="LoggerProviderBuilder"/>.</param>
    /// <param name="processor">LogRecord processor to add.</param>
    /// <returns>Returns <see cref="LoggerProviderBuilder"/> for chaining.</returns>
    public static LoggerProviderBuilder AddProcessor(this LoggerProviderBuilder loggerProviderBuilder, BaseProcessor<LogRecord> processor)
    {
        Guard.ThrowIfNull(processor);

        loggerProviderBuilder.ConfigureBuilder((sp, builder) =>
        {
            if (builder is LoggerProviderBuilderSdk loggerProviderBuilderSdk)
            {
                loggerProviderBuilderSdk.AddProcessor(processor);
            }
        });

        return loggerProviderBuilder;
    }

    /// <summary>
    /// Adds a processor to the provider which will be retrieved using dependency injection.
    /// </summary>
    /// <remarks>
    /// Note: The type specified by <typeparamref name="T"/> will be
    /// registered as a singleton service into application services.
    /// </remarks>
    /// <typeparam name="T">Processor type.</typeparam>
    /// <param name="loggerProviderBuilder"><see cref="LoggerProviderBuilder"/>.</param>
    /// <returns>The supplied <see cref="LoggerProviderBuilder"/> for chaining.</returns>
    public static LoggerProviderBuilder AddProcessor<
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        T>(this LoggerProviderBuilder loggerProviderBuilder)
        where T : BaseProcessor<LogRecord>
    {
        loggerProviderBuilder.ConfigureServices(services => services.TryAddSingleton<T>());

        loggerProviderBuilder.ConfigureBuilder((sp, builder) =>
        {
            if (builder is LoggerProviderBuilderSdk loggerProviderBuilderSdk)
            {
                loggerProviderBuilderSdk.AddProcessor(sp.GetRequiredService<T>());
            }
        });

        return loggerProviderBuilder;
    }

    /// <summary>
    /// Adds a processor to the provider which will be retrieved using dependency injection.
    /// </summary>
    /// <param name="loggerProviderBuilder"><see cref="LoggerProviderBuilder"/>.</param>
    /// <param name="implementationFactory">The factory that creates the service.</param>
    /// <returns>The supplied <see cref="LoggerProviderBuilder"/> for chaining.</returns>
    public static LoggerProviderBuilder AddProcessor(
        this LoggerProviderBuilder loggerProviderBuilder,
        Func<IServiceProvider, BaseProcessor<LogRecord>> implementationFactory)
    {
        Guard.ThrowIfNull(implementationFactory);

        loggerProviderBuilder.ConfigureBuilder((sp, builder) =>
        {
            if (builder is LoggerProviderBuilderSdk loggerProviderBuilderSdk)
            {
                loggerProviderBuilderSdk.AddProcessor(implementationFactory(sp));
            }
        });

        return loggerProviderBuilder;
    }

    /// <summary>
    /// Run the given actions to initialize the <see cref="LoggerProvider"/>.
    /// </summary>
    /// <param name="loggerProviderBuilder"><see cref="LoggerProviderBuilder"/>.</param>
    /// <returns><see cref="LoggerProvider"/>.</returns>
    public static LoggerProvider Build(this LoggerProviderBuilder loggerProviderBuilder)
    {
        if (loggerProviderBuilder is LoggerProviderBuilderBase loggerProviderBuilderBase)
        {
            return loggerProviderBuilderBase.Build();
        }

        return new LoggerProvider();
    }
}
