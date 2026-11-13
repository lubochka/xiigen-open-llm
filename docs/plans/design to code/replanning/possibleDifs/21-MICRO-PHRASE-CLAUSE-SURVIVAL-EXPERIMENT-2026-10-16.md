# MICRO-21 — проверить, уничтожает ли выученная phrase-memory отрицание при сборке нового предложения

> Действующее уточнение аудита 11 ноября находится в [датированном дополнении](#xiigen-audit-2026-11-11). Исторический текст сохранён; текущий вывод читать вместе с уточнением.


**Дата:** 2026-10-16. **Статус:** READY-TO-IMPLEMENT / NOT RUN IN NATIVE CORE / NOT MODEL-QUALITY-PROVEN.  
**Одна новая гипотеза:** [N21-01 — clause dropout](21-INDEPENDENT-CLAUSE-DROPOUT-WHITESPACE-AND-LEGACY-SCAFFOLD-2026-10-16.md#n21-01--удаление-критических-токенов-при-композиции-из-выученных-фраз).  
**Готовый fixture:** [21-MICRO-PHRASE-CLAUSE-FIXTURE-2026-10-16.json](21-MICRO-PHRASE-CLAUSE-FIXTURE-2026-10-16.json). Восемь source-disjoint composition cases из четырёх семейств, восемь коротких phrase lessons; публичный exploratory oracle, а не hidden certified evaluation.  
**Source SHA:** llm_mvp_core / codex/dpo-implementation @ 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5. Результат не переносить на другой SHA без проверки дельты.

## 1. Зачем опыт и где обучение

Точное наблюдение по коду: [ExecutorUnit.TrainOnExample](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L456) обучает PhraseMappings, word maps, KNN и exact memory; [Execute](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L262-L280) читает эти структуры. Ветка [TryComposeFromPhraseMappings](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L798-L857) может выбросить новый отрицательный/модальный токен из unlearned middle span и всё же принять phrase output.

**Вопрос:** может ли обучаемый Unit показать положительный gain на простом соединении двух выученных фраз и одновременно систематически разрушать **необученный критический модификатор** на новом полном предложении? Этот опыт измеряет именно такую композиционную ошибку обучаемой памяти. Он **не** измеряет DPO-gradient LoRA / performance random B0 Student. Проверить вызов ordinary TranslateAsync через TranslationMachine отдельно.

## 2. Единственная изменяемая ось и режимы

Сначала на одной машине с неизменным кодом и пустой памятью последовательно загрузить восемь коротких typed lessons из fixture: каждая TRAIN запись — корректная phrase source/target; в ExecutorOutput.PhraseMappings только её собственный phrase mapping; WordMappings пустые. Все lessons HeldOut=false. Точная schema:
- [ExecutorInput / ExecutorOutput / ExecutorPhraseMapping](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationTrainedUnits.cs#L25-L52).
- [TranslationLearningBatch, UnitId / HeldOut / SemanticSignature](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L105-L155).
- [TranslationMachine.TrainStructuredBatchAsync](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L89-L142).
- Создать только new TranslationMachine() — НЕ injected-constructor with empty trainers, известную ловушку [17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md).

Все 8 heldout full-sentence queries **отсутствуют** дословно в TRAIN phrase sources. Позитивный составной пример и его отрицательный contrast используют **те же выученные фрагменты**, но только negative вставляет one/two unknown critical words. Это one-factor contrast для source modifier, а не proof large-scale unseen domain generalization.

| Режим | Что изменено | Проверяемое утверждение |
| --- | --- | --- |
| **U0 / NO-LEARN** | Пустой обучаемый Unit; те же 8 queries | Что давало правило/Bootstrap до изучения phrase mappings |
| **U1 / LEARNED-AS-IS** | Именно 8 lawful phrase lessons; неизменённые Execute + TrainedTranslationAdapter | Воспроизвести learned composition и пропуск critical token; зафиксировать source→unit→output trace |
| **U2 / SAME-MEMORY-SHADOW-GUARD** | Та же U1 immutable learned state, но shadow consumer отвергает candidate, если unmapped critical modifier не покрыт и не выражен в target | Изолировать эффект семантического guard; NO extra training, NO new decoder; результат может быть ABSTAIN |
| **U3 / CORRUPTED-MAPPING-NEGATIVE-CONTROL** | Те же 8 input phrases, но все target phrases циклически сдвинуты между families; тот же route/guard | Отличить эффект содержания выученных lessons от шаблонной генерации и самовнушения evaluator |

Для действительно строгой одной переменной U1↔U3 дополнительно взять один и тот же список phrase hit IDs/spans, заменить только output lookup из correct mapping на shuffled; не перестраивать KNN/менять tokenizer. Если U3 демонстрирует такой же результат по закону, positive proof of learned attribution не пройден.

**Справедливость счёта:** U0 может возвратить Bootstrap placeholder, который не является адекватным переводом; измерять его полностью, но не называть «вредом обучения», если U1 лучше на positive и хуже на negative. U2 — safety/coverage policy, а не обучение новой модели. Не сравнивать 100% selective accuracy U2 с 50% full-coverage U1, умалчивая о воздержаниях.

## 3. Как построить native C# runner без новой инфраструктуры

Создать **один временный** маленький runner рядом с существующими tools (или крошечный изолированный проект в tests/tools), не менять production classes на первом этапе.

1. Прочитать exact исходные методы и consumers: TranslationMachine, TranslationStructuredBatchBuilder, TranslationTrainedUnits, TranslationSemanticTokenizer, TranslationDirectionAdapters и TranslationRoutingFlow. Проверить рецензируемый core HEAD; если другой, записать diff.
2. Загрузить fixture. Для каждого TRAIN lesson построить TranslationLearningExample: UnitId="executor-en_ru"; SourceId=fixture.lessonId; InputJson=ExecutorInput(source,"en","general"); ChosenOutputJson=ExecutorOutput(chosen,0.93,WordMappings=[],PhraseMappings=[(source,chosen)]); RejectedOutputJson=явно неверный контраст как DEV scaffold, НЕ аттестованный external Chinese model-created DPO; SemanticSignature из new TranslationSemanticTokenizer().Tokenize(source,"en").Signature; HeldOut=false. Вызвать new TranslationMachine().TrainStructuredBatchAsync(new TranslationLearningBatch(...)); проверить PromotedUnitIds содержит executor-en_ru, RowCount и фактическую запись PhraseMappings. Не использовать DpoTrainer: цель здесь не нейронное DPO.
3. Для каждого полного test input использовать **реальный** TranslationMachine.GetOrchestratorAsync() → ILogicalOrchestrator.RunAsync(new LogicalOrchestratorRequest(RunId, ScenarioId=TranslationRoutingFlow.ScenarioId, UserRequest=input, Flags=new LogicalExecutionFlags(), Metadata=new Dictionary<string,object> { ["source_language"]="en", ["target_language"]="ru", ["topic_domain"]="general" })). Читать FinalHumanResult и actual adapter_id/route evidence. Это именно показанный путь [TranslationMachine.cs:653–666](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L653-L666), [TranslationMachine.cs:815–857](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L815-L857), [TranslationRoutingFlow.cs:29–73](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationRoutingFlow.cs#L29-L73). **У TranslationMachine нет публичного TranslateAsync; не вызывать выдуманный метод.** При необходимости отдельно instrument ExecutorUnit.Execute и TrainedTranslationAdapter; отсутствие actual invoke trace → ROUTE_UNVERIFIED. Gold/criticalTokens держать только во внешнем scorer, не подавать в Unit metadata.
4. В shadow-контуре, **после формирования U1 candidate и вне production**, установить предварительно зафиксированную семантическую ограду: если input содержит modifier never или пару must not, а соответствующее отрицание/запрет отсутствует в target, не выдавать кандидат как полный ответ; поставить ABSTAIN и reason="DROPPED_CRITICAL_CLAUSE". Не считать это переведённой фразой; не исправлять gold в train и не подмешивать target.
5. Заморозить lesson set, module SHA, tokenizer, candidate selection, code branch, heldout cases и rubrics. U3 создаётся переадресацией target lookup **после** тех же phrase matching spans, при identical query. Никакого повторного DPO оптимизатора/Windows Actions.
6. Дополнительно сделать одну проверку after checkpoint: SaveCheckpointAsync / LoadCheckpointAsync, повторить 8 задач на restored model. Для локального causal вывода это secondary invariant; если путь дорогой/недоступен, отметить NOT_RUN, не блокируя основной опыт.

Нативная точка входа установлена чтением действующего кода: GetOrchestratorAsync().RunAsync(...), не несуществующий machine.TranslateAsync. Встроенный приватный TranslateForEvaluationAsync сейчас фиксирует ru→en, поэтому он не подходит для EN→RU панели. Не придумывать CLI и не подменять требуемый **нативный вызов** счётчиком зелёных тестов.

## 4. Числовые метрики, конкретный прогноз и критерий

В каждом из восьми кейсов записать:
- familyId, source SHA, trainLessonIds, exactSourceHit (ожидается false на новых полных фразах), phraseHitIds, matchedPhraseSpans, matchedTokens, coverage, **unmatched source tokens**, wordMapped tokens;
- direct Unit candidate, adapter ID, confidence, verifier.Pass и verifier issues, final ordinary TranslateAsync output, selected origin;
- required clause present (в том числе negation/modal polarity), critical false accept, fully-correct translation, abstained, elapsed ms; SHA frozen learned state;
- reference translation/rubric **только** во внешнем scorer после получения ответа.

Пример ожидаемой по коду асимметрии, НЕ результат нативного прогона:

| Source | TRAIN phrase 1 | TRAIN phrase 2 | Возможный U1 output | Проблема |
| --- | --- | --- | --- | --- |
| the service opens the port | the service | opens the port | сервис открывает порт | Positive composition |
| the service never opens the port | the service | opens the port | сервис открывает порт | never отброшен |
| the user never deletes the record | the user | deletes the record | пользователь удаляет запись | never отброшен; семантика отрицания потеряна |

**Важно:** Primary fixture содержит грамматически корректную пару «the user deletes the record» / «the user never deletes the record». Любые изменения gold/английской формулировки требуют фиксации ДО начала native запуска и отдельного предметного review. Публичные DEV-кейсы не являются sealed evaluator и не заменяют независимую оценку CODEX/Astra.

**Primary metrics (с обоими знаменателями):**
- CRITICAL_FALSE_ACCEPT_RATE = неверные ответы с утраченной обязательной полярностью / количество согласованных negative cases;
- LAW_CORRECT_ALL = полностью корректные ответы / все одобренные heldout cases (ABSTAIN ≠ correct);
- ANSWER_COVERAGE = число финальных содержательных ответов / все cases; SELECTIVE_ACCURACY = верные / отвеченные;
- BAD-LESSON_DELTA = разность U1 и U3 при одинаковых phrase hits; LEARNING_COMPOSITION_GAIN = U1−U0 на positive cases.
- Дополнительная заранее объявленная полезность для exploratory triage: +1 correct, −2 critical false accept, −0.25 abstain. Это **не** продуктовая метрика и её веса нельзя подгонять после получения outputs.

**Решение:** 
- Если U1 positive cases улучшаются над U0, но negative U1 отдаёт несовместимые переводы — доказан **конфликт наученного phrase retrieval и сохранения неизвестной обязательной семантики** (в этой панели); причинный механизм N21-01 подтверждён на Unit route.
- Если U2 уменьшает CRITICAL_FALSE_ACCEPT_RATE, но уменьшает COVERAGE и не увеличивает LAW_CORRECT_ALL, сообщить **safety/selection improvement, НЕ новый language learning gain**.
- Если U1 не вызывает phrase-map / unknown token не исчезает → гипотеза для данного consumer **falsified или route-not-reached**; зафиксировать вызывавший путь и не ремонтировать вслепую.
- Если U3 остаётся столь же хорош как U1 → learned attribution underdetermined; пересмотреть плохой negative control, не объявлять успех.
- Ни один исход без Student+same-consumer matched contrast не доказывает причинный эффект для исходного RP01 DPO-only free-answer 0/4.

## 5. Выход — один небольшой пакет вместо цикла Windows CI

Сохранить по окончании: MICRO21-run.json (все per-case arms, learned map digest, hashes, trace), MICRO21-summary.md (before/after, ошибки, потери, coverage, 2–3 actual примера), MICRO21-review.md (CODEX/Astra independent acceptance по финальным immutable outputs, если доступно). Это **будущие** выходы, в данной GitHub planning-сессии их ещё нет.

Запуск local dotnet с одним маленьким проектом/8 cases; **ни изменения core default product route, ни full Windows GitHub Actions, ни расширения тестового покрытия** до подтверждения causal effect. Программный смысл guard обсуждается с архитектором только после результатов.

## 6. Поручение исполнителю — skills, allowed scope, return packet

**CURRENT TASK:** one-factor native experiment MICRO-21 against actual Translation trained Unit, not another speculative design review.  
**NOT THE TASK:** rewriting B0 / DPO / LoRA / pipeline graph / RP02–RP16, generating 500 pairs, modifying frozen prior experiments, tuning semantic labels until score improves, full Windows actions.  
**EVIDENCE-ONLY:** старые RAG, E51A/E51B/E52 receipts, соседние ветки, previous possibleDifs; new runtime measurement separate.

**Required skills:** 
SK-INTENT: architect-behavior-classifier-SKILL.md + conversation-intent-and-self-reflection-SKILL.md; 
SK-PLAN: phase-planning-SKILL.md + experiment-design-SKILL.md + intent-to-plan-SKILL.md; 
SK-REVIEW: plan-review-SKILL.md + claim-verification-SKILL.md + audit-protocol-SKILL.md; 
SK-ARCH/CODE: architectural-decision-testing-SKILL.md + code-examination-SKILL.md + learning-path-audit-SKILL.md; 
SK-PROMPT: prompt-patch-authoring-SKILL.md; human output: Russian, readable, show bad AND good actual cases. Load these from [core docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills), not from memory.

**Allowed files:** NEW temporary isolated runner, NEW immutable run artifacts under results, this MICRO-21 spec/fixture for corrections **before freeze**; no production code edits until causal review. 
**Forbidden changes:** existing core Student algorithms, prior run output/gold/closed exam, ordinary customer acceptance thresholds, GitHub workflow, untouched docs; no retrospective authorship claim.
**Allowed commands:** local small runner compile/execute + existing selected train/serve API; one narrow readback; no full suite. 
**verification_required:** exact source ref and branch, actual successful trainer call and learned state, native per-case trace, separate scorer, 4 positive/4 negative preregistered DEV candidates (adjust only via documented external validity review), freeze before code mutation.

**Good example:** «after eight phrase lessons U1 produced learned answer on F02-POS; U1 incorrectly accepted F02-NEG by dropping never; U2 abstained: false accepts 4→0, coverage 8/8→4/8, full-case correct remained 4/8». Это **иллюстративная строка**, не факт. 
**Bad example:** «восстановлена LoRA, PASS всех тестов; следовательно перевод и learning улучшились», или «U2 answered-only 100%, значит качество выросло на 50 пунктов» без coverage.

**Return packet:** status, exact code SHA, actual run command, actual inputs/hashes, trained recipient IDs, U0/U1/U2/U3 per-case outputs, paired denominator, negative outcomes, uncertainty, changed-or-unmodified product files, independent reviewer verdict or NOT_RUN, next smallest experiment if unresolved. Если CODEX/Astra независимой экспертизы не было, так и писать; не выдавать авторский readback за независимый PASS.


<a id="xiigen-audit-2026-11-11"></a>

## Дополнение аудита 11 ноября 2026: не терять допустимую альтернативу до guard

Сохранить исходный исторический рецепт, fixture и raw. Настоящее уточнение не означает повтор MICRO-21. На current core `009bf226174441fa0fbf0336e74ad2f2b7174d92` installed phrase planner отличается от исторического greedy; источник/algorithm identity необходимо проверять перед любым будущим сравнением.

Новый аналитический контрпример P54 показал: единственный winner по coverage/minspans может не пройти неизменный caller gate, хотя другой доступный plan его проходит. Для 8 tokens keys[a1 a2],[a5 a6],[a2 a3 a4 a5 a6 a7] это 6/8,1span,отказ против 4/8,2 spans,допуск. Для 6 tokens keys[a1 a2],[a3 a4],[a2 a3 a4 a5] оба покрывают 4, но minimumspans опять теряет допуск. Это не результат native MICRO-21 и не доказательство русского качества.

Усилить будущие per-case поля §4: algorithm/source identity; список действительно доступных допустимых plans; выбранные source keys/spans; reason его допуска/отказа; наличие lawful runner-up; final route после fallback. Считать отдельно wrong accepted, correct answered и потерянные lawful answers. Сравнение U1/U2 должно удерживать тот же candidate set и выяснять, не выброшена ли альтернатива до shadow guard. Показать сохранённый полезный ответ сразу после вмешательства, до restore; потом проверить reading restored state без нового Train.

Проверка этого интеграционного контракта сама не является обучением нового значения отрицания. Полезное снижение false accepts сохраняет статус успеха своей оси, но потерянный lawful answer/coverage показывается рядом. Конкретный native repair P54 ведёт core owner109; доза и новая попытка не выводятся из этой документной пометки.
