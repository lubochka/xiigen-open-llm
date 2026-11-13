---
title: "289. RQI: локальная кубическая сходимость и ошибка двумерного доказательства"
source_pdf_pages: [697, 697]
book_pages: [661, 661]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 289. RQI: локальная кубическая сходимость и ошибка двумерного доказательства

## Вопрос

Верна ли кубическая локальная сходимость RQI, если переход (17.22) → (17.23) неверен в общем случае?

**Полезное кубическое ядро можно обосновать, но данный proof необоснованно сводит задачу к двум инвариантным направлениям.** Универсальный коэффициент $1$ в (17.24) также не следует. Отдельно нужны правило знака и обработка singular shift. [Lemma 17.8 и (17.19)](../source/0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md#lemma-17-8).

## Где нарушается доказательство

Автор правильно пишет $u=v\cos\theta+w\sin\theta$, $w\perp v$, и $Aw=\lambda_\perp w+r$. Но если $r\ne0$, span$\{v,w\}$ не является invariant plane. Решение $(A-R(u)I)x=u$ в общем случае не представимо как $x=\alpha v+\beta w$. Обозначенный “cross” в (17.22) исчезает в следующей строке без оценки. Поэтому равенство $\tan\theta_{\mathrm{new}}=\tan^3\theta$ выведено только для случая, когда $w$ тоже eigenvector, либо имеет место соответствующее двухспектральное сведение. [Формулы (17.22–17.24)](../source/0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md#equation-17-22).

Проверка в трёх измерениях: пусть

$$
A=\operatorname{diag}(3,2,0),\quad
v=e_1,\quad
w=(e_2+e_3)/\sqrt2,\quad
u=\sqrt{1-s^2}\,v+s\,w.
$$

Здесь $\mu=R(u)=3-2s^2$. После точного inverse step

$$
\frac{\sin\theta_{\mathrm{new}}}{s^3}
\longrightarrow\frac{2\sqrt5}{3}\approx1.490711985
\quad(s\downarrow0),
$$

а не $1$. При $s=0.01$ численный коэффициент равен примерно $1.49106486$. Кубический **порядок** сохраняется, универсальный asymptotic coefficient исходной (17.24) — нет.

## Редакторское доказательство корректной локальной оценки

Пусть $\lambda$ — простой eigenvalue симметричной $A$,

$$
g=\min_{\lambda_j\ne\lambda}|\lambda-\lambda_j|>0,\qquad
D=\|A-\lambda I\|_2.
$$

Для $u=cv+sw$, $c=\cos\theta>0$, $s=\sin\theta$, имеем $|\mu-\lambda|\le Ds^2$. Если $Ds^2<g$ и $(A-\mu I)^{-1}$ определена, то на ортогональном дополнении к $v$

$$
\|(A-\mu I)^{-1}|_{v^\perp}\|_2
\le\frac1{g-Ds^2}.
$$

Целевая компонента inverse step равна $c/(\lambda-\mu)$. Поэтому

$$
\tan\theta_{\mathrm{new}}
\le\tan\theta\,\frac{Ds^2}{g-Ds^2}
=O\!\left(\frac Dg\,\theta^3\right).
$$

Это доказывает кубическую локальную оценку с константой, зависящей от spectral geometry, для хорошо определённых шагов. Для нормы $\|u_{\mathrm{new}}-v\|$ надо выровнять знак eigenvector; нормализация сама по себе может дать $-v$.

## Простота eigenvalue не устраняет singular shifts

Фраза $\Delta_\perp=\lambda-\lambda_\perp\ge g>0$ неверна для внутреннего eigenvalue. Например, у $A=\operatorname{diag}(1,0,-1)$ target $v=e_2$ прост, но для $w=(e_1+e_3)/\sqrt2$ имеем $\lambda_\perp=0=\lambda$. Все $u=\sqrt{1-s^2}e_2+sw$ имеют $R(u)=0$, и (17.18) использует сингулярную матрицу, сколь бы малым ни был $s>0$. На точном eigenvector сингулярность возникает также для extremal target. [Заявление после (17.21)](../source/0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md#equation-17-21).

Таким образом, “for any $u_0\in U$” требует соглашения об уже достигнутом eigenpair и исключительных singular shifts. Это не опровержение локального cubic principle, а уточнение домена алгоритма и исправление конкретного proof. Для проекта экспортируется условная локальная гарантия; вход в её basin, accuracy внутренних solves и signed/multiple-eigenvalue случаи проверяются отдельно.
