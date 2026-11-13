---
title: "203. Lyapunov accounting контроллера и условия homeostatic tracking"
source_pdf_pages: [563, 564]
book_pages: [527, 528]
batch: "012"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 203. Lyapunov accounting контроллера и условия homeostatic tracking

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Что является объектом сходимости

§15.17 прямо ограничивает обсуждение architecture controller. Theorem 15.18 не заявляет глобальной сходимости обучения Transformer. Предпосылки включают stable diagnostics, finite useful cover, accepted-improvement floor, anchor-preserving pruning и bounded event perturbations. [Раздел и theorem](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#theorem-15-18).

В (15.39) предложена величина

$$
\mathcal V=a_\rho\rho+a_\Gamma\Gamma_{\mathrm{res}}
+a_CC_{\mathrm{orb}}-a_V\sum_h V_{\mathrm{marg},h}.
$$

Автор называет её accounting device, а не backpropagation loss. [Формула](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#equation-15-39).

## Неотрицательность двух слагаемых не ограничивает весь потенциал

Для finite-event доказательства требуется нижняя граница **полной** $\mathcal V$ и равномерное положительное уменьшение по принятым событиям. Из неотрицательности $\rho$ и $\Gamma_{\mathrm{res}}$ не следует нижняя граница выражения с $-\sum_h V_{\mathrm{marg},h}$, когда меняется число голов и суммарная полезность не ограничена.

Также малое увеличение crowding или residual должно учитываться с их коэффициентами. Условие «уменьшается один monitored component» не заменяет

$$
\Delta\mathcal V
=a_\rho\Delta\rho+a_\Gamma\Delta\Gamma_{\mathrm{res}}
+a_C\Delta C_{\mathrm{orb}}
-a_V\Delta\!\sum_hV_{\mathrm{marg},h}
\le-\delta.
$$

Это редакторская запись недостающего общего accounting condition. Если подходящие положительные коэффициенты существуют и такие bounds действительно поддерживаются, telescoping доказывает конечность accepted events. Однако существование коэффициентов не следует из одного ограничения амплитуды вмешательства. [Связанный H4-разбор](199-finite-growth-common-potential-and-stopping-semantics.md).

Pruning тоже может восстанавливать residual demand. Чтобы разделить процесс на «сначала конечный growth, затем конечный pruning», нужно исключить бесконечные циклы add–prune с регенерацией спроса. Hysteresis лишь удлиняет такой цикл; общий descent или другое конечное состояние являются отдельным требованием.

## Почему медленный Wasserstein drift ещё не даёт (15.40)

Источник предполагает $W_1(\mu(t+W),\mu(t))\le\nu W$ и пишет tracking gap относительно ideal instantaneous controller. Но squared distance к словарю не является глобально Lipschitz на неограниченном пространстве.

Например, при $\mathcal D=\{0\}$ возьмём

$$
\mu=\delta_0,\qquad
\widetilde\mu=(1-p)\delta_0+p\delta_R,\qquad p=\epsilon/R.
$$

Тогда $W_1(\mu,\widetilde\mu)=pR=\epsilon$, но изменение squared coverage равно $pR^2=\epsilon R$ и может быть сколь угодно большим при фиксированном малом $\epsilon$. Следовательно, для контроля coverage через $W_1$ нужны bounded support, подходящие moment/tail conditions либо другая метрика с количественной связью.

Даже при ограниченном support bound на отставание от ideal controller требует скорости адаптации: доступности нужных событий, достаточной частоты вмешательств и contraction/response inequality. Медленная внешняя цель сама по себе не гарантирует её отслеживания контроллером.

## Практический смысл

Полезно отдельно измерять task drift, изменения residual от обучения, accepted-event decrease и perturbation после pruning. Эти слагаемые определяют, находится система в stationary stopping regime или в tracking regime. Формула (15.40) здесь имеет характер условного обещания модели; она не является вычисленным error bar для произвольной реализации. [Homeostasis](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#section-15-17-1).
