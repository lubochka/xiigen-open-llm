# N37 — быстрые предметные эксперименты: C#, RU→EN, EN→RU, SQL и RU/HE morphology

**16.10.2026 · DESIGN_ONLY: native .NET, новые Student updates, CODEX/Astra independent review НЕ ПРОВОДИЛИСЬ.** Это каталог к [N37 master](37-RP01-MASTER-CROSSWALK-POSITIVE-HISTORY-NOVELTY-AND-FAST-DECISIONS-2026-10-16.md), а не новая блокирующая очередь. Каждый эксперимент имеет собственные входные семейства, обучаемый state, consumer и критерий отказа от гипотезы. Источник core: [793d7b755b23aac90445da3ac6661b1bff7e6672](https://github.com/lubochka/llm_mvp_core/tree/793d7b755b23aac90445da3ac6661b1bff7e6672).

## EX-C#-01 · Связная генерация или только DPO margin

**Адресат:** RP01 neural C#→documentation Student. **Гипотеза:** random B0 не умеет создавать связные ответы, а LoRA q/v preference-step не заменяет начальный SFT.

**Исходный закон:** метод C# с nullable string, который возвращает длину или fallback 7, при null возвращает 7, а при пустой строке — 0. Дополнительные задачи на async, lifetime и operator precedence; в обучении и независимом контроле разные AST families, не 20 переименований одного метода.

**A/B:** A — frozen random B0; B — текущий valid DPO-only tiny update; C — краткий SFT chosen-only warmup и такой же DPO с одинаковым декодером и budget. Перед обучением провести один response-mask/finite-difference LoRA-gradient sanity check. M0: одинаковая prompt/response идентичность; M1: step и состояние; M2: 8 development + 8 untouched independent AST families. Primary: право объяснить реально исполняемый C# effect, ошибки, ungrounded statements, compile/runtime if code generated. LoRA tensors changed и preference-margin сами по себе FAIL scientific gain gate. См. [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).

## EX-RUEN-02 · Значимые уроки вычеркнуты как лексические дубликаты

**Адресат:** RU_EN Batch029 → TranslationMachine structured Units, а не neural RP01. Нулевая стадия M0 уже представлена [source-only proxy](37-ruen-history-gate-source-proxy.py); C# native NOT_RUN. Более подробный порядок [N37 AB](37-MICRO-RUEN-CONTRASTIVE-REPAIR-CAUSAL-EXPERIMENT-2026-10-16.md).

**Доменная панель:** отдельно отрицание «доступен/не доступен», порядок «проверь→опубликуй/опубликуй→проверь», количество «три/четыре», дубликат пунктуации, несвязанное предложение. Gold — корректное значение английского перевода и принадлежность урока к новой semantic family.

**A/B:** A lexical Jaccard history >=0.70; B допуск contrast при independently proven different law (полярность, role, count, order), без отключения true duplicate guard; C all-admit dev negative control. Сверить действительно принятые lesson ids, state/recipient и **ordinary final answer**, не только число примеров. Blind контроль создавать на новых семьях отдельно от стандартного Batch029 heldout overlap guard: защиту тестов не выключать. Результат: law-correct/false-accept/abstain/coverage, paired by case.

## EX-ENRU-03 · KNN выучен, но word-map и ранние возвраты скрывают изменение

**Адресат:** structured EN_RU Translator, не C#500 и не neural ENRU500. [Executor.Execute](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L400) выбирает exact-source, затем phrase/word mapping и только затем KNN. [KNN store](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationUnitContracts.cs#L203-L326) хранит структурные ключи и кандидатов.

**Примеры:** “The door is ready” / “The door is not ready”, “Anna gave Mary the key” / “Mary gave Anna the key”, before/after. Отдельная blind source-family panel, учитывающая допустимые альтернативные переводы и неоднозначные местоимения.

**A/B:** A current normal path, B same state + shadow trace всех KNN candidates до early return (не менять output), C в экспериментальной ветке только один фактор — law-aware выбор между word-map и KNN там, где найдено source-contradiction. Сохранять raw source, language direction, condition, actual selected adapterId, true semantic correctness. Если обращение к памяти не было достигнуто, вывод ROUTE_NOT_REACHED; нельзя объявить неправильный KNN при недостигнутом вызове.

## EX-SQL-04 · Одни слова, разные SQL законы

**Адресат:** реальный SQL code/semantic consumer, если существует; иначе exploratory AST-oracle probe, НЕ фиктивный RP01 endpoint.

**Исходный закон:** “LEFT JOIN orders ON orders.customer_id=customers.id AND orders.paid=1” сохраняет клиентов без оплаченного заказа, а вынесение “orders.paid=1” в WHERE после LEFT JOIN может их удалить. Сравнить также “IS NULL” и “= NULL” с указанием SQL dialect и three-valued logic.

**A/B:** A bag-only retrieval или legacy semantic classifier; B same memory with AST/dialect-aware candidate rerank, если источник найден; C deterministic SQL execution oracle (оценщик, не обучаемый Student). Запустить фиксированные маленькие in-memory таблицы с unmatched customers, duplicates и NULL. Freeze same inputs/training/context/k, считать law-correct, count/cardinality, false accepted, coverage; не переносить ENRU POS правила на SQL. Если actual consumer отсутствует — только source hypothesis, не продуктовый improvement.

## EX-MORPH-05 · Соседний branch language_translate как функция, а не магический перенос

В [language_translate sibling review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/09-domain-functions-and-sibling-branches.md) сохранены RussianStemmer/HebrewRootExtractor/MorphFamilyIndex/MorphDimensionAppender. Appender добавляет **скаляр размера морфологического семейства**, не само значение корня; ранее не подтверждён live consumer и blind language gain.

**A/B:** A pinned embedding cosine; B тот же embedding + именно historical scalar; C frequency-only dimensional control. Для RU и HE использовать seen/unseen forms, одно family для нескольких surface forms, и две **разные** семьи одинакового размера. Метрика first-hit relevant@1, wrong-root false positives, rerank order; затем требовать живой state/consumer. Никаких утверждений о learned syntax или LoRA от улучшения word retrieval.

## Единое правило короткой проверки

У каждого опыта заранее записать: case/family hash, допускаемое юридически происхождение данных, current learned state/version, что изменилось единственным фактором, learner vs deterministic path, checkpoint/resume identity, final consumer answer, correct/wrong/abstain, denominator и cost. TRAIN/DEVELOPMENT/SEALED разделяются **по исходному семейству до парафразирования**. После подбора по dev новая blind проверка необходима для вывода о переносе. Положительный результат на памяти чужого домена не становится положительным результатом нейронной генерации.

**STOP:** не запускается фактический consumer → ROUTE_NOT_REACHED; native mechanism не совпал с source-only выводом → SOURCE_DRIFT; изменились только bucket counts → NO_QUALITY_GAIN_OBSERVED; ухудшена критическая законность → REGRESSION. Для научной задачи не нужны все Windows GitHub Actions gates, но узкая native проверка и независимая оценка ответа обязательны.

Соблюдать адресные навыки [architect](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/architect-behavior-classifier-SKILL.md), [experiment-design](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md), [plan-review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md), [generated-code-review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/generated-code-review-SKILL.md), [prompt-patch](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/prompt-patch-authoring-SKILL.md) и [human-readable-architecture-audit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-readable-architecture-audit-SKILL.md). Новые DPO chosen/rejected — только от юридически разрешённого open-weight teacher с независимой проверкой CODEX/Astra; в этом документе новые пары не создавались.
