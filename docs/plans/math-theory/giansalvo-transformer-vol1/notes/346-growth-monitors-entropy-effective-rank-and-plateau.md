---
title: "346. Диагностика роста: что измеряют load, entropy и effective rank"
source_pdf_pages: [778, 778]
book_pages: [742, 742]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 346. Диагностика роста: что измеряют load, entropy и effective rank

## Вопрос

Можно ли понять необходимость новой головы по одному низкому entropy?

**Эти величины описывают концентрацию использования и спектра, а не напрямую недостаток качества. В книге рост требует совместного plateau и структурного сигнала; пороги зависят от H, k и соглашений о спектре.**

Источник: [§19.6.3](../source/0777-0779-ch19-incremental-birth-monitoring-and-controller.md#section-19-6-3).

## Границы и нормировки

Так как gates нормированы, $\rho_h\ge0$ и $\sum_h\rho_h=1$. Большая $\rho_h$ означает большую среднюю долю данной головы. Но голов может быть мало, а распределение задачи может быть неравномерным: высокая load не доказывает вредную перегрузку.

При натуральном logarithm $0\le\mathcal H_{\mathrm{head}}\le\log H$ и $0\le\mathcal H_{\mathrm{route}}\le\log k$. Изменяя $H$ или $k$, мы меняем даже максимально возможную entropy. Фиксированный threshold и рост raw entropy после birth нельзя сравнивать без этого контекста. Низкая entropy может означать как collapse, так и полезную специализацию.

Низкая средняя token-wise head entropy совместима с равномерными loads: разные токены могут уверенно выбирать разные heads. И наоборот, uniform gates дают maximal entropy, но не гарантируют, что heads делают разные вычисления. Поэтому нагрузки и entropy — связанные, но разные сигналы.

## Какой именно effective rank

Книга нормирует **singular values** $s_\ell$, а не их квадраты, и берёт exponential entropy. При $X\ne0$ получаем $1\le r_{\mathrm{eff}}\le\operatorname{rank}X$. Для нулевого $X$ знаменатель равен нулю, и нужна отдельная convention. Этот effective rank отличается от energy-threshold rank и stable rank; подмена меняет trigger.

Увеличение entropy spectrum может происходить из-за noise. Снижение может отражать полезное выделение малого числа признаков. Ни один из этих сдвигов сам по себе не доказывает улучшение задачи.

## Plateau в напечатанной формуле

$\Delta L_t=L_{t-P}-L_t<\varepsilon_L$ срабатывает и когда loss **растёт**, то есть $\Delta L_t<0$. Это проверка отсутствия достаточного улучшения, не строгая проверка стационарного plateau. Нужны определение окна, сглаживание noise, частота monitoring, persistence и сравнимый loss. Algorithm 9 формулирует structural condition через «any threshold», то есть OR внутри structural group, а затем AND с optimisation condition. В source сохранены обе формулировки, чтобы не превратить их незаметно в другой controller.
