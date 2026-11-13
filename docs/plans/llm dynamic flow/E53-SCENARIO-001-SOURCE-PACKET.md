# E53 Scenario 001 Source Packet

Scenario id: `E53-SCENARIO-001-COMPLEX-ONBOARDING-MIGRATION`

Experiment: `E53-HIGH-LEVEL-ORCHESTRATION-PROOF`

Date: 2026-05-04

## Purpose

This file is the source of truth for the first E53 orchestration proof.

Every station output in the expected trace must stay inside this source packet. The trace may make hidden structure explicit, but it must not add new product decisions, new implementation choices, or new promises that are not present here.

## Original Human Problem

We are changing our customer onboarding flow.

Some existing users started with email and password.

Some users want to use Google sign-in.

Some users stopped halfway through the onboarding questionnaire and want to continue later.

Repeated provider callbacks must not create duplicate accounts.

We also need terms and privacy acceptance before a user becomes fully active.

If a token or session proof expires before onboarding finishes, the flow must stop and ask for a fresh trusted action.

A stakeholder asks whether high-risk users should also pass biometric passport verification before activation.

Another stakeholder asks to create a second account when the same Google callback repeats so the audit trail is easier to inspect.

Produce an orchestration plan and a human-readable decision.

Do not choose an implementation tool, storage product, service product, hosting product, deployment environment, or output format that binds this orchestration to a specific stack.

## Source Facts

| Fact id | Source wording | Required preservation |
|---|---|---|
| F01 | We are changing our customer onboarding flow. | The answer must be an orchestration decision, not a code or deployment plan. |
| F02 | Some existing users started with email and password. | Existing email-started users must remain visible as an account-state branch. |
| F03 | Some users want to use Google sign-in. | Provider identity entry must be present as a separate branch. |
| F04 | Some users stopped halfway through the onboarding questionnaire and want to continue later. | Questionnaire progress and resume behavior must be preserved as a state-dependent branch. |
| F05 | Repeated provider callbacks must not create duplicate accounts. | Duplicate prevention is a hard constraint. |
| F06 | Terms and privacy acceptance are required before a user becomes fully active. | Activation must be blocked until acceptance evidence exists. |
| F07 | Expired token or session proof must stop the flow and ask for a fresh trusted action. | Token and session safety must be a hard stop condition. |
| F08 | High-risk users may need biometric passport verification before activation. | Biometric passport verification is a knowledge edge unless trusted source rules are supplied. |
| F09 | A stakeholder asks to create a second account for a repeated Google callback. | This is an explicit conflict with F05 and must be rejected. |
| F10 | The audit trail should be easier to inspect. | Audit clarity is a legitimate goal, but it cannot override duplicate prevention. |
| F11 | Produce an orchestration plan and human-readable decision. | Final output must be understandable to a human decision maker. |
| F12 | Do not bind the orchestration to a specific stack. | No station may require a specific implementation environment. |

## Source Constraints

1. Provider identity must be trusted before it is used for account resolution.
2. Existing or pending account state must be resolved before questionnaire continuation.
3. A repeated provider callback must reuse or continue the existing account path and must not create another account.
4. Terms and privacy acceptance must exist before full activation.
5. Expired or invalid trust proof must stop continuation and request a fresh trusted action.
6. Resume after app closure is a boundary because saved-state restoration rules are not supplied in this source packet.
7. Biometric passport verification is a knowledge edge because proofing, review, privacy, and retention rules are not supplied in this source packet.
8. Audit clarity may be addressed only through traceability of the existing account path, not through duplicate account creation.
9. The final answer must stay stack-independent.

## Explicit Conflicts

### C01 - Duplicate account request conflicts with duplicate prevention

The request to create a second account when the same Google callback repeats conflicts with the rule that repeated provider callbacks must not create duplicate accounts.

Expected handling:

- preserve the stakeholder request;
- mark it as a conflict;
- reject the duplicate-account branch;
- explain that audit clarity must be solved without creating another account.

### C02 - Biometric passport request exceeds local coverage

The request to add biometric passport verification introduces identity-proofing rules that are not supplied by the source packet.

Expected handling:

- preserve the request;
- name the missing competence;
- prepare a knowledge-edge context pack;
- do not invent the biometric proofing flow.

### C03 - Resume request requires saved-state rules

The request to continue later after stopping halfway through onboarding requires saved-state restoration rules that are not supplied by the source packet.

Expected handling:

- preserve the resume requirement;
- mark it as a boundary;
- allow covered state-resolution work;
- avoid promising exact resume behavior until rules exist.

## Explicit Unknowns

1. How onboarding questionnaire progress is stored.
2. How saved progress is restored after the user leaves and returns.
3. Whether policy acceptance must happen before questionnaire start, before questionnaire completion, or only before final activation.
4. What qualifies a user as high-risk.
5. Whether biometric passport verification is legally, operationally, and privacy-wise allowed.
6. How biometric proof is captured, verified, retained, deleted, or audited.
7. What audit evidence is acceptable instead of duplicate account creation.

## Forbidden Assumptions

The expected trace must not assume:

- that biometric passport verification is required;
- that biometric passport verification is safe or allowed;
- that resume behavior is already defined;
- that audit clarity permits duplicate account creation;
- that terms and privacy acceptance can be skipped;
- that an expired token can be repaired silently;
- that provider sign-in and email-started registration are the same branch;
- that a final answer can hide blocked or rejected work;
- that an implementation environment has already been chosen.

## Stack-Independence Rule

The source problem asks for orchestration management, not implementation.

The expected trace and final answer may use neutral words such as:

- account;
- provider identity;
- pending state;
- trusted action;
- acceptance evidence;
- saved progress;
- context pack;
- review;
- learning gap.

The source problem and final human answer must not name an implementation language, storage product, service product, hosting product, deployment environment, package system, or platform-specific runtime.

## Expected Audience

The final result is for a human product and architecture reviewer.

The reviewer needs to understand:

- what can move forward;
- what is blocked;
- what is rejected;
- what is unknown;
- what needs outside help or more source authority;
- what the next actions are.

## Source Authority

This source packet is derived from:

- the E53 high-level orchestration proof plan;
- the accepted Flow-01 training-set work for node-00 through node-03;
- the E51B registration proof patterns for knowledge edge and arbiter rejection;
- the locked rule that arbiters decide what is accepted.

## Source Boundary

This file does not define the full solution.

This file defines the problem that the orchestrator must manage.

The expected trace is allowed to produce a plan, blocked items, rejection items, context packs, and learning gaps. It is not allowed to invent missing policy, resume, or biometric rules.
