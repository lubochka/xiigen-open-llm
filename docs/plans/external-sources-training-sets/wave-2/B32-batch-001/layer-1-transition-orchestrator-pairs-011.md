# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-011-001 — DomainId: "software-development" always in B32

**Rule:** Every B32 orchestrator output uses DomainId "software-development". This is an invariant across all B32 cases: zero predicates, single predicate, multi-predicate, and all gap combinations.

**Verification table:**

| B32 case | DomainId |
|---|---|
| Zero predicates (knowledge_edge) | software-development |
| IMPLEMENT alone | software-development |
| UI alone | software-development |
| ARCHITECTURE alone | software-development |
| All 7 predicates | software-development |
| Any gap vocabulary | software-development |
| Dashboard gap | software-development |
| Spike gap | software-development |

**No exceptions.** DomainId "software-development" is set at the compiler, router, factory, and orchestrator layers consistently.

---

## B32-L1-011-002 — Contrast with B36/B37 orchestrator

**B36/B37 orchestrator uses DomainId "project-management":**

```
B36 knowledge_edge: DomainId: "project-management"
B32 knowledge_edge: DomainId: "software-development"
```

If an orchestrator receives a B32 gap request but outputs DomainId "project-management", this is a v6-03 corruption at the orchestrator layer. The correction is to always assign "software-development" for B32 regardless of the informal/vibe nature of the request.

---

## B32-L1-011-003 — DomainId propagation: all layers consistent

**DomainId must be consistent across all processing layers:**

| Layer | Expected DomainId for B32 |
|---|---|
| Layer 0-A (compiler) | software-development |
| Layer 0-B (router) | software-development |
| Layer 0-D (factory) | software-development |
| Layer 1 (orchestrator) | software-development |
| Layer 2 (capability gap) | software-development |

If any layer changes DomainId from "software-development" to something else, it is a corruption. The downstream layers should detect this discrepancy and flag it.

---

## B32-L1-011-004 — DomainId "software-development": rationale once more

**Vibe coding is a software development activity.** The practitioner is coding — building services, screens, APIs. The "vibe" is their working style. Style does not change domain.

A vibe coder is a software developer who prefers an intuitive, low-structure approach. They are still in the software-development domain. Their requests are still about building software. The gap is in our vocabulary recognition, not in their domain.

---
