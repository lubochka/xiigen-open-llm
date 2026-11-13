# Layer 0-C — Knowledge Edge Pairs (050)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-050-001 — Rule: "API gateway" is a domain context; API_CONTRACT fires from api/contract/route terms

```
source_id: xiigen-b12-0c-050-001
pair_kind: knowledge_edge
```

### API gateway domain context rule

"API gateway" is a system component context. It does not independently fire API_CONTRACT.
API_CONTRACT fires from the API vocabulary present in the request alongside "gateway":

Examples:
  "Wire up the API gateway to the service mesh" → API_CONTRACT (wire up + api)
  "Define the API gateway contract" → API_CONTRACT (api + contract) + DOCUMENT (define)
  "Implement the API gateway routing layer" → IMPLEMENT + API_CONTRACT (api + routing)

"Gateway" alone without "api" co-occurrence would not fire API_CONTRACT. The "api" in "API gateway"
provides the necessary co-occurrence.

---

## B12-0C-050-002 — Rule: "TLS" alone does NOT fire SECURITY; "authentication" is required

```
source_id: xiigen-b12-0c-050-002
pair_kind: knowledge_edge
```

### TLS vs authentication SECURITY distinction

TLS (Transport Layer Security) is a transport security protocol. The word "TLS" does NOT fire SECURITY.
SECURITY fires from "authentication" (Core Rule 6: unconditional) or "oauth"/"oauth2".

Examples:
  "Connect the service mesh for mutual TLS" → API_CONTRACT only (connect + api) — no SECURITY
  "Connect the service mesh for mutual TLS authentication" → API_CONTRACT + SECURITY
    "authentication" present → SECURITY fires

The distinction:
  TLS alone → transport layer protocol word → no SECURITY
  "authentication" + TLS context → SECURITY fires (from "authentication" keyword)

Corrupt (v6-02 variant): treating "TLS" or "mutual TLS" as a SECURITY trigger. Only "authentication"
and "oauth" are unconditional SECURITY triggers in B12.

---

## B12-0C-050-003 — Rule: "design the API gateway architecture and define the authentication contract" fires four predicates

```
source_id: xiigen-b12-0c-050-003
pair_kind: knowledge_edge
```

### Four-predicate fire via API gateway + design + define + authentication

"Design the API gateway architecture and define the authentication contract for all inbound requests":
  ARCHITECTURE: "design" (unconditional)
  API_CONTRACT: "api" + "contract"
  DOCUMENT: "define"
  SECURITY: "authentication" (Core Rule 6: unconditional)
  TransitionType: documentation (no IMPLEMENT)
  DecisionCount: 4
  Routes: [3.1, 3.3, 2.1, 3.4]

ARCHITECTURE executes first (tier 0 because it informs all other flows).
API_CONTRACT and SECURITY can run in parallel after ARCHITECTURE.
DOCUMENT runs after API_CONTRACT (contract content needed for documentation).
QA runs last.
