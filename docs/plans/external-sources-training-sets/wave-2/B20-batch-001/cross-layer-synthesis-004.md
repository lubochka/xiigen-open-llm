# Cross-Layer Synthesis (004)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S004 — End-to-end trace: four-predicate request across all layers

### Layer 0-A (Compiler)
Input: "Write unit tests for the EncryptionService, implement the AES-256 (Advanced Encryption Standard, 256-bit) key rotation, document the algorithm, and assert no plaintext is stored."
  "write" → IMPLEMENT | "unit tests" → TEST | "implement" → IMPLEMENT | "AES-256 key rotation" → SECURITY | "document" → DOCUMENT | "assert" → TEST | "plaintext" → SECURITY
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY", "DOCUMENT"]

### Layer 0-B (Router)
  IMPLEMENT → SubPhase 4.1 | TransitionType: feature_addition
  TEST → SubPhase 5.1
  SECURITY → SubPhase 5.2 (test assertion context, not audit context)
  DOCUMENT → SubPhase 6.2
  DecisionSet: 4 decisions | SubPhases: [4.1, 5.1, 5.2, 6.2]

### Layer 0-D (Factory)
  FlowFactory.CreateMulti(["IMPLEMENT","TEST","SECURITY","DOCUMENT"]) → 4 flows:
    FeatureImplementationFlow41
    UnitTestingFlow51
    SecurityTestingFlow52
    DocumentationFlow62
  ExecutionMode: sequential (IMPLEMENT → TEST → SECURITY → DOCUMENT)

### Layer 1 (Transition Orchestrator)
  TransitionType: feature_addition | Primary: FeatureImplementationFlow41
  All four flows emitted | ExecutionOrder: [4.1, 5.1, 5.2, 6.2]

### Layer 2 (Nine-Node — 5.1 component)
  Service: EncryptionService | Tests: key rotation, plaintext assertion, AES mode switching
  Tests produced: 12 | Result: PASS

### Layer 3 (Teaching)
  Lesson: "AES-256 key rotation" fires SECURITY (cryptographic action). "document" fires DOCUMENT. "assert" and "write tests" both fire TEST (deduplicated). "write" and "implement" both fire IMPLEMENT (deduplicated).
  Four unique predicates → four flows. No collapse.

### Cross-layer invariant
  Maximum four-predicate accumulation confirmed. Deduplication within predicate, accumulation across predicates. All four flows are peers — none suppresses another.
