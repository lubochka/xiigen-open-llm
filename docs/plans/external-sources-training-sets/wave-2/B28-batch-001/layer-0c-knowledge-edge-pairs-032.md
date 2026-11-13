# Layer 0-C — Knowledge Edge Pairs (032) — LicenseComplianceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E032-001 — LicenseComplianceGate trigger edge

A: "run LicenseComplianceGate" → SECURITY | "license compliance check" → SECURITY
  Teaching: License compliance (regulatory check) fires SECURITY.

---

## E032-002 — "publishing" is gap

A: "before publishing the release" → [] → knowledge_edge
  Teaching: "publishing" is release artifact distribution vocabulary; gap.

---

## E032-003 — "check all open-source licenses" is gap (no security framing)

A: "check all open-source licenses in the deployment bundle" → [] → knowledge_edge
  Teaching: "check" alone without security/vulnerability framing → gap.

---

## E032-004 — "validate license compatibility" fires TEST

A: "validate license compatibility before release" → TEST
  Teaching: "validate" fires TEST always — including in license validation context.

---

## E032-005 — License compliance vs license check distinction

A: "license compliance check" → SECURITY | "license compatibility check" → TEST (via "validate") | "check licenses" → [] → gap
  Teaching: Compliance check → SECURITY; validation check → TEST; plain check → gap.
