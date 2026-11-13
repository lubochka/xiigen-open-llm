# Document Use Log

Run: dotnet-csharp-runner-adapter/batch-008
Status: RUNNER_VERIFIED_ACCEPTED

Documents used:

1. README.md
2. CODE-DOC-CALL-TREE-ORCHESTRATION-PLAN-v1.md
3. CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md
4. CODE-DOC-DPO-REGISTRY-GUIDE-v1.md
5. CODE-DOC-EXAMINATION-METHOD-v1.md
6. CODE-DOC-FOUR-LEVEL-TRAINING-PLAN-v1.md
7. CODE-DOC-SOURCE-REGISTRY-v1.md
8. EXECUTOR-PROMPT-CODE-DOC-50-PAIR-LEARNING-v1.md
9. Batch-007 NEXT-BATCH-DECISION.md

Batch-008 combines source-root and cross-file transit gates:

- Web.config is valid only when linked to Breeze controller meaning.
- Global.asax is valid only when linked to WebApiConfig route registration.
- EF generated artifacts are excluded even when version fields are known.
