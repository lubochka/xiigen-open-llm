# Layer 0-C — Knowledge Edge Pairs (023)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E023-001 — Hotfix deployment execution vocabulary is gap

Hotfix gap terms: "deploy hotfix", "emergency deployment", "patch to production", "restart after patch", "go-live immediately"
  Teaching: Hotfix deployment execution → gap. "hotfix" alone → gap. "deploy hotfix" → gap.

---

## E023-002 — "build the hotfix deployment service" fires IMPLEMENT

A: "Build the emergency hotfix deployment service" → IMPLEMENT (verb "build" + "service")
  Teaching: "service" fires IMPLEMENT. "emergency hotfix deployment" is the service role.

---

## E023-003 — "validate the security patch" fires both SECURITY and TEST

A: "Validate the security patch" → SECURITY fires on "security" + TEST fires on "validate"
  Teaching: Two triggers in one phrase. "validate" → TEST; "security patch" → SECURITY. Result: ["SECURITY", "TEST"].

---

## E023-004 — "verify the regression suite passes" fires TEST

A: "Verify the regression suite passes" → TEST fires on "verify"
  Teaching: "verify" fires TEST. "regression suite" is the test collection being verified.

---

## E023-005 — QA fires for emergency hotfix gate

A: "QA the hotfix patch" → QA | "QA sign-off before emergency deployment" → QA | "QA gate the hotfix" → QA
  Teaching: "QA" fires in emergency hotfix context. "patch", "emergency", "deployment" remain gap.
