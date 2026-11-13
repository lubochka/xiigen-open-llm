---
title: "305. Обусловленность signed score и устойчивость top-k routing"
source_pdf_pages: [716, 720]
book_pages: [680, 684]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 305. Обусловленность signed score и устойчивость top-k routing

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Достаточно ли signed gap для устойчивого protection score и совпадает ли top-k margin со спектральным gap?

## Корректный объект для анализа

В исходной записи score имеет вид

$$
S=\frac{a}{r+\varepsilon_\rho}(1-R)Q,
\qquad a=\bar\rho_h,\quad r=\rho(A_h^{\rm sig}).
$$

При $a,Q>0$, $r+\varepsilon_\rho>0$ и $R<1$ его первый относительный дифференциал равен

$$
\frac{dS}{S}=\frac{da}{a}-\frac{dr}{r+\varepsilon_\rho}
-\frac{dR}{1-R}+\frac{dQ}{Q},
$$

если $\varepsilon_\rho$ фиксирована. Условные числа нескольких независимых входов не становятся произведением только потому, что функция содержит произведение. При относительных perturbations вклад $R$ имеет коэффициент $R/(1-R)$, а при абсолютной ошибке $Q$ появляется $1/Q$. Ограниченность $R,Q$ сверху не ограничивает эти величины. [Определение score](../source/0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#pdf-0716), [утверждение и (17.55)](../source/0718-0721-ch17-signed-score-conditioning-and-numerical-floor.md#equation-17-55).

Например, при $R=.9999$ изменение $R$ на $10^{-5}$ меняет score на 10%, хотя само изменение redundancy очень мало. Выбор $\varepsilon_\rho$ стабилизирует один знаменатель и не устраняет этот механизм. Если score близок к нулю, абсолютная ошибка и margin до protection threshold обычно полезнее относительной ошибки.

Нужно также зафиксировать единицы perturbation. При $A\mapsto cA$ eigenvectors и относительные gaps не меняются, а $\Delta_h^\pm$ имеет масштаб $c$. Если $a,r,\varepsilon_\rho$ масштабируются вместе, их отношение в score неизменно. Поэтому выражение $1/\Delta_h^\pm$ без нормировки входной ошибки нельзя автоматически считать безразмерным относительным condition number. Для amplitude крайних eigenvalues действует оценка из [карточки 304](304-signed-eigenvector-drift-leakage-and-averaging-bias.md); внутренний eigenvector gap не обязан входить в её скалярную чувствительность.

## Top-k выбирает токены по другим разрывам

В §17.6.6 сопоставлены bilinear scores $s_j=q^\top W_sx_j/\sqrt d$ и извлечение ведущего eigenspace. Для фиксированного запроса top-$k$ устойчив, если его margin

$$
m_k=s_{(k)}-s_{(k+1)}>2\epsilon_s
$$

при ошибке каждого score не более $\epsilon_s$. Это разрыв в конечном наборе **token scores**, зависящий от запроса и ключей. Его нельзя заменить eigengap матрицы $W_s$. [Мост к MHB-G](../source/0718-0721-ch17-signed-score-conditioning-and-numerical-floor.md#section-17-6-6).

- Пусть $W_s=I_2$, $q=e_1$, ключи $e_1$ и $-e_1$. Спектр полностью кратен, а scores равны $\pm1/\sqrt2$: выбор первого ключа имеет margin $\sqrt2$.
- Пусть $W_s=\operatorname{diag}(2,1)$, $q=e_2$, ключи $(0,1)$ и $(1,1+\delta)$. Eigenvalue gap равен 1, а margin scores равен $|\delta|/\sqrt2$ и может быть сколь угодно мал.

Таким образом, near-degenerate routing scores и Type-B спектр — разные события. Вырождение $W_s$ не доказывает неустойчивый token routing, а устойчивый eigenspace не сертифицирует top-$k$ выбор.

## Что остаётся инженерной гипотезой

Связь rank collapse, repeated eigenvalues и residual connection в этом фрагменте дана как авторское объяснение со ссылкой «Lemma ??» на ещё будущую главу 19. Один forward pass не является автоматически итерационным eigen-solver; residual connection между слоями не эквивалентна накоплению обновлений параметров во времени обучения. Для переноса нужны явное отображение операторов, соответствующая метрика ошибки и сопоставимый эксперимент. [Продолжение рассуждения](../source/0718-0721-ch17-signed-score-conditioning-and-numerical-floor.md#pdf-0720).

Практическая запись должна включать score со всеми четырьмя входами, абсолютный margin решения, отдельно routing margin и spectral gap. Ни равномерная обусловленность score, ни сохранение rank MHB-G не проверены новым запуском в этой библиотеке.
