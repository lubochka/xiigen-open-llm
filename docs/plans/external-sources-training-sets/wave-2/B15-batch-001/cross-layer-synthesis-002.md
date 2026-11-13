# Cross-Layer Synthesis (002)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

## CLS-002 — IMPLEMENT+ARCHITECTURE: Build service with domain model

Request: "Build the order fulfillment service using a domain model for fulfillment state transitions."

### Layer 0-A (Compiler)
build → IMPLEMENT | "domain model" → ARCHITECTURE
Predicates: ["IMPLEMENT", "ARCHITECTURE"]

### Layer 0-B (Router)
ARCHITECTURE → ArchitectureFlow31 (3.1) | IMPLEMENT → ImplementationFlow41

### Layer 0-C (Knowledge Edge)
"domain model" fires ARCHITECTURE even when embedded inside an IMPLEMENT request.
The IMPLEMENT verb (build) and ARCHITECTURE trigger (domain model) fire independently.

### Layer 0-D (Flow Factory)
Two flows. TransitionType: feature_addition (IMPLEMENT present overrides ARCHITECTURE-only documentation).

### Layer 1 (Transition Orchestrator)
ArchitectureFlow31 → ImplementationFlow41 (design spec before build)

### Layer 2 (Flow Execution)
ArchitectureFlow31: FulfillmentDomainModel — state machine spec, aggregate root design. APPROVED.
ImplementationFlow41: FulfillmentService built to domain model spec. APPROVED.

### Layer 3 (Teaching)
"domain model" always fires ARCHITECTURE — even inside build requests. Two predicates fired from one sentence.

TransitionType: feature_addition | ARCHITECTURE first — absolute ordering.
