# E16 — быстрый причинный эксперимент потери DPO-контраста до обучения

**ExperimentId:** `E16-MODEL-VISIBLE-PAIR-CONTRAST-01`; **дата** 2026-10-16; **статус** `READY_TO_IMPLEMENT / JSON_FIXTURE_PUBLISHED / NATIVE_NOT_RUN / TRAINING_NOT_RUN / INDEPENDENT_REVIEW_PENDING`.

Основание: [новая находка и code-level доказательства](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md); [готовые 4 записи](18-E16-PAIR-CONTRAST-FIXTURE.json). Pinned core `codex/dpo-implementation@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`. **Это диагностика Translation DPO bridge, а не прямой эксперимент RP01 exact-v2.**

## Гипотеза

**H1:** если учебные `chosen/rejected` сырые тексты различны, но до модели кодируются одинаковыми ID, DPO не видит предпочтения; восстановление информации в одном слое кодирования может повысить реальную обучаемость. **H0:** таких проблем нет на фактическом допущенном corpus либо исправление кодирования не улучшает скрытую предметную панель. Один результат не является доказательством across-domain generalization.

## Программа из двух независимых стадий

### L0 — подтвердить реальный native C# маршрут (без обучения)

Добавить **только** минимальную диагностическую цель в `llm_mvp_core` в существующий test/tools host после чтения реальных project references. Загрузить `18-E16-PAIR-CONTRAST-FIXTURE.json`, получить `DpoBatch` **через действующий** `TranslationDpoTextFeaturizer.BuildRecord` и существующий путь допустимого test-only `TranslationDpoTrainingHandoffRecord`; не мутировать уже допущенные tensors, не подменять admission.

| Case | Chosen | Rejected | Ожидается `ChosenIds.SequenceEqual(RejectedIds)` |
| --- | --- | --- | --- |
| HASH01 | `accept` | `invalid` | true: hash-ID 289 = 289 |
| HASH02 | `true` | `unauthorized` | true: hash-ID 421 = 421 |
| TRUNC01 | 70×`neutral`+`allow` | 70×`neutral`+`deny` | true: хвост за пределом 61 токена |
| CONTROL01 | `allow` | `deny` | false: IDs 497 ≠ 352 |

Фиксировать `raw_sha256`, `chosen_ids_sha256`, `rejected_ids_sha256`, model-visible ID arrays и mask arrays, количество токенов до/после усечения, `MaterialVersion`, exact code call path и source SHA. Никаких агрегатных «500 ACCEPTED» вместо поэлементных данных. **Pass L0**: 3 ожидаемых identical + 1 различающийся контроль на native code. **Fail/Falsifier**: реальные массивы отличаются от прогноза — читать tokenizer/config/active route, не править fixture ради зелёных тестов.

Независимый SHA256 расчёт токен-ID уже дал HASH01=289 и HASH02=421, но это **не native code execution**. `DpoExactPairMaterial` exact-v2 проверяет equal encoded responses, так что этот дефект не переносится автоматически на RP01 direct Student.

### L1 — проверить влияние на обучаемость и ответ (только после L0)

Подготовить маленький набор **8–32 независимо разрешённых семантических DPO-пар**, которые реально используют уязвимые слова/конструкции, и 8 blind heldout примеров другого source/family. Обе финальные стороны DPO создаёт **разрешённая открытая китайская модель**, CODEX/Astra UltraCode независимо оценивает выбранную и отвергнутую стороны и утверждает точные bytes до learner. Синтетические HASH/TRUNC являются только L0 диагностикой и **не допускаются как учебные**.

**A baseline:** текущий hashed-to-508-ID encoder, с одинаковой допускающей проверкой context capacity. **B enhanced:** тот же tokenizer, тот же seed/base/LoRA/steps/beta/sampling/split, но только для *одной заранее выбранной конфликтной пары слов* использовать разные свободные token IDs; размер embedding/весь остальной token mapping оставить прежним. При отсутствии свободного ID остановить опыт, не менять размер словаря. Отдельно fail-closed reject при хвостовом truncation **в обоих arms**: нельзя смешивать два дефекта в одном причинном сравнении.

Для обоих A/B хранить одинаковые frozen train/heldout, versioned tokenizer и CheckpointIDs. Тренировать только через действующий admission/mutation API, **не** переопределять содержимое sealed DpoBatch. Не менять случайную инициализацию B0, rank LoRA, reference weights, длины, корпус между arms. Если единичная подстановка требует изменения других ID, это больше не контролируемая проверка — пометить TOOL/NEEDS_FIX.

**Главная оценка:** `heldout_semantic_law_pass/8` на **обычном consumer**, строго по независимому gold, отдельно `wrong/abstain/unsupported`. Вторичные: процент уцелевших chosen/rejected ID различий, signed DPO preference margin, LoRA norm, train-only overfit score, время. Рост margin без корректных независимых ответов ≠ новый навык.

**Предрегистрация пилотного результата:** улучшение B над A на ≥2 правильных blind ответах из 8 без регрессии на hard-negatives является сигналом к увеличению выборки, **не** окончательным статистическим доказательством. Равный результат на 8 случаях — недостаточно для исключения гипотезы. Если нет source-disjoint gold, фиксировать только L0; если модель B0 не осваивает даже train-only контроль, остановить большой DPO и проверить foundation/gradient.

## Отдельный Unit-контроль (не смешивать с A/B)

[N17](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md) проверяется отдельным experiment ID: один корректный UnitId и один intentionally wrong UnitId, одинаковый обучающий пример. Сравнить `RowsTrained` с числом фактических `TrainOnExample` вызовов, state hash, direct Unit response и Flow response. Это контроль ложного свидетельства обучения, а не часть испытания токенизатора.

## Deliverables, критерии и владельцы

Записать отдельную директорию run со следующими файлами: `experiment-result.json` (все четыре case, refs, array hashes, side equality, L1 metrics), `report.md` (русский вывод source-verified/measurement/hypothesis), `independent-review.md` (кодовая и смысловая перепроверка). **Не изменять** старые `results`, checkpoint C#500/ENRU500 и существующие проверки. Достаточно адресного локального запуска — не запускать весь GitHub Windows CI.

Исполнитель CODEX: загрузить skills `architect-behavior-classifier`, `trainable-architecture-decision`, `code-examination`, `experiment-design`, `learning-path-audit`, `plan-review`, `claim-verification`, `human-readable-architecture-audit`, `prompt-patch-authoring` из `docs/skills`; reviewer отдельно смотрит реальные `BuildRecord`, `EncodeSide`, `ToStableTokenId`, `DpoTrainer.TargetMask` и сравнивает frozen outcome. **Сейчас ни C# L0, ни L1, ни независимая Astra-рецензия не запускались.**

Общий индекс исходных 44 причин: [01-PRIORITIZED-DIFFERENCES.md](01-PRIORITIZED-DIFFERENCES.md); по другим настоящим memory-learning Unit см. [11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md).
