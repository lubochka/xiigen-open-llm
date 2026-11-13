# Layer 3 — Teaching Unit Pairs (004)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-004-001 — Teaching: "deployment" is GAP in operational context

LESSON: "deployment" alone does not fire any predicate in B38 operational context.

WHY: B38 rule 4 — "deployment alone → GAP — does not fire IMPLEMENT without code-level context."
Deployment refers to the act of running/executing a deployment. This is operational, not code authoring.
IMPLEMENT = code authoring. Running a deployment is not authoring code.

EXAMPLES:
"Monitor the deployment" → ["monitor"=GAP, "deployment"=GAP] → knowledge_edge
"Schedule the deployment" → [] → knowledge_edge
"Review the deployment" → ["QA"] only (review fires QA; deployment is gap warning)
"Deployment readiness criteria" → ["DOCUMENT"] only (criteria fires DOCUMENT; deployment is gap)

---

## B38-TU-004-002 — Teaching: "build" fires IMPLEMENT (contrast with deployment)

LESSON: "build" fires IMPLEMENT; "deployment" does not.

Side-by-side:
"Audit the build artifacts" → ["QA", "IMPLEMENT"] (build fires IMPLEMENT)
"Audit the deployment artifacts" → ["QA"] only (deployment is gap)

"Verify build provenance" → ["QA", "IMPLEMENT"]
"Verify deployment logs" → ["QA"] only (deployment is gap)

"Approve the build for release" → ["QA", "IMPLEMENT"]
"Approve the deployment for release" → ["QA"] only (deployment is gap)

MEMORABLE DISTINCTION:
Build = code artifact = IMPLEMENT
Deployment = operational act = GAP

---

## B38-TU-004-003 — Teaching: "execute" is GAP (not IMPLEMENT)

LESSON: "execute" does not fire IMPLEMENT in B38 execution context.

WHY: B38 rule 8 — "execute → GAP (not IMPLEMENT in execution context; IMPLEMENT = code authoring, not running)"
Executing is running. Writing code is authoring. These are different.

EXAMPLES:
"Execute the pipeline" → [execute=GAP, pipeline=IMPLEMENT] → ["IMPLEMENT"] (pipeline fires; execute doesn't)
"Execute the rollout" → [execute=GAP, rollout=GAP] → knowledge_edge
"Execute the deployment" → [execute=GAP, deployment=GAP] → knowledge_edge
"Execute the test" → [execute=GAP] → knowledge_edge (if no other predicate terms)

TRAP: "Execute the service deployment" → "service" fires IMPLEMENT; "execute" and "deployment" are still gap
Result: ["IMPLEMENT"] with gap warnings on execute + deployment

---

## B38-TU-004-004 — Teaching: the "deploy the build" boundary

"deploy the build":
- "deploy" → GAP (deploy/deployment = operational; not in predicate lexicon)
- "build" → IMPLEMENT (build = code artifact)
- Result: ["IMPLEMENT"] with no gap warning for "deploy" (unless it's flagged as "deployment")

Note: "deploy" (as verb) and "deployment" (as noun) are both operational vocabulary in B38. Neither fires any predicate. "build" still fires IMPLEMENT.
