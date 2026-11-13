# Сопоставление DesignToCode-clean: две конкретные находки и границы восстановления

Дата: 19 октября 2026. Область этого пакета — `DesignConversionMachine`, её checkpoint, передача условия задачи обученной DPO policy и контроль переноса выводов на другие Units. Это ограниченный разбор исходников, а не полный аудит всех доменов.

**Основной результат:** найден прямой разрыв совместимости сохранённого состояния из `DesignToCode-clean`: допустимый ранее feature `source_manifest:hash_diversity` теперь делает недопустимым весь checkpoint. Отдельно найдено, что обучение/held-out DPO policy и обычный runtime получают разные условия: при обучении — source-vector signal, в runtime — идентификаторы задачи и хранения. Устранение этих швов следует проверять независимо. Ни один из них пока не доказан причиной неудачи RP01 C#/EN→RU.

Зафиксированные снимки:

| Обозначение | Репозиторий / ветка | Commit |
|---|---|---|
| CLEAN | `lubochka/llm_mvp_core`, `DesignToCode-clean` | `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` |
| CURRENT | `lubochka/llm_mvp_core`, `codex/dpo-implementation` | `358af351e22a92a3f7938d1f9221d5aeb7ddb053` |

Основные C# исходники читались по этим SHA; прежние исследования дополнительно сверены с указанными ниже pinned planning commits. Core research `docs/plans/replanning-possible-diffs` не изменялся. Внешние модели не вызывались. CODEX с Astra 6 ultracode выполняет делегированный человеческий выбор обучающего случая и отдельно оценку точной DPO-пары; внешняя разрешённая открытая китайская модель создаёт обе стороны, chosen и rejected. Самостоятельный review этой записи указанной моделью не заявляется.

## 1. CLEAN-CP01 — приоритет восстановления CLEAN: один устаревший feature блокирует всё прежнее состояние

### Что происходит

В CLEAN обычное обучение `DesignConversionMachine` сохраняет несколько feature weights. Помимо внутренних векторных признаков, старый builder неизменно добавляет `source_manifest:hash_diversity`, если JSON-пара проходит все прежние проверки, включая непустой unit_id и visual_score обеих сторон, и имеет source hashes. CURRENT ввёл точный whitelist допустимых semantic features и исключил этот provenance surrogate. Но importer требует, чтобы **каждый** сохранённый вес соответствовал новому whitelist. Поэтому один старый feature блокирует импорт также и остальных, ещё допустимых весов.

Это **подтверждённая по исходникам несовместимость old→current**, а конкретный отказ native `ImportCheckpoint` предсказан, но здесь не исполнялся. Это состояние локальных feature weights; утверждение не относится автоматически к LoRA tensor checkpoint.

### Доказательства и полный путь

1. [CLEAN Machine, строки 2771–2815](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L2771-L2815): builder проверяет JSON стороны и source hashes, затем добавляет `source_manifest:hash_diversity`.
2. [CLEAN Machine, строки 2664–2673](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L2664-L2673): прежняя проверка запрещала отдельные score/usefulness keys и допускала этот feature.
3. [CLEAN тест, строки 85–105](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/Tests/DesignConversionMachineTests.cs#L85-L105) прямо ожидает этот вес после `TrainBatchAsync`. [CLEAN helper, строки 2239–2264](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/Tests/DesignConversionMachineTests.cs#L2239-L2264) выполняет цепь `TrainBatchAsync → SaveCheckpointAsync → LoadCheckpointAsync → RunMachineScoreAsync` и читает этот feature. Это **исходник теста**, не наш результат его исполнения.
4. [CURRENT Machine, строки 2752–2786](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2752-L2786): `BuildLearnedWeightImport → StageLearnedWeights`; условие `weights.Any(weight => !IsValidLearnedWeight(weight))` возвращает `CHECKPOINT_LEARNED_WEIGHTS_INVALID` до принятия состояния.
5. [CURRENT Machine, строки 2830–2855](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2830-L2855): `IsValidLearnedWeight → IsAllowedLearnedWeightFeatureKey`; whitelist включает vector mean/dimension, source kinds и correction categories, но не `source_manifest:hash_diversity` и не старые `pair_family:*`.

Причинная цепочка: допустимый CLEAN урок → несколько сохранённых весов, включая старый provenance feature → current fresh load → отказ whole checkpoint → обученный получатель недоступен. Последующий фактический fallback consumer надо фиксировать отдельно, а не предполагать.

### Что нельзя возвращать

Простое расширение runtime whitelist вернуло бы источник возможной утечки: число/разнообразие source hashes не означает качество HTML/CSS. Старые test PASS и `AccuracyAfter` нельзя автоматически сохранять как независимую semantic quality. Старый [Pass150 report, строки 1–9, 78–92](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/docs/evidence/design-conversion/codex-pass150-real-training-noop-closure.md#L1-L92) прямо ограничивает доказательство локальным deterministic training-state path; кроме того, его исторический `visual:rejected_score_inverse` уже отсутствует в актуальном CLEAN whitelist. Это пример того, почему report-era state и branch HEAD должны сверяться отдельно.

В дереве `docs/evidence/design-conversion` просмотрено 309 entries, включая artifacts, а также CLEAN root models. Найден [producer manifest](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/docs/evidence/design-conversion/artifacts/design-conversion-real-checkpoint.producer-manifest.json), утверждающий capture checkpoint `cf05d9f6…` и local fresh load. Он содержит временный relative locator и 11 weights, **но не байты весов**. Полный архивный checkpoint с нужным feature в просмотренной области **не найден**. Нельзя считать manifest доказательством наличия именно этого key в тех 11 weights.

### Конкретный вариант восстановления и малый опыт

Добавить отдельный offline `DesignConversionLegacyFeatureCheckpointMigrator` и typed `DesignConversionFeatureMigrationReceipt`. Он принимает pinned CLEAN checkpoint только после полной проверки прежней схемы/хеша/чисел/уникальности keys. Он записывает новый checkpoint вместе с parent hash и перечнем retired keys; сохраняет без изменения Unit IDs, значения/счётчики допустимых features, training history, полное прочее состояние и явную связь версий. Старые evidence-only features архивируются в receipt, не подаются runtime. Незнакомые keys, неверные числа, дубликаты и поддельная lineage продолжают приводить к отказу. Новый checkpoint получает собственную версию и manifest; старые measured-quality/promotion claims не наследуются без повторной проверки.

Первый native опыт ограничен импортом и cold restore; ориентир 30–60 минут после получения валидного исходного артефакта и готового local test environment. Время поиска архивного checkpoint, написания мигратора и отдельной панели качества в эту оценку не входит:

- Получить один реальный CLEAN checkpoint. Если его нет, отдельным будущим шагом снять fixture checkpoint через существующую цепь `BuildPass150Batch → TrainBatchAsync → SaveCheckpointAsync` на pinned CLEAN; такой артефакт проверяет воспроизводимую совместимость и не заменяет состояние успешной пользовательской сессии.
- A: импорт исходных bytes в CURRENT. Ожидается `CHECKPOINT_LEARNED_WEIGHTS_INVALID` при прохождении остальных guards.
- B: импорт versioned migration с сохранёнными допустимыми весами. Сравнить их content, Unit IDs, history и output после cold restart.
- Контроли: уже современный checkpoint неизменен; `unrelated:injected`, NaN, duplicate feature и несовпадающий parent hash отвергаются. Не подделывать checksum старого checkpoint и не переименовывать его как исходный.
- Панель качества: 12–24 отдельных CSS/layout случаев на spacing, overflow, typography, contrast; source-family split. Независимо оценить wrong accepts, верно выбранную correction category и доступность ранее обученных правил. Сравнивать B с чистым нетренированным current и old reference только по допустимому предметному поведению.

Успешный импорт доказывает восстановление совместимости. Полезность обучения требует отдельного улучшения на этой панели. До неё статус `SOURCE_CONFIRMED_COMPATIBILITY_GAP; QUALITY_EFFECT_NOT_MEASURED`.

## 2. CLEAN-CTX01 — новая конкретизация: DPO policy в runtime теряет условие, которое получала при обучении

### Подтверждённое несовпадение

Обучение и held-out bridge строят prompt вида `srcenc vecp=... schema=... sig=... fmap=... vecn=... guard=... unit=...`. Здесь есть нормализованное описание source encoder и первые восемь координат source vector. Обычный runtime scorer получает `design-conversion-runtime-pair root=... attempt=... generated=... incumbent=...`. Это другая строка: source vector, структура и unit отсутствуют; присутствуют идентификаторы владения и хранения.

Прямые источники:

- [CURRENT PromotionArtifactContracts, строки 539–592](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoPromotionArtifactContracts.cs#L539-L592): `Normalize` и `BuildPolicyPrompt` строят source prompt.
- [CURRENT PolicyCheckpointBridge, строки 6554–6603](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoPolicyCheckpointBridge.cs#L6554-L6603): этот prompt реально используется в evaluated packed batch и pair admission.
- [CURRENT RuntimePair, строки 53–79](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoPolicyRuntimePair.cs#L53-L79): другой `BuildPolicyPrompt`, состоящий из runtime identities.
- [CURRENT coordinator, строки 1279–1386](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs#L1279-L1386): independent receipt → runtime pair factory → `ApplyDpoPolicyRuntimePairAsync`.
- [CURRENT Machine, строки 3861–3926](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3861-L3926): scorer получает точные HTML/CSS стороны, но передаёт pair-specific identity prompt в реальные Forward/log-prob вычисления.
- [CURRENT bridge, строки 6506–6539, 6652–6681](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoPolicyCheckpointBridge.cs#L6506-L6681): same byte encoder и same windowing не обеспечивают same conditioning; prompt занимает `max(1, contextLength / 3)` ведущих bytes.

**Самый короткий фальсификатор, выведенный из исходника:** при `ContextLen=64` runtime prompt обрезается до первых 21 bytes постоянного префикса `design-conversion-run`, тогда как два учебных prompt с различными первыми source-vector координатами могут ещё различаться. Это предсказание по формуле windowing, не результат native .NET или модели. При `ContextLen=128` часть root identity уже попадает в prompt, но source signal всё равно отсутствует. Нельзя объявлять 64 фактической конфигурацией пользовательского run без его manifest.

### Исторический контраргумент

[CLEAN Machine, строки 3604–3652 и 3724–3799](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionMachine.cs#L3604-L3799) тоже использовал отдельный runtime prompt с hashes/baseline score и сравнивал synthetic preferred/baseline. [CLEAN source encoder, строки 514–571](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionDpoPromotionArtifactContracts.cs#L514-L571) строил `srcenc` prompt, дополненный provenance/usefulness. Поэтому нельзя назвать проблему возникшей только после CLEAN или предлагать полный откат runtime. CURRENT исправил реальные HTML/CSS стороны и убрал часть ошибочных training signals; оставшееся несовпадение условия — самостоятельный объект опыта.

Свежий peer [в разделе6 своего DesignConversion audit](https://github.com/lubochka/xiigen-planning/blob/6b97c01deecc0382e1a5b5c68b10d520a483c1f2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-designtocode-clean-causal-audit/02-NEW-LEARNED-DESIGN-STATE.md#L144-L158) уже описывает synthetic→real candidate sides, наличие валидной runtime pair и достижимость policy forward. Это ближайший предшественник по route. CLEAN-CTX01 дополнительно исследует **содержимое условия** train/heldout и runtime, которого это сравнение сторон само по себе не охватывает. [Последняя сверка новизны](evidence/PUBLICATION-RECONCILIATION.md).

### Конкретный опыт

Создать внутри разрешённого эксперимента общий `DesignConversionPolicyCondition` и `DesignConversionPolicyConditionEncoder`. Они берут реальные source structure/vector, доступные и training record, и coordinator-owned runtime source; выход одинаковой схемы применяется в обоих местах. Root/custody/receipt IDs сохраняются для admission и evidence, не входят в обучающий prompt. Нельзя при runtime выдавать gold `chosen`, post-hoc correction labels или baseline score за исходное условие.

Два независимых evaluator/model экземпляра из одного current checkpoint, одни exact пары HTML/CSS и одна токенизация. A — нынешний runtime prompt. B — общий source-signal encoder без изменения весов и DPO objective. 12–24 source-disjoint макета: два разных требуемых расположения тех же элементов, одинаковые candidate sides с противоположным правильным выбором под разными исходными макетами. Сначала небольшая component-проверка prompt contract отделяет bytes/windowing от ordinary runtime; это не доказательство принятия кандидата штатным consumer. Каждый arm следующего ordinary-runtime A/B получает отдельно выданную валидную pair authority: уже consumed capability повторно не используется.

Identity-контроль выполняется через независимо выданные валидные пары с одинаковыми semantic inputs и согласованными собственными root/custody bindings. Запрещены ручное переименование подписанного receipt и перенос authority между разными source cases. Каждый case остаётся immutable, root-bound guards сохраняются. Другие контроли — matched repeated checkpoint state и явный unknown при отсутствии требуемого source signal. Первая проверка уже реализованного общего encoder ориентирована на 30–60 минут в готовом окружении; создание encoder и слепая оценка полной панели планируются отдельно.

Сохранять prompt bytes/IDs, response windows/masks, checkpoint hash, выбранную сторону, independent correctness и число неправильных принятий. Если нужен neural lift, затем отдельный matched train run с фиксированными seed/LoRA/base/rows, где меняется только contract conditioning. Кодовая достижимость общего encoder и отсутствие label/provenance leakage проверяются до интерпретации качества.

Статус `SOURCE_CONFIRMED_TRAIN_RUNTIME_CONDITION_DIFFERENCE; QUALITY_EFFECT_NOT_MEASURED`. Это профиль Figma/Canva→HTML/CSS. Перенос вывода на RP01 C# documentation или EN→RU требует их собственного caller trace.

## 3. Проверки, которые не позволили записать старые или неверные гипотезы как новые

1. `TranslationTrainedUnits.cs` имеет одинаковый blob `1742abfc9fb617dfc66e4d618e0c169da01a6ccc` в CLEAN/CURRENT. Его KNN/E26/E27 проблемы могут быть наследуемыми, но не являются чистой source regression двух этих снимков. В `TranslationUnitContracts` CURRENT дополнительно применяет `TranslationMemorySourceLaw`; это отдельная дельта.
2. 4→6 translation executors и shared directionless KNN действительно видны в source, но уже описаны в planning N41 и N16. Повторная новизна не заявляется.
3. Внешне привлекательная гипотеза «DTC train использует sum, runtime mean» **не подтверждена** для проверенного evaluated bridge. [`DpoBatch.MaterialVersion`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/11.DPO/Models/DpoBatch.cs) по умолчанию `legacy-full-sequence-mean-v1`; [DTC builder](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoPolicyCheckpointBridge.cs#L6554-L6603) выставляет только IDs/masks/Admission. [`DpoExactPairMaterial.CaptureAndValidate`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/11.DPO/Models/DpoExactPairMaterial.cs#L94-L113) сохраняет legacy version. В [`DpoTrainer`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/11.DPO/DpoTrainer.cs#L1000-L1016) sum включается только для `segmented-byte-BPE-v2`. Следовательно, исправление exact CodeDocumentation objective нельзя автоматически переносить на DTC consumer. Это полезный контраргумент, а не третья выдуманная регрессия.
4. Проверены 702 первоначальных текстовых документа: 416 из planning `b0308fbaa783758104987e6544d057062ccf6fc3` и 286 из read-only core `358af351e22a92a3f7938d1f9221d5aeb7ddb053`, затем 32 новые/изменённые Markdown-версии planning на `806d1c8534f9676cd80e7c1bf3c488163e549a4d`. Перед публикацией также сверены ещё 9 изменённых Markdown-версий на `b95278b25a6697b42ef7025e6965a9f2837a34b7`. Ошибок получения нет. Свежий peer [54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW](https://github.com/lubochka/xiigen-planning/blob/806d1c8534f9676cd80e7c1bf3c488163e549a4d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW-2026-10-19.md#L12-L23) уже описал удаление hash-diversity feature и разные наборы hashes при train/inference. **Удаление признака не является нашей новой находкой.** Дополнительный вклад CLEAN-CP01 — exact whole-checkpoint rejection через StageLearnedWeights и явная миграция остальных допустимых весов. Для CLEAN-CTX01 прежнего описания конкретной цепочки source encoder → RuntimePair → runtime scorer не найдено. Смысловые соседи P123, core64, N52, canonical prefix16 и PromptToTask рассмотрены отдельно. Полная attribution и ограничения новизны — в [05](05-EVIDENCE-NOVELTY-AND-REVIEW.md). Новизна относится к конкретной source-backed цепочке в этих pinned корпусах, а не ко всем веткам и неизвестным сообщениям проекта.

## Связанные документы и фактические выполненные действия

Этот разбор продолжает [выделение исторического baseline](01-HISTORICAL-BASELINE-AND-RECOVERY.md); общая карта механизмов и граница exact neural path — в [03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md](03-MECHANISM-COVERAGE-AND-NEURAL-PARITY.md), задания для доменных запусков — в [04-DOMAIN-MICRO-EXPERIMENTS.md](04-DOMAIN-MICRO-EXPERIMENTS.md).

Выполнены pin-specific source retrieval, semantic diff, адресные searches по доступному corpus, чтение P123 и просмотр CLEAN evidence tree. Native .NET, обучение, внешнее DPO generation, CODEX/Astra judging, визуальный replay и публикация этим worker не выполнялись. До получения актуального ограничения `session/AGENTS.md` на создание runners worker успел выполнить одну локальную механическую Python-проверку membership/string truncation без обращения к модели. Она изолирована и не используется как допустимое доказательство эксперимента; документы опираются на приведённые C# spans. Новый код опыта не входит в пакет. Все описанные выше native A/B имеют статус **NOT_RUN**.

Применены смысловые разделы `architect-behavior-classifier-SKILL.md` и `trainable-architecture-decision-SKILL.md`: реальные call chains, отделение deterministic helper от model component, lifecycle, counterevidence, ограничение claims и конкретный falsifier. Их историческое ограничение роли closed judge не перекрывает прямое текущее указание пользователя о CODEX selection/evaluation.
