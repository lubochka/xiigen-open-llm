---
title: "80. Условная compatibility theorem и сокращение вкладов eigenplanes"
source_pdf_pages: [303, 304]
book_pages: [267, 268]
supplementary_source_pdf_pages: [300, 302]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 80. Условная compatibility theorem и сокращение вкладов eigenplanes

[Индекс библиотеки](../README.md) · [Theorem 8.18](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#theorem-8-18) · [Предыдущая карточка](79-transition-observables-and-compatibility-links.md) · [Следующая карточка](81-signature-bridges-and-hellinger-transport-bounds.md)

## Четыре предпосылки источника

Theorem 8.18 фиксирует переход $\ell\to\ell+1$ и голову $h$. Условия требуют: двумерной проекции на skew eigenplane с площадью не меньше $a_0$ и singular value не меньше $\sigma_0$; alignment с relevant top-1/top-2 contrasts не меньше $c_0$; верхней границы $b_0$ на opposing contributions; неотрицательных row-margin increments и положительной прибавки $m_0$ на active set. Тогда автор заявляет

$$
\Gamma_{\ell+1,h}\ge\sigma_0a_0,
\qquad
\bar\Delta_{\ell+1,h}\ge c_0\Gamma_{\ell+1,h}-b_0,
$$

и снижение entropy proxy. Remark 8.19 прямо говорит, что предпосылки нужно измерять, архитектура их не гарантирует. [Теорема, proof и remark](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#theorem-8-18).

## Первый bound требует более точного условия, чем C1

**Редакторская проверка:** «площадь row span» без выбранных векторов или нормировки ещё не является числом. Даже если понимать её как площадь проекций двух фактических строк, одной такой проекции недостаточно для нижней границы полной $\Gamma$: вклады разных skew planes могут сократиться.

Пусть

$$
J=\begin{pmatrix}0&-1\\1&0\end{pmatrix},
\qquad M_a=\operatorname{diag}(J,J),
\qquad
X=\begin{pmatrix}1&0&1&0\\0&1&0&-1\end{pmatrix}.
$$

Проекции строк на первую eigenplane — $(1,0)$ и $(0,1)$, их площадь равна 1, singular value формы также равна 1. Но вклад первой плоскости в $x_1^\top M_ax_2$ равен $-1$, второй — $+1$. Поэтому

$$
XM_aX^\top=0,\qquad\Gamma(X;M)=0.
$$

Это точный контрпример к использованному в proof переходу **от C1 к первому bound** при таком естественном прочтении C1. Он не объявляется контрпримером ко всем C1–C4 сразу: C2 и C3 пока не заданы через проверяемые формулы. [C1 и первая строка proof](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#theorem-8-18).

Достаточное редакторское уточнение для отдельной пары — потребовать, чтобы обе строки лежали в выбранной eigenplane, либо ограничить абсолютный opposing contribution остальных плоскостей числом $r_0$. Во втором случае для этой пары получается

$$
|x_i^\top M_ax_j|\ge\max(0,\sigma_0a_0-r_0).
$$

При $i\ne j$ Frobenius norm полной skew score matrix не меньше $\sqrt2$ умножить на этот модуль. Это вывод из двух симметрично расположенных элементов; книга его в таком уточнённом виде не формулирует.

## Что действительно следует для entropy proxy

При фиксированных $n\ge2$, поддержке и сопоставлении строк, условие C4 позволяет применить монотонность $B_n$. Если на active set $A$ старые margins лежат в $[0,M]$, прибавка не меньше $m_0>0$, а у остальных строк margins не уменьшаются, то редакторская конкретизация даёт

$$
\widetilde H_{\ell,h}-\widetilde H_{\ell+1,h}
\ge\frac{|A|}{n}\kappa_{n,M,m_0},
$$

$$
\kappa_{n,M,m_0}
=\min_{\delta\in[0,M]}\bigl[B_n(\delta)-B_n(\delta+m_0)\bigr]>0.
$$

Доля $|A|/n$ существенна для среднего по всем строкам. Без верхней границы старых margins нельзя получить ту же положительную uniform constant: в насыщении envelope почти не меняется. Снижение proxy не означает снижения actual entropy. [Proposition 8.14](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#proposition-8-14), [Theorem 8.18](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#theorem-8-18), [карточка 78](78-margin-entropy-proxy-and-compatibility.md).

## Диагностическая интерпретация не является обратной теоремой

В §8.12.1 отсутствие роста activation при росте cone отнесено к сбою C1. Это сильнее формального вывода: $\Gamma_{\ell+1,h}\ge\sigma_0a_0$ — абсолютная граница нового значения, а не сравнение со старой $\Gamma_{\ell,h}$. Условие может выполняться, даже если activation уменьшилась с ещё большего значения. Аналогично inequality margin–energy сама не устанавливает рост margin между слоями. Проверка должна начинаться с самих C1–C4. [§8.12.1](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-12-1).

Автор отдельно подчёркивает, что теорема не контролирует $\rho_8$. Attention может стать более концентрированным и одновременно удалиться от выбранного canonical hull. Поэтому нельзя присоединять снижение residual к выводу теоремы без новой предпосылки. [§8.12.2](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-12-2).
