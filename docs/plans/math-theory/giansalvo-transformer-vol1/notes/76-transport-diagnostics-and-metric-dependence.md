---
title: "76. Перенос сигнатур: метрика, средние, разброс и границы диагностики"
source_pdf_pages: [286, 297]
book_pages: [250, 261]
batch: "006"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 76. Перенос сигнатур: метрика, средние, разброс и границы диагностики

[Индекс библиотеки](../README.md) · [§§8.5–8.6](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#section-8-5) · [Предыдущая карточка](75-reextraction-cost-and-operator-populations.md) · [Следующая карточка](77-cone-svd-and-directional-activation.md)

## Единица сравнения

Вход фиксирован. Каждая голова даёт $\Sigma=(\Lambda,\rho)$: веса режимов и расстояние до выбранной выпуклой оболочки. Распределение по головам слоя — $\mu_\ell=H^{-1}\sum_h\delta_{\Sigma_{\ell,h}}$. В главе 8 используется

$$
d_\Sigma((\Lambda,\rho),(\Lambda',\rho'))
=\sqrt{d_H(\Lambda,\Lambda')^2+\alpha^2|\rho-\rho'|^2},
\qquad\alpha>0.
$$

Затем $v_\ell=W_2(\mu_\ell,\mu_{\ell+1};d_\Sigma)$. Без фиксации словаря, выбора barycentric coordinates, нормировки $\rho$, параметра $\alpha$ и набора входов числа разных отчётов несопоставимы. [§8.1.1](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#section-8-1-1), [§8.5.1](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#section-8-5-1).

## Оптимальное сопоставление не закрепляет личность головы

При одинаковом числе голов и равных массах задача имеет вид

$$
W_2^2=\min_{\pi\ge0}
\sum_{h,k}\pi_{hk}\,d_\Sigma(\Sigma_{\ell,h},\Sigma_{\ell+1,k})^2,
\quad
\sum_k\pi_{hk}=\sum_h\pi_{hk}=\frac1H.
$$

По стандартному определению **$H\pi$**, а не сама $\pi$, является doubly stochastic matrix с суммами строк и столбцов 1. Оптимальное назначение соответствует scaled permutation matrix; при равных стоимостях оптимальных планов может быть несколько. Это уточнение к короткой формулировке [§8.5.2](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#section-8-5-2).

**Remark 8.10 смешивает две ситуации.** Смена способа выбора между оптимальными планами при фиксированной метрике сохраняет минимальную стоимость. Смена самой метрики вообще может изменить и план, и $W_2$. Инвариантность к номерам голов не означает независимость от метрики. [Точная формулировка источника](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#remark-8-10).

## Какие изменения действительно требуют coupling

Автор вводит signed и unsigned показатели:

$$
D_\ell^\Lambda=\int(\Lambda'-\Lambda)\,d\pi_\ell,\qquad
\Phi_\ell^\Lambda=\int\|\Lambda'-\Lambda\|_2\,d\pi_\ell,
$$

$$
D_\ell^\rho=\int(\rho'-\rho)\,d\pi_\ell,\qquad
\Phi_\ell^\rho=\int|\rho'-\rho|\,d\pi_\ell.
$$

[§§8.1.3, 8.6.1–8.6.2](../source/0285-0292-ch08-reextraction-and-residual-algebra.md#section-8-1-3), [PDF 295](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#pdf-0295).

Редакторское упрощение из фиксированных marginal distributions:

$$
D_\ell^\Lambda=\bar\Lambda_{\ell+1}-\bar\Lambda_\ell,
\qquad D_\ell^\rho=\bar\rho_{\ell+1}-\bar\rho_\ell.
$$

То есть signed drift не зависит от того, какой coupling выбран, и для его расчёта транспортная оптимизация не нужна. Unsigned показатели зависят от сопоставления и могут показывать движение, которое взаимно сократилось в среднем. При неединственном оптимальном плане они сами могут зависеть от правила выбора плана. Нормы в этих показателях не тождественны квадрату полной ground metric.

Снижение $\bar\rho$ означает более близкое приближение внимания **выбранным словарём**. Оно не доказывает улучшения task quality. Рост остатка может означать полезное поведение, отсутствующее в словаре.

## Разброс относительно какого центра

Автор определяет

$$
m_\ell=\int z\,d\mu_\ell(z),\qquad
\operatorname{Spread}(\mu_\ell)=\int d_\Sigma(z,m_\ell)^2\,d\mu_\ell(z).
$$

Это корректное определение разброса относительно арифметического среднего координат. Однако для Hellinger-части это среднее не обязано минимизировать ожидаемый квадрат расстояния: его нельзя автоматически называть Fréchet barycenter. [§8.6.3](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#section-8-6-3).

Например, для обычной нормировки Hellinger минимизирующая точка имеет квадратные корни, пропорциональные $\mathbb E[\sqrt{\Lambda_a}]$, тогда как арифметический центр использует $\mathbb E[\Lambda_a]$. Это редакторское различение двух определений центра. При одинаковых средних популяции могут иметь разные разбросы; ни один из этих scalar summaries не задаёт всей функции слоя.

## Почему Хеллингер чувствителен у границы

При $\Lambda=e_1$ и $\Lambda'=(1-\varepsilon,\varepsilon,0,\ldots)$ стандартная нормировка даёт

$$
d_H^2=1-\sqrt{1-\varepsilon},\qquad
d_H\sim\sqrt{\varepsilon/2},\qquad
\|\Lambda-\Lambda'\|_1=2\varepsilon.
$$

Поэтому около вершины малая новая масса заметнее в Hellinger distance. Это локальное сравнение масштаба, а не утверждение, что $d_H$ численно больше $\ell_1$ во всех точках. [§8.5.3](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#section-8-5-3), [Figure 8.8](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#figure-8-8).

## Что означает transition layer

В Definition 8.9 это переход с необычно большим $v_\ell$ **относительно типичного уровня при том же протоколе**. Численного универсального порога источник не задаёт. Порог и способ агрегации по входам нужно определить отдельно. [Definition 8.9](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#definition-8-9).

Малый $v_\ell$ означает стабильность измеренной географии. Он не гарантирует малого изменения residual representation, values, MLP, выхода задачи или mutual information. Поэтому «conservative transmitter» из прозы надо читать в ограниченном геометрическом смысле. [Figures 8.6–8.7](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#figure-8-6) синтетические; это иллюстрации диагностики, не измерение обученной Xiigen.
