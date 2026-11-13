# ADR: .NET Queue And Cache Fabric Providers

Status: Approved for implementation
Date: 2026-05-14

## Context

Platform work package P-03 requires queue and cache fabric interfaces, but an
interface-only package is not enough for implementation. The current NestJS
baseline uses AWS SQS through the scheduler/queue fabric and Redis through
scoped memory and idempotency paths. The .NET translation needs named provider
choices before local Docker tests, auth certification, and flow cascade tests
can run.

The provider decision must preserve these rules:

- business services depend on XIIGen fabric interfaces, never provider SDKs;
- service-to-service queue messages use `CloudEventEnvelope`;
- mutations and consumers use idempotency before side effects;
- unit tests can run without external infrastructure;
- Docker integration tests use real local dependencies.

## Proposed Default

Use a provider-neutral `IQueueFabric` with MassTransit as the .NET queue host
library.

- Unit tests use a XIIGen in-memory queue provider and broker stub capable of
  proving dispatch contracts without a live broker.
- Local Docker and integration tests use MassTransit with RabbitMQ.
- SQS-compatible/cloud deployments use the MassTransit Amazon SQS transport.
- Business services never reference MassTransit, RabbitMQ, or AWS SDK types.

Use `ICacheFabric` backed by StackExchange.Redis for Docker and deployed
runtime paths.

- Unit tests use an in-memory cache provider.
- Docker integration tests use Redis.
- Idempotency uses Redis `SET NX` semantics through the fabric, not direct
  Redis calls from business services.

## MassTransit Consumer Registration

Each flow package that owns queue consumers exposes one registration class:

```csharp
public interface IFlowConsumerRegistration
{
    void RegisterConsumers(IBusRegistrationConfigurator configurator);
}
```

The platform host discovers these registrations from loaded flow manifests and
flow assemblies. The discovery rule is:

1. Resolve the flow API assembly from `flow.manifest.json`
   `serverEntrypoint`.
2. Load the assembly through the approved platform flow assembly loader.
3. Reflect for exactly one public, non-abstract type implementing
   `IFlowConsumerRegistration`.
4. Instantiate the registration through dependency injection.
5. Fail readiness if the assembly declares queue consumers but no registration
   type, or if more than one registration type is found.

The host calls discovered registrations inside the single MassTransit setup
block:

```csharp
public interface IFlowAssemblyLoader
{
    Task<DataProcessResult<FlowAssemblyLoadResult>> LoadAsync(
        FlowManifest manifest,
        CancellationToken cancellationToken = default);
}

public sealed record FlowAssemblyLoadResult(
    string FlowId,
    string AssemblyName,
    Assembly Assembly,
    IReadOnlyList<Type> ConsumerRegistrationTypes);
```

`IFlowAssemblyLoader` is the approved platform flow assembly loader referenced
by discovery. It validates package provenance and manifest path boundaries
before returning an assembly. It must not load arbitrary file paths supplied by
requests or tenant runtime data.

Generated flow assembly and namespace conventions are fixed so the assembly
loader can resolve manifests deterministically:

```text
flowSlug: user-registration
PascalSlug: UserRegistration
server project: src/Xiigen.Flow.UserRegistration/Xiigen.Flow.UserRegistration.csproj
assembly file: Xiigen.Flow.UserRegistration.dll
root namespace: Xiigen.Flow.UserRegistration
test project: tests/Xiigen.Flow.UserRegistration.Tests/Xiigen.Flow.UserRegistration.Tests.csproj
```

Rules:

- `PascalSlug` is derived from `flowSlug` by removing separators and
  Pascal-casing words;
- generated flow assemblies use `Xiigen.Flow.{PascalSlug}`;
- generated flow root namespaces use the exact assembly name;
- flow ids such as `FLOW-01` remain manifest metadata and evidence labels,
  but are not used in assembly names or namespaces;
- native adapter boundary packages may use their approved SDK package naming
  convention, but their .NET control-plane wrapper still follows this rule
  when one exists.

`flow.manifest.json` `serverEntrypoint` must point to this project/assembly
shape. `IFlowAssemblyLoader` rejects a manifest whose declared assembly name or
root namespace does not match the canonical slug-derived convention.

```csharp
services.AddMassTransit(x =>
{
    foreach (var registration in loadedFlowConsumerRegistrations)
    {
        registration.RegisterConsumers(x);
    }

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(
            context,
            new KebabCaseEndpointNameFormatter("xiigen", includeNamespace: false));
    });
});
```

Queue/cache platform registrations live in one service extension owned by the
platform host:

```csharp
public static class XiigenQueueServiceCollectionExtensions
{
    public static IServiceCollection AddXiigenQueueFabric(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IEventContractRegistry, GeneratedEventContractRegistry>();
        services.AddScoped<ICloudEventTenantContextRunner, CloudEventTenantContextRunner>();
        services.AddSingleton<IFlowAssemblyLoader, ManifestFlowAssemblyLoader>();
        services.AddSingleton<IQueueFabric, MassTransitQueueFabric>();
        services.AddSingleton<ICacheFabric, RedisCacheFabric>();
        return services;
    }
}
```

`Program.cs` calls `builder.Services.AddXiigenQueueFabric(...)` before
`AddMassTransit(...)` consumes discovered registrations. Flow projects do not
register `IEventContractRegistry`, `ICloudEventTenantContextRunner`,
`IFlowAssemblyLoader`, `IQueueFabric`, or `ICacheFabric` directly.

Flow services never call `AddMassTransit`, `UsingRabbitMq`, `AddConsumer`, or
broker SDK APIs directly. Flow packages may register only consumer types and
consumer definitions through `IFlowConsumerRegistration`. Endpoint names must
include the `xiigen` prefix, flow id, and consumer purpose so tenant cascade
evidence can trace queue behavior by flow.

Flow consumer types consume the platform envelope:

```csharp
public sealed class FlowCommandConsumer : IConsumer<CloudEventEnvelope>
{
    public Task Consume(ConsumeContext<CloudEventEnvelope> context)
    {
        // Parse context.Message.Data through the schema referenced by
        // CloudEventEnvelope.Type and the endpoint/event contract registry.
    }
}
```

`IConsumer<TSpecificPayload>` is not used in generated flow projects. Typed
payloads are parsed after envelope validation so CloudEvents metadata,
correlation id, causation id, idempotency key, tenant scope, and package
version stay attached to every inter-service message.

Envelope payload parsing uses the contract registry:

```csharp
public interface IEventContractRegistry
{
    DataProcessResult<EventContractDescriptor> Resolve(
        string cloudEventType,
        string? schemaVersion);
}

public sealed record EventContractDescriptor(
    string CloudEventType,
    string SchemaVersion,
    string SchemaRef,
    EventParserId ParserId,
    DeadLetterPolicy DeadLetterPolicy);

public enum EventParserId
{
    JsonSchema,
    CloudEventJsonSchema,
    BinaryOpaque
}

public enum DeadLetterPolicy
{
    NoRetryToErrorQueue,
    RetryThenErrorQueue,
    RetryUntilDescriptorAvailable
}
```

The registry is generated by `tools/contracts/generate-contracts` from
`contracts/events/**`, endpoint descriptors, and the flow manifest. Consumers
resolve `CloudEventEnvelope.Type` plus schema version metadata before parsing
`Data`.

Before invoking business logic, generated consumers establish tenant scope from
the CloudEvent envelope:

```csharp
public interface ICloudEventTenantContextRunner
{
    Task<DataProcessResult> RunAsync(
        CloudEventEnvelope envelope,
        Func<TenantContextSnapshot, CancellationToken, Task<DataProcessResult>> handler,
        CancellationToken cancellationToken = default);
}
```

`ICloudEventTenantContextRunner` reads the canonical `tenantid` CloudEvents
extension, validates it against the event source and package provenance, opens
a bounded `TenantContext`, and clears that context after the consumer handler
returns. The deprecated spelling `xiigentenantid` is not accepted in the .NET
initial implementation.

Parse and registry failures are deterministic:

- unknown CloudEvent type: do not retry; publish to the RabbitMQ error queue
  and record `QUEUE_UNKNOWN_EVENT_TYPE`;
- malformed JSON/data: do not retry; publish to the error queue and record
  `QUEUE_MALFORMED_EVENT_DATA`;
- schema version mismatch: retry only when the descriptor marks the schema as
  temporarily unavailable; otherwise error queue with
  `QUEUE_SCHEMA_VERSION_MISMATCH`;
- transient registry store failure: retry with the MassTransit retry policy.

RabbitMQ error/dead-letter naming:

```text
exchange: xiigen.error
queue: xiigen.error.<flowId>.<consumer-purpose>
routing key: <flowId>.<cloudEventType>.<errorCode>
```

MassTransit built-in error queue behavior may be used only if it is configured
to match this naming scheme or mirrored into this operations exchange.

## Alternatives Considered

### Direct AWS SQS SDK

This gives closest naming parity with the NestJS SQS baseline, but it leaks a
cloud provider into the first implementation path and makes local Docker tests
depend on LocalStack or live AWS. It remains available behind the queue fabric
only if MassTransit SQS is rejected.

### Azure Service Bus

This is a strong .NET-native deployment option, but it is not the current
baseline and would require a separate hosting decision before parity claims.

### RabbitMQ Only

RabbitMQ is good for local Docker and broad portability, but it does not by
itself cover the current SQS-compatible deployment expectation. It is retained
as the local integration provider rather than the whole provider strategy.

## Required Acceptance Criteria

P-03 is not complete until:

- MassTransit, RabbitMQ transport, Amazon SQS transport when used, and
  StackExchange.Redis versions are pinned in `Directory.Packages.props` under
  `ADR-DOTNET-DEPENDENCY-VERSION-PINS.md`;
- `IQueueFabric` and `ICacheFabric` compile with in-memory providers;
- in-memory queue broker stub supports publish, consume, ack, nack, retry, and
  dead-letter test scenarios;
- RabbitMQ and Redis Docker providers are implemented for integration tests;
- queue publish/consume tests prove only `CloudEventEnvelope` crosses service
  boundaries;
- consumer registration tests prove each flow contributes consumers through
  `IFlowConsumerRegistration` and the host owns the single MassTransit bus
  setup;
- discovery tests prove the host finds exactly one registration type from the
  manifest-resolved flow assembly and fails readiness on zero/multiple
  registrations when consumers exist;
- assembly loader tests prove generated flow assembly names, root namespaces,
  and `serverEntrypoint` values follow `Xiigen.Flow.{PascalSlug}` and reject
  `FLOW-XX` namespace variants;
- DI tests prove `IEventContractRegistry`, `ICloudEventTenantContextRunner`,
  `IFlowAssemblyLoader`, `IQueueFabric`, and `ICacheFabric` are registered only
  through `AddXiigenQueueFabric(...)`;
- queue consumer tests use `IConsumer<CloudEventEnvelope>` and parse typed
  payload data only after envelope validation;
- contract-registry tests prove unknown event types, malformed data, schema
  mismatches, and transient registry failures choose the correct retry or
  error-queue path;
- tenant context tests prove generated consumers read the `tenantid`
  CloudEvents extension through `ICloudEventTenantContextRunner` and clear
  tenant scope after each message;
- idempotency tests prove duplicate queue messages and duplicate mutations are
  suppressed;
- provider SDK imports are limited to provider projects;
- tenant scope and correlation metadata survive queue round trips.
- FLOW-39 queue dispatch tests can run against the in-memory broker stub before
  RabbitMQ evidence is required.

## Applies Before
- P-03 fabric implementation.
- Docker/Vault integration tests for flows that publish events.
- Any portability claim that requires real queue/cache behavior.

