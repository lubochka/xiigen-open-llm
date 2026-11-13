// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProductHost;

using System.Collections;
using System.Net;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductHost.CapabilityServing;

public static class Program
{
    public const string ProductHostBoundary =
        "xiigen_product_host_loopback_capability_serving_no_public_deployment_no_training_no_product_ready";

    public static int Main(string[] args)
    {
        try
        {
            var replayVerification =
                ServerOwnedLogicalCapabilityReplayVerificationComposition
                    .CreateFromProcessEnvironment();
            var app = replayVerification.IsSuccess &&
                      replayVerification.Value is not null
                ? CreateHost(args, replayVerification.Value)
                : string.Equals(
                    replayVerification.ErrorCode,
                    ServerOwnedLogicalCapabilityReplayVerificationComposition
                        .ConfigurationRequiredErrorCode,
                    StringComparison.Ordinal)
                    ? CreateHost(args)
                    : throw new ArgumentException(
                        replayVerification.ErrorCode ??
                        ServerOwnedLogicalCapabilityReplayVerificationComposition
                            .ConfigurationInvalidErrorCode);
            app.Run();
            return 0;
        }
        catch (ArgumentException)
        {
            Console.Error.WriteLine("PRODUCT_HOST_STARTUP_CONFIGURATION_INVALID");
            return 2;
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            Console.Error.WriteLine("PRODUCT_HOST_NOT_READY");
            return 3;
        }
    }

    public static WebApplication CreateHost(string[]? args = null)
        => CreateHostCore(args, replayVerificationCapability: null);

    public static WebApplication CreateHost(
        string[]? args,
        LogicalCapabilityReplayVerificationCapability
            replayVerificationCapability)
    {
        ArgumentNullException.ThrowIfNull(replayVerificationCapability);
        return CreateHostCore(args, replayVerificationCapability);
    }

    public static WebApplication CreateHost(
        string[]? args,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority
            legalTeacherAdmissionAuthority)
    {
        ArgumentNullException.ThrowIfNull(legalTeacherAdmissionAuthority);
        var replayVerificationCapability = legalTeacherAdmissionAuthority
            .IssueReplayVerificationCapability();
        if (!replayVerificationCapability.IsSuccess ||
            replayVerificationCapability.Value is null)
        {
            throw new InvalidOperationException(
                replayVerificationCapability.ErrorCode ??
                LogicalCapabilityReplayVerificationCapability.RequiredErrorCode);
        }
        return CreateHostCore(args, replayVerificationCapability.Value);
    }

    private static WebApplication CreateHostCore(
        string[]? args,
        LogicalCapabilityReplayVerificationCapability?
            replayVerificationCapability)
    {
        var normalizedArgs = args ?? [];
        RejectExternalEndpointConfiguration(normalizedArgs, Environment.GetEnvironmentVariables());

        // Restore and validate the persisted serving graph before Kestrel can bind a listener.
        var configuration = CapabilityServingHostConfiguration.ResolveRequired(
            normalizedArgs,
            Environment.GetEnvironmentVariable);
        var composition = replayVerificationCapability is null
            ? CapabilityServingCompositionRoot.Create(configuration)
            : CapabilityServingCompositionRoot.Create(
                configuration,
                replayVerificationCapability);
        if (!composition.IsSuccess || composition.Value is null)
            throw new InvalidOperationException(composition.ErrorCode ?? "CAPABILITY_SERVING_NOT_READY");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = normalizedArgs,
            ApplicationName = typeof(Program).Assembly.FullName
        });
        RejectConfiguredEndpointSources(builder.Configuration);
        builder.Configuration.Sources.Clear();
        builder.WebHost.UseKestrel(options =>
        {
            options.Limits.MaxRequestBodySize =
                CapabilityServingAspNetEndpointMapper.MaximumRequestBodyBytes + 1;
            options.Listen(IPAddress.Loopback, 0);
        });
        builder.Logging.ClearProviders();

        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers["X-XIIGen-Product-Ready"] = "false";
                context.Response.Headers["X-XIIGen-Performs-Training"] = "false";
                context.Response.Headers["X-XIIGen-DPO-Ready"] = "false";
                context.Response.Headers["X-XIIGen-Teacher-Ready"] = "false";
                context.Response.Headers["X-XIIGen-Host-Boundary"] = ProductHostBoundary;
                return Task.CompletedTask;
            });
            await next(context);
        });

        app.ConfigureCapabilityServing(composition.Value);
        return app;
    }

    private static void RejectExternalEndpointConfiguration(
        IReadOnlyList<string> args,
        IDictionary environment)
    {
        var forbiddenArguments = new[]
        {
            "--urls",
            "--http_ports",
            "--https_ports",
            "--kestrel:",
            "--kestrel__"
        };
        foreach (var argument in args)
        {
            if (forbiddenArguments.Any(forbidden =>
                    argument.Equals(forbidden, StringComparison.OrdinalIgnoreCase) ||
                    argument.StartsWith(forbidden + "=", StringComparison.OrdinalIgnoreCase) ||
                    argument.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase) &&
                    forbidden.EndsWith(':')))
            {
                throw new ArgumentException("External endpoint configuration is not allowed.");
            }
        }

        foreach (DictionaryEntry entry in environment)
        {
            var name = entry.Key?.ToString();
            var value = entry.Value?.ToString();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                continue;

            if (name.Equals("ASPNETCORE_URLS", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("DOTNET_URLS", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("ASPNETCORE_HTTP_PORTS", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("ASPNETCORE_HTTPS_PORTS", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("DOTNET_HTTP_PORTS", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("DOTNET_HTTPS_PORTS", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("KESTREL__ENDPOINTS__", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("External endpoint configuration is not allowed.");
            }
        }
    }

    private static void RejectConfiguredEndpointSources(IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration["urls"]) ||
            !string.IsNullOrWhiteSpace(configuration["http_ports"]) ||
            !string.IsNullOrWhiteSpace(configuration["https_ports"]) ||
            configuration.GetSection("Kestrel:Endpoints").GetChildren().Any())
        {
            throw new ArgumentException("External endpoint configuration is not allowed.");
        }
    }

    private static bool IsNonFatal(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !string.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException",
               StringComparison.Ordinal);
}
