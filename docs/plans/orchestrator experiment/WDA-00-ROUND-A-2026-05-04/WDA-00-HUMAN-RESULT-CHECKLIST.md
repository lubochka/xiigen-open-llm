# WDA-00 Human Result Checklist
# Lifecycle Topology Bootstrap
# Round A - Human-readable output standard
# Date: 2026-05-04

---

## Purpose

The machine evidence can pass while the human result is still unusable. This
checklist defines what the WDA-00 human-readable result must show.

---

## Required Sections

### Section 1 - Topology Summary

Must say that the whole development lifecycle topology was bootstrapped with:

- 7 phases
- 31 sub-phases
- 6 transitions

Fail if the result says 27 sub-phases.

---

### Section 2 - Phase Map

Must list all seven phases in order:

1. UI/UX Definition
2. Functional Specification
3. Technical Design
4. Implementation
5. Testing
6. QA
7. QA Automation

Fail if any phase is missing, renamed into a different lifecycle meaning, or
merged without explicitly saying this was not part of WDA-00.

---

### Section 3 - Sub-Phase Inventory

Must show every sub-phase under the correct parent phase.

Minimum visible requirement:

- Phase 1 has five sub-phases.
- Phase 2 has four sub-phases.
- Phase 3 has five sub-phases.
- Phase 4 has five sub-phases.
- Phase 5 has four sub-phases.
- Phase 6 has four sub-phases.
- Phase 7 has four sub-phases.

Fail if the result only states the total count without showing parent grouping.

---

### Section 4 - Transition Map

Must list all six adjacent transitions:

1. UI/UX Definition -> Functional Specification
2. Functional Specification -> Technical Design
3. Technical Design -> Implementation
4. Implementation -> Testing
5. Testing -> QA
6. QA -> QA Automation

Fail if a transition skips a phase unless the result labels it as out of scope.

---

### Section 5 - Structural Correction

Must explicitly state:

```text
The canonical count is 31 sub-phases. The earlier 27-sub-phase number is not
used by this experiment.
```

Equivalent wording is acceptable if it preserves the same meaning.

Fail if the correction is omitted.

---

### Section 6 - Out-Of-Scope Items

Must state that WDA-00 does not prove:

- natural-language topology mutation
- document classification
- transition carry-forward
- learned unit replacement
- full lifecycle execution

Fail if the result implies WDA-00 proves any of those.

---

### Section 7 - Next Action

Must name WDA-01 document classification as the next experiment.

Fail if the result ends without a next action item.

---

## Forbidden Human Result Behaviors

- Do not say the lifecycle is "fully trained."
- Do not say corpus expansion may start.
- Do not say natural-language mutation works.
- Do not hide the 31-sub-phase correction.
- Do not collapse QA and QA Automation into one phase.
- Do not omit phase transitions.

---

## Acceptance Rule

The human result passes only if all seven required sections are present and no
forbidden behavior appears. If machine evidence passes but this checklist
fails, WDA-00 Round B fails.
