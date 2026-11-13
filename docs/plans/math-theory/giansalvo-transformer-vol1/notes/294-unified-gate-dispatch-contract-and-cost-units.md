---
title: "294. Algorithm 7: проверяемый контракт dispatch и согласованные единицы стоимости"
source_pdf_pages: [702, 705]
book_pages: [666, 669]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 294. Algorithm 7: проверяемый контракт dispatch и согласованные единицы стоимости

## Что полезно в композиции

Algorithm 7 объединяет spectral scoping, kernel filtering, выбор scalar/block refinement, выделение heads и поддержание basis. Это удобная структура контроллера, но отдельные solver guarantees не становятся автоматически гарантией всей композиции. [Algorithm 7](../source/0703-0705-ch17-unified-gate-algorithm-and-regularised-block-flow.md#algorithm-7), [Figure 17.5](../source/0703-0705-ch17-unified-gate-algorithm-and-regularised-block-flow.md#figure-17-5).

До применения каждой ветви нужен явный контракт. Krylov search должен работать в активном дополнении к уже сохранённому $Q$; иначе точные нулевые направления, созданные предыдущей дефляцией, могут повторно попасть в kernel set. Число независимых Krylov vectors ограничено active dimension, поэтому $\tau_{\max}=\min(8,K+4)$ требует дополнительного ограничения этой размерностью и обработки breakdown.

## Что нельзя удостоверить одним Ritz value

Step 2 объявляет $|\widehat\lambda_k|<\theta_{\mathrm{rank}}$ достаточным для удаления направления. Для signed оператора нулевой Ritz value может сопровождаться большим eigenpair residual даже при обратимом $A$. В [карточке 292](292-lanczos-degeneracy-qr-contract-and-batch-boundary.md) дан точный двухвекторный Lanczos-пример с residual squared $8/7$ и отсутствующим kernel.

Поэтому нужны $\|A\widehat v_k-\widehat\lambda_k\widehat v_k\|$, tolerance и spectral separation. Если при truncation меняется оператор, следует либо обновить Ritz estimates, либо ограничить ошибку их повторного использования. После фильтрации надо переиндексировать spectrum, задать domain для max в Step 3 и не делить на нулевые/неподходящие signed denominators.

Ни малая фиксированная Krylov depth, ни случайный seed сами по себе не удостоверяют попадание обоих endpoints в basin RQI. Для block branch нужно задать знак trace optimisation, число доступных независимых направлений и проверку ортогональности $[U^+\mid U^-]$. Различие между $m$ heads и $2m$ columns разобрано в [карточке 299](299-structural-heads-deflation-dimensions-and-complexity-accounting.md).

## Почему Table 17.3 нельзя читать как готовый benchmark

Таблица повторяет rate claims предыдущего раздела: $\lambda_{d-1}/\lambda_d$ не является contraction factor положительного trailing endpoint; actual gate rates зависят от learning rate. Shifted power выбирает конкурента по всем shifted magnitudes, а angular rate Lanczos нельзя сравнивать с squared angular error. Эти ограничения уже проверены в [карточке 282](282-gate-updates-versus-power-method-rates.md), [288](288-shifted-power-versus-inverse-iteration.md) и [291](291-lanczos-ritz-bounds-restarts-and-error-metrics.md). [Table 17.3](../source/0701-0702-ch17-incremental-qr-and-spectral-method-comparison.md#table-17-3).

Строки $O(d)$ для одного matvec предполагают специальный operator oracle; для общей плотной $d\times d$ матрицы сам matvec требует порядка $d^2$ операций. На следующей странице source уже использует $O(\tau_{\max}d^2)$. Строка Block MCA содержит $T$ при заголовке «cost per iteration», а PCG cost наследует ограничения indefinite shifted solve из [карточки 290](290-rqi-linear-solves-conditioning-and-empirical-cost.md).

При добавлении блока из $2m$ столбцов QR cost также зависит от $m$ и фактического количества stored columns, а не только head count $K$. Формула (17.36) не даёт общего tight cost, пока не определены operator access, inner solve accuracy, block rank и единица итерации. [Общая стоимость](../source/0703-0705-ch17-unified-gate-algorithm-and-regularised-block-flow.md#equation-17-36).

В regular regime алгоритм действительно решает сходную spectral задачу, но замена Oja на RQI меняет динамику, basin, стоимость и обработку singular shifts. «Тот же fixed point при подходящих условиях» — более узкое утверждение, чем буквальное отсутствие нового поведения.
