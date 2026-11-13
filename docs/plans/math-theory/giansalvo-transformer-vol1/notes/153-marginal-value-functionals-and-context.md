---
title: "153. Marginal value: знак, контекст остальных голов и точный смысл coverage"
source_pdf_pages: [465, 466]
book_pages: [429, 430]
batch: "010"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 153. Marginal value: знак, контекст остальных голов и точный смысл coverage

[Индекс](../README.md) · [Источник и редакционная политика](../SOURCE.md)

## Одно правило разности, разные измеряемые свойства

[Definition 13.7](../source/0461-0467-ch13-crowding-and-marginal-value.md#definition-13-7) задаёт

$$
\mathcal V_\Phi(h\mid\mathcal H\setminus\{h\})
=\Phi(\mathcal H)-\Phi(\mathcal H\setminus\{h\}).
$$

Это эффект конкретного удаления при фиксированном наборе остальных голов. Он зависит от контекста $\mathcal H\setminus\{h\}$, выбранного функционала, распределения и протокола пересчёта. Это не универсальная «важность» головы.

| Функционал | Marginal | Что положительное значение означает |
|---|---|---|
| $\Phi_L=-L_{\mathrm{task}}$ | $L(\mathcal M_{\mathcal H\setminus\{h\}})-L(\mathcal M_\mathcal H)$ | Удаление увеличивает этот loss на выбранном распределении. |
| $\Phi_{\mathrm{Cov}}=\mathbb E\dim\sum_{g\in\mathcal H}\operatorname{range}\mathcal A_g$ | Потеря ожидаемой размерности диапазона | Голова добавляет направления, которых остальные не покрывают. |
| $\Phi_\Gamma=-\Gamma_{\mathrm{res}}$ | $\Gamma_{\mathrm{res}}(\mathcal H\setminus\{h\})-\Gamma_{\mathrm{res}}(\mathcal H)$ | Удаление увеличивает residual при заданном способе его вычисления. |

Источники: [Definitions 13.8–13.10](../source/0461-0467-ch13-crowding-and-marginal-value.md#definition-13-8), [PDF 466](../source/0461-0467-ch13-crowding-and-marginal-value.md#pdf-0466).

В общем определении заявлен codomain $\mathbb R_{\ge0}$, но $-L$ и $-\Gamma$ обычно неположительны. Для согласованной формализации достаточно разрешить $\Phi:\mathcal P(\mathcal H)\to\mathbb R$; добавление константы к $\Phi$ не меняет marginal. Исходный конфликт знака сохранён в английском тексте.

## Точная геометрия coverage

Пусть $W_{-h}(X)=\sum_{g\ne h}\operatorname{range}\mathcal A_g(X)$. Тогда вклад на одном cloud равен

$$
\dim\bigl(W_{-h}+\operatorname{range}\mathcal A_h\bigr)-\dim W_{-h}
=\dim\operatorname{range}\mathcal A_h
-\dim\bigl(\operatorname{range}\mathcal A_h\cap W_{-h}\bigr)\ge0.
$$

После ожидания это $\mathcal V_{\mathrm{Cov}}$. Нулевой marginal означает включение диапазона почти наверное; ненулевой — положительную вероятность добавления направления. Это вывод о подпространстве, не о величине score/output или о loss. Для $\varepsilon J$ диапазон не зависит от ненулевого $\varepsilon$, хотя действие оператора стремится к нулю вместе с $\varepsilon$.

Индивидуальное $r_{\mathrm{fn}}$ из (13.3) отвечает другому вопросу: сколько направлений сама голова не вращает. Например, две одинаковые невырожденные skew-головы на $\mathbb R^2$ имеют $r_{\mathrm{fn}}=0$, но обе имеют $\mathcal V_{\mathrm{Cov}}=0$ в присутствии другой. См. [149](149-functional-inactivity-and-joint-coverage.md).

## Почему удаление нескольких голов требует пересчёта

В том же примере совместное покрытие равно двум. Удаление любой одной головы имеет marginal ноль; удаление обеих уменьшает покрытие на два. Сумма leave-one-out значений относительно полного набора не равна эффекту совместного удаления. Правильная telescoping sum использует **последовательно обновляемый** набор голов. Нулевые marginals нельзя собирать в пакет независимых разрешений на pruning.

Для $\Gamma_{\mathrm{res}}$ источник ссылается на monotone decay из главы 11 и объявляет «additive ... at first order». Но функция на дискретных наборах не получает аддитивность из одной монотонности; нужно определить смысл первого порядка и проверить взаимодействия. Неотрицательность residual marginal требует конкретной subset-monotone процедуры: например, фиксированного target и корректно вложенных пространств ортогонального приближения. Пересчёт представлений модели, смена target или последовательные невложенные проекции не покрываются таким аргументом автоматически.

Наконец, нулевой $\mathcal V_\Gamma$ означает отсутствие изменения выбранного residual при удалении, а не $\mathcal A_h=0$. Голова может быть активна и полностью дублирована. В ресурсах следует хранить все marginals раздельно с их единицами и протоколом; потеря качества, размерность и норма residual не являются одним числом.
