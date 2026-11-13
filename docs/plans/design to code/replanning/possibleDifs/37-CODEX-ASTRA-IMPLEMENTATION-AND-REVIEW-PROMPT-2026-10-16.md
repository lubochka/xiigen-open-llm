# CODEX / Astra 6 UltraCode — ограниченный исполнительный запрос и независимая проверка N37

**16.10.2026. DOCUMENTARY HANDOFF ONLY.** Этот текст **не означает**, что CODEX получил задачу, выступил человеком в выборе, оценил DPO или запустил .NET: все эти действия `NOT_RUN`. Цель — дать одному исполнителю и второму независимому ревьюеру самостоятельный проверяемый контракт без переписывания всей программы.

## Точное поручение исполнителю

**Рабочие ссылки:**
- [Master register N37](37-MASTER-RP01-EVIDENCE-GAP-REGISTER-AND-DOMAIN-PRIORITIES-2026-10-16.md)
- [N37 mechanism/evidence](37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md)
- [MICRO37 procedure](37-MICRO-DOMAIN-KNN-METAMORPHIC-CAUSAL-PLAN-2026-10-16.md)
- [Current source TranslationUnitContracts](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationUnitContracts.cs#L241-L323)
- [Current source ExecutorUnit](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L363)
- [RAG Teacher roles](../../09.09.26WorkLeft-02-teaching-chinese-model.md)

**Инструкция CODEX исполнителю (вставлять как одно поручение):**

> Ты CODEX с моделью Astra 6 UltraCode. Используй архитектурный, planning, plan-review, generated-code-review, prompt-engineering, experiment-design и human-communication skills из `llm_mvp_core/docs/skills`. Зафиксируй core/planning HEAD SHA и pin всех исследуемых blobs.
>
> Твоя задача — **не** объявить RP01 исправленным, а получить **три независимых native falsifier** для N37-A (rule tag меняет threshold .35→0), N37-B (query-side coverage=1 для длинного кандидата с другими обязательствами), N37-C (full tier candidate rejected downstream, viable prefix tier не вызывается). Вывести route/hit/threshold/target-compatibility trace, совпадающий или несовпадающий с source-only прогнозом. Не скрывать отрицательные и неожиданные результаты.
>
> Ограничь native L0 одним малым тестовым executable и минимальными фикстурами. Ни 500 DPO, ни Windows CI, ни production refactor не являются входным gate. После подтверждения native branch обязательно докажи reachability из ordinary consumer; если route не достигнут, пометь `ROUTE_NOT_REACHED`. Для domain-quality используй frozen case IDs и source-family-disjoint split из MICRO37, независимые gold/rubric и seed/memory/checkpoint parity. Меняй только одну экспериментальную переменную на arm. Сравни correct law, wrong accepted, abstention, retention, time/PC budget.
>
> **Запрет:** НИКОГДА не создавать/изменять/удалять файл в `llm_mvp_core/docs/plans/replanning-possible-diffs`. Не делать несанкционированной замены production Student/Unit, чекпойнтов, генератора пар, evaluation labels, исторических run receipts. Всё новое документируй в `xiigen-planning@llm_plan3/llm mvp/design to code session/replanning/possibleDifs`. Временный локальный test harness не должен автоматически стать production.
>
> **Роли настоящего урока:** CODEX / Astra 6 UltraCode выполняет делегированную человеку работу отбора/разрешения law и case. Только разрешённая внешняя открытая китайская модель авторствует **обе финальные** DPO стороны chosen/rejected; CODEX **отдельно** проверяет качество точной пары, provenance, и отличия выбранного от отвергнутого. После шага Student отдельно и независимо CODEX / Astra 6 UltraCode оценивает реальные ordinary answers и retention. В L0/L1 новое DPO **не создаётся** — не изображай Chinese teacher calls.
>
> Сохраняй в каждом receipt: source code SHA; prior/new checkpoint and tokenizer/encoder/Unit hashes; selected/rejected pair fingerprints (только если пара реально создавалась), allowed model author provenance; selection/judgment receipts; exact query and ranked IDs/score; route; eval case ID and panel SHA; frozen judge rubric; prompt and output hash; duration/peak PC resources. Отдельно отмечай `SOURCE_PROVEN`, `NATIVE_CONFIRMED`, `STUDENT_QUALITY_IMPROVE`. Снижение количества ложных принятых с ростом отказов не равно гарантированному улучшению без правильных ответов.
>
> Выполняй публикацию `commit → независимая review → push → remote readback` только для разрешённой planning-ветки. Объективные outcomes: `IMPROVE`, `REGRESS`, `INCONCLUSIVE`, `INVALID_EVIDENCE`, `NOT_MEASURED`. Поднятие качества по одному домену не наследуется другими.

## Независимый плановый reviewer — до выполнения

Второй CODEX / Astra 6 UltraCode **не должен** принимать исходный документ за доказательство успеха. Проверить:

1. Доказан ли прямым source каждое звено? У N37-A есть ли фактический path, создающий tagged full signature? Для N37-B есть ли независимый wrong-law и не решает ли вопрос `TranslationMemorySourceLaw`? У N37-C может ли actual `ExecutorUnit` выполнить KNN ветку, когда exact/phrase/word-map пусты?
2. Разделены ли видоизменения **stored signature** и **ordinary tokenizer output**? Прямой вызов Store способен подтвердить баг API, но не consumer quality без L1.
3. У N37-C нельзя заменять поздний target-check идеальным oracle target без provenance. Требуется реальный lawful target language/lexical compatibility, не подсказка с sealed gold.
4. L0-A identity of both signatures должен быть **тот же** внутри каждой пары query/store; сравнение разных ключей с разными источниками не фальсифицирует guard threshold.
5. L0-B distinguish recall vs precision; query containment **может быть intentional**. Не утверждать «симметричная метрика всегда лучше».
6. Parent RP01 neural 0/3, ENRU0/4 и historical E09/E34/E43 evidence — неизменны. N37 относится Translation Units/KNN и не становится retroactive cause of neural failure.
7. Зафиксированы ли data licensing/authors/anti-leak, TEST/DEV split и resource bounds? CODEX chooser / open Chinese model author / CODEX quality judge — три роли, не один proxy.

**Gate PLAN_REVIEW_PASS**: все три теста имеют source precondition, настоящий falsifier, отрицательный контроль, domain applicability и zero-edit restriction. **Gate PLAN_REVIEW_FAIL**: не выделен настоящий consumer, допускается подсмотреть blind labels, пытаются чинить все KNN функции одним PR, выдают shadow за .NET, путают автора DPO и CODEX человеческий выбор.

## Независимый code-review — после кандидата

Снимать изменения по точному SHA, ограничить diff экспериментальным тестом и, только после доказанного L1 quality, smallest permitted adaptation. Проверить:
- не пострадал source-law hard veto и не пропали source-unknown cases;
- не нарушена честная semantically stable ordering, dedup, k budget, direction, topic, checkpoint export/import;
- negative controls A/B/C реально сравнивают тот же store/labels;
- сохраняются before/after/cold semantic results по каждой source-family, не только mean aggregate;
- отсутствуют скрытые deterministic shortcuts, oracle label injection, test-fitted tokens, changed prompts и экзаменационные данные в обучении;
- recording includes grad/optimizer receipts только когда реально было neural обучение; KNN index AddPair не маркируется LoRA step;
- Git publication restricted planning path and protected core path unchanged.

**Формат CODEX человеческого отчёта:** «Что наблюдалось» (по каждому ID и actual input→output), «Что осталось гипотезой», «Какие домены выиграли/проиграли», «Статус преподавателя DPO», «Почему принят / откат», «Точный commit + независимое readback». Не обещать доказательств там, где источник не исполнен.

## Что сознательно НЕ требуется исполнителю

Не начинать с расширения всех 500 примеров, обучения полной базы, миграции всех исторических веток или переписывания архитектуры. Не усиливать шум низкоуровневых тестов, пока нет одного законного полезного ответа на unseen domain. Если L0 не совпал с формулой, **сначала пересмотреть N37**, а не подгонять фикстуру.

**Статус этого пакета:** prompt prepared, independent reviewers not invoked, user request на публикацию документов выполнен независимо от будущих нативных опытов.
