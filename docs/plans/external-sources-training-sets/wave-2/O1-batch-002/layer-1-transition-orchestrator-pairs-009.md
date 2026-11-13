# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: orchestration

---

## O1b-L1-009-001 — TransitionType hierarchy: IMPLEMENT overrides DOCUMENT

A: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType determination

B: TransitionType: feature_addition
   Rule: IMPLEMENT present → feature_addition, regardless of DOCUMENT also being present. DOCUMENT alone (without IMPLEMENT) → documentation. IMPLEMENT always wins.

---

## O1b-L1-009-002 — TransitionType hierarchy: "refactor" verb overrides everything

A: "refactor the OAuth token validation service to use the new key management system" | Predicates: ["IMPLEMENT", "SECURITY"]

B: TransitionType: refactoring
   Rule: "refactor" verb detected in prompt → refactoring overrides feature_addition even though IMPLEMENT is present. Verb detection takes priority over predicate-based TransitionType inference.

---

## O1b-L1-009-003 — TransitionType hierarchy: QA alone → no contribution

A: Predicates: ["QA"] | TransitionType determination

B: TransitionType: knowledge_edge (or no_contribution)
   Rule: QA alone does not set TransitionType. QA is the last predicate by canonical ordering but does not independently classify the transition. With no other predicates, the prompt produces no TransitionType contribution.

---

## O1b-L1-009-004 — TransitionType hierarchy: TEST without IMPLEMENT → documentation

A: Predicates: ["TEST", "SECURITY"] | TransitionType determination

B: TransitionType: documentation
   Rule: TEST without IMPLEMENT → documentation. SECURITY present but no IMPLEMENT → TransitionType stays documentation. The nine-node pipeline for 3.4 runs in documentation mode.

---

## O1b-L1-009-005 — TransitionType hierarchy complete table

A: O1b TransitionType determination rules

B: | Condition | TransitionType |
   |-----------|---------------|
   | "refactor" verb present | refactoring (overrides all) |
   | IMPLEMENT present (no refactor) | feature_addition |
   | TEST or DOCUMENT without IMPLEMENT | documentation |
   | QA alone | no_contribution |
   | No predicates | knowledge_edge |
   | SECURITY/AUTHORIZATION without IMPLEMENT | feature_addition (design context) |
