# 19 — MICRO-CONSUMER-01: быстрый запуск для проверки, что обучаемый KNN меняет качество ответа, а не только PASS

**Дата:** 16.10.2026. **Стадия:** исполнимый work order/experimental specification, **НЕ выполнено**. Гипотезы: [18 — обход KNN в GenerateAsync](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), [16 — encoder/index-version drift](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md). Не повторяет многочисленные тесты функции KNN без реального training contrast.

## Почему этот опыт первый

Для RP01 остаются два отдельных вопроса: (1) способен ли обученный **Unit** улучшить качество ответа на новом входе; (2) используется ли его learned output в пользовательском consumer. Не начинать с 500 DPO, новой LoRA-архитектуры или Windows Actions. Сначала получить измеримый позитивный learning signal на короткой памяти и доказать, что он влияет на выбранный ответ. **Если signal не появился**, исправлять learner/data; **если появился только в Unit**, исправлять/разделять consumer contract. Нельзя называть изменение декодирования обучением весов.

## Исполнительский протокол: один мини-запуск

**Источники:** точный текущий `llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`. Использовать **готовые, уже опубликованные** [8 новых train и 8 source-disjoint heldout](15-E51A-SOURCE-DISJOINT-LEARNING-FIXTURE.json), не копировать метки из heldout в training/ranking. Их `sourceHash=HASH_AT_RUN` — placeholder; вычислить фактический `SHA256(codeContext)`, проверить уникальность `train∩heldout=∅`. Это **same-family/different-source**, не unseen-law generalization. Подтвердить предметные labels отдельно (CODEX/Astra либо независимый human); пока не подтверждены, результаты DEV_ONLY.

**Обучаемый Unit:** [KnnLogicalUnit<string,string[]>](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L8-L29). Вызвать `Train` на 8 записях:
- `Input = prompt + "\n" + codeContext`;
- `Output = requiredPredicates[]` только для **train**;
- `PredicateIds=[]`: не подставлять target labels в query metadata (это data leakage);
- `DomainTags=[domain]`, `EvidenceIds=[sourceHash]`; уникальные stable `ExampleId`;
- `textSelector=input=>input`; настройки `vectorDimensions=64, k=3, confidenceFloor=0.70` — оставить неизменными.

**Тест:** в каждый `LogicalUnitRequest<string>` передавать только `heldout.prompt+"\n"+heldout.codeContext`, пустую metadata и flags без oracle; scorer читает `heldout.requiredPredicates` и `forbiddenPredicates` **после получения output**, вне исполнения Unit.

**Режимы для тех же H01–H08**, одинаковая входная строка, tokenizer/index topology fixed там, где возможно:

| Режим | Что запускаем | Зачем |
| --- | --- | --- |
| A / RULE | Frozen deterministic rule-only baseline, который извлекает предикаты из input `codeContext` без learned labels | Контроль «систему спасли правила, а не обучение» |
| B / BAD-LABEL | Тот же `KnnLogicalUnit.Train` с теми же 8 train inputs, но output labels циклически переставлены между train ID (N01→N02→...→N08→N01) | Отрицательный контроль запоминания меток |
| C / LEARNED | Тот же `KnnLogicalUnit.Train`, только корректные train outputs | Изолированное обучение памяти, без LoRA |
| D / CONSUMER-SHADOW | Frozen ordinary generator result + `C.ExecuteAsync` и явная law-aware композиция. Идентичный shadow с `B.ExecuteAsync` как negative control | Дошло ли именно выученное содержимое до финального ответа |

**Важно для контроля:** обучение B и C заново строит HNSW с собственным статическим RNG, что может менять candidate set. Для **основного причинного сравнения** сначала выполнить C, сохранить его `hits` и выполнить два чистых label-resolution прохода на **одних и тех же hit IDs/ranks/scores**, используя output map C vs B; это строго one-factor. Отдельно повторить реальные B/C `Train` и сравнить ranks; вывод `KNN learning gain` без стабильного hit pool осторожный, поскольку HNSW-кандидаты могли различаться.

**D / consumer:** не менять `GenerateAsync` в production. Shadow adaptor должен передать `C.Output` явно (trace: `actual_unit_invoked=true; hit_id; output_sha; selected_answer_origin`) в итог. В режиме D_BAD все те же условия, только shuffled output map. Утверждение об uplift возможно лишь для endpoint, который действительно использует результат Unit; `GenerateAsync` без композиции по исходному коду не делает этого.

## Измерения по каждому из 8 случаев

Для предсказанных `P`, заранее одобренных `required` и `forbidden`:
- `law_correct = required ⊆ P && forbidden ∩ P = ∅` (строго, без partial credit);
- `required_recall`, `forbidden_false_positive`, доля abstain, `correct_when_answered`, coverage;
- `B/C output`, точные `hit_ids`/scores и `selected_answer_origin` для D;
- source SHA, frozen generator checkpoint SHA, tokenizer SHA, train example IDs, seed, ms и RAM peak.

**Главный счёт:** `law_correct_C/8` против `law_correct_B/8` **при одних и тех же hit IDs**; отдельно `law_correct_D_LEARNED/8` против `D_BAD/8` (реальный final consumer). Условно интересный результат: C ≥ B+2 правильных из 8, D_LEARNED ≥ D_BAD+2 и отсутствие дополнительных forbidden output. Это лишь дешёвый **exploratory signal**, не статистическое доказательство; следующим шагом проверять 20+ задач из новых code families и затем другие домены.

## Реально минимальная последовательность действий

1. **Подготовка:** загрузить JSON, снять current repo/commit/weights/tokenizer, hash source и проверить `train∩heldout=∅`. Подтвердить независимость evaluator от Unit; не принимать финальные labels до review.
2. **Код:** написать один изолированный runner `MicroConsumer01` с `KnnLogicalUnit.Train`, `ExecuteAsync`, fixed-hit resolver и JSON readout. **Не править продукционный KNN/LoRA, не менять план RP01.**
3. **Опыт:** запустить B→C→D на H01–H08; тот же seed/topology; публиковать все 8 строк, а не общий `PASS`. Проверить правила безопасности и точные исходные метки. При необходимости выполнить read-only `GenerateAsync` контроль с тем же checkpoint/seed.
4. **Анализ:** если C==B → `NO_GAIN_ON_THIS_PANEL`; если C>B, но D не меняется → `CONSUMER_ADOPTION_GAP`; если D_LEARNED>D_BAD с трассой → `LEARNED_MEMORY_IMPACTS_USER_ANSWER`. Никакой из этих выводов не доказывает, что DPO+LoRA сама научилась переводить или писать код.
5. **Публикация:** сохранить рядом новый `MICRO-CONSUMER-01-RESULTS.json` только **после запуска**: `status`, inputs SHA, per-row baseline/shuffled/learned/served, counts, individual failures, exact command, executable commit, outcome, independent review. При отсутствии dotnet/доступа к harness: `NOT_RUN`, нельзя выдавать будущую таблицу за измерение.

**Лимит:** 8 labeled train + 8 heldout, одна точечная compile/run, 0 DPO optimizer, 0 Windows CI, 0 обязательных широких tests. Входной JSON уже существует. В отличие от старого E51B oracle-подстановки, запрещено читать `heldout.requiredPredicates` до финального результата.

## Промпт исполнителю (один запуск, без бесконечной проверки)

> В ветке core `codex/dpo-implementation` создай локальный MICRO-CONSUMER-01 runner по этому протоколу. Читай реальные `KnnLogicalUnit`, `LogicalUnitRequest`, source-disjoint fixture, actual caller; не исправляй learner до эксперимента. Выведи per-case B/C/final D JSON и ровно 4 verdict: `LEARNING_EFFECT`, `CONSUMER_EFFECT`, `NO_GAIN`, `INCONCLUSIVE`. Один фактор — labels при frozen ranked hits, fixed policy SHA. Не запускай GitHub Windows Actions, main500, all tests. Independent reviewer проверяет входную слепоту, отсутствие oracle leakage и фактический invoked route. Если среда заблокирована — документируй blocker и точную команду для местного запуска, не подменяй прогон тестовым PASS.

**Статус на 16.10:** фикстура существует, источники API изучены, runner **ещё не реализован**, эксперимент и независимое review **не проводились**. Это work order, не свидетельство причинного исправления.
