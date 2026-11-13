# Layer 0-D — Flow Factory Pairs (060)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-060-001 — Flow factory complete reference: all factories

| Factory | Trigger predicate | Notes |
|---------|------------------|-------|
| GapHandler | [] (no predicates) | Emits SCHEDULE_CAPABILITY_LEARNING |
| ArchitectureFlow31 | ARCHITECTURE | design + structural noun |
| APIContractFlow33 | API_CONTRACT | integrate always |
| ImplementationFlow41 | IMPLEMENT | build/implement/create/develop |
| SecurityAuditFlow | SECURITY | secure/harden + injection |
| TestFlow51 | TEST | write tests |
| DocumentationFlow62 | DOCUMENT | document/write docs |
| QAGatingFlow73 | QA | QA abbreviation; always last |

---

## FF-060-002 — Flow ordering rules (canonical, all batches including B33)

1. ARCHITECTURE before IMPLEMENT (architecture must complete first)
2. API_CONTRACT before IMPLEMENT (contract spec informs implementation)
3. IMPLEMENT before TEST (tests run on implemented code)
4. IMPLEMENT before SECURITY (security audits built artifact)
5. QA always last (absolute; validates all preceding flows)

These ordering rules apply identically regardless of AI vocabulary in the request.

---

## FF-060-003 — TransitionType selection for B33 requests

| Condition | TransitionType |
|-----------|---------------|
| IMPLEMENT present (new feature) | feature_addition |
| "refactor" present (no new feature) | refactoring |
| documentation-only request | documentation |
| QA-only or security-only | N/A (sub-phase) |
| GapHandler only | N/A (gap, no flow) |

AI vocabulary does not alter TransitionType. Same hierarchy as all batches.

---

## FF-060-004 — GapHandler vs. predicate flow: mutual exclusion

Rule: When GapHandler fires (no predicates), no flow factory runs.
When any predicate fires, GapHandler does not fire (and no SCHEDULE_CAPABILITY_LEARNING is emitted).
These are mutually exclusive states. No request can simultaneously emit SCHEDULE_CAPABILITY_LEARNING and run a predicate flow.

---

## FF-060-005 — DomainId assertion for all flow factory outputs

Rule: All flow factory outputs for B33 gap requests carry DomainId: software-development.
This applies to GapHandler (gap logged) and all predicate flows (flow executed).
DomainId is always software-development for all B33 batch scenarios.
