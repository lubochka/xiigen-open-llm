# N35-A — E09 Structural KNN: сохранённый SiblingCount не способен влиять на выбор соседа

**Дата:** 16.10.2026. **Источник:** llm_mvp_core@216a1f3d6af3f40ceef081dc168b07e4ce3934f9, branch codex/dpo-implementation. **Статус:** SOURCE_CONFIRMED_FEATURE_BLINDNESS / QUALITY_EFFECT_NOT_RUN / PRODUCT_ROUTE_NOT_CONFIRMED. Это дополнительная проверяемая гипотеза, а не утверждение о причине RP01 Student 0/3 и 0/4. **Запрещённый для правок core/docs/plans/replanning-possible-diffs не изменяется.**

## Коротко человеку

В исторически перспективном структурном C# KNN вычисляется число соседних дочерних AST-узлов и сохраняется в векторе. Но функция расстояния вообще не использует это поле. Два различных по числу аргументов/детей синтаксических контекста при прочих равных становятся абсолютно неразличимыми для KNN. Этот эффект не эквивалентен ранее найденным проблемам E09 с несовместимыми ID целевых меток, равенствами голосов и ложной границей между файлами.

**Что доказано, а что нет:** поле исключено из метрики и потому не может само улучшить ranking — факт из исходника. Что именно этот сигнал нужен для правильного продолжения AST в реальных C# примерах — ещё гипотеза; нельзя приписывать ей найденные ранее архивные проценты улучшения E09.

## Первичные ссылки и механизм

1. [StructuralKnn.cs L12–35](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/12.SemanticCompiler/Prediction/StructuralKnn.cs#L12-L35): StructuralVector содержит AstNodeType, три Parent*Type, PositionInParent, **SiblingCount**, DepthInMethod, ScopeDepth, **FileId**, SyntacticRole. DistanceTo использует только роль, parent kinds, node kind, позицию, scope/method depth. Ни SiblingCount, ни FileId в формуле нет.
2. [StructuralKnn.cs L41–95](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/12.SemanticCompiler/Prediction/StructuralKnn.cs#L41-L95): Extract действительно считает node.Parent.ChildNodes().Count и помещает значение в SiblingCount (с насыщением на 255). Это не отсутствующий входной признак.
3. [StructuralKnn.cs L105–181](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/12.SemanticCompiler/Prediction/StructuralKnn.cs#L105-L181): Predictor.Train сохраняет полные векторы, Predict сортирует только по DistanceTo, берёт top-k и голосует за следующий токен; следовательно SiblingCount не может разрешить неоднозначность.
4. [Соседняя language_translate](https://github.com/lubochka/llm_mvp_core/blob/language_translate/12.SemanticCompiler/Prediction/StructuralKnn.cs) и [DesignToCode](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/12.SemanticCompiler/Prediction/StructuralKnn.cs): та же пропущенная переменная в метрике (сверены SHA исторического файла f5132ce4800082797ca7d0197373cbdbd82fdcae). Это **унаследованное ограничение**, а не установленная регрессия RP01.
5. [N26: доказанный Git-перенос E09 и изменения label/tie-break](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md), [N32: межфайловые псевдопереходы исторического E09](32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md). Две эти причины **не считаются нашим новым результатом**.
6. [RP01 actual consumer и отсутствие free improvement](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): сначала доказать маршрут именно от данного StructuralKnnPredictor до ответа продукта, а не переносить E09 на q/v-LoRA Student.

**Строгий механический контрпример (расчёт по формуле, НЕ запуск C#):** пусть vector A и vector B совпадают во всех полях, кроме SiblingCount=1/3; query совпадает с A и имеет SiblingCount=1. DistanceTo(query,A)=DistanceTo(query,B)=0. При k=1 и сохранении обоих кандидатов в разном порядке прогноз определяется порядком вставки, а не различием числа детей. Если correct label для этих двух контекстов различен, существующая метрика принципиально не способна это разучить. Это условный контрпример; реальные Roslyn пары могут иметь и другие различающиеся признаки.

**FileId отдельно:** он также исключён из DistanceTo, но это может быть **осознанно полезной** инвариантностью при переносе правил между файлами. Не добавлять штраф по FileId без независимого, file-disjoint подтверждения; иначе исправление одной оси разрушит перенос. SiblingCount и FileId — разные hypotheses.

## MINI-E09-SIB (C# syntax / Design-to-Code), одна изменяемая ось

- **L0 contract test, 2 минуты кода без обучения:** создать два StructuralVector, равных во всём кроме SiblingCount=1 и 3. Убедиться в двух нулевых расстояниях. В тестовом, не production scorer вычислить дополнительный term w * min(abs(siblingCountDifference), cap); проверить ожидаемый порядок для двух запросов. w предварительно зафиксировать, например 0.15, cap=4. Это только проверка механизма.
- **L1 честная учебная панель:** CODEX/Astra 6 UltraCode выбирает 8–16 настоящих C# синтаксических ситуаций (например, первые/последние элементы argument list, initializer и tuple), где одинаковые базовые поля и **различающийся SiblingCount** действительно требуют различных next SyntaxKind. Сохранить ParseTree, SyntaxKind и family IDs; проверить, что такие пары вообще существуют. Если нет — verdict FEATURE_NOT_DISCRIMINATIVE, остановиться.
- **A:** исходный DistanceTo. **B:** только добавление указанного слабого штрафа за SiblingCount. Те же frozen train-векторы, next-kind labels, k, tie-break, insertion order, train/heldout split и runtime; не менять AST extraction, label codec, LoRA, BPE или количество данных.
- **Holdout:** 8–16 unseen source-family-disjoint C# файлов с ролью и порядком детей; одновременно сохранить обычную случайную C# panel, где количество соседей несущественно, для защиты от регрессий. Оба arms должны оцениваться на *одних и тех же* позициях/метках. Метрики: coverage of discriminating pairs, conditional accuracy@1, overall accuracy@1, wrong-kind critical, mean neighbor margin, changed-decision count, latency и false-block. До измерения не подбирать w по holdout.
- **Критерий принятия:** B улучшает отложенные примеры, в которых SiblingCount действительно разделяет gold, без потери иных C# семейств. Если изменилось расстояние, но не gold correctness — MECHANISM_ONLY, не новый успешный алгоритм. После cold load проверить повторение результата, только если predictor включён в сохранённый runtime.
- **Неприменимость:** для EN↔RU/HE↔EN словоформы, отрицания, порядка предложений нельзя использовать Roslyn sibling count. Там нужны dependency/POS/word-order features и отдельная валидация, см. [доменный KNN crosswalk](33-MASTER-CROSSWALK-QUALITY-LOSS-AND-NOVEL-N33-2026-10-16.md).

## Роли и контроль качества

**CODEX с моделью Astra 6 UltraCode** исполняет человеческую роль выбора примера/правила; отдельно CODEX/Astra 6 UltraCode оценивает качество итогового ответа. Если тест потребует новых DPO pairs, **внешняя открытая китайская модель** создаёт **обе** финальные стороны chosen и rejected; CODEX/Astra независимо проверяет обе пары до допуска. Этот документ не утверждает, что вызовы модели/оценщика, нативные прогоны или независимый code review уже выполнены.

**Decision:** P1 для C# AST-KNN-потребителя, если caller и discriminating cases существуют; P3 / NOT_APPLICABLE для RP01 q/v-only Student без KNN или языкового перевода без AST.
