# Layer 0-A — Compiler Pairs (004)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: compiler

---

## B12-0A-004-001 — "data contracts": API_CONTRACT fires on "contract" regardless of modifier

```
source_id: xiigen-b12-0a-004-001
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-004-001"
  user_request: "Define the data contracts between the order service and the fulfillment service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-004-001-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-004-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Data contract definition between order service and fulfillment service"
  CompiledText: "Define the data contracts between the order service and the fulfillment service."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "data contracts" → API_CONTRACT fires on "contract" — modifier "data" does NOT suppress it
  No IMPLEMENT — "define" is documentation verb, not build verb
  No ARCHITECTURE — no design/system context
  No SECURITY — no auth/token terms
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

Teaching note: Core Rule 2 — "contract" fires API_CONTRACT REGARDLESS of modifier.
"data contracts" = API_CONTRACT. "service contracts" = API_CONTRACT. "message contracts" = API_CONTRACT.
"event contracts" = API_CONTRACT. The word "contract" is the trigger, not the noun phrase.

---

## B12-0A-004-002 — "service contracts": same rule — contract fires API_CONTRACT

```
source_id: xiigen-b12-0a-004-002
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-004-002"
  user_request: "Specify the service contracts for the payment and notification microservices."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-004-002-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-004-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Service contract specification for payment and notification microservices"
  CompiledText: "Specify the service contracts for the payment and notification microservices."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "specify" → DOCUMENT fires on "specify"
  "service contracts" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-004-003 — "message contracts": contract fires API_CONTRACT again

```
source_id: xiigen-b12-0a-004-003
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-004-003"
  user_request: "Document the message contracts for events published by the inventory service to the event bus."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-004-003-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-004-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Message contract documentation for inventory service events on event bus"
  CompiledText: "Document the message contracts for events published by the inventory service to the event bus."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" → DOCUMENT fires on "document"
  "message contracts" → API_CONTRACT fires on "contract"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

---

## B12-0A-004-004 — "event contracts": contract fires API_CONTRACT, no IMPLEMENT

```
source_id: xiigen-b12-0a-004-004
transition_type: documentation
phase: 3
sub_phase: 3.3
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B12-0A-004-004"
  user_request: "Define the event contracts for the customer lifecycle events and annotate each field's purpose."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B12-0A-004-004-prompt"
  SourcePath: "wave-2/B12-batch-001/compiler-input/0a-004-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Event contract definition and field annotation for customer lifecycle events"
  CompiledText: "Define the event contracts for the customer lifecycle events and annotate each field's purpose."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" → DOCUMENT fires on "define"
  "event contracts" → API_CONTRACT fires on "contract"
  "annotate" → DOCUMENT fires on "annotate"
  Result: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"], Confidence: 0.94

Teaching note: Five modifiers of "contract" that all trigger API_CONTRACT:
  1. "data contracts"    → API_CONTRACT
  2. "service contracts" → API_CONTRACT
  3. "message contracts" → API_CONTRACT
  4. "event contracts"   → API_CONTRACT
  5. "webhook contract"  → API_CONTRACT
All fire via the same rule: "contract" → API_CONTRACT regardless of modifier (Core Rule 2).
