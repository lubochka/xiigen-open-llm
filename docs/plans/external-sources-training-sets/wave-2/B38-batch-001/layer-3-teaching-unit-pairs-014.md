# Layer 3 — Teaching Unit Pairs (014)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-014-001 — Teaching: release train approval is QA alone (no IMPLEMENT by default)

LESSON: "Release train" is a delivery-planning term. Neither "release" nor "train" fires IMPLEMENT.

"Approve the Q2 release train" → ["QA"] only
"Inspect the build artifacts in the release train" → ["QA","IMPLEMENT"] (because "build" fires)

Without code vocabulary, a release train scenario is always QA-only.
With code vocabulary ("service", "build", "pipeline"), IMPLEMENT co-fires.

MEMORABLE RULE: Release train + approval verbs = QA only. Release train + build/service = QA + IMPLEMENT.

---

## B38-TU-014-002 — Teaching: "deployments" in release train is gap

LESSON: "service deployments" — "service" fires IMPLEMENT, "deployments" produces a gap warning.

"Review all service deployments in the release train" → ["QA","IMPLEMENT"]
- "service" → IMPLEMENT (rule 2)
- "deployments" → GAP warning (rule 4: deployment is operational context)
- "Review" → QA

"deployments" gets a CapabilityGapWarning but does NOT suppress IMPLEMENT (which came from "service").
The two words in "service deployments" are evaluated independently.

---

## B38-TU-014-003 — Teaching: build artifacts in release train exits knowledge_edge

LESSON: Adding "build" to any otherwise-gap sentence immediately produces an IMPLEMENT predicate.

Without build/service/pipeline:
"Launch the Q2 release train on the scheduled go-live date." → [] → knowledge_edge

With build:
"Inspect the build artifacts in each release train slot." → ["QA","IMPLEMENT"]

The word "build" is a code-vocabulary anchor. Wherever it appears, IMPLEMENT fires.
