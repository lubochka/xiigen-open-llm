---
title: "244. Неортогональная дефляция: RIP, точный projector и потерянные константы alignment"
source_pdf_pages: [628, 629]
book_pages: [592, 593]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 244. Неортогональная дефляция: RIP, точный projector и потерянные константы alignment

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Что даёт Gram-RIP

В (16.53) $\delta_K=\|Q_K^\top Q_K-I\|_2$. Условие $\delta_K<1$ помещает собственные значения Gram matrix в $[1-\delta_K,1+\delta_K]$, обеспечивает независимость столбцов и, следовательно, $K\le d$. Для неортонормированного $Q$ ортогональный projector на span равен

$$
P_Q=Q(Q^\top Q)^{-1}Q^\top,
$$

а $QQ^\top$ вообще не projector. Нужно указать, какая дефляция реально применяется: последовательные $I-vv^\top$, QR/Gram–Schmidt или projector на весь текущий span. Их поведение при коррелированных направлениях различается. [RIP condition](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#section-16-10-1), [следующий, уже другой режим](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#theorem-16-38).

## Alignment не даёт коэффициент из (16.54)

Для unit vector $v$, PSD $A$ и unit leading eigenvector $w$, условие $|v^\top w|\ge1-\varepsilon$ даёт

$$
v^\top Av\ge(1-\varepsilon)^2\lambda_{\max}(A).
$$

Чтобы заменить коэффициент на $1-\delta$, нужно $(1-\varepsilon)^2\ge1-\delta$, то есть $\varepsilon\le1-\sqrt{1-\delta}$; обозначение $\varepsilon=O(\delta)$ не задаёт нужной константы. [Исходный коэффициент](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#equation-16-54), [R1–R3](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#theorem-16-37).

Конкретный пример: $\delta=0.1$, $v_1=e_1$, $v_2=0.1e_1+\sqrt{0.99}e_2$, $w=0.9e_1+\sqrt{0.19}e_2$, $A=ww^\top$. Оба столбца нормированы, $\delta_K=0.1$ и alignment первого шага равен 0.9, но $v_1^\top Av_1=0.81<0.9=(1-\delta_K)\lambda_{\max}(A)$. После точного первого projection residual направлен по $e_2$, и второй alignment равен $\sqrt{0.99}\ge0.9$. Таким образом, даже совместимость R1–R3 при $\varepsilon=\delta$ не восстанавливает промежуточную оценку (16.54). Это контрпример к шагу доказательства; он сам по себе не вычисляет наилучший возможный global bound (16.55).

## Норма, её квадрат и trace

В (16.54) символ $\Delta\Gamma_k$ отождествлён с Rayleigh quotient. При $P=I-vv^\top$ и unit $v$ точные тождества для symmetric $A$ имеют вид

$$
\operatorname{tr}(A)-\operatorname{tr}(PAP)=v^\top Av,
$$

$$
\|A\|_F^2-\|PAP\|_F^2=2v^\top A^2v-(v^\top Av)^2.
$$

Для eigenvector второй decrement равен $\lambda^2$, а первый — $\lambda$. Поэтому recurrence для Frobenius **нормы** нельзя получить заменой её изменения Rayleigh quotient без отдельного вывода. При signed $A$ дополнительно перестаёт работать простой PSD alignment argument. [Заявленная residual recurrence](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#equation-16-56).

Из корректной верхней geometric envelope можно получить достаточное число шагов для достижения порога, округляя вверх. Это не нижняя граница на фактический first hitting time. Фактор $(1+\delta)^2$ в (16.55) не выводится одной сходимостью Neumann series; нужен tracking inequality с определённым update. Случай $\delta=O(1)$ даёт равномерный constant-factor overhead только при зазоре $1-\delta$ от нуля. [Решение по K и последующее пояснение](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#equation-16-57).

## Coherence: достаточность и необходимость

При unit columns Gershgorin действительно даёт $\delta_K\le(K-1)\varepsilon$, так что $\varepsilon<1/(K-1)$ **достаточно**. Это не необходимое условие. Для трёх vectors с единственной коррелированной парой $v_1^\top v_2=0.75$ и $v_3\perp\{v_1,v_2\}$ получаем $\delta_K=0.75<1$, хотя максимальный pairwise inner product превышает $1/2$. Поэтому утверждение о точном переходе режимов при coherence $1/K^*$ не следует из этой оценки. [Абзац Condition (R1) and coherence](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#pdf-0629).
