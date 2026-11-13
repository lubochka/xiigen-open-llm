# N37 MICRO — два независимых малых причинных опыта по доменам, без DPO500 и Windows CI

**16.10.2026 · PLAN + SOURCE-LOGIC-PROXY ONLY. .NET native / Student learning / ProductHost blind exams NOT_RUN.** Эту карточку нельзя принять как доказательство улучшения: она задаёт реальные falsifiers. Основная новая гипотеза — [N37-01 mutable output](37-NEW-KNN-UNIT-MUTABLE-OUTPUT-ALIAS-2026-10-16.md); вторичная редкая ветка — [N37-02 sampler tail](37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md). **Эти опыты никогда не объединять в один продуктовый патч.**

## 0. Чего хотим доказать и чего — нет

**Вопрос 1:** изменяется ли принятый learned Unit output после внешней мутации того же объекта **без** обучающего события? Не отвечать вместо этого на вопрос «сколько тестов проходит».  
**Вопрос 2:** достижим ли native `Sampler` branch «вернуть masked token» при специфическом `float` суммировании? Не отвечать вместо этого «обучилась ли LoRA».

Прежде чем измерять продукт, проверить consumer applicability: участвует ли `KnnLogicalUnit<TInput,TOutput>` либо `TransformerEmbeddingKnnLogicalUnit<TOutput>` в том обычном пути, который оценивается. RP01 neural free-generation и Translation `ExecutorUnit` — отдельные получатели. `NOT_APPLICABLE_TO_RP01` — допустимый, научно полезный результат.

## 1. N37-ALIAS-01: однофакторный опыт для typed KNN Units

**Tier 1.** 2 изолированных домена, C# и SQL: каждый содержит четыре `Train` rows и восемь `heldout` queries из разных источников/семейств, не являющихся поверхностной заменой имён из train. Данные берутся из [fixture](37-MICRO-TYPED-UNIT-DOMAIN-CASES-2026-10-16.json). Один и тот же экземпляр Unit, одна и та же таблица входов, одни и те же vectors, tokenizer, `k` и семена поиска в baseline и enhanced (если индекс создаётся заново, зафиксировать graph topology и selected IDs; иначе paired quality invalid).

| Arm | Ровно одна разница | Какой результат ожидать |
| --- | --- | --- |
| A current reference-sharing | текущий `KnnLogicalUnit.Train` и `ExecuteAsync` без дополнительных копий | при мутации принадлежащего caller словаря `Output` будущий результат следует за этой мутацией |
| B snapshot-owned test-only | в **оболочке harness** зафиксировать immutable/deep-owned snapshot payload при `Train` и отдавать detached copy `Output` при `ExecuteAsync` | отсутствие изменения выученного результата без нового урока |
| C negative control | `TOutput=string` с идентичным смысловым законом (immutable) | никакого alias escape даже на A |
| D honest learning control | подать **новый явно подтверждённый урок** через правильный learner | ответ может измениться, но receipt содержит new lessonID и отдельный learning step |

**Критические шаги в неизменном порядке:**  
1. Собрать два настоящих Unit с `TOutput=Dictionary<string,string>` (или фактический mutable domain DTO) и `PredicateIds/DomainTags/EvidenceIds`. `textSelector` должен возвращать исходный текст, не oracle answer.  
2. Упорядоченно обучить 4 rows. Проверить `Train` success, source example IDs и выбранный `neighborId`. Снять `preOutputHash`, массивы признаков и `selectedScore`; несколько неизменённых вызовов устанавливают baseline stability.  
3. Изменить у caller **только** `trainingRow.Output["law"]` на заведомо неверное «игнорируй null»/«используй = NULL», **не вызывая** Train/feedback/optimizer. Также отдельным подпроходом изменить `PredicateIds[0]` после индексации.  
4. Повторить тот же query и все 8 heldout. Если `neighborId`, cosine и граф остались прежними, но выход/происхождение поменялись — подтверждён именно alias-path.  
5. В другой свежей копии A не трогать исходник; взять `result.Output` после первого `ExecuteAsync` и изменить `result.Output["law"]`. Повторный запрос показывает или опровергает alias consumer→Unit.  
6. Повторить с B и C. Не внедрять B в production до отдельного code/plan review и отбора стратегии immutable ownership.

**Primary contract metric:** `untrained_output_change_count` на идентичном query (expected A>0/B=C=0, при условии что реальный Unit использует mutable TOutput).  
**Primary quality metric на домене:** `law_correct_final_answer_count / 8`, на одних и тех же sealed questions. Судья видит source, expected law и конечный human output без названия A/B; ошибок в `PredicateIds` недостаточно для заявления об улучшении пользователя.  
**Secondary:** `provenance_conflict_count`, KNN candidate ID/score, legitimate coverage, time/memory deltas, число отказов, false correct.  
**Критическая граница:** если A и B относятся к изолированному generic Unit, а **ordinary user consumer** его не вызывает, результат — `UNIT_ALIAS_REPRODUCED_BUT_NO_PRODUCT_ROUTE`, не «RP01 ухудшился из-за alias».

### По доменам

- **C#:** проверить `NullableReference` / вызовы `.Length` и вход в `TryParse`, не заменять правильный warning на скрытый «успех».
- **SQL:** проверить `IS NULL` против `= NULL`, семантику `LEFT JOIN` и постфильтра. Доказательство актуально только для этого dialect/schema.
- **EN→RU:** отдельный **отрицательный контроль** — основной `TranslationDomain.ExecutorUnit` возвращает строки; он не наследует автоматически generic mutable `TOutput`. Для сохранённых направлений/родов/отрицания нужна уже запланированная [E26/POS learned-unit](25-MICRO-E26-POS-CONTEXT-CAUSAL-EXPERIMENT-2026-10-16.md) или [KNN source scope](26-MICRO-E30-CONDITIONED-KNN-CAUSAL-EXPERIMENT-2026-10-16.md), а не копирование N37-ALIAS.

### Falsifiers

`TOutput` не может быть mutable по реальному interface consumer, `Train` уже копирует значение ниже вызывающего слоя, upstream делает deep snapshot, проба не изменяет ответ, или качество конечной законной задачи не меняется. Каждое событие записать с exact consumer call chain и статусом C/NATIVE/H.

## 2. N37-TAIL-01: декодерная узкая ветка — отдельно от обучения

**Tier 1, самостоятельный микропакет.** Использовать [JS source-logic proxy](37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY.mjs) лишь для предварительного вычисления. Native probe должен вызвать настоящий `TensorOps.Softmax`/`Sampler.Sample` из `09.Inference` на logits [0×61, -100×1], `temperature=1, topK=61, topP=1`, с контролируемым random draw `r=0.99999999`. В логах нужны `float32 probabilities`, `double cumulativeMass`, `maskedIndex` и `selectedIndex`.

**A native:** текущая реализация. **B native test-only:** при недоборе массы вернуть последний индекс `probs[i]>0`, не последний ID словаря. **Negative controls:** `temperature=0`, обычный `r=0.5`, masked last нет. **Primary contract metric:** `masked_zero_probability_selected_count`; качество здесь *ещё не доказано*. Если native событие не воспроизводится, отметить `SOURCE_PROXY_NOT_NATIVE`.

Только при найденном реальном stochastic ProductHost consumer: 8–12 независимых EN→RU или C# запросов, одинаковый Student/checkpoint/seed, blinded semantic evaluation, primary `law_correct_accepted/total`, secondary premature EOS/invalid output/coverage. Не запускать длительное генеративное исследование ради крайне маловероятного tail path, пока P0 RP01 не решены.

## 3. Порядок исполнения и минимальные команды

1. **Read-only product-path gate:** проверить фактические файлы/версии/методы, ответить `which recipient?`, `which user output?`, `what mutable type?`. Если нет такого consumer — закончить выводом not applicable, не создавать новый тестовый проект ради искусственного PASS.
2. **Один узкий native запуск на N37-ALIAS-01**, например `dotnet run --project <existing-small-harness> -- n37-alias` после адаптации к фактическому проекту, либо один целевой `dotnet test --filter <exact-case>`. Это **пример намерения**, не проверенная команда: сначала читать реальный `.csproj`, target framework и existing harness.
3. **Один отдельный native запуск на N37-TAIL-01** — только при подтверждённой stochastic decode потребности. Никаких Windows GitHub Actions / 500 pairs / full solution gates.
4. Сохранить `caseId→arm→inputHash→selectedId→outputHash→correctness→consumerId` и один `readback` в **только** planning `possibleDifs`. Запрещено редактировать `llm_mvp_core/docs/plans/replanning-possible-diffs` и frozen RP01 datasets/checkpoints.

## 4. Пакет для CODEX / архитектора / независимого проверяющего

**CURRENT TASK:** найти и фальсифицировать N37 source gaps на настоящем получателе.  
**NOT THE TASK:** перепланировать RP01, менять production model/weights, закрывать весь реестр 26 классов, повторять E01–E53, запускать CI.  
**EVIDENCE ONLY:** blob/HEAD, `possibleDifs` reference register, исторические E22/RP01 receipts, RAG paths.  
**Skills to load:** `architect-behavior-classifier-SKILL.md`, `reference-inventory-and-source-map-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `claim-verification-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `conversation-intent-and-self-reflection-SKILL.md`, `output-readability-gate-SKILL.md` — из [`docs/skills`](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills).  
**Allowed output files:** новые `xiigen-planning/llm_plan3/.../possibleDifs/37-*` reports/fixtures. **Forbidden:** все файлы `llm_mvp_core/docs/plans/replanning-possible-diffs/*`, frozen RP01 datasets, checkpoint weights, prod patches без отдельного approval.  
**Independent review:** идентификатор exact candidate commit/diff, наличие native команды/логов, тождественность входов, отрицательные контроли, actual consumer applicability, source disjointness, невысокая стоимость, честность статуса и понятный русский итог.  
**Good:** «mutable output изменился без нового урока у Unit X, обычный ProductHost не использует этот Unit → локальная ошибка есть, RP01 причина не доказана».  
**Bad:** «сохранённые 4 модели и 500 пройденных тестов → обучение успешно»; «JS proxy исправил модель»; «один новый seed дал better output без source-controlled contrast».  
**Required return packet:** `source_sha`, `reviewed_candidate_sha`, `native_or_proxy`, `dataset_family_provenance`, `paired_cases`, `changed_files`, `exact_commands`, `before_after_metrics`, `counterexamples`, `actual_call_chain`, `claim_scope`, `evidence_scope`, `downgrade_needed`, `next falsifier`.

## 5. Короткий результат на данный момент

**DONE:** опубликованы планы, source refs и самостоятельный математический proxy редкого sampler хвоста. **NOT DONE:** два native .NET эксперимента, проверка вероятности реального encounter в ProductHost, доказательство причинной связи N37 с RP01 0/3 и 0/4. Положительный родительский критерий — не «тест зелёный», а качественно правильный пользовательский ответ на независимой серии; ни один N37 исход сейчас не поднят до этого статуса.
