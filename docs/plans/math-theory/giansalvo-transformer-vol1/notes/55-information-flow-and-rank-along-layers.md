---
title: "55. Cone index, directional energy, margin и энтропия: проверка coherence chain"
source_pdf_pages: [239, 243]
book_pages: [203, 207]
supplementary_source_pdf_pages: [225, 226]
batch: "005"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 55. Cone index, directional energy, margin и энтропия: проверка coherence chain

[Индекс библиотеки](../README.md) · [Полный английский §7.3](../source/0239-0244-ch07-information-flow-and-residual-stream.md#section-7-3) · [Предыдущая карточка](54-bert-signature-geography-and-evidence.md) · [Следующая карточка](56-residual-stream-and-representation-clustering.md)

## Четыре разные наблюдаемые

В §7.3 вводятся четыре величины. Их можно измерять, но заявленная автором цепочка монотонных следствий требует отдельной проверки:

| Величина | Определение в этом фрагменте | Что непосредственно измеряется |
| --- | --- | --- |
| $\mathcal I_{\mathrm{cone}}^{(\ell)}$ | $\|\sum_i x_i^{(\ell)}\|_2$ после второго LayerNorm | Согласованность суммы токенных векторов |
| $\Gamma^{(\ell)}$ | $\|X^{(\ell)}M_a^{(\ell)}(X^{(\ell)})^\top\|_F$ | Ненормированная величина кососимметричных билинейных scores |
| $\bar\Delta^{(\ell)}$ | Средний top-1/top-2 gap по строкам и головам | Средний разрыв двух наибольших logits |
| $H^{(\ell)}$ | $(nH)^{-1}\sum_{h,i}\mathcal H(A_{i,:}^{(\ell,h)})$ | Средняя Shannon entropy вероятностей attention |

Формулы и словесные определения: [7.1](../source/0239-0244-ch07-information-flow-and-residual-stream.md#equation-7-1), [PDF 241](../source/0239-0244-ch07-information-flow-and-residual-stream.md#pdf-0241), [7.3](../source/0239-0244-ch07-information-flow-and-residual-stream.md#equation-7-3).

У $\Gamma$ есть существенная неоднозначность реализации: в многоголовом слое разные $M_a^{(\ell,h)}$, но здесь записан один $M_a^{(\ell)}$ без правила агрегирования по h. Кроме того, это не автоматически intrinsic energy $\Gamma_U(M_a)=\|P_U M_a P_U\|_F$ главы 6. Эти нормы совпадают, когда строки X — ортонормированный базис U; при произвольной X влияют их длины, кратности и Gram matrix. Нулевые значения обеих норм при $\operatorname{rowspan}X=U$ эквивалентны изотропности U для билинейной формы, а не обязательному включению $U\subseteq\ker M_a$. [Definition 6.65](../source/0225-0233-ch06-activation-corrections-and-synthesis.md#definition-6-65), [Proposition 6.68](../source/0225-0233-ch06-activation-corrections-and-synthesis.md#proposition-6-68).

## Cone index не равен рангу

Утверждение о сфере радиуса $\sqrt d$ в $\mathbf1^\perp$ относится к идеализированному LayerNorm без обучаемых affine-параметров и без изменяющего радиус $\varepsilon$. Для обычного affine LayerNorm нужны соответствующие поправки; поэтому при измерении важно фиксировать точку извлечения и нормировку. [Источник перед Definition 7.1](../source/0239-0244-ch07-information-flow-and-residual-stream.md#definition-7-1), [условия LayerNorm в карточке 28](28-gradients-factorisation-and-layer-normalisation.md).

При $\|x_i\|^2=d$ выполняется редакторское тождество

$$
\mathcal I_{\mathrm{cone}}^2
=nd+2\sum_{i<j}x_i^\top x_j,
\qquad
0\le\mathcal I_{\mathrm{cone}}\le n\sqrt d.
$$

Поэтому индекс связан с суммой попарных скалярных произведений. Для независимых центрированных строк фиксированной длины $\mathbb E\sum_i x_i=0$, но **не** $\mathbb E\|\sum_i x_i\|=0$: в этом случае $\mathbb E\mathcal I_{\mathrm{cone}}^2=nd$. Фраза источника “zero in expectation” корректна для векторной суммы, а не для её нормы.

Большая согласованность не означает точного падения ранга. Например, для ортонормированных $u,v\in\mathbf1^\perp$ строки

$$
x_1=r(\cos\theta\,u+\sin\theta\,v),\qquad
x_2=r(\cos\theta\,u-\sin\theta\,v)
$$

при $0<\theta<\pi/2$ имеют ранг 2, а индекс $2r\cos\theta$ приближается к максимально возможному $2r$ при $\theta\to0$. Убывание второстепенного сингулярного числа, точный ранг, effective rank и cone index — разные показатели. Это ограничивает вывод источника о progressively lower-dimensional subspaces.

## Proposition 7.2: абсолютная величина проекции меняет знак вклада

Пусть $a=\|\sum_i x_i^{\mathrm{old}}\|>0$, $u=(\sum_i x_i^{\mathrm{old}})/a$, а

$$
\delta=\left\langle u,\frac1n\sum_i\Delta_i\right\rangle.
$$

Из определения $\Delta_i=x_i^{\mathrm{new}}-x_i^{\mathrm{old}}$ точно следует

$$
\left\langle u,\sum_i x_i^{\mathrm{new}}\right\rangle=a+n\delta,
\qquad
\mathcal I_{\mathrm{new}}\ge|a+n\delta|.
$$

Здесь $\delta$ — **знаковая** скалярная проекция. Формула (7.2) заменяет её вклад на положительную норму и потому неверна без дополнительного ограничения. Если старый агрегат равен нулю, используемый в Proposition 7.2 unit vector вообще не определён. [Proposition 7.2 и её proof](../source/0239-0244-ch07-information-flow-and-residual-stream.md#proposition-7-2).

**Контрпример, совместимый с идеальной нормировкой.** Возьмём $n=2$, $d\ge3$, ортонормированные $u,v\in\mathbf1^\perp$, $r=\sqrt d$. Обе старые строки равны $ru$, новые — двум строкам из предыдущего примера. Тогда

$$
\mathcal I_{\mathrm{old}}=2r,\qquad
\phi=\theta,\qquad
\delta=r(\cos\theta-1),\qquad
\mathcal I_{\mathrm{new}}=2r\cos\theta.
$$

Правая часть напечатанного (7.2) равна

$$
2r\cos\theta+2r(1-\cos\theta)=2r,
$$

что строго больше левой части при $0<\theta<\pi/2$. Все углы остаются внутри указанного автором диапазона вогнутости cosine. Ссылка на Jensen не исправляет потерянный знак. Достаточное условие $\delta\ge0$ действительно даёт неубывание агрегатной нормы, а $\delta>0$ — её рост; архитектура сама по себе этого условия не обеспечивает.

## Theorem 7.3: заявленная оценка энтропии неверна

Для одной строки с двумя logits $(1,0)$:

$$
\mathcal H(\operatorname{softmax}(1,0))
=\log(1+e^{-1})+\frac1{e+1}
\approx0.582203,
$$

тогда как правая часть (7.4) равна только $\log(1+e^{-1})\approx0.313262$. Контрпример уже однострочный; усреднение по головам не требуется. Теорема помечена в источнике как preview с доказательством в другой главе, но заявленная здесь формула проверяется непосредственно. [Theorem 7.3](../source/0239-0244-ch07-information-flow-and-residual-stream.md#theorem-7-3).

**Корректное редакторское утверждение для отдельной строки.** Пусть её наибольший logit превосходит второй не менее чем на $\Delta\ge0$, $n\ge2$. Обозначим

$$
Z=1+(n-1)e^{-\Delta},\qquad
q=\frac{(n-1)e^{-\Delta}}{Z}.
$$

Тогда максимальная вероятность не меньше $1-q$, и

$$
\mathcal H(p)
\le h_2(q)+q\log(n-1)
=\log Z+\Delta q,
$$

где $h_2$ — binary entropy в натуральных логарифмах. Равенство достигается на logits $(0,-\Delta,\ldots,-\Delta)$. Для фиксированного n эта верхняя граница стремится к нулю с порядком $O((1+\Delta)e^{-\Delta})$.

**Среднего gap недостаточно.** Возьмём $n=2$, одну голову и две строки logits $(0,0)$ и $(2M,0)$. Средний gap равен M и растёт без ограничения, но средняя энтропия стремится к $\frac12\log2$, поскольку одна строка остаётся uniform. Нужна, например, нижняя граница на gap **каждой** строки или контроль доли строк с малыми gaps. Нельзя подставлять среднее в per-row bound без соответствующего доказательства; экспонента $e^{-x}$ выпукла, а обычное применение Jensen даёт противоположное направление для усреднения этой части.

Малый top-1/top-2 gap также не означает uniform attention: logits $(0,0,-K)$ при большом K дают приблизительно $(1/2,1/2,0)$. Даже увеличение gap не гарантирует уменьшения фактической энтропии, если остальные logits меняются: $(0,-0.1,-100)$ имеет entropy около 0.692, а $(0,-0.2,-0.2)$ — около 1.094 при большем gap.

## Почему coherence chain не является установленной теоремой

Первая стрелка $\mathcal I_{\mathrm{cone}}\uparrow\Rightarrow\Gamma\uparrow$ не следует из согласованности строк. Для приведённого выше $X_\theta$ положим

$$
M_a=uv^\top-vu^\top.
$$

Тогда при $0<\theta<\pi/4$

$$
\mathcal I_{\mathrm{cone}}=2r\cos\theta,\qquad
\|X_\theta M_aX_\theta^\top\|_F
=\sqrt2\,r^2\sin(2\theta).
$$

При уменьшении $\theta$ cone index **растёт**, а directional energy **падает**, хотя $M_a$ фиксирован и невырожден на плоскости $\operatorname{span}(u,v)$. Операторная норма даёт верхнюю границу действия, а не заявленную в sketch универсальную положительную нижнюю границу. [Proposition 7.4](../source/0239-0244-ch07-information-flow-and-residual-stream.md#proposition-7-4).

Вторая стрелка также не универсальна. Матрицы logits $L=c\mathbf1^\top$ дают одинаковую uniform attention и нулевые gaps при любом c, тогда как $(L-L^\top)/2$ можно сделать произвольно большой масштабированием непостоянного c. Это row-additive gauge, уже разобранный в [карточке 39](39-open-and-closed-gates.md). Для фиксированной full-row-rank X такие logits реализуемы подходящим M; это не требует считать любой gauge реализуемым при любой заранее ограниченной голове. Критерий направленного ребра главы 5 также не превращает одну глобальную Frobenius norm в контроль всех top-1/top-2 gaps.

Последняя стрелка не выводится из неверного (7.4); даже корректная per-row upper bound не устанавливает монотонность actual entropy при произвольном изменении профиля остальных logits. Фраза “sufficiently discriminative input” не задаёт количественных условий, устраняющих эти контрпримеры.

Figure 7.6 остаётся **авторской эмпирической кривой** для указанной последовательности. Figure 7.7 показывает рост многих участков, но у $\Gamma$ и margin последние участки убывают, а H в конце возрастает; строгую монотонность всех кривых рисунок не подтверждает. Численные значения, отнесённые к будущему appendix, здесь не реконструируются. [Figure 7.6](../source/0239-0244-ch07-information-flow-and-residual-stream.md#figure-7-6), [Figure 7.7](../source/0239-0244-ch07-information-flow-and-residual-stream.md#figure-7-7).

Для проекта четыре ряда измерений полезны как **раздельные диагностические метрики**. Следует фиксировать их нормировки, точку извлечения, агрегирование по головам, распределение индивидуальных gaps и фактические сингулярные числа. Автоматически подменять ими причинную цепочку, размерность представления или качество решения задачи нельзя.


## Продолжение: уточнённая глава 8

[Карточка 77](77-cone-svd-and-directional-activation.md) проверяет новое qualitative cone–SVD утверждение с точной нормировкой §7.10.1. [Карточка 78](78-margin-entropy-proxy-and-compatibility.md) разделяет корректную margin–entropy bound, условный proxy и фактическую энтропию.
