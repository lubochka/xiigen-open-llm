# Layer 0-C — Knowledge Edge Pairs (041) — SSRFAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E041-001 — SSRF fires SECURITY as solo noun

"SSRF" → SECURITY (attack vector solo noun)
"server-side request forgery" spelled out → SECURITY (full form of SSRF)
"fetch external URL" → gap (no SSRF/vulnerability vocabulary)
"call external API" → gap (no security-action vocabulary)

---

## E041-002 — Allowlist enforcement

"enforce an allowlist" → SECURITY (enforce = security-action verb)
"implement allowlist" → IMPLEMENT (implementation action)
"add URL to allowlist" → gap (configuration action)
Teaching: "enforce" fires SECURITY. "implement allowlist" fires IMPLEMENT. "add to list" is gap.

---

## E041-003 — Cloud metadata endpoint specificity

"AWS metadata endpoint 169.254.169.254" in SSRF context → SECURITY (SSRF attack-vector qualifier)
"access EC2 metadata" in operational context → gap (no audit/vulnerability vocabulary)
"audit whether metadata endpoint is reachable via SSRF" → SECURITY (audit + SSRF)

---

## E041-004 — Webhook URL boundary

"process webhook POST body" → gap (data processing)
"audit webhook URL for SSRF injection" → SECURITY (audit + SSRF)
"validate webhook URL" without security context → gap (validation without attack vocabulary)

---

## E041-005 — Trust state calibration

SSRF alone + audit → deterministic_generic
"call external API" → gap (zero predicates)
SSRF + allowlist enforcement + metadata endpoint → deterministic_generic (high)
