# P16C: сфокусированные тесты границ SkillSelection

## Краткий вывод

P16C добавляет узкий тестовый слой для текущей архитектуры SkillSelection в режиме baseline/no-claim. Тесты подтверждают, что candidate-position остается диагностикой, prompt/task/context/intermediate branches не выдают readiness claim, а runtime evidence не превращает source id, exact prompt, title/path или candidate order в финальную authority.

Это не обучение, не checkpoint, не source-disjoint eval и не claim semantic quality.

## Что проверено

| Граница | Результат | Остаточный gap |
| --- | --- | --- |
| CandidateOrderLeakageGuardUnit | Порядок-зависимый synthetic probe обнаружен, но unit не может выбирать skill. | Нужен будущий source-disjoint eval после checkpoint. |
| PromptUnderstandingSkillUnit | Решение идет через baseline prompt-feature state; source id, title и exact prompt не попадают в evidence. | Это unit test, не full positive evidence. |
| TaskUnderstandingSkillUnit | Empty machine возвращает not-ready evidence, а не candidate-position fallback. | Нужна P16D/P16E проверка на реальных split rows. |
| PromptContextSkillSelectionUnit | Baseline evidence явно маркируется; снятие context снижает confidence. | Не является полной готовностью SkillSelection. |
| IntermediateChecksSkillUnit | K-NN support остается support_evidence_only; source markers скрыты. | Нужен P16E ablation gate. |
| SkillSelectionLogicalFlow | Flow показывает 11 semantic units и no-claim boundary. | Private TryDecideWith* branches требуют дальнейшего coverage при checkpoint/eval фазах. |

## Команда проверки

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~SkillSelectionP16CFocusedBoundaryTests" --logger "console;verbosity=minimal"
```

Результат: `passed: Failed=0, Passed=6, Skipped=0, Total=6, Duration=335 ms`.

Первый запуск той же команды завершился timeout без результата; повтор с увеличенным timeout прошел успешно.

## Что осталось слабым

- P16C не создает SkillSelection checkpoint и не меняет runtime.
- P16C не продвигает P16B DPO rows в training_ready.
- P16C не доказывает full API semantic quality, host runtime readiness, dashboard readiness или final model readiness.
- Следующий безопасный gate: parent review P16C, затем P16D checkpoint candidate только если parent принимает текущие evidence/tests.
