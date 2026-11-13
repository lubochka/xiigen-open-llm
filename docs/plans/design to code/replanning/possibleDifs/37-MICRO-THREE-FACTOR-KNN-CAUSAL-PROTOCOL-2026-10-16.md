# N37-MICRO — три коротких причинных опыта: UNKNOWN-условие, KNN tier backoff, асимметричное сходство

**16.10.2026.** Документирует эксперименты, не выполненное нативное обучение. JS source-logic proxy уже выполнен на Node v22.16.0: [исходный скрипт](37-MICRO-CASCADE-UNKNOWN-PROXY.mjs), [результат](37-MICRO-CASCADE-UNKNOWN-PROXY-RESULT.json). Он показал асимметрию 1.0 против 0.5 и условное пропадание пригодного prefix-соседа после выбора непригодного full-соседа. **Не проверено:** native C# tokenizer, реальный trained memory state, ProductHost/TranslationRoutingFlow, смысл конечных ответов, научное улучшение качества Student.

Начальный core SHA: 793d7b755b23aac90445da3ac6661b1bff7e6672. Источники и три гипотезы описаны в [N37 evidence](37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md).

## 0. Общий Gate 0: изучить действительный код и выявить получателя

- Прочитать целиком TranslationSemanticTokenizer.cs, TranslationUnitContracts.cs, TranslationTrainedUnits.cs, TranslationDirectionAdapters.cs, TranslationRoutingFlow.cs, TranslationMachine.cs, TranslationMemorySourceLaw.cs; проследить producer и consumer. Сверить свежий commit SHA: указанные line refs относятся к старому pin.
- Зафиксировать для каждого case: source/target language, topicDomain, UnitId, source_id, tokenizer Pattern, full/prefix/structure signatures после фактического Tokenize, source-admission reason, все соседские ID/targets, target-compatibility veto, chosen tier, verifier, adapter, final answer, checkpoint version.
- Проверить, не ответили ли более ранние exact-source/phrase/word-map. Если KNN не вызван, результат NOT_TRIGGERED, а не нулевое улучшение KNN.
- Подготовить на каждый действительно применимый домен 12 TRAIN, 4 DEV и 12 независимых SEALED/BLIND HELDOUT с непересекающимися исходными семействами, отдельными correct-law gold и старыми retention cases. Подбор порогов разрешён по DEV, запрещён по SEALED.
- Сначала короткий standalone TranslationKnnStore/Executor witness. Только если он совпал с анализом — actual TrainStructuredBatchAsync, TrainedTranslationAdapter и TranslationRoutingFlow. Отдельно cold restore с новым процессом. **Не нужны 500 пар DPO или GitHub Windows CI.**

## EXP37-A. Исчезнувшее условие

**Домен A:** EN→RU technical: исходный пример «null CustomerId42», contrast «null CustomerId42 without consent», и 12 независимых смысловых семейств с condition/negation/actor/recipient/only. **Контроль:** добавление бессмысленной пунктуации, а не нового закона. Первым делом проверить реальный tokenizer: могут ли разные source иметь одинаковые Signature и RuleTags; не предполагать этого заранее. Проверить, что generated training SemanticSignature и три learned storage keys действительно делают colliding candidate достижимым.

**Домен B:** RU→EN с POS и морфологией; проверять отдельно, поскольку русские слова могут создавать другие concept/rule tags. В C# AST/SQL применять только общий принцип «сохранить значение условия», но не объявлять этот Translator-класс действующим code consumer.

| Arm | Один изменяемый фактор | Ожидаемое различение |
| --- | --- | --- |
| A | Current exact full semantic signature / guard | Wrong-law accepted, correct accepted, abstain; final answer |
| B | На полном tier добавить только symmetric meaningful-source compatibility для незакодированного условия, с frozen DEV-selected threshold | Уменьшились ли ложные заимствования без потери правильных переводов |
| C | Те же пары без вызова KNN | Доказывает, влияет ли KNN на обычный ответ |
| D | No-law-change sentence control | Защищает от ненужных отказов |

**Falsify:** подписи отличаются; источник не достигает KNN; SourceLaw уже фильтрует; другой learned map выигрывает до KNN; B не улучшает final semantic gold; рост precision получен ценой отказа от большинства хороших ответов. Нельзя считать уменьшение wrong-accept без учёта coverage полноценным обучением.

**Отдельный обязательный boolean negative contrast:** одинаковая техническая строка с true и false, например «null customerId42 true» против «null customerId42 false» (все остальное неизменно). На core source только null получает NULL_LITERAL; true/false POS=TECH_TERM, Concept=UNKNOWN. Native tokenizer должен вывести полный ключ обеих сторон; затем надо удостовериться, что learned candidate другого булева значения реально может попасть в ordinary user output, а boolean-sensitive one-factor key либо guard это устраняет. Если не достигает KNN — NOT_TRIGGERED. **Это не synthetic FALSE_LITERAL: такого члена SemanticConcept нет.**

## EXP37-B. Ранний return full tier перед downstream eligibility

**Домен:** EN→RU technical с настоящим full/prefix key различием. Trigger можно начать с «null customerId42 requestFlag9» (NULL_LITERAL и два CODE_IDENTIFIER), но обязан проверить реальные RuleTags, keys и similarity, а не переносить буквальный JS proxy как C# output.

**L0:** standalone store: full bucket содержит лишь wrong-script neighbor, source admissible; prefix bucket — lawful Cyrillic candidate с overlap выше 0.25. Current FindNearestCascade вернёт непустой full; Executor отсеет target и не проверит prefix. Вмешательство меняет только продолжение к следующему tier, **когда все кандидаты предыдущего tier уже отвергнуты существующими downstream законами**. Не ослаблять языковую/семантическую валидацию, не менять k, источник, порядок обучения и модель.

**L1:** построить approved trained examples и показать тот же trigger в настоящем Unit/adapter/flow; direct inserted wrong-script lesson является только negative fixture и **не должен быть легализован как good production teacher data**. При невозможности создать законный реальный scenario — verdict MECHANISM_ONLY, L1 NOT_TRIGGERED.

Controls: удалить неверный full (оба варианта видят prefix); положить допустимый full (оба должны остановиться на нём); добавить bad prefix (никакого обхода verifier); повторить после свежего process restore. Ключевой output: tier candidates before/after target filter, final correct/wrong/abstain. Никакой вывод о Student q/v LoRA.

## EXP37-C. Directional containment против symmetric matching

**Домен:** технический текст с расширениями условий, только prefix/structure tiers (не full без source guard). SourceTokenRegex/stopword-set и формула в [TranslationUnitContracts.cs L186–200, L308–325](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1bff7e6672/27.TranslationDomain/TranslationUnitContracts.cs#L186-L325).

A: текущий overlap(query, source) / count(query). B: симметричный Jaccard на тех же исходных токенах и **том же пороге**, без перенастройки по blind answers. C: переставить query/source при fixed predicate; D: длинное, но допустимое расширение без смены значения, где B не должен ухудшить coverage.

Pre-registered toy для threshold 0.35: у short query два содержательных токена, у long lesson шесть, включая эти два. Актуальная формула даёт short→long=1.0, long→short=2/6≈0.333; Jaccard=2/6≈0.333 в обоих направлениях. Нативно измерить **реальные token sets и выбранный tier**; в full tier threshold=0 эта разница может вообще не использоваться. Смысл extra phrase judge оценивает отдельно. Не путать симметрию расстояния с правильностью перевода.

## Общая проверка итогового качества

| Метрика | Обязательна? | Интерпретация |
| --- | --- | --- |
| correct_final / total independent blind | Да | Настоящий успех конечного ответа |
| wrong_accepted / critical negation, operator, actor | Да | Не навредил ли поиск |
| abstained / total, answered / total | Да | Не принимать abstain-all за высокий precision |
| paired B_better/A_better/tied на тех же frozen IDs | Да | Причинный paired contrast |
| old-law retention и fresh process checkpoint parity | Да | Обучение не разрушило прежние навыки |
| neighbor recall, tier reached, collision rate | Диагностика | Доказывает механизм, но **не** успешный перевод |
| peak RAM, latency, source/manifest SHA, training legal provenance | Да | Воспроизводимость и цена |

12 blind cases — маленький пилот, не сертификат 95%. Отсутствие независимого gold = INCONCLUSIVE. Native .NET не запущен = NOT_RUN. Улучшенный source-logic proxy без consumer gain = MECHANISM_ONLY. Blind correct_final прирос без критических ошибок и больших coverage losses = SMALL_POSITIVE_SIGNAL, не generalization proven. Параллельный Student DPO не включать в этот причинный contrast.

## Исполнительское поручение и проверка плана

**CURRENT TASK:** эксперимент N37 один фактор за раз. **NOT THE TASK:** полное переобучение Student, rewrite KNN, перепланирование RP01, закрытие всех 292 существующих findings, Windows/GitHub CI. **Skills:** architect-behavior-classifier, phase-planning, plan-review, code-examination, prompt-patch-authoring, human-readable-architecture-audit, experiment-design, trainable-architecture-decision (из llm_mvp_core/docs/skills). **Forbidden files:** llm_mvp_core/docs/plans/replanning-possible-diffs/**, frozen RP01 datasets/results, existing reviewed artifacts. **Allowed write:** новые исследовательские файлы в xiigen-planning/llm_plan3/replanning/possibleDifs и отдельная *локальная* экспериментальная копия кода, не production modifications.

**GOOD:** «На source-family-disjoint N=12 правильных user-flow ответов 3→6, wrong 4→1, abstain 5→5; тот же frozen state, one-factor source guard; cold restore совпал». **BAD:** «KNN теперь идеально обучается: 100% JS proxy и 200 green tests» без реального конечного ответа. Reviewer требует exact code/commit, caller/consumer trace, именованную причину rejected, blind per-case verdict, observed vs inferred tags и независимое чтение изменённого кандидата. Не считать review PASS без кандидатного SHA.

Return packet: source SHA; dataset hash/family split; baseline/variant diff; command+exit code; key dumps; per-case target admission; final answers/independent ratings; coverage and old-law retention; cold restore; cost; verdict; remaining open questions. Документы должны оставаться понятными по-русски, технические идентификаторы вторичны.
