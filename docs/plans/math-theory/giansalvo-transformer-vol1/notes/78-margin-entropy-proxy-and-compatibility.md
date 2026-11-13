---
title: "78. Margin, верхняя оценка энтропии и профиль совместимости"
source_pdf_pages: [298, 300]
book_pages: [262, 264]
batch: "006"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 78. Margin, верхняя оценка энтропии и профиль совместимости

[Индекс библиотеки](../README.md) · [§§8.8–8.10.1](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#section-8-8) · [Предыдущая карточка](77-cone-svd-and-directional-activation.md) · [Точная граница пакета](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#batch-006-boundary)

## Корректная построчная оценка

Для softmax-строки из $n\ge2$ элементов, натурального логарифма и разницы двух наибольших **фактически поданных в softmax** logits $\Delta\ge0$ автор доказывает

$$
H(p)\le B_n(\Delta)
=\log\!\left(1+(n-1)e^{-\Delta}\right)
+\frac{\Delta(n-1)e^{-\Delta}}{1+(n-1)e^{-\Delta}}.
$$

[Theorem 8.12](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#theorem-8-12).

Максимальная при данном margin энтропия достигается, когда все $n-1$ проигрывающих logits одинаковы:

$$
p_1=\frac{e^\Delta}{e^\Delta+n-1},\qquad
p_2=\cdots=p_n=\frac1{e^\Delta+n-1}.
$$

При $\Delta=0$ верхняя граница равна $\log n$, а при $\Delta\to\infty$ стремится к нулю. Margin 0 сам по себе не означает равномерность всей строки: достаточно равенства только двух верхних logits. [Remark 8.13](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#remark-8-13).

При $q=(n-1)e^{-\Delta}$ редакторская проверка производной даёт

$$
B_n'(\Delta)=-\frac{\Delta q}{(1+q)^2}\le0.
$$

Для $\Delta>0$ она отрицательна; $B_n$ строго убывает на $[0,\infty)$, несмотря на нулевую производную в самой точке 0. Если есть маска, нужно согласовать допустимые позиции и размер поддержки; сравнение разных $n$ не покрывается простой монотонностью по $\Delta$.

## Убывающий envelope и фактическая энтропия — разные выводы

Из $H_{\mathrm{old}}\le B_n(\Delta_{\mathrm{old}})$ и $H_{\mathrm{new}}\le B_n(\Delta_{\mathrm{new}})$ при меньшей новой верхней границе не следует $H_{\mathrm{new}}\le H_{\mathrm{old}}$. Старая энтропия могла лежать намного ниже своей границы.

Проверенный редакторский пример при $n=3$:

| Logits | Top-1/top-2 margin | Фактическая энтропия |
|---|---:|---:|
| $(1,0,-10)$ | 1 | примерно 0.58235 |
| $(2,0,0)$ | 2 | примерно 0.66557 |

Margin вырос, но энтропия тоже выросла, потому что третий вариант перестал быть практически исключённым. Оба распределения удовлетворяют Theorem 8.12. Поэтому фразы о «sharpening» в [§8.9.1](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#section-8-9-1) должны сопровождаться различием фактической энтропии и её верхней оценки.

## Рост среднего margin недостаточен даже для среднего envelope

В [§8.8.2](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#section-8-8-2) сказано, что рост среднего row margin принуждает энтропию следующего слоя снижаться. Это слишком сильная формулировка.

Возьмём attention matrix с тремя строками. Сначала logits каждой строки равны $(2,0,0)$, поэтому все margins равны 2. Затем у двух строк logits становятся $(0,0,0)$, а у третьей — $(7,0,0)$. Средний margin увеличивается с 2 до $7/3$, но средняя энтропия, совпадающая здесь со средним envelope, растёт:

$$
B_3(2)\approx0.66557
<
\frac{2B_3(0)+B_3(7)}3\approx0.73726.
$$

Изменение среднего не контролирует изменения каждой строки. Этот пример проверяет ограничение утверждения, не заменяя текст оригинала.

## Что действительно утверждает Proposition 8.14

Автор определяет proxy

$$
\widetilde H^{(\ell,h)}
=\frac1n\sum_iB_n(\Delta_i^{(\ell,h)}).
$$

Если для выбранного сопоставления и каждой строки

$$
\Delta_i^{(\ell+1,h)}
\ge\Delta_i^{(\ell,h)}+m_i^{(\ell)},\qquad m_i^{(\ell)}\ge0,
$$

то $\widetilde H^{(\ell+1,h)}\le\widetilde H^{(\ell,h)}$. Это корректный **условный вывод о proxy**. Он не доказывает, что обучение обеспечит такие margins, и не превращает числовую метку $h$ в каноническую связь голов разных слоёв. [Proposition 8.14](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#proposition-8-14), [Remark 8.15](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#remark-8-15).

При равномерной прибавке $m_i^{(\ell)}\ge m_0>0$ и ограниченных старых margins $\Delta_i\in[0,M]$ можно взять

$$
\kappa_{n,M,m_0}
=\min_{\Delta\in[0,M]}
\bigl[B_n(\Delta)-B_n(\Delta+m_0)\bigr]>0.
$$

Это делает источник точнее: минимум непрерывной положительной функции на compact interval положителен. Такое условие нельзя продолжать на бесконечно много слоёв одновременно с фиксированной верхней границей margins и одинаковой положительной прибавкой.

## Совместный профиль без универсальной причинной цепи

На PDF 300 автор вводит

$$
\mathcal C_\ell=
(I_{\mathrm{cone},\ell},\Gamma_\ell,\bar\Delta_\ell,H_\ell,\bar\rho_\ell).
$$

Это **Definition 8.16**, способ совместно фиксировать наблюдаемые. Он не является законом, заставляющим все компоненты монотонно изменяться с глубиной. Для проектного отчёта полезно добавить отдельно $\widetilde H$ и точное определение фактической $H$, чтобы не смешивать их. [§8.10.1 и Definition 8.16](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#definition-8-16).

Граница пакета проходит после законченного абзаца этого подраздела. Дальнейшие compatibility bounds не использованы: следующая обработка начнётся с PDF 301 / книги 265.


## Продолжение в проходе 007

Историческая граница выше закрыта: [карточки 79](79-transition-observables-and-compatibility-links.md), [80](80-conditional-compatibility-and-eigenplane-cancellation.md) и [81](81-signature-bridges-and-hellinger-transport-bounds.md) разбирают новые compatibility conditions и переход к signature space. Условия не превращают proxy в фактическую энтропию; глава 8 теперь завершена.
