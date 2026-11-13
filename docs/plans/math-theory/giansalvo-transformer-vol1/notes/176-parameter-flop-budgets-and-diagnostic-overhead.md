---
title: "176. Бюджеты параметров и FLOPs: условия формул и стоимость diagnostics"
source_pdf_pages: [514, 516]
book_pages: [478, 480]
batch: "011"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 176. Бюджеты параметров и FLOPs: условия формул и стоимость diagnostics

[Индекс](../README.md)

## Какие предпосылки стоят за стандартным бюджетом

[§14.5.1](../source/0512-0515-ch14-sa-channels-and-cost-accounting.md#section-14-5-1) использует $H$ голов ширины $d_k=d/H$, одинаковую value-width и модельную ширину $d$. Без bias четыре projection matrices дают

$$
P_{\mathrm{att}}=4Hdd_k=4d^2.
$$

Обычный двухматричный FFN ширины $d_{\mathrm{ff}}$ имеет $2dd_{\mathrm{ff}}$ параметров. При $d_{\mathrm{ff}}=4d$ весь attention+FFN блок имеет $12d^2$ параметров. Здесь не посчитаны bias, нормализация, дополнительные gates, embeddings и output head. Для gated FFN с дополнительной матрицей или неравных Q/K/V widths формула меняется.

Это корректный базовый подсчёт при указанных shapes. Он не доказывает избыточность ширины $4d$ или минимальность другого числа. Ошибки прежних lower bounds ранга и FFN подробно разобраны в [166](166-head-output-rank-and-feedforward-width.md) и [167](167-specialisation-aware-width-and-self-sufficiency.md).

## Число параметров не определяет FLOPs независимо от длины

Для плотного forward на последовательности длины $n$, при стоимости умножения и сложения как двух FLOPs, ведущие члены равны

$$
F_{\mathrm{att}}\approx8nd^2+4n^2d,
\qquad F_{\mathrm{ff}}\approx4ndd_{\mathrm{ff}}.
$$

Первый член attention — Q/K/V/output projections; второй — $QK^\top$ и multiplication by values. Здесь опущены softmax, activation, normalization и прочие меньшие для данного режима операции; это модель полного плотного forward, а не декодирования с cache.

При $d_{\mathrm{ff}}=4d$ получается

$$
F_{\mathrm{block}}\approx24nd^2+4n^2d.
$$

FFN превышает весь attention по этим ведущим членам лишь при $n<2d$. На длинных последовательностях квадратичный attention term меняет соотношение. Поэтому замечание источника о доминировании FFN нельзя читать как независимое от длины правило. Реальное время также зависит от batching, memory traffic, kernel shapes и устройства; подсчёт FLOPs сам по себе его не измеряет.

## Разложение рационализированного бюджета — бухгалтерская рамка

[§14.5.2](../source/0512-0515-ch14-sa-channels-and-cost-accounting.md#section-14-5-2) записывает

$$
P_{\mathrm{block}}^{\mathrm{rat}}
=P_S+P_A+P_M+P_{\mathrm{gate}}+P_{\mathrm{out}}
+2d\,d_{\mathrm{ff}}^{\mathrm{rat}}+P_{\mathrm{route}}.
$$

Термы нужно определять как непересекающиеся наборы параметров; например, output projections нельзя одновременно включать в $P_S$ и $P_{\mathrm{out}}$. Само разбиение одного массива на группы не меняет бюджет. Но независимые полноразмерные S/A matrices, дополнительные attention passes или adapters могут увеличить и число параметров, и вычисления. Утверждение о lower-order overhead требует конкретного масштабирования этих компонентов с $d,H,n$.

Схематическая ширина $d+\lambda_Sr_S+\lambda_Ar_A+\lambda_Rr_{\mathrm{res}}$ прямо названа allocation formula, а не законом. Для исполнения нужны определения effective ranks, коэффициенты, округление до целого, допустимая минимальная ширина и связь с качеством. Это не автоматически следствие Pythagorean identity для S/A.

## Дешёвое применение gate и дорогая оценка его входов

Умножить уже известный scalar gate на head output относительно дёшево. Получить $V_{\mathrm{marg},h}$, orbit metric и coverage может стоить существенно больше. Точный loss marginal требует сравнения исходной модели с вмешательством; даже при повторном использовании общих tensors downstream computation может повторяться для разных heads. Простая попарная метрика траекторий из $T$ checkpoints и $q$ координат уже имеет порядок $H^2Tq$; transport metrics могут быть дороже.

[§14.5.4](../source/0512-0515-ch14-sa-channels-and-cost-accounting.md#section-14-5-4) разумно допускает периодические training-time estimates и замораживание allocations. При этом следует отдельно учитывать частоту оценки, объём сохранённых trajectories, стоимость interventions и inference path. Если gate зависел от текущего input, замена его константой меняет функцию, за исключением специально проверенных случаев; «compile into a mask» не является автоматическим алгебраическим равенством.

## Три режима и критерий сравнения

[Table 14.2](../source/0512-0515-ch14-sa-channels-and-cost-accounting.md#table-14-2) различает iso-parameter, compression и expansion. Источник прямо не обещает, что rationalised block всегда меньше или дешевле. Для Xiigen полезно сопоставлять фиксированную задачу и качество, общий training compute вместе с diagnostics, peak memory и inference cost после выбранного упрощения.

Compression может уменьшить физический tensor shape и дать экономию; простое зануление некоторых коэффициентов плотной матрицы может не уменьшить выполняемую плотную операцию. Expansion остаётся проектным выбором по diagnostics, а не доказательством, что исходный блок математически недостаточен. В этом проходе приведены расчёты и чтение источника; training runs и измерения скорости не выполнялись.
