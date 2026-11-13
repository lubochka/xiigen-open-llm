---
title: "204. Безопасное появление головы и сохранение функции при split"
source_pdf_pages: [564, 565]
book_pages: [528, 529]
batch: "012"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 204. Безопасное появление головы и сохранение функции при split

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Почему инициализация входит в алгоритм обучения

§15.18 требует ввести новый компонент так, чтобы существующая вычисляемая функция почти не изменилась. Query/key motor должен выразить желаемую геометрию, а output contribution начинается с малых $W_O$ и gate. Иначе post-event marginal value смешивает полезность новой ёмкости с ущербом от её появления. [Инициализация](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#section-15-18), [Remark 15.20](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#remark-15-20).

Схема (15.41) задаёт

$$
M_{\mathrm{new}}=M(\sigma^\star)+\xi Z,\qquad
W_{O,\mathrm{new}}=\epsilon_O\widetilde W_O,\qquad
g_{\mathrm{new}}=g_{\min}>0.
$$

Proposition 15.19 ограничивает birth perturbation через $g_{\min}\epsilon_O$ и $O(\xi)$. [Формула и proposition](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#equation-15-41).

## Явная оценка для одной головы

Для вклада

$$
Y_{\mathrm{new}}=g\,P(X)XW_VW_O
$$

получаем редакторскую нормовую оценку

$$
\|Y_{\mathrm{new}}\|_F
\le |g|\,\|P(X)\|_2\|X\|_F\|W_V\|_2\|W_O\|_2.
$$

Поэтому малое $gW_O$ действительно ограничивает вставку, если остальные множители контролируются. Row-stochastic attention не означает автоматически $\|P\|_2\le1$: для совпадающих one-hot rows она равна $\sqrt n$. Для post-LN или последующих нелинейностей нужен их локальный Lipschitz bound в рассматриваемой области.

Нельзя считать $\|W_O\|\le\epsilon_O$ следствием $W_O=\epsilon_O\widetilde W_O$ без нормировки $\|\widetilde W_O\|\le1$. При $g_{\min}$ как жёстком нижнем пороге условие proposition $g(0)\le g_{\min}$ вместе с floor означает фактически равенство на старте.

## Quiet birth и обучение компонента

При $W_O=0$ выход новой головы точно равен нулю, и градиент к query/key/value через эту выходную ветвь также может быть нулевым. Градиент к самому $W_O$ при этом может оставаться ненулевым. Следовательно, staged opening или малое ненулевое $W_O$ могут понадобиться для перехода от безопасного появления к полезному обучению.

Это не опровергает near-zero birth. Оно требует явного warm-up schedule и проверки, что новая голова получает нужный gradient signal. Положительный gate floor один эту проверку не заменяет.

## Когда split сохраняет функцию

В (15.42) motors детей сдвинуты противоположно, а output projections поделены пополам. Если у детей одинаковы с родителем values, gates, masks и остальные shared параметры, то при $\epsilon=0$ сумма их outputs точно равна старому output. [Split](../source/0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#equation-15-42).

Для гладкой функции $F(M)$ и одинаковых коэффициентов детей:

$$
\tfrac12F(M+\epsilon\Delta M)
+\tfrac12F(M-\epsilon\Delta M)
=F(M)+O(\epsilon^2).
$$

Первый порядок сокращается благодаря симметрии. При разных gates, независимых dropout masks, изменении нормировки или различающихся values это сокращение уже не обеспечено. Если доступны только Lipschitz bounds, гарантирован лишь $O(|\epsilon|)$, без второго порядка.

Реализуемость $M_h\pm\epsilon\Delta M$ через query/key factors с фиксированной $d_k$ также требует rank constraints. Таким образом, «копировать и поделить пополам» — полезный конструктивный принцип, но сохранение вычисления относится ко всей head contribution и режиму исполнения, а не только к двум матрицам, указанным в display.
