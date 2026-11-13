# Document Use Log

Run: dotnet-csharp-runner-adapter/batch-002
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
9. External CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md
10. Batch-001 NEXT-BATCH-DECISION.md

Batch-002 intentionally adds negative held-out cases:

- wrong runtime family: netcoreapp3.1 marked as NET_FRAMEWORK
- wrong language default: legacy net46 marked as CSharp8
- missing accepted callee summaries for D3 caller documentation

Runner observations:

- runner-001 rejected JSONL because one-element known_callees was serialized as an object.
- runner-002 exposed empty known_callees as [{}], causing transit expectation mismatch.
- runner-003 passed after normalizing empty callees to [].
