---
title: "201. Bidirectional gate: направление изменения, масштаб окна и neutralisation"
source_pdf_pages: [560, 561]
book_pages: [524, 525]
batch: "012"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 201. Bidirectional gate: направление изменения, масштаб окна и neutralisation

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Локальная переменная архитектурного действия

В (15.30) нормированные coverage, residual, marginal value, crowding и directional ratio объединены в $S_{\ell h}$; (15.31) задаёт $g_{\ell h}=\sigma(S_{\ell h})$. Definition 15.13 называет gate opening или closing по среднему изменению на окне. [Формулы и определение](../source/0560-0563-ch15-gates-growth-criteria-and-budget.md#section-15-14-1).

Это bidirectional в смысле **открытия и закрытия** gate. Сам коэффициент остаётся в $[0,1]$; он не меняет знак вклада головы. Более поздняя signed gate theory не должна ретроспективно подменять здесь смысл слова.

Фраза «большой coverage открывает, большой crowding закрывает» требует соответствующих знаков $b_\rho,b_\Gamma,b_V,b_C$. Directional bonus, согласно прозе, действует только при task demand или anchor status, но отдельного индикатора этого условия в (15.30) нет. Его реализация должна согласовать формулу с прозой, а не начислять всем головам одинаковый бонус за $\alpha$.

## Масштаб threshold зависит от длины окна

Если $\Delta g(\tau)=g(\tau)-g(\tau-1)$, то среднее из Definition 15.13 телескопируется:

$$
\frac1W\sum_{\tau=t-W+1}^{t}\Delta g(\tau)
=\frac{g(t)-g(t-W)}W.
$$

Поскольку $0\le g\le1$, его модуль не превышает $1/W$. Поэтому при $\epsilon_g\ge1/W$ строгий opening/closing test невозможен для любого сигнала. При более длинном окне и прежнем $\epsilon_g$ изменяется не только шум, но и доступный масштаб критерия.

Это точное следствие напечатанного определения при обычном finite-difference смысле $\Delta$. Если автор подразумевал derivative или иначе нормированные изменения, единицы и нормировку нужно определить отдельно. [Definition 15.13](../source/0560-0563-ch15-gates-growth-criteria-and-budget.md#definition-15-13).

## Logistic gate и ненулевой floor — разные свойства

Обычная logistic имеет значения строго между 0 и 1 для конечного $S$, но не гарантирует заранее выбранного положительного floor $g_{\min}$. Для такого ограничения нужна явная параметризация или projection. При этом floor сам по себе не исключает нулевой training gradient: contribution может быть нулевым или task gradient может быть ортогонален ему.

Даже для score derivative

$$
\frac{dg}{dS}=g(1-g)
$$

приближение к насыщению делает управление медленным. Следовательно, «gate существует», «gate способен открываться» и «компонент получает полезное обучение» остаются разными проверяемыми утверждениями. См. также [предыдущую карточку о floor](191-positive-gate-floor-and-gradient-starvation.md).

## Что доказывает neutralisation bound

Proposition 15.14 ограничивает output perturbation через $|g-g'|L_h\|F_{\ell h}\|$ и training drift. Для фиксированного линейного gated contribution разность точно равна $(g-g')F_{\ell h}$. Если уменьшать gate маленькими шагами, можно ограничить **разовую** perturbation и наблюдать адаптацию. [Proposition 15.14](../source/0560-0563-ch15-gates-growth-criteria-and-budget.md#proposition-15-14).

Но это не доказывает, что итоговое удаление после медленного закрытия всегда меньше меняет функцию относительно исходного состояния. При неизменном $F$ итоговая разность между $gF$ и $0$ равна $gF$ независимо от разбиения пути. Преимущество gradual neutralisation требует адаптации других компонентов, возможности отменить удаление и контроля validation. Сохранённые параметры quiet component обеспечивают возможность reopening, но не гарантируют сохранения их полезности после последующего drift.
