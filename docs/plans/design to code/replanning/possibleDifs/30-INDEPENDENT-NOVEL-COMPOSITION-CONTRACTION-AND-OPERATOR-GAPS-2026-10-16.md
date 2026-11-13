# N30 — независимый аудит новых пробелов: перекрывающиеся фразы, английские сокращения, исчезающие операторы

**Дата:** 2026-10-16. **Целевой repo для этого документа:** \`xiigen-planning@llm_plan3\`. **Зафиксированный исходник:** \`llm_mvp_core@79082340f01b744cdf08185616f2bd818dbd06b8\`, ветка \`codex/dpo-implementation\`. **Статус:** три **SOURCE-CONFIRMED CONDITIONS**, source-only JS proxy выполнен; реальный native .NET/AI learning A/B и независимая экспертная проверка — **NOT_RUN**. Это **не доказанные причины** 0/3 или 0/4 RP01 Student. Папка \`llm_mvp_core/docs/plans/replanning-possible-diffs\` — **READ ONLY, NOT TOUCHED**.

## 0. Зачем этот проход при наличии >200 отчётов

Уже проверенные другими агентами темы не выдаются здесь за новые: N16 — FIFO, top-k, language leakage; N20 — nonlocal candidate/reranker normalization; N21 — **пропуск непокрытых слов при сборке phrase-map**, whitespace и pseudo-accuracy; N23/N27 — Verifier rule conflicts и bias; N27 — удаление буквального \`not/no\`; N28 — Unicode и символы в admission; N29 — pseudoalignment первого слова и rejected annotation в адресе памяти. Отличие новых N30: **оба конкурирующих перекрывающихся фрагмента полностью покрывают исходную строку**, **апостроф внутри сокращения меняет полярность и теряется без наличия токена "not"**, а **нормализация склеивает слова через оператор, создавая фразу, которой в source не было**.

Ключевой урок исследования: изначальная гипотеза «переставить две lessons, и поменяется фразовый ответ» **НЕ ПОДТВЕРДИЛАСЬ аналитическим proxy**. При перекрытии в разных начальных позициях результат определяется **левым начальным адресом**, а не порядком этих двух уроков. Исправлена формулировка до публикации — это отдельный falsification record, а не замаскированный PASS.

## N30-01 · P1 · Жадный leftmost phrase-match не сравнивает альтернативные полные композиции

**Source trace:**
1. [\`ExecutorUnit.Execute\` L264–276](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L276): \`TryComposeFromPhraseMappings\` вызывается **раньше** wordmap и KNN; успешный ответ немедленно возвращается с confidence \`0.86\`.
2. [\`TryComposeFromPhraseMappings\` L798–859](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L798-L859): source разбивается в \`sourceTokens\`; phrase entries сортируются только по длине; цикл идёт \`i=0..\`, выбирает **первую** подходящую фразу на текущей позиции; при отсутствии — wordmap; \`coverage >= 0.85\` допускает ответ, но **нет поиска альтернативных сегментаций** и law-aware score.
3. [\`TrainedTranslationAdapter.TranslateAsync\` L115–142](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationDirectionAdapters.cs#L115-L142): \`-phrase-map\` confidence \`>=0.85\` может пройти даже при \`verification.Pass=false\`. Сам обход verifier известен ранее, **новый аспект — семантически иной fully-covered contender вообще не генерируется**.

**Короткий source-only контрпример:** обучение допустило \`close user → закрыть пользователя\`, \`user session → сеанс пользователя\`, два отдельно выученных слова \`close → закрыть\`, \`session → сеанс\`. Новый запрос \`close user session\` при контексте управления сеансами. Жадный \`close user\` + \`session\` даёт «закрыть пользователя сеанс». Альтернативная сегментация \`close\` + \`user session\` даёт «закрыть сеанс пользователя». Обе покрывают **3/3** исходных слова; критерий покрытия не различает их. При противоположном порядке двух lessons **обе выдачи proxy остались первым вариантом** — это существенное отрицательное наблюдение.

**Гипотеза:** на source-disjoint запросах с несколькими полностью покрывающими phrase+word composition paths, domain/role-aware rerank улучшит смысловую правильность и не ухудшит обученный exact-source retrieval. Реальный эффектор возможен **EN→RU/RU→EN и код-документация с иерархией именных/глагольных групп**. Не переносить такой reranker вслепую на чисто числовой KNN, SQL grammar или краткие уникальные термины.

**Альтернативные объяснения/контроли:** обучающий запрос полностью совпадает с \`_exactSourceMap\` и маскирует проблему; PhraseMappings не допущены admission; Verifier и другой language routing не используют candidate; сами исходные phrase pairs неверны. Проверять caller, пары, domain gold и ответ ordinary adapter. **Native result NOT_RUN**.

## N30-02 · P1 · \`can't\` и \`can\` становятся одинаковыми признаками в первом source-aware KNN guard

**Source trace:**
1. [\`TranslationKnnStore.SourceTokenRegex\` L192 и \`LowInformationSourceWords\` L194–201](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationUnitContracts.cs#L190-L201): токены через \`[\p{L}\p{N}_$]+\`, отбрасываются односимвольные и низкоинформативные, в т.ч. \`not/no\`. **Никакого специального разбора apostrophe contraction нет.**
2. [\`SourceTokens\`, \`SourceSimilarity\` L308–325](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationUnitContracts.cs#L308-L325): \`can't\` делится на \`can\` + односимвольный \`t\`, и \`t\` удаляется. Для \`we can't ship the package\` и \`we can ship the package\` выходной set в обоих случаях \`{can,package,ship}\`, similarity=1.
3. [\`TranslationMemorySourceLaw\` L14–47](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationMemorySourceLaw.cs#L14-L47): hard contradiction доказуем только для узкого набора \`is/was [not] adjective\`; здесь вернёт \`Unknown\`.
4. [\`ExecutorUnit.SourceContentTokens/SourceTokenSimilarity\` L963–999](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L963-L999): **вторая** EN→RU проверка различает \`cant/can\`, но на том же запросе пересечение двух оставшихся слов из трёх даёт 2/3=0.667, выше порога \`0.60\`, и выполняет \`minimum shared tokens=2\` ([L234–235](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L234-L235)).
5. [\`TranslationSemanticTokenizer.ClassifyPos\` L258–299](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationSemanticTokenizer.cs#L258-L299) при английском \`sourceLang\` не получает полноценной POS/polarity структуры; совпадение full signature остается условно возможным.

**Гипотеза:** при общей structural signature, отсутствии exact/phrase/word-map победителя и наличии преимущественно положительного KNN memory, запрос с отрицательным сокращением может получить противоположный перевод. **Не утверждать, что final ordinary answer уже доказан** — нужный маршрут может не достигаться; extractor\`cant\` на другом уровне частично сохраняет сигнал. Это **новый конкретный контрпример для contraction, а не новое открытие старой проблемы \`not/no\`**. Применимо **EN→RU, EN→HE и английским заданиям, требующим модальности/отрицания**; не распространять на произвольный C# neural DPO.

**Гипотеза исправления:** canonical polarity law на original raw source **до** stop-word erasure: \`cannot / can't / can not\` контрастируется с \`can\`; hard veto только при надёжно распознанной полярности/области действия; UNKNOWN не считать доказанным соглашением. Микро native A/B измеряет false positive KNN, источник, кандидат, veto verdict и обратную сторону recall.

## N30-03 · P1 / domain-specific · Потеря операторного токена создаёт *несуществующее соседство* для phrase memory

**Source trace:** [\`TranslationSemanticTokenizer.Tokenize\` L85–120](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationSemanticTokenizer.cs#L85-L120) сохраняет original tokens из \`Split(' ')\`. Но [\`ExecutorUnit.TryComposeFromPhraseMappings\` L803–808](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L803-L808) строит \`sourceTokens\` после \`NormalizeMapKey\` и **удаляет пустые**. [\`NormalizeMapKey\` L1008–1024](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/27.TranslationDomain/TranslationTrainedUnits.cs#L1008-L1024) удаляет пунктуацию и специальные символы; один исходный токен \`->\` исчезает. Получается false adjacency: \`delete -> archive\` превращается в \`delete archive\` **для фразового поиска**, и полностью изученная фраза \`delete archive → удалить архив\` может совпасть, хотя вход описывает переход между состояниями, а не «удалить архив».

**Отличие от N21:** здесь **не просто удалено неизвестное смысловое слово после частичного совпадения**. Предварительная нормализация **создаёт новую смежность** двух оставшихся слов, формируя **100% coverage** у заведомо другого синтаксического отношения. Применимость условная: source-грамматика, в которой \`->\`, \`=>\`, \`!\`, \`-\` или скобки значимы (DSL, code/graph documentation, логические правила). Для обычной EN→RU пунктуации некоторые знаки могут быть безопасно незначимыми; нужен domain policy, не глобальное запрещение.

**Гипотеза исправления:** не удалять операторные границы до phrase-span matching; хранить token-source span или \`OPERATOR_BOUNDARY\` и разрешать матч через границу только при доказанном доменном допуске. Микро: A=existing normalization, B=boundary-preserving shadow composer, same memory + source; positive examples без операторов должны оставаться достижимыми. No semantic gain claimed from proxy alone.

## 1. Что это значит и не значит для RP01

Эти три различия относятся к **TranslationMachine structured Unit → TrainedTranslationAdapter → final request**, а не к прямому \`RP01 random B0 → q/v LoRA → TextGenerator\`. До доказательства, что RP01 обычный consumer вообще подключает соответствующую structured memory, **причинный статус к RP01 = UNPROVEN/recipient mismatch**. Реальная RP01 ось P0 — случайная база, DPO pair quality, exact mask/gradient, inability to overfit, generation/decode and absence of valid trained consumer.

Исторические успешные **проверенные в архиве** результаты не объявлять универсальными:
- E26 contextual dictionary: 0.40→1.00 на 15 заданных случаях; есть seeded словарь, нужен new-family контроль.
- E14 context action KNN: 0.75→1.00 на четырёх Tier2 val; расширенный Tier3 0.8393→0.875, PASSED=false и другой баланс.
- E43 replay: retention 0.3226→0.3978 в отдельном bigram learner, не DPO.
- E09 structural KNN positive report .2413→.3507 **с разными validation subsets**, поэтому не matched causal proof.
- Реальные DPO в соседней ветке \`DesignToCode\` существуют: \`StepMany\`, optimizer, checkpoint; **train accuracy** не доказывает free-generation generalization.
- Exact source identification «та самая неудачная матрица → удачная функция KNN» **OPEN**: E07/E09/E12, E40V2 и другие — кандидаты, прямой donor SHA→diff→active KNN consumer→matched gain пока не установлен.

[Подробные позитивные источники](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md) · [E07 ошибка интерпретации](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) · [успешные branch proofs](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md) · [RP01 диагностика](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md).

## 2. Минимальный способ фальсификации

[Micro-N30 protocol](30-MICRO-NATIVE-CAUSAL-PILOTS-AND-CODEX-EXECUTOR-WORK-ORDER-2026-10-16.md); [source-only proxy](30-SOURCE-LOGIC-PROXY.mjs), [receipt](30-SOURCE-LOGIC-PROXY-RESULT.json). Каждое вмешательство — **одна переменная**, одни learner, lessons и freeze; сперва проверить \`Unit+consumer reachability\` и только потом quality. Сравнивать запрограммированный контрпример ≠ пользовательскую эффективность.

**Жёсткие DPO-роли:** роль человека по выбору случая выполняет **CODEX на модели Astra 6 UltraCode**; обе стороны final DPO \`chosen\` и \`rejected\` создаёт **разрешённая внешняя открытая китайская модель**; качество её пары и качество итоговых Student/Unit ответов независимо оценивает **CODEX на Astra 6 UltraCode**. Этот source-only аудит **не запускал** этих внешних ролей и не создавал DPO. Для каждого будущего допуска проверять модель, провайдера, права/terms, оригинальные outputs, provenance и полноценный blind judge.

**Skill stack фактически просмотрен:** \`human-readable-architecture-audit\`, \`phase-planning\`, \`plan-review\`, \`code-examination\`, \`prompt-patch-authoring\`, \`conversation-intent-and-self-reflection\`, \`experiment-design\`, \`claim-verification\`, \`learning-path-audit\`, \`training-data-gap-audit\` в \`llm_mvp_core/docs/skills\`. Упоминание skill **не** выдаётся за независимый agent/Astra review. Native/code changes/independent reviews = **NOT_RUN**.

