# Wave 2 Center Phase Batch 001 - Sub-Phase Router Set v1
# Date: 2026-05-05

---

## Purpose

Provide the first real Wave 2 training pair set for the `sub-phase-router`
logical unit.

Each A is the exact `SubPhaseRouterSourceDocument` input shape. Each B is the
exact `LogicalUnitResult<SubPhaseRoutingDecision>` output shape.

This file contains 20 training pairs, matching the 20 training rows in
`WAVE-2-CENTER-PHASE-BATCH-001-SOURCE-LEDGER-v1.md`.

---

## Common B Envelope

Unless a pair overrides it:

```text
UnitId: unit-sub-phase-router-v1
UnitRole: sub-phase-router
DomainTag: whole_development_area
Warnings: []
FailureClassification: source_classified_by_sub_phase_router
KnowledgeEdge: none
```

---

## Pair Index

| pair_id | A.DocumentId | B.Output.PredictedPhaseId | B.Output.PredictedSubPhaseId | B.Confidence | B.Output.NearestWrongSubPhaseScore | B.PredicateIds | B.EvidenceIds |
|---|---|---|---|---:|---:|---|---|
| W2C-RTR-001 | W2C-3.1-A-001 | phase-3-technical-design | 3.1-architecture-and-component-topology | 0.88 | 0.31 | DECLARE, CREATE, RETURN | W2C-3.1-A-001 |
| W2C-RTR-002 | W2C-3.1-B-002 | phase-3-technical-design | 3.1-architecture-and-component-topology | 0.86 | 0.33 | DECLARE, GET_OP, VALIDATE_OP | W2C-3.1-B-002 |
| W2C-RTR-003 | W2C-3.1-C-003 | phase-3-technical-design | 3.1-architecture-and-component-topology | 0.87 | 0.32 | BRANCH, CREATE, RETURN | W2C-3.1-C-003 |
| W2C-RTR-004 | W2C-3.1-D-004 | phase-3-technical-design | 3.1-architecture-and-component-topology | 0.84 | 0.36 | VALIDATE_OP, FAIL, RETURN | W2C-3.1-D-004 |
| W2C-RTR-005 | W2C-3.3-A-005 | phase-3-technical-design | 3.3-api-contract-definition | 0.89 | 0.30 | DECLARE, SEND_OP, AUTH_OP | W2C-3.3-A-005 |
| W2C-RTR-006 | W2C-3.3-B-006 | phase-3-technical-design | 3.3-api-contract-definition | 0.90 | 0.29 | DECLARE, GET_OP, VALIDATE_OP | W2C-3.3-B-006 |
| W2C-RTR-007 | W2C-3.3-C-007 | phase-3-technical-design | 3.3-api-contract-definition | 0.87 | 0.32 | DECLARE, VALIDATE_OP, RETURN | W2C-3.3-C-007 |
| W2C-RTR-008 | W2C-3.3-D-008 | phase-3-technical-design | 3.3-api-contract-definition | 0.85 | 0.35 | DECLARE, FAIL, VALIDATE_OP | W2C-3.3-D-008 |
| W2C-RTR-009 | W2C-3.4-A-009 | phase-3-technical-design | 3.4-security-and-authorization-model | 0.93 | 0.24 | AUTH_OP, VALIDATE_OP, FAIL | W2C-3.4-A-009 |
| W2C-RTR-010 | W2C-3.4-B-010 | phase-3-technical-design | 3.4-security-and-authorization-model | 0.90 | 0.27 | AUTH_OP, BRANCH, VALIDATE_OP | W2C-3.4-B-010 |
| W2C-RTR-011 | W2C-3.4-C-011 | phase-3-technical-design | 3.4-security-and-authorization-model | 0.92 | 0.25 | AUTH_OP, FAIL, VALIDATE_OP | W2C-3.4-C-011 |
| W2C-RTR-012 | W2C-3.4-D-012 | phase-3-technical-design | 3.4-security-and-authorization-model | 0.91 | 0.26 | AUTH_OP, FAIL, BRANCH | W2C-3.4-D-012 |
| W2C-RTR-013 | W2C-4.1-A-013 | phase-4-implementation | 4.1-backend-services | 0.88 | 0.31 | CALL, CREATE_OP, HANDLE_ERROR | W2C-4.1-A-013 |
| W2C-RTR-014 | W2C-4.1-B-014 | phase-4-implementation | 4.1-backend-services | 0.87 | 0.32 | CALL, GET_OP, RETURN | W2C-4.1-B-014 |
| W2C-RTR-015 | W2C-4.1-C-015 | phase-4-implementation | 4.1-backend-services | 0.89 | 0.30 | CALL, VALIDATE_OP, HANDLE_ERROR | W2C-4.1-C-015 |
| W2C-RTR-016 | W2C-4.1-D-016 | phase-4-implementation | 4.1-backend-services | 0.86 | 0.34 | VALIDATE_OP, FAIL, LOG_OP | W2C-4.1-D-016 |
| W2C-RTR-017 | W2C-4.3-A-017 | phase-4-implementation | 4.3-integration-and-provider-connectors | 0.88 | 0.31 | SEND_OP, AUTH_OP, HANDLE_ERROR | W2C-4.3-A-017 |
| W2C-RTR-018 | W2C-4.3-B-018 | phase-4-implementation | 4.3-integration-and-provider-connectors | 0.91 | 0.27 | SEND_OP, AUTH_OP, BRANCH | W2C-4.3-B-018 |
| W2C-RTR-019 | W2C-4.3-C-019 | phase-4-implementation | 4.3-integration-and-provider-connectors | 0.89 | 0.30 | AUTH_OP, VALIDATE_OP, RETURN | W2C-4.3-C-019 |
| W2C-RTR-020 | W2C-4.3-D-020 | phase-4-implementation | 4.3-integration-and-provider-connectors | 0.87 | 0.33 | SEND_OP, FAIL, AUTH_OP | W2C-4.3-D-020 |

---

## Full Pair Shape

For each row in the pair index, the runtime pair expands to this shape:

```text
A:
  SubPhaseRouterSourceDocument
    DocumentId: <A.DocumentId>
    SourcePath: from source ledger
    SourceTier: from source ledger
    LicenseOrAuthority: from source ledger
    DocumentType: from source ledger
    DocumentTitle: from source ledger document_type
    CompiledText: three_verifiable_claims + known_failure_mode from source ledger
    CompiledPredicateIds: B.PredicateIds
    SourceStatus: ready
    KnownFailureMode: from source ledger

B:
  LogicalUnitResult<SubPhaseRoutingDecision>
    UnitId: unit-sub-phase-router-v1
    UnitRole: sub-phase-router
    DomainTag: whole_development_area
    Output:
      DocumentId: <A.DocumentId>
      SourceStatus: ready
      PredictedPhaseId: <B.Output.PredictedPhaseId>
      PredictedSubPhaseId: <B.Output.PredictedSubPhaseId>
      Confidence: <B.Confidence>
      NearestWrongSubPhaseScore: <B.Output.NearestWrongSubPhaseScore>
      EvidencePredicates: <B.PredicateIds>
      KnowledgeEdge: none
      FailureClassification: source_classified_by_sub_phase_router
    Confidence: <B.Confidence>
    PredicateIds: <B.PredicateIds>
    DomainTags: [whole_development_area, <B.Output.PredictedPhaseId>, <B.Output.PredictedSubPhaseId>]
    EvidenceIds: <B.EvidenceIds>
    Warnings: []
```

This compact expansion is the actual output contract. It is not a description of
the desired behavior.

---

## Reason

Each pair is correct because the source ledger row contains a ready source
packet whose predicates and failure mode are specific to the target sub-phase.
The nearest wrong-domain score remains below the 0.50 guard, and the confidence
score remains above the 0.70 promotion floor.

---

## Phrasing B Companions

Each companion below changes the surface text while preserving the same A/B
output row from the index.

| pair_id | phrasing_b_compiled_text |
|---|---|
| W2C-RTR-001 | Completed phase outputs must become downstream architecture context; accepted decisions and transition constraints stay attached to the component topology. |
| W2C-RTR-002 | The phase registry defines lifecycle structure, source ownership, and transition rules, so this is topology contract material rather than implementation storage. |
| W2C-RTR-003 | A nested orchestrator trace shows generated parent-child topology and recursive flow ownership, which belongs to architecture and component topology. |
| W2C-RTR-004 | Corpus promotion depends on architectural proof gates, and a failed WDA gate blocks broad topology learning. |
| W2C-RTR-005 | The typed logical unit interface defines request, response, result envelope, and cancellation boundary for downstream implementers. |
| W2C-RTR-006 | Runtime contract records show flow id, unit id, confidence, evidence ids, and flags as API-level guarantees. |
| W2C-RTR-007 | The router output contract names predicted phase, predicted sub-phase, confidence, wrong-domain score, and knowledge-edge result. |
| W2C-RTR-008 | A valid training pair must show the exact input object and exact logical result object, not prose about the desired output. |
| W2C-RTR-009 | Authorization server PKCE enforcement is a security design issue; compatibility cannot override the current public-client requirement. |
| W2C-RTR-010 | Two concern-level security gaps stay concerns, and the security model must not invent a stronger MUST-level rejection. |
| W2C-RTR-011 | Token signing and token expiry combine into a compound security risk when forged access tokens have no resource-server time limit. |
| W2C-RTR-012 | A current PKCE MUST failure dominates an IP-only throttling concern, and a future release cannot satisfy present compliance. |
| W2C-RTR-013 | Backend implementation must add one logical unit presence at a time and keep existing tests as the acceptance oracle. |
| W2C-RTR-014 | The assistant runtime selects registered flows, returns a result envelope, and should not hide topology mutation in inline branches. |
| W2C-RTR-015 | Flow execution code calls each node unit, returns operation failures through the envelope, and publishes node metrics. |
| W2C-RTR-016 | A backend implementation round is accepted only after focused checks, full-suite checks, and traceable source-anchor verification. |
| W2C-RTR-017 | Provider connector implementation wraps existing classes as swappable adapters before replacing internal behavior. |
| W2C-RTR-018 | Arbiter selection is connector routing: pair anchors choose the authority families that are allowed to review the result. |
| W2C-RTR-019 | Multi-authority arbiter evidence must keep each selected provider authority visible rather than compressing it to one reason. |
| W2C-RTR-020 | Connector gates must remain internal-first, preserve source anchors, and reject external fallback before local proof. |

The Phrasing B output for every row is the same `LogicalUnitResult` shown in
the pair index.
