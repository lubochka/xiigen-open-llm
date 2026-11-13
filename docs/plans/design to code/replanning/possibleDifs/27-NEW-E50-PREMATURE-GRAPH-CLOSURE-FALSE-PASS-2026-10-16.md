# N27-B — E50: преждевременное graph-closure и ложная причинная интерпретация PASS; E49 donor ≠ production consumer

**Дата:** 2026-10-16. **Статус:** SOURCE_CONFIRMED / ARCHIVED_RUN_CONFIRMED / STATIC_FORMULA_CHECK / NO_NATIVE_RP01_RUN / NO_INDEPENDENT_CODEX_REVIEW. Только публикация в planning/llm_plan3/possibleDifs; core docs read-only.

## 1. Почему это важно

E50 приписал обучающейся архитектуре устойчивость графа, но фактически сравнил среднее изменение графа с последним изменением из ОДНОЙ И ТОЙ ЖЕ детерминистической траектории. Первый небольшой delta был объявлен closure, даже если на следующем шаге граф кардинально перестроился. Такой PASS нельзя использовать как доказательство обученной устойчивости Flow/Graph или того, что гипотеза о «matrix→KNN» была правильно перенесена.

## 2. Прямое чтение source и архивов

- [E50DomainGraphClosure.cs, строки 8–30](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E50DomainGraphClosure.cs#L8-L30), blob SHA cfc00dc35a0ad0443a54de9f81d44d9eaa54e0f9: порог EdgeDeltaThreshold = 0.05, 10 заранее заданных в коде profiles домена. Их признаки — синтетические int predicate sets, не параметры ученика.
- [E50DomainGraphClosure.cs, строки 32–66](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E50DomainGraphClosure.cs#L32-L66): delta=(added+removed)/max(1,currentEdges.Count+prevEdges.Count). ClosureRound фиксирует первое delta меньше .05. Нет требования 2+ последовательных окон и нет отмены closureRound, если будущий delta вырос. Primary baseline = среднее deltas, enhanced = deltas.Last; отдельного контрфактического baseline run НЕ создают.
- [E50DomainGraphClosure.cs, строки 68–104](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E50DomainGraphClosure.cs#L68-L104): результат включает derived fields, а BuildEdges выбирает Jaccard на фиксированном threshold=.35; этот код не оптимизирует параметры графа и не обучает kNN/LoRA.
- [E50 сохранённый результат 2026-04-30](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E50-DOMAIN-GRAPH-CLOSURE/results/run-2026-04-30-001.json) и [2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E50-DOMAIN-GRAPH-CLOSURE/results/run-2026-05-01-001.json), последний blob 8fce46b9ce804314857840b00992974d8e88acef: baseline=.2883, enhanced=.0714, improvement_pct=75.22, passed=true, closureRound=3/10, deltas D3=0%, D4=100%, D5=33%, D6=43%, D7=9%, D8=20%, D9=18%, D10=7%. Совпадающие последовательности — не два независимых подтверждения.
- **Прямое несоответствие заявленному абсолютному closure criterion:** финальный delta≈.0714 больше .05; ранний delta=0 на D3 — и уже D4=1.0. Нет подтверждения устойчивого closure, несмотря на PASS. Возможно, общий ExperimentV2Result считает PASS по относительному улучшению, а не по domain closure requirement; это не оправдывает научный вывод «closure достигнут».
- [17.DynamicDomains/DynamicDomainGraph.cs, строки 10–68](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/17.DynamicDomains/DynamicDomainGraph.cs#L10-L68), blob 026def695cc0404f85d182f6aff4da4a66f46bbc: production DomainGraphClosure = вычисление достижимости/числа предшественников по уже данным typed transitions. Это НЕ экспериментальный подсчёт temporal-delta, НЕ Jaccard closure, НЕ критерий ранней остановки. **Не обнаружено доказательства, что E50 closure-stop вызван из RP01 ordinary request.**
- [E49OrchestratorExpander.cs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E49OrchestratorExpander.cs#L7-L77), blob d1a488339321f1735c3bcdd89cb4859176290660: экспериментальный Jaccard predicate overlap выбирает новые зависимости. В [production OrchestratorExpander](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/17.DynamicDomains/DynamicDomainGraph.cs#L70-L121) мосты ищутся по имеющимся graph transitions; **не показан перенос самой learned/profile-matching функции**. [E49 архив](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E49-ORCHESTRATOR-EXPANDER/results/run-2026-05-01-001.json) сообщает synthetic 0.60→0.8667 PASS при 3 new domains, не подтверждает кросс-доменное обучение production.

## 3. Три новые причинные гипотезы и уровень доказательства

**N27-B1 — first-low-delta false closure.** Доказана возможность на фактическом сохранённом ряду [0,1,...,.0714]. Причинная гипотеза: premature promotion/stop в будущем обучаемом графе отрежет следующую полезную адаптацию. **Граница:** в текущем production this exact early stop не найден — НЕ установленная причина RP01.

**N27-B2 — false treatment effect от average vs last.** Доказано code-level: из одной траектории построены две разные сводные статистики, поэтому +75.22% — не эффект альтернативного обучающего алгоритма. Опровержение гипотезы о полном отсутствии полезного графового обучения требует отдельного matched baseline графа/истории обучения с независимым test topology, не одной траектории.

**N27-B3 — experimental Jaccard graph donor не дошёл до production graph consumer.** Названия E49/E50 сохранены в production, но формулы и смысл изменились. Предсказание: в реальном новой семье источников граф без донорского learned profile matcher не предсказывает дополнительные законные edges, тогда как matched augment variant предсказывает. Опровержение: call trace обнаруживает отдельный активный обучаемый scorer с теми же inputs/outcomes либо matched quality A/B нулевой.

## 4. Минимальные проверки

**Cheap static shadow (без .NET):** вход — ровно сохранённые 8 deltas; вычислить earlyClosure=min(index where delta<.05), finalPass=(last<.05), stable2=(последние два delta<.05). На архиве earlyClosure=D3; finalPass=false; stable2=false. Контроль: две завершающие 0.02,0.01 должны пройти final+stable2; искусственный sequence с ранним 0 и следующим 1 не должен пройти. Нельзя объявить этот арифметический proxy новым native performance run.

**Отдельный causal arm при наличии реального consumer:** A production unchanged graph; B тот же graph с candidate donor scorer (вызов по трассе); C frozen/shuffled wrong donor; 12 source-disjoint typed cases, заранее объявленные graph lawful dependency F1 + critical edges + consumer task success. Обучаемые Unit/LoRA не менять одновременно. Результат запрашивается только после code-path trace, иначе — честный STOP_NOT_REACHABLE.

См. [детальный runbook](27-MICRO-LOW-BUDGET-TRANSFER-AND-GRAPH-CLOSURE-2026-10-16.md).

## 5. Independent review boundary

Исторические PASS и FAIL остаются неизменными архивами. CODEX с Astra 6 UltraCode подтверждает семантические графовые зависимости/quality и человеческий выбор; внешняя open Chinese model только генерирует real DPO chosen/rejected, здесь DPO не создавались. Новая причинная метрика отсутствует до native matched run и независимого review.
