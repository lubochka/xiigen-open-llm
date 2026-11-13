# N26-A — Потеря структурных преобразований E27 при переносе в Translation ExecutorUnit

**16.10.2026. Состояние: новое SOURCE-CONFIRMED расхождение; native causal run НЕ выполнен; независимый code review НЕ выполнен.** Источник: llm_mvp_core, ветка codex/dpo-implementation, прочитанный HEAD b0afc08dc0df110b10ed5ed38b518906665c16f8. Назначение: дополнить, а не заменить 01-PRIORITIZED-DIFFERENCES, 10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER, 23-NEW-E26-POS-CONTEXT и исторический реестр E27. В папке docs/plans/replanning-possible-diffs другого репозитория ничего не менять.

## Человеческое объяснение

Старый эксперимент E27 проверял **перестановку структуры целого предложения**: SOV→SVO, VSO→SVO, TOPIC_OV→SVO и удаление лишнего местоимения из императива. Нынешний метод, названный E27 в переводческом ExecutorUnit, делает **только одну локальную перестановку соседних понятий**: VALIDATE_CHECK + NULL_LITERAL. Это не один и тот же алгоритм даже при безошибочной передаче словаря, LoRA и DPO.

Следствие: если обучаемый словарь правильно выучил переводы отдельных слов, но порядок нового предложения SOV или VSO требует структурной перестройки, текущий fallback может построить неверный порядок. Это объясняет возможное ухудшение **качества гибридного переводчика**, но **не** напрямую нулевые свободные ответы isolated RP01 Student, который старый ExecutorUnit не вызывает. Для RP01 нужно отдельно доказать связь через конкретный consumer/adoption.

## Прямые доказательства

1. [Историческая реализация E27, строки 5–51](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/35.ExperimentsV2/Experiments/E27PatternReorderer.cs#L5-L51) прямо задаёт PatternReorderer.Reorder(inputWords, sourcePattern) и ожидаемое соответствие порядку слов. [Набор семи случаев, строки 66–108](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/35.ExperimentsV2/Experiments/E27PatternReorderer.cs#L66-L108): SOV (в том числе с предлогом), VSO, IMPERATIVE_V, IMPERATIVE_VS, TOPIC_OV, SVO.
2. [Сохранённый результат E27 от 01.05.2026](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/35.ExperimentsV2/results/E27-PATTERN-REORDERER/results/run-2026-05-01-001.json): baseline 0.2857, enhanced 1.0 на **семи заранее заданных** случаях; это *не* результат обучения модели. Сам исходник E27 честно указывает: «No ML here — pure rules».
3. [Текущий ExecutorUnit.ApplyE27Reorder, строки 731–742](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/27.TranslationDomain/TranslationTrainedUnits.cs#L731-L742) не принимает SentencePattern, SourcePattern, POS-последовательность или reorder table. Его единственная условная ветка: если соседние понятия VALIDATE_CHECK и NULL_LITERAL, поменять их местами. **Исходный код прямо доказывает сокращение набора трансформаций.**
4. [Текущий ExecutorUnit.Execute, строки 264–329](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L329): exact-source→phrase-map→word-map→KNN→fallback; ApplyE27Reorder вызывается при построении fallback. [Training, строки 403–455](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455) действительно кладёт chosen в обучаемые словарь/phrase/KNN. **Это другой, обучаемый слой**, эффект которого следует отделять от фиксированного reorder.
5. [RP01 current simulator](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/tools/Rp01SimulationDpo/Program.cs): training строит LoRA Student; наличие класса ExecutorUnit не означает, что его E27 вызван при RP01 генерации.

### Что нельзя утверждать

- Нельзя переносить 100% E27 на языковую генерацию, открытые диалоги, другой корпус или unseen задачи: семь фиксированных случаев и полностью известный SourcePattern.
- Нельзя приписать все случаи нулевого RP01 free score сокращению E27: пока не доказан вызов данного метода в анализируемом RP01.
- Нельзя назвать сам E27 «нейронным обучением»: это удачный *алгоритмический компонент*; обучение может происходить в word maps/KNN, которые подают ему данные.
- Если exact-source или phrase/KNN перехватывают запрос, текущий reorder вообще не является причиной результата.

## Минимальный причинный опыт MICRO-N26-A: 7+7 без Windows CI и без 500 DPO

**Цель:** установить, теряет ли нынешний обычный переводческий consumer качество на новых предложениях именно из-за отсутствующих перестановок при **неизменной обученной памяти**.

- **A (baseline):** текущий ExecutorUnit fallback со своим ApplyE27Reorder.
- **B (enhanced):** тот же Frozen trained ExecutorUnit / word maps / tokenizer / candidate state, но для одного локального исследовательского вызова применить старый PatternReorderer.Reorder с корректно переданным SourcePattern и POS вместо сокращённой функции. Не менять DPO, LoRA, train corpus, другие условия.
- **Control C:** тот же B с ошибочно указанным или неизвестным SourcePattern; должен проиграть или отказаться. Это проверит, что эксперимент не получает oracle-метку структуры при работе.
- **Сначала:** воспроизвести семь старых E27 cases отдельным structural-unit тестом. Это лишь проверка semantic parity, не доказательство нового обучения. Затем собрать **семь НОВЫХ** source-disjoint предложений двух и более структур, без exact-source и phrase/KNN совпадения; не вставлять их в train. Если SentencePattern не получается из фактического токенизатора/входа, зафиксировать INPUT_CONTRACT_MISSING и прекратить заявление о native gain.
- **Primary:** доля правильного порядка/значения на 7 новых источниках, которую независимо оценивает frozen rubric; вторично exact_match, law violations, SourcePattern detection, fraction actually reached fallback, coverage, abstentions.
- **Evidence:** на каждом кейсе source text hash, learned-state SHA, route trace, pattern supplied by ordinary parser (не gold), A/B outputs, verifier verdict, ошибочное/правильное место перестановки, fixed judge rubric, same PC process.
- **Заранее заданный признак эффекта:** B проходит не менее двух новых случаев, где A ошибается, и не портит ни одного случая, где A был прав; иначе NOT_CONFIRMED. Это диагностический порог 7 кейсов, не статистическое доказательство production-качества.
- **Failure label:** ORIGINAL_E27_PARITY_FAIL, PARSER_NO_PATTERN, FALLBACK_NOT_REACHED, QUALITY_DELTA_NOT_PROVED, NATIVE_RESULT_PENDING. Ни один из этих ярлыков не превращать в GREEN после переписывания fixture.

**Повторный этап только при реальном улучшении:** на равных новых supervised word-map lessons сравнить pre→post learned map у A/B, с отдельным C=frozen/no-learn и source-disjoint holdout; тогда вывод касается **полезности структуры для переноса выученных слов**, а не чудесного обучения детерминистического E27.

## Отличие от существующих отчётов

- DIF-40 про curriculum; N23-E26 про контекст/POS lexical mapping; [10-INFERENCE-STOP-TOKENS](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md) лишь перечисляет E27 и не сравнивает **размер исторической таблицы структур** с единственной текущей swap-веткой. Здесь новое — code-to-code behavioral parity и native falsifier на структурном преобразовании.
- Зависит от родительского [исторического алгоритма создания DPO/Units](../../Алгоритм%20создания%20ДПО.md), [RAG master index](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md), от текущего [RP01 source/architecture](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md) и родословной [исторических экспериментов](02-HISTORICAL-EXPERIMENTS.md).

**Итог:** SOURCE-CONFIRMED алгоритмическое сужение **7-структурного E27** до **одного семантического swap**. Эффект на обучение/обычный ответ — гипотеза, ожидающая A/B, а не объявленный факт. Не изменять текущий продукт до такого опыта.
