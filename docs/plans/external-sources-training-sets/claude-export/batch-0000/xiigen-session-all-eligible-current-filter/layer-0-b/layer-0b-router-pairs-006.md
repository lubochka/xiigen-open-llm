# Layer 0-B - Router Pairs (006)
# Batch: XIIGen session export 1000 oldest-first | pair_kind: multi-sub-phase-routing

---

## XIIGEN-SESSION-1000-0251 - router target

```
source_id: xiigen-session-1000-0251
source_title: "Azure OCR definition and setup"
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

A_document_id: "XIIGEN-SESSION-1000-0251-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0251-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0252 - router target

```
source_id: xiigen-session-1000-0252
source_title: "Validating task report JSON data"
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

A_document_id: "XIIGEN-SESSION-1000-0252-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "ARCHITECTURE"
  - "IMPLEMENT"
  - "QA"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0252-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0252-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0252-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0253 - router target

```
source_id: xiigen-session-1000-0253
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: qa_release
domain: CODE_Q
phase: 6
sub_phase: 6.1
task_type: manual-qa
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0253-prompt"
A_compiled_predicate_ids:
  - "QA"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0253-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0254 - router target

```
source_id: xiigen-session-1000-0254
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0254-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0254-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0255 - router target

```
source_id: xiigen-session-1000-0255
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: qa_release
domain: CODE_Q
phase: 6
sub_phase: 6.1
task_type: manual-qa
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0255-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "QA"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0255-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0256 - router target

```
source_id: xiigen-session-1000-0256
source_title: "Full-stack development requirements in Hebrew"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: qa_release
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0256-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "QA"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0256-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0256-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0256-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0256-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0256-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.2"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0256-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0257 - router target

```
source_id: xiigen-session-1000-0257
source_title: "Kling video generation from Google Drive images"
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

A_document_id: "XIIGEN-SESSION-1000-0257-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0257-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0257-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0258 - router target

```
source_id: xiigen-session-1000-0258
source_title: "Validating task report JSON data"
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

A_document_id: "XIIGEN-SESSION-1000-0258-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0258-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0258-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.4"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST", "DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0259 - router target

```
source_id: xiigen-session-1000-0259
source_title: "Validating task report JSON data"
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

A_document_id: "XIIGEN-SESSION-1000-0259-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "QA"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0259-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0259-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0259-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0260 - router target

```
source_id: xiigen-session-1000-0260
source_title: "Facebook data pipeline to Azure AI Search"
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

A_document_id: "XIIGEN-SESSION-1000-0260-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0260-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0260-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0260-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0261 - router target

```
source_id: xiigen-session-1000-0261
source_title: "Validating task report JSON data"
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

A_document_id: "XIIGEN-SESSION-1000-0261-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "SECURITY"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0261-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0261-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0261-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0262 - router target

```
source_id: xiigen-session-1000-0262
source_title: "Validating task report JSON data"
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

A_document_id: "XIIGEN-SESSION-1000-0262-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"
  - "QA"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0262-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0262-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0262-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0262-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.2"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0262-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0262-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0263 - router target

```
source_id: xiigen-session-1000-0263
source_title: "AWS messaging alternatives to SQS for Nest.js"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0263-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0263-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0263-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0263-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0264 - router target

```
source_id: xiigen-session-1000-0264
source_title: "AWS messaging alternatives to SQS for Nest.js"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0264-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0264-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0264-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.4"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST", "DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0265 - router target

```
source_id: xiigen-session-1000-0265
source_title: "Validating task report JSON data"
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

A_document_id: "XIIGEN-SESSION-1000-0265-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0265-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0265-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0266 - router target

```
source_id: xiigen-session-1000-0266
source_title: "Chipset and input field side-by-side layout"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: PLAN
phase: 2
sub_phase: 2.1
task_type: business-capability
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0266-prompt"
A_compiled_predicate_ids:
  - "DOCUMENT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0266-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0267 - router target

```
source_id: xiigen-session-1000-0267
source_title: "Connecting Claude desktop with Visual Code and Figma"
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

A_document_id: "XIIGEN-SESSION-1000-0267-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0267-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0267-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0268 - router target

```
source_id: xiigen-session-1000-0268
source_title: "Connecting Claude desktop with Visual Code and Figma"
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

A_document_id: "XIIGEN-SESSION-1000-0268-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0268-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0268-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0269 - router target

```
source_id: xiigen-session-1000-0269
source_title: "Checking the filesystem"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0269-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0269-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0270 - router target

```
source_id: xiigen-session-1000-0270
source_title: "Checking the filesystem"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0270-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0270-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0270-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0271 - router target

```
source_id: xiigen-session-1000-0271
source_title: "Access to external projects and websites"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0271-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0271-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0271-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0271-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0272 - router target

```
source_id: xiigen-session-1000-0272
source_title: "Access to external projects and websites"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0272-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "IMPLEMENT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0272-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0272-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0272-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0272-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0272-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0273 - router target

```
source_id: xiigen-session-1000-0273
source_title: "JSON config file not found in Azure DevOps pipeline"
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

A_document_id: "XIIGEN-SESSION-1000-0273-prompt"
A_compiled_predicate_ids:
  - "DOCUMENT"
  - "IMPLEMENT"
  - "QA"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0273-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0273-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0273-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0273-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0274 - router target

```
source_id: xiigen-session-1000-0274
source_title: "Data validation and HTML report generation"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 5
sub_phase: 5.1
task_type: unit-tests
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0274-prompt"
A_compiled_predicate_ids:
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0274-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0275 - router target

```
source_id: xiigen-session-1000-0275
source_title: "Data validation and HTML report generation"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: qa_release
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0275-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "QA"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0275-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0275-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0275-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0276 - router target

```
source_id: xiigen-session-1000-0276
source_title: "Data validation and HTML report generation"
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

A_document_id: "XIIGEN-SESSION-1000-0276-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"
  - "SECURITY"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0276-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0276-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0276-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0276-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0276-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0276-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0277 - router target

```
source_id: xiigen-session-1000-0277
source_title: "Project files access request"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0277-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0277-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0277-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0277-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0277-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0278 - router target

```
source_id: xiigen-session-1000-0278
source_title: "Project files access request"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0278-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0278-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0278-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0278-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0278-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0279 - router target

```
source_id: xiigen-session-1000-0279
source_title: "Project files access request"
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

A_document_id: "XIIGEN-SESSION-1000-0279-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0279-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0279-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0279-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0280 - router target

```
source_id: xiigen-session-1000-0280
source_title: "Project files access request"
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

A_document_id: "XIIGEN-SESSION-1000-0280-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0280-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0280-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0281 - router target

```
source_id: xiigen-session-1000-0281
source_title: "Project files access request"
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

A_document_id: "XIIGEN-SESSION-1000-0281-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0281-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0282 - router target

```
source_id: xiigen-session-1000-0282
source_title: "Project files access request"
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

A_document_id: "XIIGEN-SESSION-1000-0282-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0282-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0282-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0282-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0283 - router target

```
source_id: xiigen-session-1000-0283
source_title: "Missing npm script build:dev error"
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

A_document_id: "XIIGEN-SESSION-1000-0283-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0283-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0283-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0283-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0283-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0284 - router target

```
source_id: xiigen-session-1000-0284
source_title: "Missing npm script build:dev error"
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

A_document_id: "XIIGEN-SESSION-1000-0284-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0284-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0284-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0284-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0284-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0284-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0285 - router target

```
source_id: xiigen-session-1000-0285
source_title: "Missing npm script build:dev error"
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

A_document_id: "XIIGEN-SESSION-1000-0285-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0285-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0286 - router target

```
source_id: xiigen-session-1000-0286
source_title: "S3 404 error with Angular routing on direct path access"
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

A_document_id: "XIIGEN-SESSION-1000-0286-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "DOCUMENT"
  - "IMPLEMENT"
  - "SECURITY"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0286-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0286-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0286-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0286-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0286-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0286-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0287 - router target

```
source_id: xiigen-session-1000-0287
source_title: "Debugging API connectivity with command line tools"
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

A_document_id: "XIIGEN-SESSION-1000-0287-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "DOCUMENT"
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0287-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0287-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0287-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0287-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0288 - router target

```
source_id: xiigen-session-1000-0288
source_title: "Debugging API connectivity with command line tools"
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

A_document_id: "XIIGEN-SESSION-1000-0288-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0288-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0288-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0288-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0288-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0289 - router target

```
source_id: xiigen-session-1000-0289
source_title: "Debugging API connectivity with command line tools"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: qa_release
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0289-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0289-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0289-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0289-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.4"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST", "DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0290 - router target

```
source_id: xiigen-session-1000-0290
source_title: "Debugging API connectivity with command line tools"
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

A_document_id: "XIIGEN-SESSION-1000-0290-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "DOCUMENT"
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0290-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0290-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0290-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0290-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0291 - router target

```
source_id: xiigen-session-1000-0291
source_title: "Debugging API connectivity with command line tools"
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

A_document_id: "XIIGEN-SESSION-1000-0291-prompt"
A_compiled_predicate_ids:
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0291-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0292 - router target

```
source_id: xiigen-session-1000-0292
source_title: "Resolving NestJS dependency version conflicts"
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

A_document_id: "XIIGEN-SESSION-1000-0292-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "DOCUMENT"
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0292-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0292-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0292-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0292-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0292-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0292-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0292-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0292-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.3"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST", "QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0293 - router target

```
source_id: xiigen-session-1000-0293
source_title: "Resolving NestJS dependency version conflicts"
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

A_document_id: "XIIGEN-SESSION-1000-0293-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0293-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0293-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0293-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0293-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.1"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0293-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0293-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["TEST", "DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0293-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0294 - router target

```
source_id: xiigen-session-1000-0294
source_title: "Windows Server 2019 image retirement error"
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

A_document_id: "XIIGEN-SESSION-1000-0294-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "IMPLEMENT"
  - "TEST"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0294-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0294-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0294-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0294-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.5"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT", "API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0294-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0295 - router target

```
source_id: xiigen-session-1000-0295
source_title: "Fourth implementation prompt review"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0295-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0295-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0295-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0295-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0296 - router target

```
source_id: xiigen-session-1000-0296
source_title: "Fourth implementation prompt review"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0296-prompt"
A_compiled_predicate_ids:
  - "ARCHITECTURE"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0296-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0296-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0296-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0297 - router target

```
source_id: xiigen-session-1000-0297
source_title: "Fourth implementation prompt review"
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

A_document_id: "XIIGEN-SESSION-1000-0297-prompt"
A_compiled_predicate_ids:
  - "API_CONTRACT"
  - "ARCHITECTURE"
  - "DOCUMENT"
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0297-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0297-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0297-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.5"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0297-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0297-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0297-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0297-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "6"
      PredictedSubPhaseId: "6.4"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0297-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.72
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0298 - router target

```
source_id: xiigen-session-1000-0298
source_title: "Design patterns для начинающих"
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

A_document_id: "XIIGEN-SESSION-1000-0298-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0298-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0298-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0299 - router target

```
source_id: xiigen-session-1000-0299
source_title: "Изучение Next.js с нуля"
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

A_document_id: "XIIGEN-SESSION-1000-0299-prompt"
A_compiled_predicate_ids:
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0299-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

## XIIGEN-SESSION-1000-0300 - router target

```
source_id: xiigen-session-1000-0300
source_title: "Code documentation validation and UI specification"
source_license_state: first_party_private
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: external_source_integration
domain: PLAN
phase: 4
sub_phase: 4.2
task_type: frontend-client-integration
coverage_status: covered
pair_kind: multi_sub_phase_router
```

A_document_id: "XIIGEN-SESSION-1000-0300-prompt"
A_compiled_predicate_ids:
  - "DOCUMENT"
  - "IMPLEMENT"
  - "TEST"
  - "UI"

B:
  routing_decisions:
    - DocumentId: "XIIGEN-SESSION-1000-0300-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.2"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["UI", "IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0300-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.88
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0300-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.3"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0300-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "7"
      PredictedSubPhaseId: "7.2"
      Confidence: 0.80
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["AUTOMATION"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"
    - DocumentId: "XIIGEN-SESSION-1000-0300-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.76
      NearestWrongSubPhaseScore: 0.00
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: "source_classified_by_multi_sub_phase_router"

---

