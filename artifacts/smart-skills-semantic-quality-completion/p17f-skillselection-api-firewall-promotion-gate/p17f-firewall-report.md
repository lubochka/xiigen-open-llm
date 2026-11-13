# P17F: API/firewall promotion gate after SkillSelection P17D/P17E

## Короткий вывод
P17F подтверждает через настоящий `XiigenExtensionApiService.ShouldClaimSemanticQuality`, что P17D runtime improvement и P17E lifecycle import/export являются полезной компонентной evidence для SkillSelection, но не дают права заявлять semantic quality, SkillSelection readiness, full API readiness или final model readiness.

## Что теперь поддержано
- SkillSelection candidate прошел runtime improvement gate в P17D: source-disjoint rows passed, shortcut leaks were zero, diagnostics-only rows stayed excluded.
- SkillSelection candidate прошел lifecycle import/export gate в P17E через production `LoadCheckpointAsync`, `ExportAsync`, `ImportAsync`, повторную загрузку и held-out non-regression.
- API firewall остается активным: component evidence alone is not `SEMANTIC_QUALITY_CLAIMED_TRUE`.

## Что остается заблокировано
- Full semantic quality claim.
- SkillSelection readiness claim.
- Full API readiness claim.
- Final model ready claim.
- Dashboard or host runtime readiness claim.

## Почему
`ShouldClaimSemanticQuality` требует явный положительный semantic-quality signal от PromptToTask, OrchestrationReasoning и SkillSelection одновременно, отсутствие no-claim evidence, отсутствие `CandidatePositionScaffoldEvidence`, отсутствие second pass и загруженные common assets. P17D/P17E не создавали такой full positive signal; они закрывали component runtime/lifecycle evidence only.
