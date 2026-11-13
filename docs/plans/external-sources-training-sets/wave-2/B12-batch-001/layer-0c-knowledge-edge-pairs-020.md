# Layer 0-C — Knowledge Edge Pairs (020)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-020-001 — FileUploadAPI: "authentication" in upload API context → SECURITY (unconditional)

```
source_id: xiigen-b12-0c-020-001
rule_id: B12-RULE-AUTHENTICATION-ALWAYS
pair_kind: knowledge_edge
```

A:
  CompiledText: "Document the file upload REST API authentication requirements and signed URL contract."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Predicate analysis:
    "document" → DOCUMENT fires
    "REST" → API_CONTRACT fires
    "API" → API_CONTRACT fires
    "authentication" → SECURITY fires ALWAYS (Core Rule 6)
    "requirements" → DOCUMENT fires
    "contract" → API_CONTRACT fires
  Result: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Teaching: "signed URL" is an authentication mechanism. "authentication requirements"
  explicitly contains "authentication" → SECURITY fires. The context (file upload) is irrelevant.

---

## B12-0C-020-002 — FileUploadAPI: "idempotent upload contract" edge case

```
source_id: xiigen-b12-0c-020-002
rule_id: B12-RULE-IDEMPOTENT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the idempotent upload contract for the file storage API."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "define" → DOCUMENT fires
    "idempotent" → API_CONTRACT fires
    "contract" → API_CONTRACT fires
    "API" → API_CONTRACT fires
  Result: ["API_CONTRACT", "DOCUMENT"]
  Teaching: "idempotent upload contract" combines three API_CONTRACT triggers:
  idempotent + contract + api. All three fire the same predicate independently.

---

## B12-0C-020-003 — FileUploadAPI: "multipart request" fires API_CONTRACT in API context

```
source_id: xiigen-b12-0c-020-003
rule_id: B12-RULE-REQUEST-API-CONTEXT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Specify the file upload API endpoint contract including multipart request format and response schema."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "specify" → DOCUMENT fires
    "API" → API_CONTRACT fires (API co-occurrence for "request" and "schema")
    "endpoint" → API_CONTRACT fires
    "contract" → API_CONTRACT fires
    "request" → API_CONTRACT fires (in API context — api + endpoint co-occurrence present)
    "multipart" — domain modifier for request, does not suppress "request" trigger
    "response" → API_CONTRACT fires
    "schema" → API_CONTRACT fires (API co-occurrence present)
  Result: ["API_CONTRACT", "DOCUMENT"]
  Teaching: "multipart request" — "multipart" is a modifier for "request". The trigger is
  "request" in API context. Multipart-form encoding is an API contract concern.
