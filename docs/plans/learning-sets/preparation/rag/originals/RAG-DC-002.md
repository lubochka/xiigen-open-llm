# ADR: .NET Tenant Context Lifetime

Status: Approved for implementation
Date: 2026-05-14

## Context

The NestJS baseline uses CLS to bind tenant scope to a request. The .NET
translation uses `AsyncLocal<TenantContext>` to provide the equivalent scope to
kernel services and fabric providers.

`AsyncLocal<T>` is not automatically cleared when an ASP.NET Core request
finishes. If `TenantContextMiddleware` sets the current tenant and does not
clear it in a `finally` block, async continuations or background work can
observe a stale tenant from a completed request. That is a tenant-isolation
failure even when authentication and authorization are otherwise correct.

## Decision

`TenantContextMiddleware` must own the request-bound scope lifecycle.

The required shape is:

```csharp
public async Task InvokeAsync(HttpContext httpContext)
{
    var previous = tenantContextAccessor.Current;

    try
    {
        tenantContextAccessor.Current = ResolveTenantContext(httpContext);
        await next(httpContext);
    }
    finally
    {
        tenantContextAccessor.Current = previous;
        tenantContextAccessor.ClearRequestScope();
    }
}
```

For normal HTTP requests `previous` is expected to be null. Restoring it keeps
the accessor safe for approved nested scopes, while `ClearRequestScope()` must
remove the request tenant before the middleware returns.

Background work must never inherit tenant scope implicitly from a request.
Approved background work uses an explicit `TenantContextSnapshot` and opens a
bounded scope:

```csharp
await tenantContextRunner.RunAsync(snapshot, cancellationToken, work);
```

## TenantContextSnapshot Shape

`TenantContextSnapshot` is immutable and serializable enough to be stored in an
outbox record or background command metadata:

```csharp
public sealed record TenantContextSnapshot(
    string TenantId,
    string? UserId,
    IReadOnlySet<string> Roles,
    string CorrelationId,
    string? CausationId,
    string Source,
    DateTimeOffset CapturedAt);

public interface ITenantContextRunner
{
    Task<DataProcessResult> RunAsync(
        TenantContextSnapshot snapshot,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task<DataProcessResult>> work);
}
```

The snapshot is created only from an already-enriched `TenantContext` plus the
current `CorrelationContext`. It must not be constructed from arbitrary route
payload tenant ids. FLOW-46's superJudge exception must record the snapshot
source and causation id so the deliberate cross-tenant evaluation is auditable.

## Required Tests

P-02 is not complete until tests prove:

- `TenantContext` is null after a successful request completes;
- `TenantContext` is null after an exception path completes;
- two parallel requests cannot observe each other's tenant;
- fire-and-forget or delayed continuations do not inherit request tenant scope;
- approved background work receives only the explicit `TenantContextSnapshot`;
- `TenantContextSnapshot` carries tenant id, optional user id, roles,
  correlation id, optional causation id, source, and capture time;
- FLOW-46's superJudge exception uses the approved bounded scope mechanism.

## Prohibited Patterns

- No `AsyncLocal<TenantContext>` writes outside the accessor or bounded scope
  runner.
- No middleware path that sets tenant context without a `finally` reset.
- No background task that reads ambient tenant context captured from HTTP.
- No flow service method that accepts a free-form tenant id to repair scope.

## Applies Before
- P-02 tenant context and auth scope implementation.
- Any Docker/Vault live auth certification.

