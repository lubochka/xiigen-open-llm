# Layer 0-C — Knowledge Edge Pairs (058) — ZeroTrustAudit Edge Cases
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E058-001 — Zero Trust vocabulary boundary

"Zero Trust micro-segmentation" in audit context → SECURITY (security architecture audit)
"enforce least-privilege access" → SECURITY + AUTHORIZATION (enforce + least-privilege = dual)
"adopt Zero Trust model" → gap (strategic planning without audit vocabulary)
"Zero Trust architecture" as a design topic alone → gap

---

## E058-002 — mTLS (Mutual Transport Layer Security) vocabulary

"mTLS certificate authentication" → SECURITY (TLS = crypto group, mutual auth = security action)
"configure mTLS" → gap (configuration without audit vocabulary)
"audit mTLS between services" → SECURITY (audit + TLS)
Teaching: mTLS fires SECURITY via TLS (cryptographic group). Configuration of mTLS is gap.

---

## E058-003 — Unencrypted channel vocabulary

"unencrypted east-west traffic" → SECURITY (unencrypted = cryptographic vulnerability)
"east-west traffic" alone → gap (network topology term without security qualifier)
"scan for unencrypted channels" → SECURITY (scan + unencrypted)

---

## E058-004 — Least-privilege access dual predicate

"least-privilege access" → SECURITY + AUTHORIZATION (dual: security principle + authorization vocabulary)
"minimum permissions" → AUTHORIZATION (permission boundary vocabulary)
"grant only required permissions" → AUTHORIZATION (permission management action)
Teaching: "least-privilege access" fires both predicates. "minimum permissions" fires AUTHORIZATION alone.

---

## E058-005 — Trust state calibration

mTLS + unencrypted channels + least-privilege audit → deterministic_generic (high, dual predicates)
"adopt Zero Trust" alone → gap (zero predicates)
Zero Trust segmentation + enforce + least-privilege → deterministic_generic (dual predicates)
