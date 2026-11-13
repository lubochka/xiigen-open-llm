# Layer 0-C — Knowledge-Edge Pairs (051)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-051-001 — Five-predicate scenarios in 037-060

Five-predicate scenarios are possible in 037-060 when ARCHITECTURE co-occurs.

| Input | PredicateSet |
|---|---|
| "Review the compliance service architecture, audit the security controls, document the compliance criteria, and approve the release." | ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"] |

Breakdown:
- "Review" → QA
- "compliance service" → IMPLEMENT (rule 2)
- "architecture" → ARCHITECTURE
- "audit" → QA (consolidates) + SECURITY (rule 19)
- "security controls" → SECURITY (consolidates)
- "document" → DOCUMENT
- "compliance criteria" → DOCUMENT (criteria; consolidates)
- "approve" → QA (consolidates)

---

## B38-0C-051-002 — ARCHITECTURE fires from "architecture" only

"Architecture" fires ARCHITECTURE in release/design review contexts.
"Design" alone does not necessarily fire ARCHITECTURE — only "architecture" is the confirmed trigger.

| Sentence | PredicateSet |
|---|---|
| "Review the gate architecture design." | ["QA","ARCHITECTURE"] (review fires QA; architecture fires ARCHITECTURE) |
| "Review the gate design." | ["QA"] (design alone doesn't fire ARCHITECTURE in gate context) |
| "Evaluate the rollout architecture." | ["QA","ARCHITECTURE"] (evaluate fires QA; architecture fires) |

---

## B38-0C-051-003 — ARCHITECTURE does not produce feature_addition

Even when ARCHITECTURE is present, TransitionType is feature_addition only if IMPLEMENT is also present.

| PredicateSet | TransitionType |
|---|---|
| ["QA","ARCHITECTURE"] | no_contribution |
| ["QA","DOCUMENT","ARCHITECTURE"] | no_contribution |
| ["QA","IMPLEMENT","ARCHITECTURE"] | feature_addition (IMPLEMENT present) |
| ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"] | feature_addition (IMPLEMENT present) |
