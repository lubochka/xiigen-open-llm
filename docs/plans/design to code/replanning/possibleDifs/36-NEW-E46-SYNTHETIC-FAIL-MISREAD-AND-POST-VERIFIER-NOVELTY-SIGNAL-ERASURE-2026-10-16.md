# N36 — «неудачный» E46 может быть неверно прочитан: 100% synthetic unknown detection ≠ 0 utility; post-verifier confidence уничтожает сигнал novelty

**16.10.2026 · CODE-SOURCE + ARCHIVED-RECEIPT VERIFIED; PRODUCT/NATIVE TRANSFER NOT_RUN; BLIND QUALITY UNKNOWN.**
Происхождение: [llm_mvp_core@216a1f3d](https://github.com/lubochka/llm_mvp_core/tree/216a1f3d6af3f40ceef081dc168b07e4ce3934f9), ветка `codex/dpo-implementation`. Писать только в `xiigen-planning@llm_plan3/replanning/possibleDifs`, закрытая папка `llm_mvp_core/docs/plans/replanning-possible-diffs` — READ_ONLY.

## 1. Почему это новый вопрос — и почему он важен для KNN

В прошлом исследовании E46, названном `ConfidenceGate`, была попытка улучшить обнаружение незнакомого домена, но протокол пометил результат **`passed=false`**, потому что относительный прирост составил `0%`. При этом сохранённый tier-1 и tier-2 результат уже содержал **`unknown_detection_rate=1.0` и `false_known_rate=0` на его СИНТЕТИЧЕСКИХ данных**. Поэтому вердикт `FAILED/ABANDON` не означает «механизм раннего отказа бесполезен»: E46 не имел пространства для прироста над успешным на этих данных baseline. И, что важнее, **экспериментировал он с вектором до принятия решения, а ныне ordinary Translation flow сообщает скаляр уже ПОСЛЕ выдачи безусловного Verifier score**, где `.99` возникает даже из слабого word-map. Следовательно, прямой перенос порога E46 на post-verifier confidence — это перенос не той измеряемой величины.

Это **новая проверяемая связка** к [N22 о некалиброванной уверенности](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) и [N35 о score .99](35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md). Она не пытается заново приписать себе открытие old `confidence != P(correct)`. Новая причинная проблема: в археологии **PASS/FAIL по относительному improvement** смешали с абсолютной полезностью и **в последующем product исчезло исходное upstream распределение confidence**, необходимое для novelty detection. Не доказано, что E46 раньше вообще входил в ProductHost: это вопрос **возможности переноса**, а не существующей регрессии.

## 2. Проверенные первичные источники

| Этап | Source / receipt | Что именно можно утверждать |
| --- | --- | --- |
| Исторический опыт E46: абсолютные метрики | [tier1 JSON 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E46-CONFIDENCE-GATE/results/run-2026-05-01-001.json), [tier2 JSON](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/results/E46-CONFIDENCE-GATE/results/run-2026-05-01-002.json) | tier1 `val=13 / unknown=8`, tier2 `val=25 / unknown=14`; baseline=enhanced=1.0, `false_known=0`, `passed=false`, `0% improvement`. **Это synthetic statistical result, не точность на реальных пользовательских запросах.** |
| Реальный источник synthetic dataset | [E46ConfidenceGate.cs L10–35, L74–97](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/Experiments/E46ConfidenceGate.cs#L10-L97) | Созданы `GenerateConfidenceExamples(Random(42))`, labels `Known/Unknown`, 5 confidence значений; для known задаётся яркий компонент `.60–.90`, для unknown — более плоские `.10–.30` до нормировки |
| Смысл historical score | [E46ConfidenceGate.cs L55–72](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/Experiments/E46ConfidenceGate.cs#L55-L72) | Используется `max(confidences[0..4])`; `TuneThreshold` на train, threshold grid `.30–.80`, validation heldout. **Не probability correctness переводчика** |
| Нынешний verifier выдаёт единицу при отсутствии правил | [QualityVerifierUnit.Verify L1052–1092](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationTrainedUnits.cs#L1052-L1092) | При пустых issue `score=1`, `Pass=true`, даже если нет проверки реального перевода |
| Adapter заменяет малый score большого verifier | [TrainedTranslationAdapter L115–140](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationDirectionAdapters.cs#L115-L140) | `min(.99,max(candidate.Confidence,verification.Score))` ⇒ `.99` при `Score=1` |
| Flow записывает лишь итоговый scalar | [TranslationRoutingFlow L85–123](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationRoutingFlow.cs#L85-L123) | `EnhancedPrimary` и `confidence` — это один scalar, никакого upstream 5-way novelty-vector в этих полях нет |

**Уточнение по границам:** E46 — старый самостоятельный `ExperimentsV2`, а не training `TranslationMachine`. Исторический `passed=false` правильно отражает отсутствие **относительного улучшения**, но не доказательство «порог плох». Синтетический prior сильно упрощает unknown-vs-known, и E46 не учится распознавать новый реальный домен. Ни один результат tier1/2 не измеряет современный KNN и не доказывает готовность к product.

## 3. Новая H36 и математический фальсификатор

**H36-1 (measurement archaeology):** если отбрасывать все эксперименты `passed=false` без чтения абсолютных метрик и baseline ceiling, можно потерять кандидатов на полезные доменные функции, включая ранний отказ/unknown gate, которые в другое место архитектуры переносятся как вспомогательная функция, не как замена learner. Tier1/2 E46 — конкретное подтверждение неправильности отождествления `passed=false` и `unknown detection failed`, но **не** подтверждение выигрыша на реальных данных.

**H36-2 (feature transport):** если novelty gate питается текущим **post-verifier scalar**, то при пустом Verifier и принятом любом word-map ответе он получает `.99` независимо от входного `max(confidences)` и не может воспроизвести E46 `unknown` threshold `<=.80`. Эта инвариантность относительно старого 5-way vector доказывается по функциям при указанной предпосылке; actual product-gate **не установлен**. Нельзя выводить `novelty` из `translation_confidence` без отдельного domain classifier / KNN coverage trace.

**H36-3 (domain-conditional):** в домене orchestration/ENRU early abstain может быть полезнее, чем ложное `TRANSLATED`; в C#/SQL AST law-check нужно проверять operator safety, для Figma — structure/DOM. «Отсутствие сигналов отклонения» у Verifier не равно `known-domain` и не равно `lawful`.

## 4. Малый быстро исполнимый эксперимент (отдельно от N35, без DPO и Windows CI)

**Панель:** 12 train/dev classifier labels и 24 заранее замороженных evaluator inputs: 8 известные ENRU source, 8 near-OOD (отрицание/порядок/смешанные имена), 8 foreign law/cross-domain. Измерить отдельно **unknown recall**, **false known**, **correct translated**, **wrong accepted**, **abstain**, **worst domain**, latency. До split зафиксировать domain family и provenance. Никогда не выбирать threshold по blind labels.

| Arm | Единственное отличие от baseline | Проверка |
| --- | --- | --- |
| A — historical E46 scorer | `max(raw_class_5way)` до verifier; threshold calibrate только по train/dev | Проверяет, переносим ли OLD signal в реальный classifier; если класса нет, `NOT_APPLICABLE`, НЕ выдумывать probabilities |
| B — post-verifier scalar | Использовать `translation_confidence` после `Math.Max(...)`; тот же threshold/calibration split | Если все принятые `.99`, unknown detector здесь деградирует по определению, даже когда A разделяет |
| C — raw candidate/novelty trace | До verifier сохранить candidate confidence, retrieval coverage, novelty distance; калибровать source-disjoint, оставить ordinary output неизменным | Проверяет, есть ли более информативный pre-verifier сигнал без возвращения к «матрице» |
| D — randomized/calibration negative control | random labels/permuted known/OOD только в synthetic shadow | Любой впечатляющий `gain` на D — marker leakage/overfit |

**Решающее условие:** полезность считать только по **реально верным и уверенно возвращённым ответам вместе с возможными отказами**, а не чистому `unknown_detection_rate` или 0% relative improvement. Если A/C не лучше B на real blind panel либо ценой сильной потери coverage, H36-2 по качеству не поддержана. Если настоящий classifier и explicit OOD signal отсутствуют, опыт ставит `FEATURE_GAP`, а не имитирует E46 результаты в новой модели.

## 5. Роль CODEX / внешней китайской модели и publication

**CODEX с Astra 6 UltraCode** выбирает реальные train/eval случаи, независимо проверяет разрешённую пару DPO и оценивает продуктовую семантику. **Внешняя открытая китайская модель генерирует ОБЕ стороны chosen и rejected** при новом реальном DPO. В данном E46/novelty-тесте финальной DPO-пары **не создаётся**: 12/24 cases — экспериментальные probes/holdouts и independent evaluation. `CODEX-ASTRA-GRADED` и `CHINESE-TEACHER-PAIRS` **NOT_RUN**; не приписывать их этому source audit.

**Смежные материалы:** [N35](35-NEW-NEGATION-OMISSION-WORDMAP-TO-099-CONFIDENCE-2026-10-16.md), [N22](22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md), [historic E09/E34 caveats](02-HISTORICAL-EXPERIMENTS.md). Вердикт: **новое требование к проверке misread historical `FAIL` и boundary качества, не готовый algorithm fix**.
