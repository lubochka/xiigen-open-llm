# Layer 0-A — Compiler Pairs (008)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-008-001 — "interface" fires BOTH: ARCHITECTURE + API_CONTRACT

```
source_id: xiigen-b12-0a-008-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-008-001"
  user_request: "Design the service interface for the event bus and specify the API contract."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-008-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-008-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Service interface design and API contract specification for event bus"
  CompiledText: "Design the service interface for the event bus and specify the API contract."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design" (system design verb)
  "service interface" → ARCHITECTURE fires on "interface" (system design context: service boundary)
                      → API_CONTRACT fires on "interface" (API context: contract definition)
  "event bus" — integration component; "API" implied
  "specify" → DOCUMENT fires on "specify"
  "API contract" → API_CONTRACT fires on "api", "contract"
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Teaching note: Core Rule 4 — "interface" fires BOTH API_CONTRACT (API context) AND ARCHITECTURE
(system design context) when both signals are present. "design the service interface" contains
both: "design" triggers ARCHITECTURE and "interface" with API vocabulary ("API contract")
triggers API_CONTRACT. Both predicates fire simultaneously.

---

## B12-0A-008-002 — "interface" in system-only context: ARCHITECTURE but not API_CONTRACT

```
source_id: xiigen-b12-0a-008-002
transition_type: documentation
phase: 3
sub_phase: 3.1
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-008-002"
  user_request: "Design the interface between the order processing and inventory systems."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-008-002-prompt"
  CompiledPredicateIds: ["ARCHITECTURE"]
  Predicate evidence:
    "design" → ARCHITECTURE fires on "design"
    "interface" → ARCHITECTURE fires on "interface" (system design context only)
    No API vocabulary present (no: api, rest, grpc, graphql, webhook, openapi, swagger, contract)
    → API_CONTRACT does NOT fire
  Result: CompiledPredicateIds: ["ARCHITECTURE"], Confidence: 0.94

---

## B12-0A-008-003 — "interface" in API-only context: API_CONTRACT but not ARCHITECTURE

```
source_id: xiigen-b12-0a-008-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-008-003"
  user_request: "Specify the REST API interface for the product search endpoint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-008-003-prompt"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate evidence:
    "specify" → DOCUMENT fires on "specify"
    "REST API" → API_CONTRACT fires on "rest", "api"
    "interface" → API_CONTRACT fires on "interface" (in API context with "REST API" present)
    No "design" or architecture context → ARCHITECTURE does NOT fire
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-008-004 — "interface" with full context: event bus with both signals

```
source_id: xiigen-b12-0a-008-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-008-004"
  user_request: "Design the messaging interface for the event bus service and document the API contract for publishing and consuming events."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-008-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-008-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Event bus messaging interface design and API contract documentation for publish/consume operations"
  CompiledText: "Design the messaging interface for the event bus service and document the API contract for publishing and consuming events."
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" → ARCHITECTURE fires on "design"
  "interface" → ARCHITECTURE fires (system design context: "messaging interface")
               → API_CONTRACT fires (API context: "API contract" present later in sentence)
  "document" → DOCUMENT fires on "document"
  "API contract" → API_CONTRACT fires on "api", "contract"
  Result: CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"], Confidence: 0.93

Disambiguation table for "interface":
  | Context signals                    | Predicates fired                    |
  |------------------------------------|-------------------------------------|
  | "design" + no API vocab            | ARCHITECTURE only                   |
  | API vocab + no "design"            | API_CONTRACT only                   |
  | "design" + API vocab both present  | ARCHITECTURE + API_CONTRACT (both)  |
