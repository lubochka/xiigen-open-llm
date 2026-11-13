# WDA-00 Source Packet
# Lifecycle Topology Bootstrap
# Round A - Oracle source
# Date: 2026-05-04

---

## Human Request

Represent the whole development lifecycle as mutable logical topology data so
the orchestrator can later route documents, phase runs, sub-phase runs, and
transition checks through the same dynamic flow surface.

---

## Scope

WDA-00 tests topology representation only.

In scope:

- Add or update seven phases.
- Add or update thirty-one sub-phases.
- Add or update six adjacent phase transitions.
- Verify stable identifiers.
- Verify edit semantics preserve identity.
- Produce a human-readable lifecycle map.

Out of scope:

- Natural-language topology mutation.
- Document classification.
- Sub-phase orchestration.
- Phase transition validation.
- Learning from transition failures.
- External AI calls.

---

## Source Facts

### Phase facts

F01. The lifecycle contains Phase 1: UI/UX Definition.
F02. The lifecycle contains Phase 2: Functional Specification.
F03. The lifecycle contains Phase 3: Technical Design.
F04. The lifecycle contains Phase 4: Implementation.
F05. The lifecycle contains Phase 5: Testing.
F06. The lifecycle contains Phase 6: QA.
F07. The lifecycle contains Phase 7: QA Automation.

### Sub-phase facts

F08. Phase 1 contains 1.1 Design system and tokens.
F09. Phase 1 contains 1.2 Screen and layout structure.
F10. Phase 1 contains 1.3 Interaction model and state transitions.
F11. Phase 1 contains 1.4 Accessibility and compliance.
F12. Phase 1 contains 1.5 Motion and animation.

F13. Phase 2 contains 2.1 User stories and acceptance criteria.
F14. Phase 2 contains 2.2 Business rules.
F15. Phase 2 contains 2.3 Integration point specification.
F16. Phase 2 contains 2.4 Data model boundaries.

F17. Phase 3 contains 3.1 Architecture and component topology.
F18. Phase 3 contains 3.2 Data schema and migration design.
F19. Phase 3 contains 3.3 API contract definition.
F20. Phase 3 contains 3.4 Security and authorization model.
F21. Phase 3 contains 3.5 Infrastructure and deployment topology.

F22. Phase 4 contains 4.1 Backend services.
F23. Phase 4 contains 4.2 Frontend and client rendering.
F24. Phase 4 contains 4.3 Integration and provider connectors.
F25. Phase 4 contains 4.4 Data migration execution.
F26. Phase 4 contains 4.5 Documentation.

F27. Phase 5 contains 5.1 Unit and contract tests.
F28. Phase 5 contains 5.2 Integration tests.
F29. Phase 5 contains 5.3 End-to-end scenario tests.
F30. Phase 5 contains 5.4 Performance and load tests.

F31. Phase 6 contains 6.1 Functional review.
F32. Phase 6 contains 6.2 Regression coverage verification.
F33. Phase 6 contains 6.3 Security and compliance review.
F34. Phase 6 contains 6.4 Accessibility audit.

F35. Phase 7 contains 7.1 Test scenario automation.
F36. Phase 7 contains 7.2 Regression suite maintenance.
F37. Phase 7 contains 7.3 CI/CD gate configuration.
F38. Phase 7 contains 7.4 Coverage and reporting.

### Transition facts

F39. Transition T01 connects Phase 1 to Phase 2.
F40. Transition T02 connects Phase 2 to Phase 3.
F41. Transition T03 connects Phase 3 to Phase 4.
F42. Transition T04 connects Phase 4 to Phase 5.
F43. Transition T05 connects Phase 5 to Phase 6.
F44. Transition T06 connects Phase 6 to Phase 7.

---

## Constraints

C01. The canonical whole-development topology contains exactly seven phases.
C02. The canonical whole-development topology contains exactly thirty-one
sub-phases.
C03. The canonical whole-development topology contains exactly six adjacent
phase transitions.
C04. Every sub-phase belongs to exactly one phase.
C05. Every transition source and target must reference an existing phase.
C06. Add or update behavior must preserve the identifier when editing an
existing phase, sub-phase, or transition.
C07. The same identifier must not produce duplicate registry entries.
C08. Topology mutation must use typed commands in this experiment.
C09. No product code change is required to add a company-specific lifecycle
topology once the typed topology mutation surface is available.
C10. Safety flags remain true, even though this experiment does not exercise
runtime safety paths.

---

## Explicit Conflict

X01. One source plan says the corpus covers twenty-seven sub-phases, while the
phase guide and quota table list thirty-one. WDA-00 resolves this conflict by
using thirty-one as canonical.

---

## Explicit Unknowns

U01. Natural-language topology intent classification is not proven in WDA-00.
U02. Real source document availability for every sub-phase is not proven in
WDA-00.
U03. Transition predicate carry-forward is not proven in WDA-00.
U04. Learned unit replacement is not proven in WDA-00.
U05. Full lifecycle execution is not proven in WDA-00.

---

## Forbidden Assumptions

A01. Do not treat twenty-seven sub-phases as complete coverage.
A02. Do not silently omit a sub-phase because a source document is missing.
A03. Do not implement edit semantics as delete plus add unless the public
identifier and registry identity are preserved.
A04. Do not add extra phases beyond the seven-phase lifecycle.
A05. Do not claim natural-language topology mutation passed from typed-command
success.
A06. Do not produce a human result that hides the 31-sub-phase correction.

---

## Expected Top Predicate Profile

The topology bootstrap input should activate:

- `DECLARE`: phases, sub-phases, and transitions are declared.
- `CREATE`: registry entries are created when absent.
- `UPDATE_OP`: registry entries are updated when identifiers already exist.
- `VALIDATE_OP`: counts, identifiers, and edge references are checked.
- `BRANCH`: add-versus-update decisions branch on existing identifiers.
- `RETURN`: final topology summary is returned.

No `AUTH_OP`, `SEND_OP`, `DELETE_OP`, or `NULL_GUARD` predicates should lead
the profile for this experiment.
