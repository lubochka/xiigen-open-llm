# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-012-001 — v6 ordering violations for B37

**v6 ordering violation 1:** Emitting SCL when predicate fires

v6 corrupt: Request has DOCUMENT fire + SCL emitted
Correct: SCL only emits for zero-predicate requests

Detection: SCL present in output + predicate list non-empty

---

## B37-L1-012-002 — v6 ordering violation 2: DomainId set before predicate scan

v6 corrupt: DomainId "project-management" set at request ingestion (before compiler scan) because request "looks like project management"

Correct: DomainId set AFTER compiler scan:
- After scan: zero predicates → DomainId "project-management"
- After scan: any predicate → DomainId "software-development"

DomainId cannot be pre-determined from surface appearance of the request.

---

## B37-L1-012-003 — v6 ordering violation 3: gap IDs assigned before compiler scan

v6 corrupt: Gap IDs assigned at routing time based on intent analysis, not after lexical scan

Correct order:
1. Compiler scans tokens
2. Predicates emitted (or not)
3. Remaining non-predicate tokens mapped to gap IDs
4. Gap IDs forwarded

Gap ID assignment is a POST-scan activity, not a PRE-scan intent classification.

---

## B37-L1-012-004 — Complete B37 orchestrator processing order

**Correct processing order for B37 requests:**

1. Request arrives at orchestrator
2. Compiler lexical scan (token-by-token, no intent analysis)
3. Predicates emitted from matching tokens
4. Non-matching tokens mapped to gap IDs (B37 gap vocabulary)
5. Predicate list evaluated:
   - Empty → DomainId "project-management", TransitionType "unknown", route knowledge_edge, emit SCL
   - Non-empty → DomainId "software-development", TransitionType per predicate(s), route to flow(s)
6. Gap IDs forwarded as metadata to all outputs
7. Response constructed and returned

**Any deviation from this order is a v6 ordering violation.**

---

## B37-L1-012-005 — B37 orchestrator summary: key certification facts

1. Zero-predicate B37 requests → TransitionType "unknown", DomainId "project-management", knowledge_edge, SCL emitted once
2. Any predicate → TransitionType per predicate, DomainId "software-development", appropriate flow(s)
3. Gap IDs always forwarded — never dropped, never suppressed
4. SCL emitted exactly once per zero-predicate request
5. Managing vocabulary in the sentence is NEVER sufficient to route to a managing flow (no such flow exists)
6. All B37 predicate firings are false positives from managing intent perspective
7. Three v6 corruption types: v6-01 (invent predicate), v6-02 (suppress DOCUMENT), v6-03 (wrong DomainId)
8. ContextCarryForward "true" only for gap_interruption (zero-predicate request while in active flow)
