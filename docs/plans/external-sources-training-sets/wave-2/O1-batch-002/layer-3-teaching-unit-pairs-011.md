# Layer 3 — Teaching Unit Pairs (011)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-011-001 — v6-02 teaching: "client portal" misclassification detection

A: ModelError: { prompt: "the client portal is not loading", predicted: "SECURITY", correct: "knowledge_edge" }

B: ErrorAnalysis: { error_type: "v6-02 gap suppression (inverted)", root_cause: "Model associated 'client' with OAuth client, triggering false SECURITY prediction", correction: "'client portal' is colloquial UI language — 'client' alone without technical context does not fire any predicate", training_signal: "Add more negative examples where 'client' appears in non-technical context" }

---

## O1b-L3-011-002 — v6-02 teaching: correct detection rule

A: When does "client" fire a predicate vs. not?

B: FIRES: "client credentials" (SECURITY — explicit lexicon item), "client_id" (SECURITY), "client secret" (SECURITY), "oauth client" (SECURITY). DOES NOT FIRE: "client portal", "client dashboard", "client app" (in UI context), "client is complaining". Rule: "client" must be paired with a technical OAuth/credential term to fire SECURITY.

---

## O1b-L3-011-003 — v6-02 teaching: training intervention

A: Training intervention for "client portal" false SECURITY classification

B: Intervention: { negative_examples_added: 5, source: "layer-0c-knowledge-edge-pairs-002.md (all 5 pairs)", pattern: "All 5 pairs use 'client portal X' in various operational complaints → all correctly produce knowledge_edge", expected_impact: "Model learns 'client portal' context is always knowledge_edge regardless of complaint type" }

---

## O1b-L3-011-004 — v6-02 teaching: contrast training pair

A: Contrast training: "client portal down" vs. "client credentials failing"

B: "the client portal is down" → Predicates: [] → knowledge_edge → no routing.
   "the client credentials are failing for service accounts" → Predicates: ["SECURITY", "AUTHORIZATION"] → 3.4 routing.
   Training signal: same word "client" in different technical context produces opposite outcomes. Context of adjacent words ("portal" vs. "credentials") is the determining factor.

---

## O1b-L3-011-005 — v6-02 teaching: measurement metric

A: How to measure v6-02 misclassification rate?

B: Metric: { test_set: "50 prompts with 'client portal' variants", expected_knowledge_edge: 50, acceptable_false_positive_rate: 0, gate_b: "v6-02 error rate ≤ 0% for 'client portal' class of prompts", measurement: "Run test set before and after batch 002 training; compare false positive count" }
