# Layer 0-A — Compiler Pairs (011)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1b-0A-011-001 — DOCUMENT: oauth flow documentation

A: "document the OAuth 2.0 authorization flow for the developer portal"

B: Predicates: ["DOCUMENT", "SECURITY"] | TransitionType: documentation

---

## O1b-0A-011-002 — DOCUMENT: scope specification

A: "write the functional specification for tenant scope management"

B: Predicates: ["DOCUMENT", "AUTHORIZATION"] | TransitionType: documentation

---

## O1b-0A-011-003 — DOCUMENT: api spec + security

A: "describe the authentication requirements in the API specification for external partners"

B: Predicates: ["DOCUMENT", "SECURITY", "API_CONTRACT"] | TransitionType: documentation

---

## O1b-0A-011-004 — KNOWLEDGE EDGE boundary: "client portal is down"

A: "the client portal is down again and users can't access their dashboards"

B: Predicates: [] | TransitionType: knowledge_edge
   Reasoning: BOUNDARY CASE — "client portal" is not in any predicate lexicon. "down" and "users can't access" are colloquial operational complaints. Zero technical predicate keywords present. Result: knowledge_edge. Compare to O1b-kedge-002: "oauth is not working" fires SECURITY because "oauth" IS in the lexicon.

---

## O1b-0A-011-005 — DOCUMENT: use-case narrative

A: "write a use-case narrative for the service account authorization flow"

B: Predicates: ["DOCUMENT", "AUTHORIZATION"] | TransitionType: documentation
