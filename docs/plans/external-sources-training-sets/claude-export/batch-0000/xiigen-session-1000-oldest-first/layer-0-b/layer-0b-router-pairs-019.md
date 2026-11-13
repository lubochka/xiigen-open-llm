# Layer 0-B - Router Pairs (019)
# Batch: XIIGen session export 1000 oldest-first | pair_kind: multi-sub-phase-routing

---

## XIIGEN-SESSION-1000-0901 - router target

```
source_id: xiigen-session-1000-0901
source_title: "Improving Figma node building in engine"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: qa_release
domain: CODE_Q
phase: 1
sub_phase: 1.3
task_type: interaction-navigation
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0901-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0901-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0901-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0901-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.4"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST", "DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0902 - router target

```
source_id: xiigen-session-1000-0902
source_title: "Fixing duplicate text nodes and missing image exports"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0902-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"
  - "SECURITY"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0902-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0902-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0902-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0902-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0902-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0902-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0902-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0902-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0903 - router target

```
source_id: xiigen-session-1000-0903
source_title: "Fixing duplicate text nodes and missing image exports"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0903-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "DOCUMENT"
  - "IMPLEMENT"
  - "LEARNING"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0903-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0903-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0903-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0903-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0903-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0903-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.5"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT", "API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0903-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0903-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0904 - router target

```
source_id: xiigen-session-1000-0904
source_title: "Component sidebar z-index layering issues"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0904-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0904-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0904-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0905 - router target

```
source_id: xiigen-session-1000-0905
source_title: "Empty frames in image generation pipeline"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0905-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0905-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0905-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0905-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0906 - router target

```
source_id: xiigen-session-1000-0906
source_title: "Improving CSS for responsive group styling"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0906-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "DOCUMENT"
  - "IMPLEMENT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0906-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0906-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0906-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0906-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0907 - router target

```
source_id: xiigen-session-1000-0907
source_title: "Master plan phase 5 continuation"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 7
sub_phase: 7.5
task_type: learning-feedback
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0907-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "LEARNING"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0907-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["LEARNING"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0907-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0908 - router target

```
source_id: xiigen-session-1000-0908
source_title: "Master plan phase 5 continuation"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 7
sub_phase: 7.5
task_type: learning-feedback
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0908-prompt"
A_compiled_predicate_ids:
  - "LEARNING"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0908-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["LEARNING"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0909 - router target

```
source_id: xiigen-session-1000-0909
source_title: "Master plan phase 5 continuation"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 7
sub_phase: 7.5
task_type: learning-feedback
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0909-prompt"
A_compiled_predicate_ids:
  - "LEARNING"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0909-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["LEARNING"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0910 - router target

```
source_id: xiigen-session-1000-0910
source_title: "Master plan phase 5 continuation"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 7
sub_phase: 7.5
task_type: learning-feedback
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0910-prompt"
A_compiled_predicate_ids:
  - "LEARNING"
  - "SECURITY"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0910-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["LEARNING"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0910-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0911 - router target

```
source_id: xiigen-session-1000-0911
source_title: "Master plan phase 5 continuation"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 7
sub_phase: 7.5
task_type: learning-feedback
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0911-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "LEARNING"
  - "SECURITY"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0911-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["LEARNING"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0911-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0911-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0911-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0912 - router target

```
source_id: xiigen-session-1000-0912
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 1
sub_phase: 1.3
task_type: interaction-navigation
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0912-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "SECURITY"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0912-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0913 - router target

```
source_id: xiigen-session-1000-0913
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 1
sub_phase: 1.3
task_type: interaction-navigation
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0913-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0913-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0914 - router target

```
source_id: xiigen-session-1000-0914
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0914-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0914-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0915 - router target

```
source_id: xiigen-session-1000-0915
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0915-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0915-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0916 - router target

```
source_id: xiigen-session-1000-0916
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.2
task_type: data-persistence
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0916-prompt"
A_compiled_predicate_ids:
  - "SECURITY"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0916-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0917 - router target

```
source_id: xiigen-session-1000-0917
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0917-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0917-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0917-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0917-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0917-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0917-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0918 - router target

```
source_id: xiigen-session-1000-0918
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0918-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0918-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0918-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0918-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0919 - router target

```
source_id: xiigen-session-1000-0919
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.2
task_type: frontend-client-integration
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0919-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0919-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0920 - router target

```
source_id: xiigen-session-1000-0920
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0920-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "DOCUMENT"
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0920-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0920-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0920-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0920-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0920-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0921 - router target

```
source_id: xiigen-session-1000-0921
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.2
task_type: frontend-client-integration
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0921-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0921-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0921-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0922 - router target

```
source_id: xiigen-session-1000-0922
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0922-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0922-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0922-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0923 - router target

```
source_id: xiigen-session-1000-0923
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0923-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0923-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0924 - router target

```
source_id: xiigen-session-1000-0924
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0924-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0924-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0924-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0924-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0924-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0925 - router target

```
source_id: xiigen-session-1000-0925
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0925-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0925-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0925-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0925-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0925-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0926 - router target

```
source_id: xiigen-session-1000-0926
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0926-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0926-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0927 - router target

```
source_id: xiigen-session-1000-0927
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0927-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0927-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0927-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0928 - router target

```
source_id: xiigen-session-1000-0928
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0928-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0928-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0928-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0928-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0929 - router target

```
source_id: xiigen-session-1000-0929
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0929-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0929-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0930 - router target

```
source_id: xiigen-session-1000-0930
source_title: "Node matrix design fixes"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 7
sub_phase: 7.5
task_type: learning-feedback
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0930-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"
  - "QA"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0930-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["LEARNING"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0930-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0930-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0930-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0930-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0930-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0930-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0930-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0931 - router target

```
source_id: xiigen-session-1000-0931
source_title: "CSS code generation issues"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0931-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "SECURITY"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0931-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0932 - router target

```
source_id: xiigen-session-1000-0932
source_title: "CSS code generation issues"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0932-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0932-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0933 - router target

```
source_id: xiigen-session-1000-0933
source_title: "CSS code generation issues"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0933-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0933-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0933-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0933-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0933-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0933-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0933-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0933-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0934 - router target

```
source_id: xiigen-session-1000-0934
source_title: "CSS code generation issues"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0934-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0934-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0934-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0934-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0934-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0934-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0934-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0934-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0934-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0935 - router target

```
source_id: xiigen-session-1000-0935
source_title: "Restore function placing cards outside canvas"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 1
sub_phase: 1.3
task_type: interaction-navigation
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0935-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0935-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0935-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0936 - router target

```
source_id: xiigen-session-1000-0936
source_title: "Restore function placing cards outside canvas"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.2
task_type: frontend-client-integration
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0936-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0936-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0936-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0936-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0937 - router target

```
source_id: xiigen-session-1000-0937
source_title: "Improving CSS for responsive group styling"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0937-prompt"
A_compiled_predicate_ids:
  - "DOCUMENT"
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0937-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0937-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0937-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0937-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0937-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0937-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0937-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0938 - router target

```
source_id: xiigen-session-1000-0938
source_title: "Improving CSS for responsive group styling"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0938-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0938-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0938-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0938-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0938-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0938-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.4"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST", "DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0939 - router target

```
source_id: xiigen-session-1000-0939
source_title: "Improving CSS for responsive group styling"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 7
sub_phase: 7.5
task_type: learning-feedback
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0939-prompt"
A_compiled_predicate_ids:
  - "LEARNING"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0939-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["LEARNING"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0939-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0940 - router target

```
source_id: xiigen-session-1000-0940
source_title: "Improving CSS for responsive group styling"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: learning_cycle
domain: PLAN
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0940-prompt"
A_compiled_predicate_ids:
  - "LEARNING"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0940-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0941 - router target

```
source_id: xiigen-session-1000-0941
source_title: "Improving CSS for responsive group styling"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0941-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0941-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0941-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0941-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0941-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.4"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0941-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.2"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0941-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0941-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0942 - router target

```
source_id: xiigen-session-1000-0942
source_title: "Improving CSS for responsive group styling"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.3
task_type: infrastructure-deployment
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0942-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0942-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0942-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0942-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0943 - router target

```
source_id: xiigen-session-1000-0943
source_title: "Improving CSS for responsive group styling"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0943-prompt"
A_compiled_predicate_ids:
  - "DOCUMENT"
  - "IMPLEMENT"
  - "SECURITY"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0943-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0943-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0943-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0943-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0943-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.4"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0943-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0943-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0943-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST", "DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0944 - router target

```
source_id: xiigen-session-1000-0944
source_title: "CSS history file breaking style.ts execution"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0944-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0944-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0945 - router target

```
source_id: xiigen-session-1000-0945
source_title: "CSS history file breaking style.ts execution"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 5
sub_phase: 5.1
task_type: unit-tests
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0945-prompt"
A_compiled_predicate_ids:
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0945-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0945-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0946 - router target

```
source_id: xiigen-session-1000-0946
source_title: "CSS history file breaking style.ts execution"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0946-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "SECURITY"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0946-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0946-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0947 - router target

```
source_id: xiigen-session-1000-0947
source_title: "CSS history file breaking style.ts execution"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.2
task_type: frontend-client-integration
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0947-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0947-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0947-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0948 - router target

```
source_id: xiigen-session-1000-0948
source_title: "CSS history file breaking style.ts execution"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.2
task_type: frontend-client-integration
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0948-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0948-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0948-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0949 - router target

```
source_id: xiigen-session-1000-0949
source_title: "CSS history file breaking style.ts execution"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0949-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0949-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0950 - router target

```
source_id: xiigen-session-1000-0950
source_title: "Debugging Figma-generated CSS with full change logs"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: repair
domain: BUG
phase: 7
sub_phase: 7.5
task_type: learning-feedback
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0950-prompt"
A_compiled_predicate_ids:
  - "DOCUMENT"
  - "IMPLEMENT"
  - "LEARNING"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0950-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["LEARNING"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0950-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0950-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0950-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

