# Held-Out and Corruption Pairs (024) — Complete Held-Out and Corruption Reference for 7.1
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H024-001 — Held-out: "service" fires IMPLEMENT in any release context

A: "Review the deployment service." vs "Build the deployment service."
  Expected (review): ["IMPLEMENT"] — "service" fires IMPLEMENT even with "review" verb
  Expected (build): ["IMPLEMENT"] — "build" + "service" both fire IMPLEMENT (dedup)
  Held-out: "service" fires IMPLEMENT regardless of the verb. Even "review the service" fires IMPLEMENT.

---

## H024-002 — Held-out: "criteria" fires DOCUMENT in any release context

A: "Review the release criteria." vs "Define the deployment criteria."
  Expected (review): ["DOCUMENT"] — "criteria" fires DOCUMENT even with "review" verb
  Expected (define): ["DOCUMENT"] — "define" + "criteria" both fire DOCUMENT (dedup)
  Held-out: "criteria" fires DOCUMENT regardless of verb.

---

## H024-003 — Held-out: pure gap → knowledge_edge (not GapHandler or SCHEDULE_CAPABILITY_LEARNING)

A: "Plan the deployment rollout and set up the go-live checklist."
  Expected: [] → knowledge_edge output
  Held-out: Pure gap in release domain → knowledge_edge only. Not GapHandler. Not SCHEDULE_CAPABILITY_LEARNING.

---

## C024-001 — Corruption summary: invented predicates in 7.1 context

Invented predicates seen in corruptions: DEPLOYMENT, RELEASE, FEATURE_FLAG, AB_TESTING, COMPLIANCE, EMERGENCY, DEPENDENCY, ROLLOUT
  Correct: None of these exist. The valid predicate set is: QA, IMPLEMENT, TEST, DOCUMENT, ARCHITECTURE, SECURITY, AUTHORIZATION, API_CONTRACT.
  Teaching: Any predicate not in the valid set is an invention. Models must not create new predicates.

---

## C024-002 — Corruption summary: FAIL/PASS_WITH_WARNINGS normalization

Corrupted: FAIL → PASS | PASS_WITH_WARNINGS → PASS for any release context
  Correct: Verdict must reflect actual gate results. FAIL means FAIL. PASS_WITH_WARNINGS means warnings present.
  Teaching: Models must not normalize verdicts. Emergency, hotfix, DR, and compliance contexts do not override FAIL verdicts.
