# N38-A — новая условная ошибка: forward DPO использует дробную маску, backward — бинарную

**16.10.2026 | SOURCE-CONFIRMED MATHEMATICAL DISCREPANCY | NUMERICAL PROXY VERIFIED | NATIVE .NET NOT_RUN | LEARNING QUALITY NOT_MEASURED.**

**Repo с документацией:** xiigen-planning / llm_plan3 / replanning/possibleDifs. **Исходный код только для чтения:** llm_mvp_core / codex/dpo-implementation @ aa284ba21790c5d7601328997cd6174c85cee878. Папка core/docs/plans/replanning-possible-diffs остаётся неизменной.

## 1. Для человека — почему это может портить обучение

Если будущий конструктор DPO-урока передаст дробный вес критического токена, например 0.25 для вспомогательного слова и 1.0 для смыслового, **прямой расчёт признаёт эту разницу**, но **backward учит модель так, будто веса одинаковые**. В результате отчётный loss и фактическое изменение модели не являются производной одной и той же функции. Отдельно от обычного вопроса, правильно ли выбраны chosen и rejected.

Это **не доказанная причина отрицательного исхода RP01**: текущий exact-v2 использует зафиксированные бинарные 0/1 response masks. Риск касается legacy/material и будущих caller-ов, которые захотят маскировать признаки непрерывными весами. Нельзя рекомендовать fractional masks для RP01 без native и quality A/B.

## 2. Кодовые доказательства, точные методы

- [DpoBatch: legacy/material contract](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/Models/DpoBatch.cs#L10-L30): публичный Tensor ChosenMask/RejectedMask; legacy-full-sequence-mean-v1 — значение MaterialVersion по умолчанию. Комментарий описывает обычную 0/1 mask, но тип Tensor **не кодирует** бинарность.
- [DpoExactPairMaterial.CaptureAndValidate](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/Models/DpoExactPairMaterial.cs#L92-L114): legacy при отсутствии exact дополнительных полей возвращает snapshot; проверка конкретных значений legacy mask в этом участке отсутствует. **Не путать** с exact-v2, где маска создаётся через Array.Fill(1f) и валидируется по identity.
- [DpoTrainer.ComputeTargetLogProbs L1036–1068](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/DpoTrainer.cs#L1033-L1069): прямой расчёт добавляет logP * maskVal и cnt += maskVal. Для legacy возвращает sum/cnt.
- [DpoTrainer.BackwardLogProbs L1072–1119](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/DpoTrainer.cs#L1072-L1119): cnt увеличивается **на единицу** на каждой позиции, если mask > 0; градиент добавляется как (oneHot-softmax) * upstream/cnt **без множителя maskVal**.
- [TargetMask и ForwardPolicy L1002–1030](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/11.DPO/DpoTrainer.cs#L1002-L1030): тот же legacy mask используется в прямом и обратном проходах; отдельного нормализующего adapter там нет.

### Проверяемая математика

Пусть одна binary-logit координата z задаёт вероятность правильного токена sigmoid(z) в каждой из двух позиций. При legacy mask = [0.25, 1.00], z = [-1, 1], прямой score:

~~~text
S(z) = [0.25 * log(sigmoid(z1)) + 1.00 * log(sigmoid(z2))] / 1.25
истинный dS/dz_i = [mask_i / 1.25] * [1 - sigmoid(z_i)]
текущий BackwardLogProbs = [1 / 2] * [1 - sigmoid(z_i)]
~~~

| Координата | Центральная численная разность | Верная аналитическая производная | Производная, реализованная в backward | Сдвиг |
| --- | ---: | ---: | ---: | ---: |
| z1 = -1, mask 0.25 | 0.146211716 | 0.146211716 | 0.365529289 | ×2.500 |
| z2 = +1, mask 1.00 | 0.215153137 | 0.215153137 | 0.134470711 | ×0.625 |

Числа получены независимым JS-примером [N38 source proxy](38-MICRO-DPO-MASK-FINITE-DIFFERENCE-PROXY.mjs) и [receipt](38-MICRO-DPO-MASK-PROXY-RESULT.json), без исполнения C# тренера. При mask=[1,1] и при exact-v2 бинарных response masks расхождение **исчезает**. Это обязательный отрицательный контроль.

В DPO есть дополнительный upstream от softplus/sigmoid margin: он умножает обе производные, но не исправляет их относительное расхождение. Если upstream=0, конкретный шаг может иметь нулевой вклад; сам контракт математически остаётся несогласованным.

## 3. Границы новизны и релевантности

Это **не** известный дефект [N16 translation hashed DPO](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md): тот теряет model-visible текст и сравнивает две objective; здесь новая ось — **несоответствие производной и оцениваемого score при дробных mask**, даже когда ID, objective и pair уже корректны. [Core gradient review](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md) разбирает именно бинарный exact-v2 case, не утверждает поддержку fractional legacy masks. По просмотренным картам N30/N33/N37 отдельной записи на эту формулу не найдено. **Глобальная уникальность среди всех 330 файлов не доказана.**

- **C# и Design-to-Code:** релевантно, если caller захочет усилить вес идентификаторов, операторов или AST-critical tokens именно дробной legacy mask. Нет такого доказанного current caller в RP01.
- **EN↔RU, HE↔EN:** релевантно при маскировке артиклей и акценте на отрицании, роде, контексте; без native подтверждения это только scope.
- **Любой exact-v2 RP01:** недостижимо с текущим зафиксированным mask builder, поэтому **НЕ менять current RP01 для исправления этой причины**.

## 4. Минимальный фальсификатор

Один минимальный C# native тест в disposable harness: построить **допущенный legacy DpoBatch** с положительными дробными масками, двумя активными target positions и фиксированными logits в одном маленьком LlmModel; сравнить производные DPO-loss по одному доступному LoRA scalar с центральной разностью. Повторить с mask=[1,1] и exact-v2 response-only masks. Не запускать Windows CI, не требовать 500 pairs. До изменения code проверить, принимает ли реальный server-owned caller fractional mask — если нет, маркировать finding LATENT_NOT_REACHED.

Различение результата: SOURCE_CONFIRMED_MATH → NATIVE_CONFIRMED only после C# → STUDENT_DOMAIN_GAIN only после отдельного обучающего A/B с frozen source-disjoint law cases. Нельзя превращать toy derivative PASS в заявление об улучшении качества.

**Независимый review:** проверить Tensor.Backward semantics и не приписать двойную аккумуляцию (см. [Tensor.Backward](https://github.com/lubochka/llm_mvp_core/blob/aa284ba21790c5d7601328997cd6174c85cee878/00.TensorCore/Core/Tensor.cs#L94-L102): стартовый gradient копируется), actual caller, статус до и после.