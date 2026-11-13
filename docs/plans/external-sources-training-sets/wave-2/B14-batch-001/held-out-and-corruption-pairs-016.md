# Held-Out and Corruption Pairs (016)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H016-001 — Held-out: "edge topology" fires ARCHITECTURE

A: "Architect the edge CDN topology for the static asset delivery."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## H016-002 — Held-out: "TLS termination" fires SECURITY

A: "Configure TLS termination and HSTS (HTTP Strict Transport Security) at the CDN edge."
  Expected: ["IMPLEMENT", "SECURITY"]
  Held-out for evaluation.

---

## H016-003 — Held-out: "purge the CDN cache" is gap

A: "Purge the CloudFront cache for the homepage assets."
  Expected: []
  Held-out for evaluation.

---

## C016-001 — Corruption v6-01: CDN invented as predicate

A: "Design the CDN edge topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "CDN"] ← v6-01 (CDN is not a valid predicate — it is a platform technology name)
  Teaching: "CDN" is a platform abbreviation. It does not fire any predicate. "design" + "edge topology" → ARCHITECTURE.

---

## C016-002 — Corruption v6-03: cache purge fires IMPLEMENT

A: "Purge the CDN cache."
  Correct: []
  Corrupt: ["IMPLEMENT"] ← v6-03 (treats cache purge as an implementation action)
  Teaching: "purge" is an operational action, not in IMPLEMENT lexicon. IMPLEMENT requires: write, build, create, scaffold, implement, configure (code context). "purge" is a cache management operation = gap.
