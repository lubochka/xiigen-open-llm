# MANIFEST — B34-batch-001
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06
# Total files: 201

---

## File Count by Layer

| Layer | Files |
|-------|-------|
| Layer 0-A (compiler pairs) | 24 (001–024) |
| Layer 0-B (router pairs) | 24 (001–024) |
| Layer 0-C (knowledge edge pairs) | 24 (001–024) |
| Layer 0-D (flow factory pairs) | 24 (001–024) |
| Layer 1 (transition orchestrator pairs) | 24 (001–024) |
| Layer 2 (capability gap extended pairs) | 24 (001–024) |
| Layer 3 (teaching unit pairs) | 24 (001–024) |
| Held-out and corruption pairs | 24 (001–024) |
| Cross-layer synthesis | 8 (001–008) |
| MANIFEST | 1 |
| **Total** | **201** |

---

## Layer 2 Capability Gap Extended Pairs

| File | Request type | Predicate result | Verdict |
|------|-------------|-----------------|---------|
| 001 | Original batch | mixed | — |
| 002 | GapHandler: mentor junior developers | [] gap | GAP_LOGGED |
| 003 | DocumentationFlow: write onboarding guide | [DOCUMENT] | DOCUMENT_APPROVED |
| 004 | ImplementationFlow41: build skill assessment platform | [IMPLEMENT] | IMPLEMENT_APPROVED |
| 005 | GapHandler: train team on Kubernetes | [] gap | GAP_LOGGED |
| 006 | GapHandler: onboard new engineers | [] gap | GAP_LOGGED |
| 007 | E2ETestingFlow53: write E2E tests for training portal | [TEST] | TEST_PASS |
| 008 | APIContractFlow33: integrate LMS with SSO | [API_CONTRACT] | API_CONTRACT_APPROVED |
| 009 | ArchitectureFlow31: architect training platform | [ARCHITECTURE] | ARCHITECTURE_APPROVED |
| 010 | GapHandler: pair programming sessions | [] gap | GAP_LOGGED |
| 011 | UnitTestingFlow51: write unit tests for scoring algorithm | [TEST] | TEST_PASS |
| 012 | QAGatingFlow73: QA onboarding portal | [QA] | QA_PASS |
| 013 | Shadow vocabulary cases: gap and embedded IMPLEMENT/TEST | mixed | GAP_LOGGED / TEST / IMPLEMENT |
| 014 | Pair-with vocabulary: gap and embedded TEST/API_CONTRACT/IMPLEMENT | mixed | GAP_LOGGED / TEST / API_CONTRACT / IMPLEMENT |
| 015 | Walk-through explanation → gap; walk-through integrating → API_CONTRACT | mixed | GAP_LOGGED / API_CONTRACT |
| 016 | Facilitate workshop → gap; facilitate integration → API_CONTRACT | mixed | GAP_LOGGED / API_CONTRACT |
| 017 | Advise → gap; advise+design → ARCHITECTURE; advise+harden → SECURITY | mixed | GAP_LOGGED / ARCHITECTURE / SECURITY |
| 018 | Coach through writing tests → TEST+gap warning | [TEST] + gap warning | TEST_PASS |
| 019 | Guide through integrating → API_CONTRACT+gap warning | [API_CONTRACT] + gap warning | API_CONTRACT_APPROVED |
| 020 | Explain+build → IMPLEMENT fires; explain is gap warning | [IMPLEMENT] + gap warning | IMPLEMENT_APPROVED |
| 021 | Teach to QA → gap; QA abbreviation direct → QA fires | mixed | GAP_LOGGED / QA_PASS |
| 022 | Multiple guidance verbs + embedded code verbs | mixed | GAP_LOGGED / IMPLEMENT / DOCUMENT / API_CONTRACT |
| 023 | Guidance vs instruction disambiguation | mixed | GAP_LOGGED / IMPLEMENT / TEST |
| 024 | Complete B34 capability-gap-extended summary | mixed | all rules |

---

## Key Teaching Points

1. **Guidance vocabulary → gap** — mentor, coach, teach, train (human), guide, explain, onboard, shadow, upskill, advise, facilitate → all gap; DomainId: software-development
2. **"explain" never fires any predicate** — not DOCUMENT, not SECURITY, not ARCHITECTURE; explain = teaching → always gap
3. **Embedded verb rule** — guidance frames do not suppress embedded code verbs: "guide through writing tests" → TEST fires
4. **"teach to X" → gap** — the infinitive is what others will learn; requester's action = teach = gap
5. **"create [noun]" disambiguation** — documentation nouns → DOCUMENT; software component nouns → IMPLEMENT; test nouns → TEST
6. **"design [noun]" disambiguation** — structural nouns → ARCHITECTURE; curriculum/content nouns → gap
7. **"integrate" always fires API_CONTRACT** — even LMS/HR/SSO contexts; all guidance-context integrations fire API_CONTRACT (except "teach to integrate")
8. **QA abbreviation only** — "QA" abbreviation fires; "quality assurance/quality of training/ensure quality" → gap
9. **Three gap IDs** — gap-skills-guidance-mentor-001 (priority); gap-skills-guidance-onboard-001; gap-skills-guidance-train-001
10. **DomainId: software-development** — no education, training, HR, or learning-management domain
11. **TransitionType unchanged** — same hierarchy; no B34-specific overrides
12. **v6 corruptions** — v6-01: invented MENTORING/TRAINING/ONBOARDING predicates; v6-02: explain→DOCUMENT, coach→IMPLEMENT, ensure quality→QA; v6-03: guidance+code predicates lost
13. **Shadow vocabulary** — shadow = observation vocabulary → gap; third-party actions during shadowing do not fire predicates; embedded requester actions do fire
14. **Pair-with vocabulary** — pair-with = collaborative learning → gap; "pair with to [code verb]" = embedded verb fires; "pair with to learn" = learning intent → gap
15. **Walk-through vocabulary** — walk-through = explanation → gap; "walk through integrating" → API_CONTRACT; "walk through building + implement" → IMPLEMENT
16. **Workshop vocabulary** — workshop as event → gap; workshop topic never fires topic-associated predicate; curriculum noun → gap; software system noun → IMPLEMENT
17. **Advise vocabulary** — advise = advisory → gap; "advise and [code verb]" = code verb fires; "advise to [verb]" = recommend others act = gap
18. **Facilitate vocabulary** — facilitate = gap; "facilitate integration" → API_CONTRACT (special rule); team's actions in facilitated session = third-party = gap
19. **"teach to integrate" exception** — the only construction where "integrate" does not fire API_CONTRACT; infinitive "to integrate" = learning target

---

## Cross-Layer Synthesis Traces

| File | Scenario | Predicate Set |
|------|----------|---------------|
| 001 | Build portal + write guide | IMPLEMENT+DOCUMENT |
| 002 | Gap: "Mentor team on architecture" | [] → GapHandler |
| 003 | Build + E2E test + QA the skill platform | IMPLEMENT+TEST+QA |
| 004 | Full LMS cycle with guidance context | ARCHITECTURE+API_CONTRACT+IMPLEMENT+SECURITY+TEST+DOCUMENT+QA |
| 005 | Pure gap: "Explain the architecture to the team" | [] → GapHandler |
| 006 | Embedded TEST: "Guide through writing unit tests" | TEST + gap warning |
| 007 | Embedded API_CONTRACT: "Walk through integrating the SSO" | API_CONTRACT + gap warning |
| 008 | Multiple embedded predicates: IMPLEMENT+API_CONTRACT+TEST all fire alongside guidance vocabulary | IMPLEMENT+API_CONTRACT+TEST + gap warnings |
