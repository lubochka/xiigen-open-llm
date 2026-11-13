# 06 — Restore, policy/reference identities, runtime consumer/adoption

<!-- POSTQ011-2026-11-04:START -->
## POSTQ011 — Три уровня восстановления и граница Q007

После Q011 в фиксированном 3502-path inventory92 identity равны original,3358 сохраняют прежнее изменение,19 иные,33 additions. Это Git census; actual LFS payload и nativeLoad отдельны. Два shared S1 P11/P12 всё ещё требуют originals по 29-D3/29-W1 и отдельного reader без обучающих fixtures. Уже восстановленные 43 дополнительных пути Q005–Q011 не чинить повторно по старым навигационным текстам.

Q007 native-result-002: optimizer-empty и optimizer-nan имеют NativeAccepted=true,FullStateAccepted=false и изменённый digest. Внешний reader guard защищает свой объект; продуктовый AdamWimporter остаётся иной границей. Numeric checkfinite(m/v),v>=0 выполняется до swap detachedstate. Completeness конкретного continuation проверяется по trusted expected initialized keyset/step/owner; не по suppliedEntries и не обязательно по всем параметрам модели. Сохранить emptyStep0 и legitimate sparsepositive. После отказа полный live bundle остаётся прежним; actual DpoTrainer consumer проверяется отдельно. Q04paused/Q05dependent сохраняются; byte repair не назначает новый Train.

Основание: [единый разбор](codex-2026-11-04-POSTQ011-TRANSFER-AND-EXPERIMENT-AUDIT.md), [core158](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md) и [план 159](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-159-postq011-addressed-repair-plan-2026-11-04.md). В передаваемое поручение включить собственные положительный/отрицательный случаи, actual consumer и предел доказательства. Исходные результаты, данные, бюджет и очередь сохранены; текущая правка документная.
<!-- POSTQ011-2026-11-04:END -->


<!-- POSTMERGE-AUDIT-2026-11-02:START -->
## После объединения веток: читать именно импортированную память

**WRONG:** вызвать `target.ImportCheckpoint`, проверить его повторный экспорт, затем создать третью единицу, снова дать ей урок и объявить её ответ доказательством восстановления target. Именно так заканчивается `SavedLesson_SurvivesCheckpointRoundTrip_AndIsStillAppliedAfterImport` в новом PAIR-тесте.

**CORRECT:** сохранить связь source instance → checkpoint → target instance → ordinary reader target. После импорта additional Train=0; запрос и verdict идут через импортированный target. Повторный экспорт подтверждает перенос данных, а behavioral read — их использование; обе проверки сохраняются раздельно.

Отрицательный контроль: импорт оставляет данные доступными экспорту, но обычный reader target получает пустую/старую память. Behavioral gate обязан упасть; отсутствие падения обнаруживает подмену получателя. Положительный контроль сохраняет полный прежний перевод и verdict. Для миграции PAIR одновременно проверяются рабочие ключи, RULE, provenance и конфликт старых записей. Исторические checkpoints не переписываются.

Основание: [Аудит нового среза](codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md); неизменные исходные результаты и назначенные бюджеты сохраняются.
<!-- POSTMERGE-AUDIT-2026-11-02:END -->

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Сохранить файл и восстановить память — разные действия.** P107 после Save читает тот же живой Unit. P106 создаёт новый экземпляр, вызывает ImportCheckpoint и подтверждает точные 2/2. Для каждого результата сохраняются режим чтения, исходные байты, загрузчик и обычный потребитель. Новый объект и новый процесс — разные уровни доказательства.

В полный набор состояния входят все применимые обученные поля, версии ключей и provenance. Указатель LFS не является прочитанными весами; пересчёт digest из JSON подтверждает согласованность записей, не бинарного содержимого. Порча компонента должна вызвать настоящий отказ загрузчика и сохранить прежнее опубликованное состояние, а не подставить false по имени флага.

Миграция PAIR сохраняет RULE и обнаруживает коллизии до публикации. Отдельно проверяется LastEvaluationGate: ImportCheckpoint не может доверять одному PassesGate=true при accuracy ниже общего порога. После позднего сбоя отчёта читается уже сохранённое S1 без нового Train.

Основание: [POST107-аудит](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и [проверка реального кода](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md). Исторические результаты и бюджеты сохранены; это условия будущего переноса, не новый запуск.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->

## DIF-601 — Состояние сохранено, но полезный навык не доказан (REPORTED_RUN)
[C#500 commit](https://github.com/lubochka/llm_mvp_core/commit/c4c5fc2ddca56036d901ad26bf91c6998fb96ae6): все 16 LoRA изменены, 39 base неизменны, restored weights/optimizer/RNG. [ENRU continuation](https://github.com/lubochka/llm_mvp_core/commit/a338cea10660871967568c8b3bf6ced1d2857709): steps 51–100 из C50, separate frozen reference C50, research only. Сохранение ≠ изменившаяся генерация ≠ правильный ответ.

## DIF-602 — Ref mismatch и последовательное смешивание доменов (OPEN; P1)
C# → ENRU на одном Student сохраняет общий trainable state. Проверить reference boundary B0/C50, optimizer moments and carried scalar step, allowed parameter key identity, sampled generation settings. Одинаковые C# ответы 0/3→0/3 не доказывают retention useful skill, поскольку первоначально было 0/3. Negative transfer или catastrophic forgetting НЕ подтверждены, но должны измеряться после достижения nonzero baseline.

## DIF-603 — Frozen research adapter не внедрён в ordinary product (CODE + OPEN)
[Runner README](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/tools/Rp01SimulationDpo/README.md) явно фиксирует `RESEARCH_ONLY_NOT_ADOPTED`. Это правильная осторожность, но product path не получил научно подтверждённое состояние. Для новой выигрышной модели проверить separate canonical production admission, exact manifest, cold start loader, ordinary CodeDocumentation/translation consumer, no cross-domain staleness; пока NONADOPTED, не называть ошибкой отсутствия публикации.

## DIF-604 — Serial checkpoint success не проверяет ID space и кэш (HYPOTHESIS)
Potential masks: tokenizer mismatched hash/vocab/config, BOS/EOS, prompt framing, LoRA target module registration, KV cache invalidation, storage pointer, shadowed state on fresh process, ANN index schema migration, disposed adapter. Выполнить same prompt at pre-save/post-save/fresh-load/ordinary caller, compare full token ID distributions and selected tokens, log exact tokenizer/model/reference SHA. Reject any comparison with different framing or sampling.

## DIF-605 — Fast scientific gate vs 5h evaluation (REPORTED_RUN; process cost)
History: one pair/one optimizer step, 12×5 repeated tasks/arm, ~5h43m; many equivalent repeated outputs and serial local Qwen evaluations. Later runner trained 500 C# pairs in 6272.8s (~104.5min) excluding preparation and independent review, ENRU training 2330s (~38.8min). Optimization phase-only and whole-PC accounting were not fully instrumented in original runs. Benchmark forward/backward/optimizer, checkpoint, external evaluator separately; never add nested time twice. No recurring Windows CI/coverage gates for quality.

## Accept/reject evidence
Persist exact source and training IDs; pre/post keyed state, all immutable reference hashes; preference+free semantic results; ordinary consumer state ID; distinct domain family counts; CPU/RAM/time. Report `MECHANISM_VERIFIED`, `HELDOUT_GAIN_OBSERVED`, `NOT_MEASURED` independently.