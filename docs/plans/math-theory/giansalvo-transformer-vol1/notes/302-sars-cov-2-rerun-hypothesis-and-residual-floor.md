---
title: "302. SARS-CoV-2 rerun: проверяемая гипотеза и масштаб numerical-kernel floor"
source_pdf_pages: [715, 716]
book_pages: [679, 680]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 302. SARS-CoV-2 rerun: проверяемая гипотеза и масштаб numerical-kernel floor

## Три головы вместо шести — предсказание, а не результат

Раздел 17.6.4 интерпретирует flattening residual trajectory после третьего growth event и предлагает rerun с unified gate. Автор прямо пишет, что такой rerun **не выполнен**, и формулирует диагностическую гипотезу: около трёх structural units вместо шести без потери test accuracy. В библиотеке это отдельное prediction, не theorem и не подтверждённый empirical outcome. [Исходная интерпретация](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#section-17-6-4), [diagnostic prediction](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#prediction-0716-sars-cov-2-rerun).

Сам scalar plateau не определяет его причину. Он совместим с numerical kernel, малыми ненулевыми modes, особенностями optimisation, движущимся target и другими ограничениями данного controller. Для выбора между этими механизмами нужны spectrum estimates, certified residuals, margins до thresholds и logs accepted/rejected events.

## В source присутствуют две разные гипотезы

На PDF 715 рассматривается mixed regime: часть modes ниже $\theta_{\mathrm{rank}}$, а часть выше него, но ниже growth threshold $\theta_w$. На PDF 716 prediction объясняет все три дополнительные головы именно Type-C kernel directions. Это более узкое утверждение, чем отсутствие достаточного growth gain.

Если rerun даст три головы, но исключённые modes окажутся non-kernel и лишь sub-growth, sparsity prediction может подтвердиться, а предложенный Type-C механизм — нет. Поэтому head count и механизм остановки следует оценивать отдельно. Аналогично шесть heads не определяют единственную альтернативную причину: изменение trajectory и finite-budget accuracy тоже требуют контроля.

## Чему соответствует формула kernel contribution

Источник пишет

$$
\Gamma_{\mathrm{ker}}
=\sqrt{\sum_{k\in K_C}\lambda_k^2}.
$$

Это Frobenius **norm** spectral tail, а не её квадрат. Если $r$ точных eigen-directions классифицированы как numerical kernel по $|\lambda_k|<\theta_{\mathrm{rank}}$, то непосредственно

$$
\Gamma_{\mathrm{ker}}\le\sqrt r\,\theta_{\mathrm{rank}}.
$$

При prescription $\theta_{\mathrm{rank}}=\sqrt{\varepsilon_{\mathrm{mach}}}\|A\|$ получаем $\Gamma_{\mathrm{ker}}\le\sqrt{r\varepsilon_{\mathrm{mach}}}\|A\|$. Здесь $r$ считает directions, не автоматически heads, и нужно учитывать точность eigenvalue estimates. Подмена $\|A\|$ величиной $\Gamma_0$ требует согласованных norms и установленной upper bound.

Numerical kernel по положительному cutoff может содержать малые **ненулевые** eigenvalues даже в exact arithmetic. Только exact kernel имеет строго нулевую spectral energy. Поэтому фразу source «under exact arithmetic would be zero» можно применять к истинному kernel, но не ко всем modes, попавшим под фиксированный численный threshold.

## Численный масштаб зависит от Gamma0

Для записанного в source порядка

$$
O\!\left(\sqrt{K\varepsilon_{\mathrm{mach}}}\,\Gamma_0\right)
$$

при $K=100$ и условном $\varepsilon_{\mathrm{mach}}=10^{-16}$ множитель перед $\Gamma_0$ равен $10^{-7}$. Чтобы получить значение порядка $10^2$ при единичной скрытой константе, нужен масштаб $\Gamma_0$ порядка $10^9$. В этом рассуждении нельзя вывести абсолютный floor $100$ только из $K$ и machine precision. Неизвестные исходный scale и Big-O constant не позволяют и объявить такое значение арифметически невозможным.

Observed plateau около $750$ сам по себе не раскладывается на $100$ numerical contribution и остальной sub-growth residual. Source называет decomposition plausible; именно такой статус сохраняется. [Feature 2 и продолжение](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#pdf-0715).

## Что проверять при будущем rerun

Минимальный полезный protocol должен зафиксировать data split и accuracy metric, одинаковые исходные условия и compute budget, число structural heads, rank $Q$, количество **новых** kernel directions и обе группы margins: до $\theta_{\mathrm{rank}}$ и до $\theta_w$. Тогда можно отдельно проверить экономию архитектуры, сохранение качества и причинное объяснение plateau.

В текущем пакете такой rerun не запускался. Подготовлено точное описание гипотезы и того, какие наблюдения могли бы подтвердить или опровергнуть её разные части.
