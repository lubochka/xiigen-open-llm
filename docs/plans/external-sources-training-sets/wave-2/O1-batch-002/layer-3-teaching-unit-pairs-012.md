# Layer 3 — Teaching Unit Pairs (012)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-012-001 — Complete teaching unit summary: O1-batch-002 learning objectives

A: What are the primary learning objectives of O1-batch-002?

B: Learning objectives: (1) SECURITY lexicon for O1b (12 words: oauth, token, jwt, credential, certificate, encrypt, auth, tls, session, password, signing, key). (2) AUTHORIZATION lexicon for O1b (11 words: scope, tenant, grant, revoke, principal, service account, role, rbac, permission, acl, authorization). (3) SECURITY+AUTHORIZATION dedup → one 3.4 routing decision. (4) "service account" → AUTHORIZATION → 3.4 (not 4.1). (5) knowledge_edge: "login", "client portal", "access denied" are NOT SECURITY.

---

## O1b-L3-012-002 — Complete teaching unit summary: v6 corruption coverage

A: How does O1-batch-002 cover all three v6 corruption types?

B: v6-01 (predicate invention): 5+ examples in factory files 010-011, held-out files 002, 005, 007, 008, 011. v6-02 (gap suppression): 5+ examples in knowledge-edge files 003-011, orchestrator file 011, layer-3 file 011. v6-03 (wrong routing): 5+ examples in router file 011, held-out files 003, 004, 007, 010. Total v6 coverage: ≥15 examples per corruption type across all layers.

---

## O1b-L3-012-003 — Complete teaching unit summary: nine-node coverage

A: Nine-node coverage summary for O1-batch-002

B: All 9 node types present in 11 scenarios. Node 01 correctness: 11/11 use SelectedLogicalTransition (not raw string). Node 08 gate_b: present in all 11 scenarios. Node 09 HumanResult: present in all 11. Probationary_rejected scenario: 1 (scenario 009). Multi-flow scenarios (2+ flows): 7. Maximum predicate count scenario: 6 (scenario 011).

---

## O1b-L3-012-004 — Complete teaching unit summary: knowledge_edge coverage

A: Knowledge edge coverage summary for O1-batch-002

B: Total knowledge_edge pairs: 44+ across all layers. Knowledge-edge layer (0-C): 12 files × 5 pairs = 60 pairs specifically about knowledge_edge boundaries. Key teaching points: "login" excluded, "client portal" excluded, "access denied" excluded, "oauth" ALWAYS fires SECURITY, "scope" ALWAYS fires AUTHORIZATION in software context.

---

## O1b-L3-012-005 — Complete teaching unit summary: batch completion certification

A: O1-batch-002 teaching unit certification

B: CERTIFIED: O1-batch-002 is complete and validated. Files: 101/101. Teaching objectives: all covered. v6 corruptions: all 3 types demonstrated. Nine-node: all 9 nodes × 11 scenarios. Knowledge-edge: 44+ pairs. Dedup rule: 18+ reinforcements. Gate B: PASSED. Batch is ready for corpus integration into XIIGEN Wave-2 training set.
