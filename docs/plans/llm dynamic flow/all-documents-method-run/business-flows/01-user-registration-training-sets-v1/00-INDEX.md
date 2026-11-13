# 00 - Index

## What this file is

This is a learning set for the dynamic-flow UML nodes. It is not a line ledger of
the source document.

The source document remains the evidence:

`C:\Users\lubao\Documents\xiigen\business flows\01-user-registration.md`

The input side of each pair is shaped like the real thing that node receives at
runtime. A source-document line may justify a pair, but the source line itself is
not used as the training input unless a human would actually type that line.

Each pair uses this rule:

A -> trained LLM node -> B

The B is always concrete. It avoids vague placeholders and shows the actual
statement, the actual labels, the actual plan, the
actual review decision, or the actual final answer.

## Correct UML placement used here

The flow is treated as nine training targets:

1. Node-01 Domain splitter: raw user prompt -> domain labels d1...dm.
2. Node-02 Domain translator: one prompt fragment plus one domain -> formal domain wording.
3. Node-03 Scheme planner: all domain wordings -> planned pieces b1...bn and prompts fb1...fbn.
4. Node-04 Domain processor array: each bi is processed by its own unit.
5. Node-05 Applier array: each processed bi and fbi produces one result Rqi.
6. Node-06 Result translator array: each Rqi becomes user-language wording, with all R1...Rn visible for consistency.
7. Node-07 Arbiter selector: all R1...Rn plus context -> selected arbiters.
8. Node-08 Arbiter units: each selected arbiter reviews the relevant result and can pass it or send fbx back for reprocessing.
9. Node-09 Formatter: all approved user-language results -> one readable answer.

The arbiter selector and arbiter units are not after the formatter. They operate
before final formatting. Their feedback returns a reprocessing fragment back into
the front of the flow.

## Document examination method used to build this set

1. Read the single selected document from beginning to end.
2. Mark its real runtime situations: Google sign-up, email sign-up, email verification,
   questionnaire delivery, account merge, provider outage, mobile return, tenant
   registration, abandonment, rate limiting, duplicate callbacks, expired links.
3. Mark the human vocabulary for each situation. This vocabulary becomes Node-01 input.
4. Convert each situation into domain-specific formal wording. This becomes Node-02 output.
5. Decompose the situations into pieces that can run in sequence or in parallel. This
   becomes Node-03 output.
6. For each planned piece, write the result a specialized processor should produce.
   This becomes Node-04 output.
7. For each processed piece, write the concrete action and outcome. This becomes
   Node-05 output.
8. Translate each formal result into what the human user or stakeholder should see.
   This becomes Node-06 output.
9. Decide which reviewers are required for each result group. This becomes Node-07 output.
10. Write actual pass or flag decisions for those reviewers. This becomes Node-08 output.
11. Fold approved translated results into one answer for a real audience. This becomes
    Node-09 output.
12. Simulate the full flow from raw human prompt to final answer. Any missing fact means
    the responsible node needs more training pairs.

## Shared domain labels

These labels are used by Node-01 and Node-02:

- authentication
- communication
- onboarding
- security
- event coordination
- user management
- operations
- privacy and compliance
- client experience
- tenant context
- product metrics

The labels are not final architecture names. They are the training vocabulary extracted
from this one registration document.

---

