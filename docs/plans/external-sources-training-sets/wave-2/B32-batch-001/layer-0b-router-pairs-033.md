# Layer 0-B — Router Pairs (033)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-033-001 — "good enough" routing: four predicate combinations

**Summary table — "good enough" qualifier (gap-vibe-coding-style-001) with all predicate options:**

| Predicates | Route | SCL? | GapWarning? |
|---|---|---|---|
| [] | knowledge_edge | YES | NO (not applicable when no predicates) |
| ["IMPLEMENT"] | ImplementationFlow | NO | style_qualifier_noted |
| ["DOCUMENT"] | DocumentFlow | NO | style_qualifier_noted |
| ["TEST"] | TestFlow | NO | style_qualifier_noted |
| ["QA"] | QAFlow | NO | style_qualifier_noted |
| ["ARCHITECTURE"] | ArchitectureFlow31 | NO | style_qualifier_noted |
| ["IMPLEMENT", "TEST"] | ImplementationFlow + TestFlow | NO | style_qualifier_noted |

---

## B32-0B-033-002 — "minimum viable" routing: same table

**Summary table — "minimum viable" qualifier (gap-vibe-coding-style-001):**

Identical to "good enough" routing outcomes above. Both produce gap-vibe-coding-style-001.
Router treats them identically — the source vocabulary is irrelevant to routing logic.

---

## B32-0B-033-003 — prototype-family routing

**Summary table — prototype-family qualifiers (gap-vibe-coding-prototype-001):**

Members: proof-of-concept, PoC, prototype (alone), paper prototype, wireframe

| Predicates | Route | SCL? |
|---|---|---|
| [] | knowledge_edge | YES |
| ["UI"] | UIFlow | NO |
| ["IMPLEMENT"] | ImplementationFlow | NO |
| ["ARCHITECTURE"] | ArchitectureFlow31 | NO |

---

## B32-0B-033-004 — spike-family routing

**Summary table — spike-family qualifiers (gap-vibe-coding-spike-001):**

Members: spike, technical spike, run a spike, tracer bullet, walking skeleton, thin slice, vertical slice

| Predicates | Route | SCL? |
|---|---|---|
| [] | knowledge_edge | YES |
| ["IMPLEMENT"] | ImplementationFlow | NO |
| ["ARCHITECTURE"] | ArchitectureFlow31 | NO |
| ["IMPLEMENT", "TEST"] | ImplementationFlow + TestFlow | NO |
| ["IMPLEMENT", "DOCUMENT"] | ImplementationFlow + DocumentFlow | NO |

---

## B32-0B-033-005 — DomainId invariant confirmation

**For every routing outcome in B32:**
- DomainId = "software-development"
- DomainId is never "project-management"
- DomainId is never "vibe-coding"
- DomainId is never null or missing

This invariant holds regardless of predicate count, gap ID count, or routing target.

---
