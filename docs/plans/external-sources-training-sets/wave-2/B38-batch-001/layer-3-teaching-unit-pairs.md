# Layer 3 — Teaching Unit Pairs
# Batch: B38-batch-001
# Scenario: B38 — Project delivery — release gates and go-live checklists
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B38B1-WDA-teach-001 — "service" fires IMPLEMENT everywhere

```
source_id: xiigen-b38-teach-001
pair_kind: teaching_unit
```

A: "Review the release checklist and approve the go-live for the payment service upgrade."
   Does IMPLEMENT fire? Should it?

B:
  Rule: "IMPLEMENT fires on 'service'. 'payment service upgrade' contains 'service'
  which is in the IMPLEMENT lexicon. The compiler fires on term presence regardless
  of context. Both QA (review, approve) and IMPLEMENT (service) fire. The router
  produces two routing decisions: 7.1 for QA and 4.1 for IMPLEMENT."

---

## Pair B38B1-WDA-teach-002 — "criteria" fires DOCUMENT (even as release criteria)

```
source_id: xiigen-b38-teach-002
pair_kind: teaching_unit
```

A: "Validate all release criteria." Does DOCUMENT fire on 'criteria'?

B:
  Rule: "Yes. 'criteria' is in the DOCUMENT lexicon (acceptance criteria, release
  criteria, exit criteria — the term 'criteria' fires DOCUMENT regardless of the
  surrounding modifier). A release criteria review request fires both QA (validate)
  and DOCUMENT (criteria). This is a false positive in intent terms but correct in
  lexicon terms. The model must not suppress DOCUMENT."

---

## Pair B38B1-WDA-teach-003 — "deployment" alone is gap vocabulary

```
source_id: xiigen-b38-teach-003
pair_kind: teaching_unit
```

A: "Monitor the deployment." Does IMPLEMENT fire on 'deployment'?

B:
  Rule: "No. 'deployment' in an operational monitoring context does not fire
  IMPLEMENT. IMPLEMENT fires on: implement, code, build, develop, write, create,
  function, class, logic, module, service, feature. 'Deployment' is an operational
  term, not a code-authoring term. 'Deploy a service' might fire on 'service' but
  not on 'deploy' or 'deployment' alone."

---

## Pair B38B1-WDA-teach-004 — go-live is gap vocabulary; QA covers the gate context

```
source_id: xiigen-b38-teach-004
pair_kind: teaching_unit
```

A: "The user wants to manage the go-live. How does the orchestrator handle 'go-live'?"

B:
  Rule: "'go-live' has no predicate. When the user writes 'approve the go-live',
  QA fires on 'approve' — and that is the relevant predicate. The go-live intent
  is partially captured via the QA approval routing. 'go-live' itself is a gap term
  that produces a CapabilityGapWarning but does not prevent routing when QA or other
  predicates also fire. If the request contains ONLY 'go-live' vocabulary with no
  QA/IMPLEMENT/other predicate terms, the result is knowledge_edge."

---

## Pair B38B1-WDA-teach-005 — seven-sub-phase overview for Phase 7 (Automation)

```
source_id: xiigen-b38-teach-005
pair_kind: teaching_unit
```

A: "What are the sub-phases of Phase 7 and which predicates route to each?"

B:
  Phase7SubPhases:
    - SubPhase: "7.1"
      Name: "Release and Deployment Automation"
      PrimaryPredicate: "QA"
      Scenarios: "Release gate reviews, go-live approvals, deployment readiness checks"

    - SubPhase: "7.2"
      Name: "CI/CD Pipeline Automation"
      PrimaryPredicate: "QA + IMPLEMENT"
      Scenarios: "Pipeline construction, build automation, artifact management"

    - SubPhase: "7.3"
      Name: "QA Test Automation"
      PrimaryPredicate: "TEST + QA"
      Scenarios: "Automated test suite authoring, test framework configuration, coverage targets"

    - SubPhase: "7.4"
      Name: "Infrastructure Automation"
      PrimaryPredicate: "ARCHITECTURE + IMPLEMENT"
      Scenarios: "IaC (Infrastructure as Code), provisioning automation, environment management"
