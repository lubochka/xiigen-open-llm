# Orchestration Problem List

## P-001. Static brain remains inside the orchestration pipeline

Это хуево.

Внутри текущей pipeline часть "мозга" все еще статичная:

- `BuildIntent` режет prompt regex-ами.
- `EvidencePlanner` добавляет одинаковые вопросы.
- `UnitSelector` выбирает units через `if`.
- `HumanRouteComposer` строит одинаковый маршрут шаблоном.
- `RequiresExternalInput` решает по словам `figma` / `audio` / `platform`.

Решение пока не описывается.

STATUS: RESOLVED — Phase 37 (ORCHESTRATION-MACHINE-TRAINING-PLAN-v20, 2026-06)
Resolution: PromptFlowGroundingModel replaces all if/regex/deterministic flow routing in 42.ProjectWikiDomain.
FlowReferenceDetector moved to build-time corpus/audit only (never runtime).
exactPromptLeakageRate gate added (must be 0.0). See D-16 in DECISIONS-LOCKED.md.
