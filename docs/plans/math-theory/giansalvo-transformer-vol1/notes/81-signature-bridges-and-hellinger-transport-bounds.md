---
title: "81. Переход к signature mass и границы Hellinger transport"
source_pdf_pages: [304, 305]
book_pages: [268, 269]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 81. Переход к signature mass и границы Hellinger transport

[Индекс библиотеки](../README.md) · [§8.13](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-13) · [Предыдущая карточка](80-conditional-compatibility-and-eigenplane-cancellation.md) · [Следующая карточка](82-measurement-protocol-and-finite-head-transport.md)

## Margin growth и directional mass — разные координаты

Proposition 8.20 заявляет, что stable directional sources, условия compatibility и residual не больше $\varepsilon$ ведут к оценке

$$
\Lambda_{\mathrm{dir}}^{(\ell+1,h)}
\ge\Lambda_{\mathrm{dir}}^{(\ell,h)}
+c\bigl(\bar\Delta^{(\ell+1,h)}-\bar\Delta^{(\ell,h)}\bigr)-C\varepsilon.
$$

В proof sketch фигурирует local conditioning проекции около directional face. Однако не определены стабильное множество источников, область conditioning, положительность и диапазон действия констант, правила выбора barycentric representation. [Proposition 8.20](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#proposition-8-20).

**Редакторское ограничение:** положительный uniform $c$, зависящий лишь от dictionary/metric, не следует из роста logits. Масса ограничена единицей, а margins сверху не ограничены. Softmax в насыщении может почти не менять attention при большом приросте margin. Для нетривиальной линейной нижней границы нужны локальный диапазон logits, ненулевая чувствительность выбранной координаты и запас до насыщения.

Простой тест недостающей предпосылки: возьмём словарь только из двух directional atoms $D_1=\mathbf1e_1^\top$, $D_2=\mathbf1e_2^\top$. Матрица $S=\mathbf1(p,1-p)$, $p>1/2$, лежит в их hull, имеет residual 0 и directional mass 1. Рост $p$ увеличивает row margin $\log(p/(1-p))$, но общая directional mass уже не растёт. Словарь содержит соответствующие directional atoms; это показывает, что одного условия их наличия недостаточно. При полном словаре остаётся дополнительный вопрос о том, как масса распределяется между пересекающимися семьями. Этот пример — редакторская проверка логики, не новый опыт автора.

## Cone–residual proposition пока не задаёт вычислимой оценки

Proposition 8.21 предполагает high cone alignment и stable singular structure, совместимую с canonical face, затем обещает lower bound как функцию leading singular-mass ratio с Lipschitz error по residual. Функция, константа, выбор центрированного/нецентрированного спектра и значение «compatible» не заданы. Поэтому нельзя извлечь численный критерий для кода из одной этой формулировки. [Proposition 8.21](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#proposition-8-21).

Малый attention residual сам не контролирует token cone или SVD: одинаковые scores $XMX^\top$ можно получить при $X\mapsto XT$, $M\mapsto T^{-1}MT^{-\top}$ с обратимой $T$, меняя евклидову геометрию представления. Это аргумент о недостаточности информации в attention observable, а не утверждение об инвариантности всего блока под такой заменой. Дополнительные cone/SVD условия должны быть самостоятельными и количественными; прежние контрпримеры остаются актуальны. [Карточка 77](77-cone-svd-and-directional-activation.md).

## Первая transport bound корректна для допустимого matching

Если головы имеют одинаковые массы $1/H$ и $\pi_\ell$ — **биекция** индексов, то она задаёт допустимый coupling. Поэтому

$$
W_2(\mu_\ell,\mu_{\ell+1})^2
\le\frac1H\sum_h d_\Sigma(\Sigma_{\ell,h},\Sigma_{\ell+1,\pi_\ell(h)})^2.
$$

Это верхняя оценка стоимостью любого допустимого плана. Она становится равенством для оптимального assignment. Произвольное many-to-one правило не сохраняет marginals и не даёт тот же вывод; при разных числах или весах голов нужен coupling с соответствующими массами. [Proposition 8.22](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#proposition-8-22).

## Для второй bound недостаточно одного слова «compact»

Источник заменяет Hellinger term обычной нормой изменения $\Lambda$ «up to the equivalence constant … on the relevant compact subset». На compact subset, содержащем границу симплекса, uniform linear equivalence с евклидовым расстоянием неверна.

При $p=(1,0)$ и $q=(1-\varepsilon,\varepsilon)$ нормировка главы 7 даёт

$$
d_H(p,q)^2=1-\sqrt{1-\varepsilon}\sim\varepsilon/2,
\qquad \|p-q\|_2^2=2\varepsilon^2.
$$

Отношение стремится к бесконечности. Следовательно, нельзя поставить одну конечную константу перед $a_h^2$ на всём замкнутом симплексе. [Продолжение Proposition 8.22, PDF 305](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#pdf-0305), [карточка 69](69-hellinger-wasserstein-layer-geometry.md).

**Редакторская рабочая версия:** если все сравниваемые ненулевые координаты находятся на одной фиксированной face и не меньше $\delta>0$, то

$$
d_H(p,q)^2
=\frac12\sum_j\frac{(p_j-q_j)^2}{(\sqrt{p_j}+\sqrt{q_j})^2}
\le\frac{\|p-q\|_2^2}{8\delta}.
$$

При $\|\Delta\Lambda_h\|_2\le a_h$ и $|\Delta\rho_h|\le b_h$ это даёт

$$
W_2^2\le\frac1H\sum_h\left(\frac{a_h^2}{8\delta}+\alpha^2b_h^2\right).
$$

Без interior bound сохраняется глобальная оценка $d_H^2\le\frac12\|p-q\|_1$. Тогда при $\|\Delta\Lambda_h\|_1\le a_h$ barycentric term в transport bound равен $a_h/2$, а не $a_h^2$. Ещё один точный вариант — измерять $a_h$ непосредственно в Hellinger metric. Все эти уточнения отделены от буквального текста автора.
