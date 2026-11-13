# Layer 3 — Teaching Unit Pairs (021)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-021-001 — Sprint + QA: "review sprint deliverables"

A:
  Question: "'Review the sprint deliverables and approve the features.' Which predicate dominates?"

B:
  Answer: "QA fires on 'review' and 'approve'. IMPLEMENT fires on 'features'. Two predicates
    fire. QA-last structural constraint applies: IMPLEMENT flow (4.1) runs first, then
    QA flow (6.1). 'Sprint' is gap warning. DomainId: software-development."

---

## B36-L3-021-002 — Velocity + QA: "review velocity" always fires QA

A:
  Question: "Is 'velocity review' a QA review? Does 'review' fire QA in that context?"

B:
  Answer: "Yes. 'review' fires QA always — even 'velocity review'. The compiler is literal.
    'velocity' is gap. 'review' drives the QA routing. This may feel wrong (velocity review
    is a planning ceremony) but the lexicon is literal. Gap warning for velocity-001 is emitted.
    The model must not suppress QA to preserve the planning ceremony intent."

---

## B36-L3-021-003 — Milestone + QA + DOCUMENT: QA-last ordering

A:
  Question: "'Review milestone criteria and approve Q3 deliverables.' QA fires. What order do flows execute?"

B:
  Answer: "DOCUMENT fires on 'criteria'. QA fires on 'review' and 'approve'. QA-last rule:
    documentation flow (2.4) runs first, then QA flow (6.1). 'Milestone' is gap warning.
    DomainId: software-development. Two routing decisions produced."

---

## B36-L3-021-004 — The QA-last structural constraint: general rule

A:
  Question: "State the QA-last structural constraint for B36 mixed-signal cases."

B:
  Answer: "When QA predicate co-occurs with any other predicate (IMPLEMENT, DOCUMENT, TEST,
    ARCHITECTURE), the QA flow (6.x sub-phase) always executes last. All other predicate
    flows execute first or in parallel with each other. QA flow is the final gate.
    This applies regardless of the planning vocabulary present in the sentence.
    One predicate or three predicates — QA is always last when present."
