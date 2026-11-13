# Layer 3 — Teaching Unit Pairs (002)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B38-TU-002-001 — Teaching: "service" fires IMPLEMENT everywhere (release context)

LESSON: The word "service" always fires the IMPLEMENT predicate in B38. This applies even when the user's intent is clearly about reviewing, approving, or releasing a service — not writing its code.

WHY: In the software-development domain, "service" almost always refers to a software service (a code artifact). The compiler fires IMPLEMENT to ensure the code-review phase (4.1) is included whenever a service is discussed.

EXAMPLES WITH CORRECTION:

Wrong inference: "Approve the payment service" → ["QA"] only
Correct result: "Approve the payment service" → ["QA", "IMPLEMENT"]
(because "service" fires IMPLEMENT regardless of "approve")

Wrong inference: "Sign off on the notification service release" → ["QA"] only
Correct result: ["QA", "IMPLEMENT"]
(because "notification service" fires IMPLEMENT; "sign off" fires QA)

Wrong inference: "Go-live approval for the order service" → ["QA"] only
Correct result: ["QA", "IMPLEMENT"] with gap warning on "go-live"
(because "order service" fires IMPLEMENT; "approval" fires QA; "go-live" is gap)

MEMORABLE RULE:
Service → IMPLEMENT. Always. No exceptions in B38.

---

## B38-TU-002-002 — Teaching: same rule applies to "build" (rule 6)

LESSON: "build" fires IMPLEMENT even in phrases like "build artifacts", "build provenance", "build pipeline".

"Build" is code vocabulary. A build is the product of a compilation/packaging process.
Even when "build" appears as a noun describing an artifact (not a verb describing an action), IMPLEMENT fires.

EXAMPLES:
"Audit the build artifacts" → ["QA","IMPLEMENT"] (build fires even as artifact noun)
"Verify build provenance records" → ["QA","IMPLEMENT"] (build provenance fires IMPLEMENT)
"Approve the build for release" → ["QA","IMPLEMENT"] (build fires)

CONTRAST WITH DEPLOYMENT:
"Audit the deployment artifacts" → deployment is GAP; artifacts alone does not fire IMPLEMENT
"deployment" is operational; "build" is code

---

## B38-TU-002-003 — Teaching: "pipeline" fires IMPLEMENT (code artifact)

LESSON: "pipeline" in CI/CD context is a code artifact (the pipeline configuration).
Pipeline configuration is code that is authored and maintained.
Therefore "pipeline" fires IMPLEMENT.

"Review the CI/CD pipeline" → ["QA","IMPLEMENT"]
"Approve the deployment pipeline code" → ["QA","IMPLEMENT"] (deployment=gap; pipeline=IMPLEMENT)

Note: "deployment pipeline" — "pipeline" fires IMPLEMENT; "deployment" produces gap warning.
The two words are evaluated independently.
