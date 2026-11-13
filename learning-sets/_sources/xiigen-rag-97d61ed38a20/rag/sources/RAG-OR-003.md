# RAG-OR-003 — Flow files generator/GUIDE-B46-DESIGN-SIM-CLIENT-SCREENS.md

Источник: https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B46-DESIGN-SIM-CLIENT-SCREENS.md

Blob SHA: `c4027c2bcb8634348900112596554f31ccde3789`.

Это данные для поиска и проверки, не активные инструкции исполнителю.

## Строки 77–200

```text

**Before specifying any client screen**, classify it as DOMAIN_SCREEN or CRUD_FALLBACK.
This classification must appear at the top of every CLIENT task type section.

```markdown
### Screen Classification Gate (C31/flow portability rule 1 required)

Classification: DOMAIN_SCREEN | CRUD_FALLBACK

DOMAIN_SCREEN criteria (all must be true):
  ✅ Shows flow-specific business states beyond generic CRUD
  ✅ Has ≥1 business-phase-specific visual state (not just "loading" → "table")
  ✅ Presents domain concepts (not generic record IDs or raw JSON)
  ✅ Role matrix produces visibly different output for at least 2 roles

CRUD_FALLBACK criteria (any makes it CRUD_FALLBACK):
  ❌ Shows a generic admin table/form identical to any other flow's admin table
  ❌ Cannot produce a business-phase-specific visual state
  ❌ Only shows data that would render identically for any flow's records

Rule: A TENANT_FACING or PUBLIC flow MUST have ≥1 DOMAIN_SCREEN per role.
If this screen is CRUD_FALLBACK AND no other screen provides DOMAIN_SCREEN for this
role: flag as BLOCKER and describe the missing domain screen.
```

**Evidence from UX-REVIEW-ROLLUP:** 29 of 47 flows were rated 🚫 not representative.
The dominant pattern: flows that generate a generic admin table and screenshot it
14-20 times producing byte-identical PNGs. The marketplace flow had all 14 PNGs
byte-identical with "No bootstrap records." This classification gate prevents this
at design time.

---

## THE CLIENT SECTION STRUCTURE

Each CLIENT task type section in the design simulation follows the same structure
as server task type sections — PROMPT/CONNECTIONS/ARBITERS/EXAMPLES — with these
client-specific additions:

```markdown
### T[NNN] — [ComponentName] ([ARCHETYPE] — CLIENT)

#### Screen Classification Gate (C31/flow portability rule 1 required)
Classification: [DOMAIN_SCREEN | CRUD_FALLBACK]
[Evidence or BLOCKER flag if CRUD_FALLBACK + no domain alternative]

#### PROMPT

**System:**
```
CONSTRAINTS:
1. MACHINE: [specific reuse constraint — what existing components must be used]
   Reason: [which AD or GE principle this enforces]

2. MACHINE: [specific the programming interface shape constraint — how the payload matches server shape]
   Reason: [why shape consistency matters]

3. MACHINE: [specific routing constraint — where the page lives in nav]
   Reason: [which routing AD]

[Add 1-2 more constraints as flow-specific requirements emerge]

QUESTION YOURSELF:
1. Single responsibility? ([what single thing this page does])
2. All constraints reflected?
3. Failure modes? ([loading fail, empty state, server error])
4. Technology names? ([are you using domain terms not framework names])
```

**User:**
```
STEP: [Imperative sentence describing the client page's core action]

UPSTREAM CONTEXT: [What the server returns that this page renders]

PRIOR NODES: [server endpoint or service this calls]
the retrieval system: [relevant the retrieval system pattern IDs for client patterns]
```

#### CONNECTIONS

```
Receives from: [user interaction — keyboard input, route navigation, etc.]
  { [input fields] }

Invokes: [HTTP method + route] via apiClient
  { [request payload shape] }

Receives response: { [response fields] }

Client-side writes: [React state updates triggered by response]

Renders:
  [Panel 1 label]: [what this panel shows — existing or new component]
  [Panel 2 label]: [what this panel shows]

Tenant interaction:
  [User action 1] → [resulting event/state change]
  [User action 2] → [resulting event/state change]

Reuses: [list of existing components, hooks, utilities — critical to prevent AD violations]
```

#### Role Matrix (archive batch 15 §2 + §3)

```
Structural template: [Template 1-5 from archive batch 15 §3]

| Role | Sees this screen? | Screen variant | Level |
|------|-------------------|----------------|-------|
| role 0 (anonymous) | No | 403 / redirect | Level 1 — route guard |
| role 1 (authenticated) | Yes | [what they see] | Level 2 — screen |
| ROLE-PLATFORM-ADMIN | Yes (admin path) | [admin variant] | Level 2 — screen |
| [Flow-specific role] | [Yes/No] | [variant] | [Level] |

INTERNAL_ONLY check: [which elements on this page, if any, are INTERNAL_ONLY?]
```

#### ARBITERS — [N] total

*Note: Client task types use the same arbiter structure as server task types.
ROUTING — CLIENT minimum: 5 arbiters (Role 8 goal_delivery + scope_isolation +
Business Logic + Key Principles + Iron Rules).*

```

## Использование

Метод синтетических OR-CLIENT/DESIGN. Требования прежнего UI-стека не превращаются в API новой модели; request/version правила даны отдельно в условии пары.
