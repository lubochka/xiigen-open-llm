---
title: "109. Послойные наблюдаемые: Γ, α, γ и effective rank"
source_pdf_pages: [373, 374]
book_pages: [337, 338]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 109. Послойные наблюдаемые: Γ, α, γ и effective rank

[Индекс библиотеки](../README.md) · [§11.4](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#section-11-4) · [Предыдущая карточка](108-two-sars-orbits-and-local-interventions.md) · [Следующая карточка](110-skew-gradient-flow-and-symplectic-eigenplanes.md)

## Три уровня измерений

Глава меняет точку наблюдения: вместо траектории одной головы рассматривается последовательность слоёв. В §11.4 названы directional energy, нормированная активация каждой головы и effective rank выходного проектора. Их нельзя сводить к одному числу.

| Величина | Определение здесь | Что фиксирует |
|---|---|---|
| Энергия одной головы | $\Gamma_h(X)=\lVert XM_{a,h}X^\top\rVert_F$ | Видимую на данном $X$ антисимметричную часть scores |
| Средняя энергия слоя | $\Gamma^{(l)}=H_l^{-1}\sum_h\Gamma_h(X^{(l)})$ | Среднее по выбранному множеству активных голов |
| Parameter-side asymmetry | $\alpha_h^{(l)}=\lVert M_a^{(h,l)}\rVert_F/\lVert M^{(h,l)}\rVert_F$ | Долю **нормы**, приходящуюся на антисимметричную часть |
| Относительная активация | $\gamma_h^{(l)}(t)=\alpha_h^{(l)}(t)/\alpha_h^{(l)}(0)$ | Изменение этого отношения относительно инициализации |
| Effective output rank | $r_{\mathrm{eff}}(W_O^{(l)})$ | Эффективное число выходных направлений при выбранном определении rank |

Источники: [Definition 11.2, (11.1)](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#definition-11-2), [Definition 11.4, (11.2)](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#definition-11-4), [§11.4.3](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#section-11-4-3).

$M=W_QW_K^\top$ имеет размер $d\times d$, тогда как $XM X^\top$ имеет размер $n\times n$. Называя $M$ «score matrix», этот фрагмент использует сокращённый язык; при вычислении размерностей два объекта нужно разделять. Здесь $\Gamma$ — **норма**, без квадрата; ранее в §9.15 использовалась квадратная score-energy. Температура или множитель $1/\sqrt{d_k}$ дополнительно меняют масштаб наблюдаемой, если включить их в scores. [§11.4.1](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#section-11-4-1), [карточка 91](91-signature-differential-chain-and-sa-motors.md).

## Почему вход нельзя исключить

Для $X=uv^\top$ ранг входа не превосходит единицы. Тогда

$$
XM_aX^\top=u(v^\top M_av)u^\top=0,
$$

поскольку $v^\top M_av=0$ у любой вещественной антисимметричной матрицы. Это точный смысл примера с коллапсом токенов в Remark 11.3. Сам источник называет такой вход “isotropic”; здесь слово употреблено неточно: коллапс к одной прямой не является изотропной ковариацией в многомерном пространстве. [Remark 11.3](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#remark-11-3).

Обратное утверждение требует условий. Ранг $X\ge2$ ещё не гарантирует $\Gamma>0$: строки могут не видеть активную плоскость $M_a$, а вклады нескольких eigenplanes могут компенсироваться. Например, при $M_a$ с действием только в плоскости $(e_3,e_4)$ и строках $X$ в $\operatorname{span}(e_1,e_2)$ получаем $XM_aX^\top=0$. Поэтому representational diversity не означает автоматического усиления любого антисимметричного оператора. [Нормальная форма и формула площадей](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#equation-11-5), [подробный пример компенсации](80-conditional-compatibility-and-eigenplane-cancellation.md).

У $\Gamma^{(l)}$ есть ещё две конвенции: $H_l>0$ и точное множество «active heads». Структурно включённые головы, головы с ненулевым выходом и головы с $\gamma>1$ — разные множества. Изменение знаменателя или состава множества способно изменить среднее без роста каждой головы. Definition 11.2 не задаёт отдельной процедуры для случая $H_l=0$. [Definition 11.2](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#definition-11-2).

## Нормировки α различаются

Из ортогональности $M_s$ и $M_a$ следует

$$
\alpha_h^2
=\frac{\lVert M_a\rVert_F^2}
{\lVert M_s\rVert_F^2+\lVert M_a\rVert_F^2}.
$$

Таким образом, нынешняя $\alpha_h$ — квадратный корень из **parameter-side squared balance** §10.9.1. Это также не **score-side** отношение $\Gamma_a/(\Gamma_s+\Gamma_a+\varepsilon)$ из §10.4: последнее зависит от $X$, использует другую нормировку и может включать стабилизатор. При сопоставлении случайной инициализации $\alpha_h\approx1/\sqrt2$ с эмпирическими $\bar\alpha\approx0.45$ сначала нужно установить, какая переменная изображена. [Definition 11.4](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#definition-11-4), [§10.9.1](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#section-10-9-1), [§10.4](../source/0342-0346-ch10-canonical-pair-and-score-splitting.md#section-10-4).

Формула $\gamma(t)$ определена только при ненулевых необходимых знаменателях: $M(t)\ne0$, $M(0)\ne0$ и $\alpha(0)>0$. При этих условиях $0\le\alpha(t)\le1$, поэтому

$$
0\le\gamma(t)\le\frac1{\alpha(0)}.
$$

Условие $\gamma>1$ означает увеличение **отношения**, не обязательный рост $\lVert M_a\rVert_F$ или $\Gamma$. Например, при фиксированных $X,M_a\ne0$ и уменьшающейся симметричной части $M_s$ величина $\alpha$ растёт, а $\Gamma$ остаётся постоянной. Состояние $M=0$ требует отдельной конвенции; автоматическая подстановка $0/0=0$ в исходнике не предусмотрена. [Формула (11.2)](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#equation-11-2).

## Связь с cone и rank — отдельное утверждение

Remark 11.5 связывает $\gamma>1$ с ростом $I_{\mathrm{cone}}$ и отсылает к Proposition 11.40. Эта связь не следует из определения: $\gamma$ измеряет параметры, а cone — геометрию представлений. Изменение одного показателя можно реализовать при фиксированном другом. На этих двух страницах связь лишь заявлена и снабжена отсылкой. [Remark 11.5](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#remark-11-5).

Аналогично, §11.4.3 сообщает эмпирическое уменьшение $r_{\mathrm{eff}}(W_O)$ с глубиной, но не воспроизводит здесь данные, определение effective rank и правила порога/нормировки. Это не тождественно уменьшению точного алгебраического ранга. При чтении дальнейших оценок необходимо сохранять выбранное определение и различать независимые координаты: глубину $l$ и время обучения $t$. [§11.4.3](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#section-11-4-3).

