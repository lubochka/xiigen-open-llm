# Layer 3 — Teaching-Unit Pairs (011)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-011-001 — Teaching: false-positive taxonomy in B37

**Five categories of B37 false positives (tokens that fire predicates but managing intent was primary):**

**Category 1: stakeholder → DOCUMENT**
Token: "stakeholder(s)" — appears in managing notification sentences
Predicate fired (incorrectly in managing context): DOCUMENT
Gap warning: gap-project-managing-stakeholder-update-001
Frequency: Very common (stakeholder appears in most managing communication)

**Category 2: structure → ARCHITECTURE**
Token: "structure" — appears in governance, RACI, escalation structure sentences
Predicate fired: ARCHITECTURE
Gap warning: managing vocabulary (accountability, escalation, governance)
Frequency: Common (governance language often uses "structure")

**Category 3: review → QA**
Token: "review" — appears in managing review sessions, RAID log reviews
Predicate fired: QA
Gap warning: escalation, RAID, action items
Frequency: Moderate (review is common managing activity)

**Category 4: test → TEST**
Token: "test" — appears in "test the estimates", "test assumptions"
Predicate fired: TEST
Gap warning: managing estimation context
Frequency: Moderate (project managers use "test" in non-technical sense)

**Category 5: build/service/module → IMPLEMENT**
Token: "build/service/module" — appears when PM describes software being delivered
Predicate fired: IMPLEMENT
Gap warning: managing delivery context
Frequency: Moderate (PM sentences often reference software artifacts)

---

## B37-L3-011-002 — Teaching: false positive vs true positive

**True positive (correct predicate firing in correct context):**
"Gather stakeholder requirements for the spec." → DOCUMENT fires correctly (documentation intent).
"Review the code quality." → QA fires correctly (software review intent).
"Test the authentication service." → TEST fires correctly (software testing intent).
"Build the payment module." → IMPLEMENT fires correctly (software build intent).

**False positive (predicate fires but managing intent was primary):**
"Notify stakeholders of blockers." → DOCUMENT fires (notification intent ≠ documentation).
"Review the RAID log." → QA fires (managing review ≠ software QA).
"Test the timeline assumptions." → TEST fires (evaluation intent ≠ software testing).
"Manage delivery of the service module." → IMPLEMENT fires (managing intent ≠ software build).

---

## B37-L3-011-003 — Teaching: false positives are expected and documented

**False positives in B37 are documented, expected, and handled:**

1. They fire (correct lexical behaviour)
2. They route (correct routing per predicate)
3. They generate gap warnings (correct gap capture)
4. They do NOT need suppression (suppression = v6 error)
5. The gap warning captures the false positive for learning

**The training set B37-batch-001 teaches the model to:**
- Recognize B37 false positive patterns
- Fire predicates correctly despite managing context
- Emit gap warnings for the actual managing intent
- NOT attempt to suppress false positives

---

## B37-L3-011-004 — Teaching: false positive frequency by predicate

| Predicate | False positive rate in B37 | Primary trigger |
|----------|--------------------------|----------------|
| DOCUMENT | High (stakeholder appears frequently) | stakeholder |
| ARCHITECTURE | High (structure in governance) | structure |
| QA | Moderate (review in managing) | review |
| TEST | Moderate (test in estimation) | test |
| IMPLEMENT | Moderate (service/build in delivery) | service, build, module |
| API_CONTRACT | Low (requires explicit API vocabulary) | dependencies + api |

---

## B37-L3-011-005 — Teaching: the gap warning system resolves false positives

**The gap warning system is the B37 solution to false positives.**

Without gap warnings, false positives would:
- Silently route to wrong flows
- Leave managing intent completely uncaptured
- Generate no learning signal for capability improvement

With gap warnings:
- False positive is logged
- Managing intent is captured as a gap ID
- Learning signal is generated for capability development
- The system accumulates evidence for future predicate registration

**Gap warnings are the bridge between current capability (predicate-based routing) and future capability (managing-aware routing).**
