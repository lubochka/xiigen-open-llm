# ADAPTIVE-EFFECT-GAP-02

Root cause: Previous user adaptive assets proved lifecycle on test-local prompts, but they were not aligned to TOKEN-01 scenario prompts; the capture also compared only ChatDisplayText, hiding API field changes.

Probe scenario: `TQ-007-skill-architecture-audit`.

What changed:
- built scenario-aligned user-local PromptToTask, OrchestrationReasoning, and SkillSelection adaptive assets from TOKEN-01 universal rows;
- wrote ref-only DPO rows with chosen/rejected refs and no raw prompt text;
- proved backup/share/import/rollback directories exist;
- proved adaptive API output projection differs from baseline and share-import preserves adaptive behavior.

What this does not prove:
- token savings;
- quality improvement;
- semantic quality;
- common model readiness;
- target promotion or locator update.