# Shared Service Design

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## Core Problem

Building two implementations of the same service — one inside subproject 21
and another inside subproject 22 — that diverge over time and require double
maintenance. The same capability written twice in different subprojects is the
most expensive class of architectural mistake in a multi-subproject system.
This skill defines when a service is shared infrastructure (requiring a fabric
interface) vs per-subproject, and how to build the shared services map before
any subproject design session begins.

---

## When to Invoke

- Before any multi-subproject planning session
- During subproject-examination-SKILL.md when capability appears in multiple subprojects
- When a capability name appears in 2+ subproject plans
- Before service-boundary-design-SKILL.md when the boundary spans subprojects

---

## Section 1 — Count Service Appearances Across Subprojects

For each capability or service mentioned in any subproject plan, count how many
subproject plans reference it:

```
ISessionStorage:       ████████ 6 of 6 subprojects  ← CRITICAL: shared infrastructure
IAgentMessageBus:      ██████   5 of 6 subprojects  ← shared infrastructure
IVectorIndex:          █████    4 of 6 subprojects  ← shared infrastructure
IBm25Scorer:           ████     3 of 6 subprojects  ← shared
ISemanticCompiler29:   ███      2 of 6 subprojects  ← review: may be shared
DomainModelRegistry:   ██       2 of 6 subprojects  ← review: may be shared
CapabilityAnalyzer:    █        1 of 6 subprojects  ← per-subproject
```

**Classification threshold:**
- Appears in ≥ 3 subprojects → SHARED INFRASTRUCTURE: fabric interface required
- Appears in 2 subprojects → REVIEW: may be shared depending on usage type
- Appears in 1 subproject → PER-SUBPROJECT: build inside that subproject

---

## Section 2 — Classify Each Shared Service

For each shared service, determine its sharing pattern:

**Pattern A — Shared Write + Shared Read (most complex)**
Multiple subprojects write to this service AND multiple subprojects read from it.

Example: `ISessionStorage` — multiple subprojects write session data; multiple read it.

Resolution: Define as a fabric interface with write AND read methods. Build as
standalone infrastructure (not inside any single subproject) BEFORE the first
subproject that writes to it.

**Pattern B — Shared Read Only**
One subproject builds the service; subsequent subprojects only read from it.

Example: `IBm25Scorer` — built in 22.AgentAlgorithms; called by 21, 23 for retrieval.

Resolution: Build in the first subproject. Document the read interface as a
cross-subproject dependency. Subsequent subprojects use it via the existing
interface — never rebuild it.

**Pattern C — Shared Notification/Event Bus**
Multiple subprojects publish events; multiple subprojects subscribe.
Each subproject provides different message types.

Example: `IAgentMessageBus` — 21 publishes SessionCreated; 22 publishes ScoreUpdated;
23 subscribes to both.

Resolution: Build `IAgentMessageBus` as a fabric interface in the infrastructure
subproject. Each subproject publishes its own message types. Never build a
per-subproject message bus implementation.

---

## Section 3 — Order the Build Sequence

Shared infrastructure must be built (or at least stubbed as fabric interfaces)
BEFORE the first subproject that uses them:

```
Pre-subproject 22.AgentAlgorithms infrastructure needed:
  ISessionStorage         ← AgentAlgorithms needs session context for scoring
  IAgentMessageBus        ← AgentAlgorithms publishes ScoreUpdated events
  IVectorIndex (stub)     ← AgentAlgorithms needs vector search capability

Pre-subproject 23.StageExplorer infrastructure needed:
  IBm25Scorer             ← StageExplorer uses BM25 for document retrieval
  IVectorIndex (active)   ← StageExplorer uses vector search for semantic retrieval
  HybridRetriever         ← StageExplorer combines BM25 + vector
```

---

## Section 4 — Shared Services Map Format

Before any multi-subproject design session, produce:

```markdown
## Shared Services Map

| Service | Appearances | Pattern | Built in | Fabric interface |
|---|---|---|---|---|
| ISessionStorage | 6 subprojects | A: write+read | 21.AgentInfrastructure | ISessionStorage |
| IAgentMessageBus | 5 subprojects | C: event bus | 21.AgentInfrastructure | IAgentMessageBus |
| IVectorIndex | 4 subprojects | A: write+read | 22.AgentAlgorithms | IVectorIndex |
| IBm25Scorer | 3 subprojects | B: shared read | 22.AgentAlgorithms | IBm25Scorer |

## Per-Subproject Services (build inside the subproject, share only read)

| Service | Subproject | Not shared because |
|---|---|---|
| CapabilityAnalyzer | 23.StageExplorer | Only StageExplorer writes; others only read results |
| IncrementalTrainer | 22.AgentAlgorithms | Only AgentAlgorithms trains; others read model state |
```

---

## Anti-Patterns

1. **"Build SessionWriter in 21 and SessionStore in 22 — same domain, different names"** —
   same underlying capability with two implementations. Identify the shared service
   before building either. One `ISessionStorage` interface covers both.

2. **"Build IMessageBus in 21 and IEventPublisher in 22"** — both subprojects need
   a message bus. Build one `IAgentMessageBus` interface, register once in DI, use in both.

3. **"Discover in subproject 23 that IVectorIndex needs a new method, requiring retroactive
   changes to 22's already-implemented code"** — this is the canonical cross-cutting mistake.
   The shared services map prevents it by declaring the full interface before any subproject builds against it.

4. **"The interface can be built when the first subproject needs it"** — it is too late.
   Build the stub interface and DI registration before the first dependent subproject starts.

---

## Integration

- service-boundary-design-SKILL.md uses this skill's shared service classification to decide
  whether a service boundary should be a subproject boundary or a namespace boundary
- COMPONENT-INTERFACES.md is the persistence layer for the shared services map
- prerequisite-chain-SKILL.md uses ACTIVE/STUB status from the shared services map
- docs/ARCHITECTURE.md captures the build sequence determined by this skill
