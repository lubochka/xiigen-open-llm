---
title: "307. Tracking и optimiser drift: соответствие кластеров, гладкость и состояние momentum"
source_pdf_pages: [721, 722]
book_pages: [685, 686]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 307. Tracking и optimiser drift: соответствие кластеров, гладкость и состояние momentum

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Какие дополнительные условия превращают малое изменение residual operator в контролируемое отслеживание eigenspace?

## Малое движение оператора требует выбранного соответствия

Lemma 17.18 сопоставляет $\|A_{t+1}-A_t\|\le\alpha\delta_t$ и $\operatorname{dist}(U_{t+1},U_t)\lesssim\alpha$. Корректное ядро — perturbation bound для **продолжения одного и того же изолированного кластера одинаковой размерности**. Это соответствие нельзя опускать. При неизменной $A_t=\operatorname{diag}(2,1)$ можно формально выбрать $U_t=\operatorname{span}(e_1)$, $U_{t+1}=\operatorname{span}(e_2)$: оба подпространства invariant с gap 1, движение оператора равно нулю, но расстояние projectors равно 1. [Lemma 17.18](../source/0718-0721-ch17-signed-score-conditioning-and-numerical-floor.md#lemma-17-18), [продолжение proof](../source/0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#pdf-0722).

Для согласованного кластера условие $\|E\|<\delta/2$ даёт простой запас до закрытия gap: по perturbation eigenvalues новый gap не меньше $\delta-2\|E\|$. Затем применима конкретная оценка projector с нужной константой. Одного абстрактного $\alpha<1$ недостаточно для гарантии неизменной классификации при произвольных spectral crossings. Hysteresis защищает дискретное решение только при явно выбранных thresholds и margins.

Близость истинных $U_t$ ещё не гарантирует точность вычисленного gate. Если один solver step даёт contraction $q<1$, типичная tracking recurrence имеет вид $e_{t+1}\le q e_t+c\,d_t$, где $d_t$ — дрейф цели. При постоянном upper bound $d$ это даёт steady-state масштаб $cd/(1-q)$. Требуются число внутренних шагов, solver error и правило обновления target; их не заменяет один Davis–Kahan bound.

## Что нужно для остатка второго порядка

Proposition 17.19 предполагает differentiability и печатает

$$
\|A(\theta-\eta P g)-A(\theta)\|
\le\eta\|DA(\theta)\|\,\|Pg\|+O(\eta^2\|Pg\|^2).
$$

Одна дифференцируемость даёт остаток $o(\eta\|Pg\|)$, но не обязательно квадратичный. Если derivative локально Lipschitz с константой $L$, интегральная формула даёт проверяемую оценку

$$
\|A(\theta+h)-A(\theta)\|
\le\|DA(\theta)\|\,\|h\|+\frac L2\|h\|^2.
$$

Возьмём $A(\theta)=\operatorname{diag}(1+|\theta|^{3/2},-1-|\theta|^{3/2})$. В нуле derivative равен нулю, а для шага $h=-\eta$ изменение нормы равно $\eta^{3/2}$. Его отношение к $\eta^2$ равно $\eta^{-1/2}$ и растёт без ограничения: 10, 100, 1000 для $\eta=10^{-2},10^{-4},10^{-6}$. Это симметричный traceless counterexample именно к недостаточному smoothness assumption. [Proposition 17.19](../source/0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#proposition-17-19).

## Что кодирует preconditioner

Для SGD запись $h=-\eta P_tg_t$ с текущим градиентом естественна. Momentum/Adam содержат состояние истории. Если текущий $g_t=0$, но предыдущий момент ненулевой, обновление может быть ненулевым; никакая конечная линейная матрица $P_t$ не превращает нулевой вектор в такой шаг. Например, $m_t=.9m_{t-1}+.1g_t$ при $m_{t-1}=1,g_t=0$ даёт $m_t=.9$.

Применять Taylor bound можно к **фактическому** вектору шага, включая moment, clipping, decay и ограничения, либо переопределить $g_t$ как уже сформированный update direction. Но тогда он не тождествен текущему raw gradient. Даже малое первое слагаемое не достаточно, если не контролируется остаток второго порядка.

Для проекта нужен журнал $\Delta\theta_t$, оценки изменения оператора, labeled target cluster, gap и solver tracking error. На этом уровне возможно отделить optimiser effect от ошибки eigensolver; утверждение об optimiser-invariant dispatch без этих условий не следует. Здесь проверены формулы и контрпример, не запускался optimiser на модели.
