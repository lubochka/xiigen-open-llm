---
title: "303. Signed gap, class-internal separation и особый случай нулевого оператора"
source_pdf_pages: [716, 717]
book_pages: [680, 681]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 303. Signed gap, class-internal separation и особый случай нулевого оператора

## Корректное ядро signed operator

При симметричной $\Sigma$ и skew-symmetric $K$,

$$
A^{\mathrm{sig}}
=\operatorname{sym}(\Sigma K)
=\frac{\Sigma K-K\Sigma}{2}.
$$

Транспонирование сохраняет эту матрицу, а cyclicity trace даёт $\operatorname{tr}A^{\mathrm{sig}}=0$. Поэтому ненулевой $A^{\mathrm{sig}}$ имеет хотя бы один положительный и один отрицательный eigenvalue: иначе сумма eigenvalues не могла бы быть нулём. [Signed residual display](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#display-0716-signed-residual-operator).

Это не означает попарной симметрии spectrum $\{\lambda,-\lambda\}$, равенства числа положительных и отрицательных modes или существования отдельной involution, переводящей каждую sign class в другую. Для таких выводов нужна дополнительная структура. Утверждения о signed gap следует читать в общем indefinite setting.

## Два gap отвечают на разные вопросы

В (17.50) $\Delta^\pm=\min(\lambda^+,|\lambda^-|)$ измеряет удалённость выбранных крайних signed eigenvalues от нуля. В (17.51) $g^+$ и $g^-$ измеряют расстояния до следующего eigenvalue **в той же sign class**. Первое относится к сохранению знака и численной различимости пары; второе — к устойчивости выбора одного ведущего направления. [(17.50)–(17.51)](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#equation-17-50).

Например, для trace-zero spectrum

$$
A=\operatorname{diag}(1,\ 1-\epsilon,\ -(2-\epsilon)),
\qquad 0<\epsilon<1,
$$

signed gap равен 1, а positive internal gap — $\epsilon$. Знаки устойчивы к perturbations масштаба существенно меньше единицы, но конкретный leading positive eigenvector чувствителен при perturbations масштаба $\epsilon$.

Для полного angular bound нужна separation выбранного eigenvalue от **всего** остального spectrum. Если есть zero modes или противоположный sign cluster ближе, учитывается и это расстояние. Если sign class содержит только один eigenvalue, «second-largest positive» либо «second-most-negative» не существует; implementation должна задать отдельную convention, а не обращаться к отсутствующему значению.

## Relative rank threshold не распознаёт абсолютный нуль сам по себе

Definition 17.15 полезно разделяет numerical rank и architectural growth: mode может быть ненулевым при $\theta_{\mathrm{rank}}<|\lambda|<\theta_w$, но не заслуживать новой head. Однако fallback требует явной обработки zero operator. [Формула (17.52) и Definition 17.15](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#equation-17-52).

Если $\|A\|$ понимается как operator norm, то для ненулевой symmetric indefinite matrix

$$
\max(\lambda^+,|\lambda^-|)=\|A\|_2.
$$

При $0<\sqrt{\varepsilon_{\mathrm{mach}}}<1$ обе крайние амплитуды одновременно не могут оказаться ниже $\sqrt{\varepsilon_{\mathrm{mach}}}\|A\|_2$. Значит, case (iii) с **обоими** endpoints ниже этого чисто relative cutoff не сработает для такого ненулевого оператора. Это вывод при указанной norm convention; source не уточняет norm во всех occurrences.

При $A=0$ сам threshold тоже равен нулю, а strict positive–negative pair не существует. Если code искусственно положит оба endpoints равными нулю, проверки “$\ge\theta_{\mathrm{rank}}$” в case (i) станут истинными, а “below” в case (iii) — ложными. Нужен предварительный zero/empty-class guard и, если требуется абсолютная численная малость, отдельно заданный absolute scale. Relative cutoff определяет обусловленность внутри данного оператора, но не его абсолютную значимость.

## Block fallback ещё должен определить score

При Type A/B recovery subspace устраняет зависимость от неустойчивого отдельного eigenvector только для величин, инвариантных к смене basis этого subspace. Если $R_h,Q_h$ или последующая motor construction используют один выбранный direction, требуется определить, как они вычисляются из block. Само направление на block solver ещё не делает исходный scalar score basis-invariant. [Level-two score](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#display-0716-level-two-protection-score).

При averaging signed eigenvectors также надо согласовывать их произвольные знаки и нормировку; иначе усреднение противоположных представлений одного direction может дать нуль. Это отдельная implementation задача, не следствие существования signed gap.

Definition 17.15 полностью заканчивается на PDF 717. Дальнейший анализ conditioning и leakage продолжается в [следующем source-файле, PDF 718](../source/0718-0721-ch17-signed-score-conditioning-and-numerical-floor.md#pdf-0718). Текущая карточка фиксирует algebra, различие gaps и начальные guards; она не объявляет доказанными все последующие утверждения раздела.
