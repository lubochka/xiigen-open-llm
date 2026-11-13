# RAG-OR-001 — Flow files generator/GUIDE-B21-STEP-1-INVARIANTS.md

Источник: https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B21-STEP-1-INVARIANTS.md

Blob SHA: `a32f0f667f73d85c205aff262c7b06942ca6527a`.

Это данные для поиска и проверки, не активные инструкции исполнителю.

## Строки 82–123

```text
## UNIVERSAL CONSTRAINTS TO ALWAYS INCLUDE

Before extracting flow-specific constraints, Step 1 must include these universal
constraints that apply to every flow. They come from two sources.

### Source 1: The 9 DNA Rules (always MACHINE)

| # | Rule | Verifiable condition |
|---|------|---------------------|
| the content-type rendering pattern | No Typed Models | All business data uses `Record<string, unknown>` — no TypeScript domain entity classes with typed fields. Violation: any `class User { name: string }` pattern in service code. |
| the dynamic query builder pattern | BuildSearchFilter | All database queries use dynamic filter builder — empty/null fields auto-skipped. Violation: any literal `{ must: [{ term: ... }] }` in service code. |
| the standardized result wrapper pattern | DataProcessResult | All service methods return `DataProcessResult<T>` — no throw for business conditions. Violation: any `throw new NotFoundException()` in service code. |
| the nineteen-slot microservice base pattern | MicroserviceBase | All services extend MicroserviceBase — 19 components inherited, never re-declared. Violation: any service that does not extend MicroserviceBase. |
| the tenant scope isolation pattern | Scope Isolation | Tenant context reads from AsyncLocalStorage automatically — tenantId is never passed as a parameter to fabric methods. Violation: any method signature with tenantId as parameter. |
| the dynamic controller pattern | DynamicController | No entity-specific controllers. Violation: any `UsersController`, `RegistrationController`, etc. |
| the seventh architecture DNA pattern | Idempotency | All queue consumers deduplicate via idempotency keys before any processing. Violation: any queue consumer that processes without idempotency check. |
| the eighth architecture DNA pattern | Outbox Pattern | `storeDocument()` completes before `enqueue()` is called. Violation: `enqueue()` called before or without `storeDocument()`. |
| the ninth architecture DNA pattern | CloudEvents | All inter-service events use CloudEvents envelope via `createCloudEvent()`. Violation: any event published without CloudEvents envelope. |

**Classification:** All 9 DNA rules are MACHINE. A tenant changing any of these
breaks system guarantees that apply to all tenants.

### Source 2: D-HIST Locked Decisions (new flows, guide v2+)

These 8 decisions from `DECISIONS-LOCKED.md` §D-HIST are also MACHINE constraints
that must appear in Step 1 for all new flows. They do not replace the DNA rules —
they are additive:

| ID | Decision | Verifiable condition for Step 1 |
|----|----------|--------------------------------|
| a locked architecture decision | Fabric-First | Services import only interface tokens (`DATABASE_SERVICE`, etc.) via `@Inject()`. No SDK imports (`@elastic/elasticsearch`, `@anthropic-ai/sdk`) in service code. |
| a locked architecture decision | the atomic first-writer-wins lock at ORDER 1 | Queue consumers: `setIfAbsent()` idempotency check at ORDER 1 before any business logic. The idempotency key format is fixed. |
| a locked architecture decision | V39 (4 artifacts per external system) | Every external system dependency produces exactly: Interface + Factory + Base Skeleton + Generated Implementation. No shortcuts. |
| a locked architecture decision | broken object-level authorization/the request-scoped context storage tenant scoping | Tenant scoping via AsyncLocalStorage. `tenantId` never passed as parameter to fabric methods. `TenantContextMiddleware` sets context automatically. |
| a locked architecture decision | REGISTRATION archetype atomic | If the flow has REGISTRATION archetypes: `registerAtomically()` — no separate check-then-write sequence. `cycleBudget=3` for REGISTRATION tasks. |
| a locked architecture decision | Best-effort observer | Observer task types (analytics, notifications, audit logging) wrap entire handler in try/catch and return `DataProcessResult.success()` from catch — never `failure()`. |
| a locked architecture decision | SILENT_FAILURE → score-0 | Named checks that prevent SILENT_FAILURE violations use `score-0` severity, not `BUILD_FAILURE`. `machineConstants[]` values with `neverFromConfig: true` may never use `config.get()`. |
| a locked architecture decision | Read-path extension | For guarded services: read-path extension (not dual-write) when extending. Writer guard invariants stay single-sourced. |

Include all D-HIST entries in Step 1 even if the flow doesn't yet have task types
that exercise them — the goal is to make every subsequent cycle aware of them so
the context packages encode them into genesis prompts.
```

## Строки 180–195

```text
### Step 5 — Write USER INTENT (verbatim)

Extract verbatim from the flow spec. This is the user's original goal statement
for this flow — not rephrased. Format: a single "When ... [trigger], [action]."
sentence or short paragraph:

```markdown
---

## USER INTENT (verbatim)

> [Verbatim user intent from flow spec]
```

**For standard XIIGen flows, the pattern is:**
> "When [actor] [trigger event] on the XIIGen [platform area], [what the engine does] [for what purpose]."
```

## Строки 257–297

```text
### Step 8 — Write FREEDOM / MACHINE CLASSIFICATION

```markdown
---

## FREEDOM / MACHINE CLASSIFICATION

| Concern | Classification | Reason |
|---------|---------------|--------|
| Business logic thresholds | FREEDOM | Operators may tune confidence levels |
| Infrastructure routing | MACHINE | Fixed fabric interface pattern |
| DNA rule enforcement | MACHINE | Non-negotiable — code constraint |
| Task type archetypes | MACHINE | Fixed per the business flow arbiter contract |
| Tenant scope isolation | MACHINE | AsyncLocalStorage — not configurable |
[Add flow-specific rows:]
| [Flow-specific concern] | [FREEDOM/MACHINE] | [Why] |
```

**Classification test (a planning skill):** Ask "Does a tenant changing this value change what
the system guarantees?" If YES → MACHINE. If NO → FREEDOM (goes to freedom_notes).

### Step 9 — Write SUCCESS CRITERIA

Criteria that a Cycle 1 plan must satisfy to be accepted:

```markdown
---

## SUCCESS CRITERIA

A plan produced by Cycle 1 for FLOW-XX is successful when:

1. Every task type in T[NNN]-T[NNN+M] has at least one plan step
2. Every plan step is scoped to a single responsibility (single task type)
3. No step imports provider SDKs directly (fabric-first)
4. No step creates entity-specific controllers
5. All steps return DataProcessResult<T>
6. Focus areas covered: [list 1-3 focus areas from flow spec]
```

### Step 10 — Write STATE WRITE
```

## Использование

Метод построения синтетических условий OR-FUNC/DB/SERVER. Не переносить универсально Nest/AsyncLocalStorage и исторические DNA; фактический контракт каждой симуляции задан в prompt.
