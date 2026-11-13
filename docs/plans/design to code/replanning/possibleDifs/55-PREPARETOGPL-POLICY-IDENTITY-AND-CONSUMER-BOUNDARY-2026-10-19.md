# N55 — новая проверяемая дельта: политика выбора ответа не входит в идентичность обученной памяти

**Дата:** 19.10.2026. **Режим:** прямое чтение двух Git refs и конкретных вызывающих методов; **новых native .NET training/inference run нет**. GOLD: [PrepareToGPL @31346d8](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2); CURRENT: [codex/dpo-implementation @6d36492](https://github.com/lubochka/llm_mvp_core/tree/6d36492714ebc1b9ae5f31a5a1da00673f524d83). Защищённые core docs **READ ONLY**. Результат исследования — **условная причинная гипотеза**, а не установленный факт деградации RP01.

## 0. Что действительно новое, а что уже было известно

**Не новая находка:** в current появился закон фильтрации противоположной полярности простой английской copular-clause; до этого N54 уже описал [`TranslationMemorySourceLaw`](54-DESIGNTOCODE-CLEAN-DIRECT-DIFFS-AND-NOVEL-BOUNDARIES-2026-10-19.md) и правильный порядок «veto до cap». У N35 ранее отмечен риск пропуска отрицания через word-map. Это **не дублировать и не присваивать**.

**Новая исследуемая комбинация N55-P1:** код политики выбора ответа и версия этого фильтра не являются частью полного сохранённого `TranslationMachineCheckpoint` или `TranslationModelManifest`. Поэтому одинаковый checkpoint/хеш/объём памяти **не гарантирует одинаковый ответ между ветками**. Меняется *потребитель обученного состояния*, а не обязательно результат обучения. Это особенно важно для сравнения GOLD→CURRENT и трактовки восстановления.

**Новая конкретизация N55-P2:** новый запрет применяется в `TranslationKnnStore.FindExactWithSourceGuard`, тогда как `ExecutorUnit.Execute` **сначала** обращается к точной, фразовой и словарной памяти. Для запроса, которому отвечает один из этих путей, новый filter kNN вообще не вызывается. Значит проверка «негативная кNN-пара отфильтрована» не доказывает, что обычный перевод не вернёт ту же смысловую ошибку другим путём. Это новая проверка композиции **нового** source-law с **прежними** learned paths; сам риск word-map-omission — известный N35.

**Условная API-граница N55-P3:** публичный `TranslationKnnStore.FindNearest` [остался с возвратом exact bucket без source-law](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationUnitContracts.cs#L219-L239), в отличие от `FindNearestCascade`. Прямой производственный caller публичного API **не установлен**; в изученном `ExecutorUnit` вызывается Cascade, так что не заявлять production bypass без доказательства consumer.

## 1. Прямая цепь доказательств

| Шаг | GOLD / CURRENT | Что подтверждает, и чего НЕ подтверждает |
|---|---|---|
| P1-A | [GOLD FindExactWithSourceGuard](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationUnitContracts.cs#L268-L287) → [CURRENT](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationUnitContracts.cs#L268-L292) | CURRENT вставляет `Where(pair => TranslationMemorySourceLaw.Compare(...) != Contradiction)` ДО `Take(k)`. Это реальный delta в поведении поиска при подходящих source |
| P1-B | [CURRENT source-law](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMemorySourceLaw.cs#L12-L44) | Версия `en-simple-copular-polarity-v1`, закрытый набор слов, `is/was`, literal `not`; UNKNOWN допускается к следующему scorer. Не полный синтаксический анализ и не универсальная функция negation |
| P1-C | [Checkpoint fields](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationCheckpointContracts.cs#L20-L67) / [model-manifest contract](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationModelManifestContract.cs#L10-L113) | Они включают schema/version, learned KNN entries, identity/manifest-поля, но `TranslationMemorySourceLaw.PolicyVersion` в этих контрактах **не фигурирует**. Это source-contract факт; внешняя telemetry теоретически может хранить code SHA отдельно |
| P2-A | [Unchanged trained Unit blob](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L355) = [CURRENT](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L355) (git blob SHA `1742abfc9fb617dfc66e4d618e0c169da01a6ccc`) | `Execute`: exact source → phrase-map → word-map → KNN. Ранний return блокирует вызов новой law. Веточный regression внутри этого Unit **не подтверждён** |
| P2-B | [Current KNN call](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L362) | В исследованном ordinary Executor используется `FindNearestCascade` с source-текстом, не прямой `FindNearest`; для других callers требуется отдельный поиск |
| P3-A | [CURRENT public FindNearest](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationUnitContracts.cs#L219-L239) | На exact bucket возвращает `exact.Take(k)` без анализа source. Само существование public API ≠ доказательство реального использования в production |

### Сценарий, который должен быть проверен, а не принят на веру

- Один сохранённый разрешённый урок: `the box is open → коробка открыта`.
- Контрфакт: `the box is not open`; одна перемена — полярность. При равной структурной подписи GOLD может выдать старую пару через kNN, CURRENT может отфильтровать её. Предпочтительный результат — правильное отрицание либо явное воздержание, **не** «любой перевод вместо плохого».
- Второй контрфакт `the box isn't open` и вариант `no longer open` — новая narrow law не распознаёт их как тот же literal `not`. Это **граница регулярного правила**, а не доказательство неверного ответа у продукта.
- Третий контрфакт: предварительно обученная `phrase-map`/полное `word-map`, позволяющая ранний return; сравнить с тем же KNN состоянием без этих ранних путей. Это тест *routing/precedence*, не тест потери LoRA.

## 2. Чем это отличается от известных найденных причин

| Уже существующий материал | Граница, которую N55 не присваивает |
|---|---|
| [N35 — wordmap пропускает отрицание](35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md) | Сам false confidence уже описан. N55 спрашивает, помогает ли **новый** source-law реальному обычному ответу при этом precedence |
| [N27 — mutable checkpoint cache](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) | Другое явление: RAM mutable alias против committed disk. N55 — одинаковый сохранённый checkpoint, **разная версия политики inference** |
| [N41 — 4→6 executor checkpoint](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md) | Там loader может отказать до inference; N55 опыт выполняется **только после подтверждённой загрузки** в обеих ветках, иначе статус BLOCKED_NOT_COMPARABLE |
| [N54 — source-law protection](54-DESIGNTOCODE-CLEAN-DIRECT-DIFFS-AND-NOVEL-BOUNDARIES-2026-10-19.md) | Сам filter и pre-cap timing уже известны; N55 добавляет **модель идентичности политики** и явную multi-consumer проверку |

## 3. Где применимо

**Приоритетная тема:** перевод EN→RU, отдельно EN→HE если реальные данные/политика позволяют и есть валидные пары. `RU→EN` не считать идентичным случаем — здесь recognizer анализирует англоязычный source. **Неприменимо напрямую:** C# AST, SQL, документация, DPO/LoRA Student — без отдельного доказательства использования именно этого `TranslationKnnStore`. В этих доменах переносится *метод контроля policy-fingerprint и consumer precedence*, не английская regex.

**Статус доказательств:** SOURCE_DIFFERENCE P1, SOURCE_CONFIRMED_ORDER P2, CONDITIONAL_API_P3; `native outcome`, `semantic quality gain`, `RP01 root cause` — **NOT_RUN/NOT_PROVEN**.

**Короткие опыты и критерии фальсификации:** [N55 small domain experiments](55-PREPARETOGPL-GOLD-PARITY-SMALL-DOMAIN-EXPERIMENTS-2026-10-19.md). Исправление обсуждать только после matched native A/B: логировать `model_checkpoint_sha256` + `source_law_policy_version` + `code_commit` + `selected_adapter_id` как separate decision identity. Не добавлять скрытую модельную feature/вектор, если проблему объясняет смена consumer.
