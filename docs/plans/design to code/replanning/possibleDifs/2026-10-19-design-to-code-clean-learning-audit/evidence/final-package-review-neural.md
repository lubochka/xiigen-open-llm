# Финальная независимая проверка пакета: neural / DCM / однофакторность

Проверяющий: `/root/dpo_lora_audit`. Проверены README и главы 01, 02, 03, 04 в `audit/out/2026-10-19-design-to-code-clean-learning-audit/`; глава 00 адресно прочитана для R4/исторического replay. Source core/current и clean повторно сверялись только по конкретным неясностям. Файлы публикационного пакета не изменялись.

**Окончательный итог: PASS_BOUNDED_SOURCE_AND_EXPERIMENT_DESIGN после исправлений F1/F2/R4.** Существенная ошибка inheritance выявлена и исправлена до публикации. Старые формулировки ниже сохранены как история review, не как описание текущего пакета. Native обучение, Import/Load и quality gain по-прежнему не выполнялись.

## F1 — существенная поправка: не всё поведение DC-01b унаследовано

### Найденная ошибка классификации

README L15 («все шесть унаследованы»), глава 01 L7 («все три механизма есть и на clean») и глава 04 L43 («все эти гипотезы INHERITED») обобщали идентичность normalizer/ranking на дополнительный **изменённый** consumer — фильтр допущенных features.

Первичные источники:

- [Clean Machine L2651–2672](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L2651-L2672): `IsAllowedLearnedWeightFeatureKey` использует denylist, допускающую `correction_category:generic_visual_defect`.
- [Current Machine L2830–2854](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2830-L2854): allowlist допускает только `correction_category:generic-visual-defect`.
- Одинаковый [normalizer L283–295](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L283-L295) и [санитайзер L419–429](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L419-L429) превращают явно переданный canonical generic в underscore-вариант в обеих ветках.
- AddDelta применяет соответствующий feature gate: [clean L2908–2929](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L2908-L2929), [current L3094–3115](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3094-L3115).

Минимальное условие: все пять scalar fields chosen явно равны `generic-visual-defect`; все пять rejected — `overflow`. Phantom от отсутствующих полей выключен. В clean accepted deltas содержат `generic_visual_defect:+1, overflow:-1`; в current только `overflow:-1`. Следовательно, нормализационный mismatch — inherited, а исчезновение этого feature из принятого состояния — реальная clean→current граница фильтрации.

### Цепь до Import / Load: это отказ, не тихое удаление

[ImportCheckpoint L651–652](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L651-L652) → [ImportCheckpointCore L731–733](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L731-L733) → [BuildLearnedWeightImport L2752–2764](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2752-L2764) → [StageLearnedWeights L2779–2786](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2779-L2786): любой `!IsValidLearnedWeight` даёт `CHECKPOINT_LEARNED_WEIGHTS_INVALID`. Это происходит **до** mutation состояния L738.

File load также распространяет ошибку: [Load core L1783–1825](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L1783-L1825) читает/десериализует snapshot, проверяет policy payload/scorer, затем вызывает `ImportCheckpointCore` и возвращает отказ. [Public ImportAsync L568–570](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L568-L570) передаёт Load failure наружу.

Граница: пример предполагает иначе валидный checkpoint, прошедший shape/version/runtime-unit/policy/numeric проверки. Для no-policy checkpoint payload loader не должен создавать другую проблему; реальный файл здесь не загружался. Общий CheckpointSchemaVersion остаётся 1: clean L25, current L36. Потому само равенство schema не доказывает совместимость допустимых features.

Другие методы имеют отличную семантику: [ExportLearnedWeights L2722–2732](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2722-L2732) фильтрует невалидные entries; [counts L2817–2828](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2817-L2828) их не считает. Это не означает, что обычный import тихо пропустит неверный feature: импорт отклоняет такой snapshot целиком.

### Допустимый вывод и маленький опыт

Классификация DC-01b должна стать **MIXED: inherited normalization mismatch + current feature-admission change**. Новое последствие import — потенциальная несовместимость конкретного старого feature-state, не уже доказанная причина провала RP01. Clean runtime тоже строит generic key с дефисами ([L4373–4386](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L4373-L4386)); поэтому его сохранённый underscore сам по себе не давал положительного generic runtime selection. Восстановление может быть полезно из-за отказа всего checkpoint, включая другие полезные weights, но нужны настоящий artifact и ordinary consumer.

Сохранять current allowlist. Проверять canonical identity и при необходимости узкую версионированную migration, не возврат общего denylist. Контроли: правильный canonical feature; недопустимый чужой feature остаётся отвергнутым; **одновременные hyphen и underscore**. Последняя коллизия возможна у sparse clean payload с phantom; простое alias создаст duplicate FeatureKey, который current также отвергает L2782. Веса/counts и provenance нельзя молча суммировать без определённой семантики.

Никакого native Import/Load, реального checkpoint-migration или quality gain этим source review не выполнено. Самого архивного DCM checkpoint с underscore feature не найдено: наличие триггера в реальном неудачном RP01 run **NOT_OBSERVED**. Для restore-пилота сначала надо установить наличие такого feature в настоящем исходном artifact.

Обновлённый root probe прочитан; проверен только новый cross-branch boundary, без повторения старых осей. Три exact blob проверки прошли: current Machine `947383043f9b40c352bdf28ab492febc2ed7ca5e`, current TrainedUnits `3ef9f245d70df2c6dee673f7b9af5a15b3584701`, clean Machine `bad9b74f153afdd00ca6c7d42e185e312b9634a3`. Результат `audit/notes/dcm-cross-branch-boundary-independent.json`: **PASS**. Clean explicit-all-fields допускает underscore:+1; current исключает; sparse clean содержит обе формы. Known spacing и canonical hyphen допускаются обоими; review_priority отвергается обоими. Source-equivalence функции allowed_feature подтверждена на указанных ASCII ключах. SHA256 проверенного скрипта `c8779e41a0461a65488a8630e3aa164a5426ad00cd99bdc18df9c1a7f2c269d7`.

## F2 — малая правка ссылки на Backward

В главе 03 current DpoTrainer ссылка `L1038–1119` включает знак и scale, но заканчивается перед `logits.Backward(logits.Grad)` на **L1120**. Рекомендуемый range `L1038–1121`. Clean ссылка корректно включает Backward на L478. Это неточность доказательной ссылки, не дефект DPO.

## Проверенные пункты без новых замечаний

| Пункт | Результат |
|---|---|
| Шесть разобранных mechanisms ≠ шесть новых доказанных причин RP01 | После свежего dedup README различает четыре семьи bounded novelty и два известных подтверждения; область активации/качества ограничена. Inheritance F1 исправлена |
| Sign / response masks / LoRA RETAIN | Правильно: exact response masks, sum objective, `oneHot-softmax`, Backward и LoRA forward не требуют отката. Матрица «одинаковый blob» не объявлена native parity |
| KU-01 frozen mappings | Предыдущее существенное замечание учтено в 02, 03, 04: заморожены chosen/WordMappings/PhraseMappings, новый word-map mediator отделён |
| Source chain DC | Admitted pipeline → feature preparation → accumulator → runtime improver/selection отражены; no-bridge численная граница сохранена |
| Source chain KU | Правильная ссылка Adapter124–139; no-concept/no-PAIR/early-return условия сохранены; текущий SourceLaw не выдан за прежний неизменный KNN |
| C0 | Правильно описан как тот же parent с тем же load/eval и без update. Drift P/C0 не выдаётся за learning; parent/teacher/scorer не смешаны с C0 |
| Однофакторность | DC-01 2×2, DC-02 same-context negative, DC-03 compatible cardinality, KU-02 gold diagnostic bound, KU-03 surface-only gender отделены; controls и wrong-accept метрики есть |
| Роли настоящего DPO | CODEX Astra 6 ultracode выбирает/независимо оценивает; внешняя открытая китайская модель пишет обе финальные стороны; receipts/model availability не выдуманы |
| Цена/масштаб | Размеры диагностические, никаких 95% product claims из n=12; 1–4 update вместо большого обучения, native NOT_RUN явно сохранён |

## R4 и завершённость сборки

При первом чтении 00 L107 ещё советовал сравнить new-only с 70/30 без достаточного разграничения исторического bigram rebuild и нынешнего once-only. При повторном чтении строка исправлена: главный memory contrast сохраняет/заменяет old records с фиксированным new material; архивный 70/30 отделён как bigram reproduction, для neural повторов требуется самостоятельный допустимый режим. **R4 RESOLVED.** Глава 04 также разделяет эти режимы правильно.

README ссылается на 05/06, которые при начале review ещё создавались; позже они появились. Их промежуточное отсутствие не объявляется ошибкой остальных относительных repository links. Содержательное чтение 05/06 не входит в этот delegated review.

## Окончательная сверка исправлений

Повторно прочитаны изменённые README, 01 DC-01b/DC-01b-R, 03 ссылка Backward и статусы DC, 04 restore panel/inheritance, 00 начало restore contrast и R4.

- **F1 RESOLVED:** blanket inheritance удалён. DC-01b имеет точное clean→current отличие allowlist и отказ всего otherwise-valid import. Не заявлено наличие такого archive, выполненный native load или visual regression; denylist rollback исключён. Одновременно актуализирована новизна: DC-02/03 признаны известными по параллельному пакету54; DC-01 представлен как узкое уточнение с кредитом прежнему whitelist-наблюдению.
- **F2 RESOLVED:** current DpoTrainer ссылка расширена до L1121, включая реальный Backward на L1120.
- **R4 RESOLVED:** 70/30 явно относится к архивному bigram эксперименту; текущий memory contrast и условия neural replay разделены.
- **Новый boundary повторён только один раз:** `dcm-cross-branch-boundary-independent.json` подтверждает clean/current allowed deltas и controls. Старыe механические axes заново не запускались.
- **Mapping/C0/RETAIN сохранены:** исправление input/encoding не названо C0; C0 остаётся no-update load/eval. Узкий restore refusal относится к совместимости и не превращается в learning-quality claim. KU01 фиксирует полные mapping payloads, neural graph/sign/masks/LoRA не откатываются.

Новых блокирующих фактических замечаний в проверенных главах не осталось. Это разрешение считать данную независимую смысловую проверку завершённой, а не подтверждение product-ready алгоритма или общего качества модели.
