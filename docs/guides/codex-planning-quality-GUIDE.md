# Codex Planning Quality Guide Path Shim

This file exists to close the guide-path loophole found during the
ILearningDynamicGraph DPO planning repair chain.

Canonical guide:

```text
C:\Projects\llm_mvp_core\docs\skills\codex-planning-quality-GUIDE.md
```

Required behavior:

1. Treat this file as a compatibility guide path only.
2. After reading this file, read the canonical guide above completely.
3. A sub-agent packet may report this path as `found/read` only when it also
   reports the canonical guide as `found/read`.
4. Do not treat this shim as a replacement for the canonical guide.

Reason:

Some governed work orders require
`C:\Projects\llm_mvp_core\docs\guides\codex-planning-quality-GUIDE.md`, while
the maintained full guide lives under `docs\skills`. A missing guide path makes
otherwise valid work orders look partially loaded. This shim preserves the
expected guide path and keeps the full guide authoritative.
