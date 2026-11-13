---
title: "69. Метрика Hellinger, Wasserstein-география и энергия по глубине"
source_pdf_pages: [276, 278]
book_pages: [240, 242]
batch: "006"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 69. Метрика Hellinger, Wasserstein-география и энергия по глубине

[Индекс библиотеки](../README.md) · [Исходник §7.38](../source/0268-0276-ch07-experiments-dictionary-and-reporting.md#section-7-38) · [Определения и доказательство](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-38-1)

## Почему нужна метрика для сигнатуры

Сигнатура содержит две разные части:

$$
\Sigma=(\Lambda,\rho)\in\Delta_8\times[0,R].
$$

Здесь $\Lambda$ — восемь неотрицательных regime weights с суммой 1; simplex имеет размерность 7. $\rho$ — длина residual, а не probability mass вне simplex. Само слово “mass” у автора в этом месте следует понимать как описание координаты, а не как дополнительную вероятность. [Открытие §7.38, PDF 276](../source/0268-0276-ch07-experiments-dictionary-and-reporting.md#section-7-38).

Чтобы вычислить расстояние, нужно определить масштаб residual относительно изменения $\Lambda$. Иначе одно и то же изменение модели может выглядеть большим или малым просто из-за выбора единиц.

## Hellinger distance и её чувствительность у границы

Источник использует

$$
d_H(\Lambda,\Lambda')
=\frac1{\sqrt2}\|\sqrt\Lambda-\sqrt{\Lambda'}\|_2,
$$

где корень берётся покоординатно. Эквивалентная редакторская запись для probability vectors:

$$
d_H^2(\Lambda,\Lambda')
=1-\sum_{j=1}^{8}\sqrt{\Lambda_j\Lambda'_j},
\qquad 0\le d_H\le1.
$$

Она определена и при нулевых координатах. Это существенное отличие от записи Fisher tensor с делением на текущую массу. [§7.38.1](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-38-1).

Например, из чистого режима $e_1$ переместим массу $\epsilon$ в другой:

$$
\Lambda'=(1-\epsilon,\epsilon,0,\ldots,0),
\qquad
d_H(e_1,\Lambda')=
\sqrt{1-\sqrt{1-\epsilon}}
\sim\sqrt{\epsilon/2}.
$$

Это **редакторский расчёт**: появление малого нового компонента имеет масштаб $\sqrt\epsilon$ в Hellinger geometry. Внутри simplex, где все рассматриваемые $\Lambda_j>0$, малое касательное изменение $d\Lambda$ даёт

$$
d_H^2(\Lambda,\Lambda+d\Lambda)
=\frac18\sum_j\frac{(d\Lambda_j)^2}{\Lambda_j}
+o(\|d\Lambda\|^2).
$$

Именно в этом локальном смысле геометрия совпадает с Fisher geometry с постоянным множителем. Hellinger — расстояние по хорде после square-root embedding; Fisher geodesic distance — по дуге, с соответствующим масштабом. Они не являются одним и тем же глобальным расстоянием. [Remark 7.37](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#remark-7-37).

## Совместная метрика

Definition 7.36 задаёт

$$
d_\Sigma^{(\alpha)}
\bigl((\Lambda,\rho),(\Lambda',\rho')\bigr)
=
\sqrt{d_H^2(\Lambda,\Lambda')+\alpha^2|\rho-\rho'|^2},
\qquad\alpha>0.
$$

Положительность $\alpha$ нужна, чтобы различать сигнатуры с одинаковой $\Lambda$, но разными residuals. При $\alpha=0$ получилась бы лишь pseudometric на полном пространстве пар.

В отчёте фиксируются $\alpha$ и нормировка residual. Если перейти от $\rho$ к $\rho/\sqrt n$, сохранив прежнее $\alpha$, относительный вес второй координаты изменится. Сравнение разных запусков должно учитывать эту смену масштаба. [Definition 7.36](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#definition-7-36).

Метрика определена на **значениях сигнатур**. Она не устраняет неоднозначности построения $\Lambda$ из несимплициального словаря; правила atoms/families/tie-break нужно задать отдельно, как в [карточке 58](58-global-geographies-and-layer-observables.md).

## Распределение слоя и transport

Для каждого входа:

$$
\mu_\ell(X)=\frac1H\sum_{h=1}^{H}\delta_{\Sigma_{\ell,h}(X)}.
$$

Definition 7.38 вводит

$$
v_\ell(X)=W_2(\mu_\ell(X),\mu_{\ell+1}(X)),
\qquad
\mathcal E_{\rm geo}(X)=\sum_{\ell=1}^{L-1}v_\ell(X)^2.
$$

Здесь “velocity” означает величину изменения между соседними слоями, а не скорость вычисления в секундах. Большой $v_\ell$ показывает перестройку распределения измеренных signatures. Малый $v_\ell$ не доказывает, что слой вычислительно бесполезен: информация вне сигнатуры и task effect остаются отдельными вопросами. [Definition 7.38](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#definition-7-38).

При одинаковом числе равновесных heads оптимальное значение можно вычислить через assignment:

$$
W_2(\mu_\ell,\mu_{\ell+1})^2
=\min_{\pi\in\mathfrak S_H}
\frac1H\sum_{h=1}^{H}
d_\Sigma^{(\alpha)}
\bigl(\Sigma_{\ell,h},\Sigma_{\ell+1,\pi(h)}\bigr)^2.
$$

Это редакторская явная запись утверждения §7.38.3. Существует permutation optimizer, но при ties оптимальный coupling не обязан быть единственным; смеси оптимальных assignments тоже оптимальны. Полученное сопоставление не доказывает наследование функций между головами. [§7.38.3](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-38-3).

## Корректная нижняя граница энергии

Для $L\ge2$ Proposition 7.39 доказывает

$$
\mathcal E_{\rm geo}(X)
\ge\frac{W_2(\mu_1(X),\mu_L(X))^2}{L-1}.
$$

Сначала triangle inequality ограничивает конечное расстояние суммой промежуточных $v_\ell$, затем Cauchy–Schwarz ограничивает квадрат этой суммы через $(L-1)\sum v_\ell^2$. Доказательство не требует интерпретации transport как физического канала и не утверждает существование оптимального пути обучения. [Proposition 7.39 с proof](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#proposition-7-39).

Энергия зависит от числа и расположения слоёв. Если одну и ту же смену географии распределить на большее число маленьких переходов, сумма квадратов изменится. Поэтому raw energy разных глубин нельзя без дополнительной конвенции трактовать как общую вычислительную сложность.

## Индекс равномерности

При $\sum v_\ell>0$:

$$
U_{\rm geo}
=\frac{(\sum_{\ell=1}^{L-1}v_\ell)^2}
{(L-1)\sum_{\ell=1}^{L-1}v_\ell^2}.
$$

Книга указывает $0<U_{\rm geo}\le1$. Для неотрицательных velocities более точная редакторская граница:

$$
\frac1{L-1}\le U_{\rm geo}\le1.
$$

Значение 1 достигается при равных $v_\ell$; нижняя граница — если ненулевой только один переход. Если все $v_\ell=0$, исходная формула не определена: convention для такого случая надо указывать явно. При $L=1$ переходов нет. [Definition 7.40](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#definition-7-40).

Эти величины полезны как воспроизводимые характеристики географии. Они не измеряют сами по себе скорость обучения, качество переноса или необходимое число примеров.

