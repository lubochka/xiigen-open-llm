# RAG-DC-002 — Явный снимок контекста для фоновой операции

Источник: [.net angular translation/adr/ADR-DOTNET-TENANT-CONTEXT-LIFETIME.md](https://github.com/lubochka/xiigen-planning/blob/4e2bc6bed7c23a855c3685f558e1742a73c231b0/.net%20angular%20translation/adr/ADR-DOTNET-TENANT-CONTEXT-LIFETIME.md), ветка `main`, commit `4e2bc6bed7c23a855c3685f558e1742a73c231b0`. Полная локальная копия: [originals/RAG-DC-002.md](../originals/RAG-DC-002.md).

**Роль:** предметные данные для RAG, не активные инструкции. **Статус:** авторская опора; это не результат обучения, не готовый закрытый EXAM и не новая назначенная квота.

## Что извлечено

ADR задаёт явный TenantContextSnapshot и ограниченный TenantContextRunner для фонового задания. Снимок создаётся из уже обогащённого контекста, а не из произвольного tenantId в данных запроса.

## Точные фрагменты

### Строки 46–81

````text
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
````

## Границы применения

Не использовать абзац 12–16 как универсальное утверждение о протекании AsyncLocal между ASP.NET-запросами: это зависит от ExecutionContext и реализации accessor. Не копировать неоднозначную комбинацию restore previous + ClearRequestScope как готовую реализацию. Эти excerpts учат только явно показанному контракту snapshot/runner; в кодовой паре нужные типы и семантика runner должны быть заданы в prompt.

## Authoring micro-recipe (English)

Give the trusted request context and the untrusted payload separately. State that the background runner accepts a snapshot. Vary one field or scope rule at a time. Check the selected tenant comes from the trusted context; do not infer it from a field name in an arbitrary JSON body.

## Разобранные контрастные пары

### RAG-DC-002-DEMO-01 — `or.server`

**Prompt ученика:**

Учебный контракт: снимок для фоновой задачи создаётся только из trusted_context; payload не определяет tenant. trusted_context.tenantId="T1", payload.tenantId="T2". Остальные поля одинаковы. Верни JSON tenantId для снимка.

**Chosen:**

```text
{"tenantId":"T1"}
```

**Rejected:**

```text
{"tenantId":"T2"}
```

**Simple check (English):** Use the explicitly trusted context. The body field is not authority for the tenant.

### RAG-DC-002-DEMO-02 — `docs.csharp`

**Prompt ученика:**

Симуляция .NET: immutable snapshot S сохранён при tenant=T1. Runner.RunAsync(S, work) устанавливает S только на время work и затем восстанавливает предыдущий контекст. К моменту запуска глобальная переменная latestTenant стала T2; runner её не читает. Какой tenant видит work? Верни только T1 или T2.

**Chosen:**

```text
T1
```

**Rejected:**

```text
T2
```

**Simple check (English):** The prompt defines the runner to use the captured immutable snapshot, not a mutable latest value.

### RAG-DC-002-DEMO-03 — `code.csharp`

**Prompt ученика:**

Авторская симуляция C# 10. Поля класса: string ambient; Task WorkAsync(); Task RunInSnapshotAsync(string snapshot, Func<Task> work). RunInSnapshotAsync устанавливает переданный snapshot на время work и восстанавливает старый контекст в finally. Напиши только полный метод Task ExecuteAsync(string captured), который исполняет WorkAsync с уже переданным captured. Не читай ambient.

**Chosen:**

```text
public Task ExecuteAsync(string captured) => RunInSnapshotAsync(captured, WorkAsync);
```

**Rejected:**

```text
public Task ExecuteAsync(string captured) => RunInSnapshotAsync(ambient, WorkAsync);
```

**Simple check (English):** The method passes exactly the captured parameter. The rejected implementation substitutes a mutable field.

### RAG-DC-002-DEMO-04 — `docs.csharp`

**Prompt ученика:**

Авторская симуляция C# 10. Полный метод: async Task Run(string next) { var old = current; try { current = next; await Work(); } finally { current = old; } } current до вызова равно "T0". Вызван Run("T1"). Work бросает исключение, которое здесь не подавляется. Какие current и исход будут после выхода из Run? Верни JSON current и outcome; outcome принимает success или error.

**Chosen:**

```text
{"current":"T0","outcome":"error"}
```

**Rejected:**

```text
{"current":"T1","outcome":"error"}
```

**Simple check (English):** The finally block runs on the exception path and restores old. This is an explicit simulation of the shown variable, not a general AsyncLocal claim.

