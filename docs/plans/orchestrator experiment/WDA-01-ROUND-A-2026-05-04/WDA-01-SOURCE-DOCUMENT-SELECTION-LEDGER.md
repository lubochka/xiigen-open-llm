# WDA-01 Source Document Selection Ledger
# Document to sub-phase classification
# Round A - Oracle source ledger
# Date: 2026-05-04

---

## Purpose

This ledger defines the first classification probe for every development
sub-phase. It does not claim the broad corpus exists. It defines the minimum
source shape needed to test whether the orchestrator can route documents into
the 31 canonical sub-phases.

---

## Readiness Rule

Each source document or hand-authored packet must contain:

1. one primary phase
2. one primary sub-phase
3. at least three verifiable claims
4. at least one known failure mode
5. expected top-three predicate profile
6. source authority note

If a real document is unavailable, WDA-01 records that as a source knowledge
edge and may use a hand-authored source packet for the deterministic proof.
That hand-authored packet cannot count toward broad corpus quota.

---

## Ledger

| Probe | Phase | Sub-phase | Source document shape | Claims required | Failure mode | Expected predicates | Source priority |
|---|---|---|---|---|---|---|---|
| WDA-01-01 | 1 Design | 1.1 Design system and tokens | design token or component primitive rules | tokens named; typography scale named; spacing or color rule named | component uses value outside token set | VALIDATE_OP, GET_OP, RETURN | local design guide, otherwise synthetic packet |
| WDA-01-02 | 1 Design | 1.2 Screen and layout structure | screen inventory and layout hierarchy | screens named; navigation model named; breakpoint behavior named | screen omitted or assigned to wrong navigation group | CREATE, ITERATE, RETURN | local UI plan, otherwise synthetic packet |
| WDA-01-03 | 1 Design | 1.3 Interaction model and state transitions | interaction state model | trigger named; resulting state named; recovery state named | state transition missing error or loading branch | BRANCH, UPDATE_OP, RETURN | local UI/e2e plan, otherwise synthetic packet |
| WDA-01-04 | 1 Design | 1.4 Accessibility and compliance | accessibility requirement or audit rule | standard named; keyboard rule named; contrast or reader rule named | visual behavior passes but accessibility rule missing | VALIDATE_OP, FAIL, RETURN | local QA/accessibility doc, otherwise synthetic packet |
| WDA-01-05 | 1 Design | 1.5 Motion and animation | motion rule document | motion category named; duration/easing named; reduced-motion rule named | animation used where reduced motion should disable it | PROCESS_OP, BRANCH, RETURN | synthetic likely, local if available |
| WDA-01-06 | 2 Functional spec | 2.1 User stories and acceptance criteria | user story and acceptance criteria set | actor named; goal named; observable outcome named | implementation detail mistaken for acceptance criterion | DECLARE, VALIDATE_OP, RETURN | business flow docs |
| WDA-01-07 | 2 Functional spec | 2.2 Business rules | rule or policy catalog | condition named; consequence named; authority named | rule contradicts another rule or lacks authority | BRANCH, FAIL, VALIDATE_OP | business flow docs |
| WDA-01-08 | 2 Functional spec | 2.3 Integration point specification | integration need document | external system named; needed result named; fallback or constraint named | implementation mechanism appears without business need | SEND_OP, AUTH_OP, HANDLE_ERROR | business flow or integration docs |
| WDA-01-09 | 2 Functional spec | 2.4 Data model boundaries | conceptual entity boundary document | entity named; relationship named; ownership/lifecycle named | technical schema detail replaces business boundary | CREATE, GET_OP, VALIDATE_OP | business flow or data plan |
| WDA-01-10 | 3 Technical design | 3.1 Architecture and component topology | architecture or component topology plan | component named; responsibility named; forbidden coupling named | build order mistaken for runtime topology | DECLARE, CREATE, RETURN | dynamic flow architecture docs |
| WDA-01-11 | 3 Technical design | 3.2 Data schema and migration design | schema or migration design | schema change named; migration sequence named; rollback/safety named | migration lacks rollback or compatibility path | CREATE_OP, UPDATE_OP, VALIDATE_OP | migration/session docs |
| WDA-01-12 | 3 Technical design | 3.3 API contract definition | API contract plan | request shape named; response shape named; error/version rule named | endpoint described without error contract | DECLARE, SEND_OP, AUTH_OP | API or contract docs |
| WDA-01-13 | 3 Technical design | 3.4 Security and authorization model | security/authorization model | role named; permission named; enforcement point named | permission listed but not enforced | AUTH_OP, VALIDATE_OP, FAIL | security or auth docs |
| WDA-01-14 | 3 Technical design | 3.5 Infrastructure and deployment topology | deployment/infrastructure topology | deployment unit named; scaling/network rule named; rollback/gate named | deployment gate omitted or network rule contradicted | CREATE, PROCESS_OP, HANDLE_ERROR | infra/deployment docs |
| WDA-01-15 | 4 Implementation | 4.1 Backend services | backend implementation session plan | class/interface named; method behavior named; test/gate named | implementation violates service/fabric rule | CALL, CREATE_OP, HANDLE_ERROR | project 21-23 session docs |
| WDA-01-16 | 4 Implementation | 4.2 Frontend and client rendering | frontend implementation plan | component named; state/data binding named; API/design constraint named | rendered state ignores design or API contract | GET_OP, UPDATE_OP, RETURN | UI implementation docs |
| WDA-01-17 | 4 Implementation | 4.3 Integration and provider connectors | connector implementation plan | provider/interface named; auth named; failure path named | connector bypasses abstraction or misses auth failure | SEND_OP, AUTH_OP, HANDLE_ERROR | connector/session docs |
| WDA-01-18 | 4 Implementation | 4.4 Data migration execution | migration execution plan | batch step named; verification named; rollback named | migration runs without verification or rollback | ITERATE, UPDATE_OP, LOG_OP | migration execution docs |
| WDA-01-19 | 4 Implementation | 4.5 Documentation | documentation update plan | audience named; artifact named; decision trace named | doc describes behavior without source decision | CONVERT_OP, RETURN, VALIDATE_OP | project documentation docs |
| WDA-01-20 | 5 Testing | 5.1 Unit and contract tests | unit/contract test plan | unit named; input/output assertion named; failure assertion named | test checks implementation detail not contract | VALIDATE_OP, FAIL, RETURN | unit/contract test docs |
| WDA-01-21 | 5 Testing | 5.2 Integration tests | integration test plan | components named; boundary named; failure simulation named | integration test actually tests one isolated unit | CALL, SEND_OP, HANDLE_ERROR | integration test docs |
| WDA-01-22 | 5 Testing | 5.3 End-to-end scenario tests | e2e scenario plan | user action named; system path named; observable outcome named | scenario omits acceptance criterion evidence | ITERATE, VALIDATE_OP, RETURN | e2e plans |
| WDA-01-23 | 5 Testing | 5.4 Performance and load tests | performance/load plan | load profile named; latency/throughput target named; failure threshold named | target stated without measurable load profile | PROCESS_OP, LOG_OP, FAIL | perf/load docs |
| WDA-01-24 | 6 QA | 6.1 Functional review | QA functional review checklist | acceptance criterion named; evidence link named; verdict named | pass verdict without evidence | VALIDATE_OP, RETURN, FAIL | QA review docs |
| WDA-01-25 | 6 QA | 6.2 Regression coverage verification | regression review | changed area named; preserved behavior named; coverage gap named | regression gap hidden as not applicable | ITERATE, VALIDATE_OP, LOG_OP | regression docs |
| WDA-01-26 | 6 QA | 6.3 Security and compliance review | security/compliance review | rule named; evidence named; violation/remediation named | compliance item marked pass without evidence | AUTH_OP, FAIL, VALIDATE_OP | security QA docs |
| WDA-01-27 | 6 QA | 6.4 Accessibility audit | accessibility audit | criterion named; screen/component named; pass/fail evidence named | visual pass hides keyboard/screen-reader fail | VALIDATE_OP, FAIL, RETURN | accessibility audit docs |
| WDA-01-28 | 7 QA automation | 7.1 Test scenario automation | automation scenario plan | scenario named; automation fixture named; assertion named | automation omits manual scenario step | CREATE, ITERATE, CALL | automation docs |
| WDA-01-29 | 7 QA automation | 7.2 Regression suite maintenance | regression suite maintenance plan | add/update rule named; retirement rule named; owner named | obsolete test remains as false confidence | UPDATE_OP, ITERATE, LOG_OP | regression maintenance docs |
| WDA-01-30 | 7 QA automation | 7.3 CI/CD gate configuration | CI/CD gate plan | gate named; threshold named; blocking behavior named | failing gate is non-blocking when it should block | BRANCH, FAIL, SEND_OP | CI/CD docs |
| WDA-01-31 | 7 QA automation | 7.4 Coverage and reporting | coverage/reporting plan | metric named; threshold named; audience/report format named | coverage report hides missing scenario coverage | GET_OP, CONVERT_OP, RETURN | coverage/report docs |

---

## First-Pass Source Priority

Use local documents first. Candidate local families seen in the workspace scan:

- e2e test plans
- UI e2e plans
- learning pipeline plans
- LLM integration plans
- architecture plans
- business flow plans
- master execution plans
- session state documents
- task type catalogs
- validation reports
- QA and regression plans

Do not count `.claude\worktrees` duplicate copies as separate source authority
unless they differ materially from the main document and that difference is
named in the ledger.

---

## Knowledge Edge Rule

If no ready source document exists for a probe, the probe still runs with a
hand-authored source packet, but the result must include:

```text
source_status: knowledge_edge
source_gap: no ready local source document found for this sub-phase
```

That probe may pass as a routing-shape proof, but it cannot promote broad
training data for that sub-phase until a real source document is found or a
reviewed synthetic document is approved.
