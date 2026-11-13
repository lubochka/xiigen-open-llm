# P7A parent review

## Вердикт

`ACCEPTED_FAIL_EVIDENCE_TRAINING_COVERAGE_REQUIRED`.

P7A не является проходом P7 и не заявляет semantic quality. Это принятая граница
fail-evidence: прежний ремонт через production-маппинг был отклонен, маппинг
удален, а оставшийся результат честно показывает, что P6 checkpoint не хватает
train-only покрытия для source-disjoint PromptToTask строк.

## 4 цикла review

1. Goal delivery.
   P7A должен был проверить, можно ли честно улучшить PromptToTask после P7 fail.
   Итог: честного pass нет. Принята не победа, а правильный fail: модель дает
   `0/4` положительных reserved rows и требует новую training-only фазу.

2. Leakage / no shortcut.
   Parent review нашел, что предыдущий кодовый ремонт подмешивал exact P7
   reserved/eval/holdout signal combinations. Это было deterministic fixture
   fitting. После исправления в production-коде не осталось
   опасного derived-signal helper, не осталось pass-candidate marker, а P7A не
   использует row id, source family, filename/path/title, exact prompt или
   reserved signal combinations как runtime authority.

3. Trainable architecture.
   Production code diff не удержан. P6 checkpoint не перезаписан, новый
   checkpoint не создан, eval/holdout rows не использованы для обучения.
   Следующий допустимый ремонт: добавить train-only PromptToTask coverage или
   заранее существующую common taxonomy, затем создать новый checkpoint
   candidate и заново пройти P7.

4. Verification / docs / no-claim.
   Focused test прошел `1/1`. JSON и JSONL артефакты парсятся. Overclaim scan
   не нашел положительного semantic-quality claim или машинного маркера прохода P7.
   Документация обновлена в `docs/projects/43-smart-skills-domain.md`.

## Следующее действие после commit/push

P7B: через того же bounded sub-agent добавить train-only source-disjoint
PromptToTask coverage или pre-P7 common taxonomy, создать новый checkpoint
candidate и только потом заново запускать P7 eval/ablation gate.
