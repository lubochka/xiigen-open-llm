# Полный навигатор возможных причин RP01 с учётом N49 и ограничений домена

19.10.2026. **Это index+delta, НЕ новый список доказанных root causes.** Основной живой уже составленный каталог [25 причин с прямыми ссылками, источниками и небольшими проверками](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md). До расширения этого каталога прочитать его **полностью**, а не повторять готовые карточки. В этой странице фиксируется, куда вставляются **новые N49-01..03 и новая ветка `local_wiki_planning`**, а также взаимосвязь алгоритмов и доменов.

## I. Общая карта: уже найденное + что добавлено сейчас

| Механизм, запрошенный пользователем | Уже зарегистрировано и что смотреть | Новая адресная дельта к `local_wiki_planning` | Домены и быстрый критерий |
|---|---|---|---|
| **DPO chosen/rejected и реальные учебные законы** | [44/01–02](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md): качество contrasts, 25 semantic families вместо 500 разных законов; исходный RP01 diagnostic | **N49-01: сам bridge стирает prompt и обе стороны**, оставляя defect type, даже если original generator их сохранил | Wiki routing и C# code trace; same-type opposite-target examples, сравнить exact triple до tokenizer |
| **DPO→Unit преобразование и адрес правильного получателя** | [44/15](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md): rejected annotations, mapping labels и `TranslationStructuredBatchBuilder` | Два параллельных адресата: structural perceptron получает original `Prompt/ExpectedRouteKind`, neural DPO — synthetic defect tokens. Один reported batch ≠ один обучаемый Unit | Wiki routing, code trace; map `PairId → derived lesson → recipient → saved state → ordinary consumer` |
| **LoRA матрицы, модули, rank/alpha, optimizer** | [44/04–08](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md) и [neural investigations](../43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/02-NEURAL-SCORES-AND-STATE.md): q/v, gradients, DPO masking, optimizer moments, absolute chosen logP | До этого уровня имеется **информационная потеря раньше LoRA**. Re-tune rank на одинаковой synthetic тройке не восстановит отсутствующую зависимость от original prompt | Neural C#/EN↔RU: только после raw→token→mask proof; same initialization exact learner, semantic quality not just margin |
| **Векторная токенизация, размерность, POS, порядок, AST** | [44/09–13](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md): exact byte/token mask, E26 context/order, E07 negative matrix vs E09 positive KNN | `RejectedType` projection уменьшает model-visible information независимо от того, насколько хорош downstream tokenizer. Восстанавливать информационный вход до настройки расстояний | RU↔EN POS/morphology/order; C# syntax/AST; wiki routing intent categories; не один общий codec |
| **Dynamic KNN, k, distance, thresholds, FIFO, consumer** | [44/14](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md), [E09/E26 experiments](../44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md) | В N49 не выявлен новый баг KNN. Но если routing/trace memory хочет обучаться по конкретному промпту или source, дефект-only signature так же рискует схлопнуть KNN ключи — **НОВАЯ производная гипотеза, не проверено, и не заявлено в source** | Wiki routing: correct unit id; C#: method edges; RU: word sense. KNN test only if actual key producer uses synthetic projection |
| **Неправильно понятые failed matrix эксперименты** | [44/13](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md), [12-MATRIX-TO-KNN-EXPERIMENT-MISREADS](../12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) | Не объявлять E07 fail запретом функций AST в E09 kNN. N49 аналогичен ошибке архитектурного уровня: defect classifiers могут быть полезны, но их output нельзя выдавать за обученный router | Только capability-specific (AST C# vs linguistic order), explicit ablation + restored consumer |
| **Unit/Machine/Flow/Graph many→many** | [original task](../../../original%20task.md), [RP01 architecture](../../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md), [44/15,20,24](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md) | N49-01 исходный route label существует, но нейронному Unit передаётся type-only; N49-02 отсутствие published/consumed learned state | По каждой принимающей Unit: tracked input→lesson→new policy/memory→saved ID→Flow/Orchestrator/Graph query; не только типы в плане |
| **Checkpoint / cold restore / совместимость направлений** | [44/18–19](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md): 4 vs 6 directions, LoRA/optimizer/RNG/ref/tokenizer identity, [N26 blueprint](../26-ARCHITECTURE-BLUEPRINT-TOKENIZER-KNN-STATE-AND-RESTORE-2026-10-16.md) | **N49-02:** `semanticQualityClaimed=false`→`PublishAllowed=false`, потому checkpoint может быть создан, но не опубликован для normal use | Wiki routing: valid gate+locator+new process; translation: old/new 4/6; neural: byte-for-byte logits after load |
| **Replay, кросс-доменная интерференция** | [44/08,17](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md) и [E43 receipts](../44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md) | N49 без нового measured replay delta. Полезно после восстановления точного получателя обучения, иначе replay старых дефектных tokens = повторение ошибки | Old+new acquisition and retention one learner + fixed budget + restore |
| **Hidden deterministic route/extractor вместо обучения** | [44/25](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md), protected code trace investigations | **N49-02:** `ProjectWikiTrainingOrchestrator` вызывает static `ProjectWikiRequestRouterUnit` по умолчанию, тогда как trained token-perceptron и neural DPO живут отдельно | Wiki routing: learned-disabled vs learned-loaded control, actual `_router` provider trace |
| **Holdout, leakage, семантическая оценка, публикация** | [44/21,24](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md) и [N45 semantic gate](../45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md) | **N49-01:** архивная проекция 50 raw prompts→4 model-visible triples и все 4 классы across split; **N49-02:** publication false без semantic reviewer | Разделить raw split independence vs **transformed-feature split independence**; independent semantic judge, non-vacuous |
| **Доказанные исторические полезные функции и false attribution** | [44/10,17,22–23](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md); [первая глава](../00-LOCAL-WIKI-SUCCESS-RECOVERY-P0-2026-10-19.md) | Донор `local_wiki_planning` действительно хранит real 50+50 router run и исходные DPO bridges; её neural результат не называть свободным learned capability | P0 сначала вернуть конкретную способность на её собственном получателе, не переносить название model на другой stack |

## II. Что НЕ следует применять во всех доменах

| Домен | Применимые входы и retrieval features | Не внедрять по умолчанию | Основная независимая метрика |
|---|---|---|---|
| Перевод RU→EN / EN→RU | word/phrase+POS+morphology+word order, language direction, context-aware KNN, typed memory, law-aware negation, semantic scorer | Roslyn SyntaxKind, C# caller/callee graph, одно общее EN/RU без direction key | Word-sense, negation, grammar, actor/object, text quality after new lesson |
| Язык программирования C# / Design-to-Code | AST nesting, method/owner scope, control/dataflow, caller/callee edges, source-file family split, structure-sensitive kNN | Copy русских POS tags; pure phrase match на source code | Correct referenced symbol, call edge precision+recall, compilability, code behavior |
| Проектная Wiki, документация | Cross-stack evidence graph, callers/callees, verified citations, project boundary, source grounded Unit | Claim100% по extractor same-gold; DPO defect tokens вместо actual evidence text | Correct citations, verifiable source, wrong-project rejection, unseen questions |
| Маршрутизация и оркестрация | Intent/action taxonomy, chosen Unit, required inputs, valid next-step, wrong-route consequence, dynamic routing | C# AST for natural-language route prompts; claim learned from `ContainsAny` | Macro-F1 per route, cross-domain task completion, proper Unit adoption |
| Непрерывное обучение | Serializable memory, replay/exposure, old/new family balance, per-domain checkpoint compatibility | Always-on same kNN feature set; loss-only optimization | Old retention & new acquisition + saved state after new process |

## III. Архитектурное дерево необходимого восстановления

```text
Адмиссия пары (реальные авторы chosen/rejected, legal/use rights)
  -> оригинальная prompt/chosen/rejected c семантическим правилом
    -> subject-specific decomposition (Unit/Flow/Orchestrator/Graph)
      -> model-visible feature/tokenization/structure (НЕ только defect-type)
        -> нужный обучаемый recipient: LoRA | learned KNN | typed memory | route weights
          -> independent heldout AFTER projection, negative controls
            -> publication quality gate (не выключать ради PASS)
              -> exact model/memory state + tokenizer + manifests, cold restore
                -> actual normal consumer answer (with learned-disabled A/B)
                  -> regression/forgetting across domains and real resource cost
```

Каждый переход помечать `CODE_CONFIRMED`, `HISTORY`, `PROXY`, `NATIVE_L`, `CONSUMER_U` или `UNKNOWN`. Никакое изменение LoRA ранга не закрывает потерю prompt на входе и никакая зелёная DPO метрика не заменяет проверку новой способности.

## IV. Дополнительная производная гипотеза N49-04 (только проверка, не объявлять факт)

**N49-04 / routing-neighbor-key leakage:** если KNN index/learned graph унаследовал ключ `defectType` из neural DPO surrogate вместо original `prompt+target`, то шесть правильных маршрутов из одной `wrong_training_unit` группы тоже станут неразличимыми в neighbour shortlist. **Это сейчас гипотеза без source confirmation пути подачи ключа в KNN.** Проверить caller chain: search `ToDpoMetricPair` outputs → `KnnLogicalUnit` / `OrchestrationKnnStore` / `ProjectWiki...` memory insertion; только при доказанном вызове выполнять one-pair kNN test same rejectedType / different target. При отсутствии caller linkage N49-04 **REJECT**, не дописывать несуществующий дефект.

**Финальный статус:** Систематизированы все 25 ранее отмеченных механизмов ссылкой на существующий индекс и три новые source-specific difference families N49-01..03; N49-04 зарегистрирован исключительно как *conditional hypothesis pending caller trace*. Нет оснований говорить «все возможные причины в природе найдены» или «RP01 причина доказана».
