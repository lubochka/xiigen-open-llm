---
title: "Chapter 16: extension status, positive task operator, and exact deflation"
source_pdf_pages: [593, 597]
book_pages: [557, 561]
batch: "012"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: true
---

# Chapter 16: extension status, positive task operator, and exact deflation

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: Chapter 15 conclusion](0591-0592-ch15-regimes-and-dynamic-conclusion.md) · [Next: figure, moving targets, and block growth](0598-0600-ch16-moving-targets-block-growth-and-centering.md)

> Editorial note: Full transcription of PDF 593–597 (printed pages 557–561), including Tables 16.1–16.2, Lemma 16.1 and its proof, and Theorem 16.2 and its proof. Source inconsistencies in integer counts, relative versus additive assumptions, numerical calibration, and factors of two are retained. Bibliographic placeholders and the two separate source-status paragraphs are retained. The final sentence continues on PDF 598.

<a id="pdf-0593"></a>
## PDF 593 · Book page 557

<a id="chapter-16"></a>
# Chapter 16
# INCRT II: Extensions, Guarantees, and Applications

<a id="section-16-1"></a>
## 16.1 Role of the second INCRT chapter

INCRT I isolated the core dynamic controller of INCRT: a Rationalised Transformer equipped with diagnostic tracking, growth, preservation, pruning, and finite termination. The purpose of the present chapter is different. It collects the extensions that are not needed to define the core controller, but are needed to understand the range of the original INCRT programme and its modern rationalised form.

The guiding principle is that the original INCRT is not discarded. It is expanded along the axes where real architectures depart from the ideal core setting: tasks drift, inputs may route to different experts, training uses adaptive optimisers, time-scale separation can fail, operators may be indefinite, deployment imposes finite budgets and quantisation, and modern systems perform in-context or time-conditional computation. The role of this chapter is therefore to show that the INCRT I controller is not a single algorithmic trick, but the core of a family of incremental architectural laws.

**Organisation.** The chapter is divided into four families of extensions:

1. *Geometric and statistical guarantees:* compressed-sensing bounds, three-level growth, PAC guarantees, and non-orthogonal deflation.
2. *Continual and trans-task extensions:* persistent drift, expert count, architectural transfer, and adaptive optimiser dynamics.
3. *Mechanism refinements:* single-time-scale recovery, signed or indefinite task operators, rank-adaptive heads, and identifiability.
4. *Deployment and modern generative regimes:* minimum budgets, quantisation, in-context learning, and diffusion-time task families.

The material has been assembled from the parts deliberately left out of INCRT I and from the later extension notes. The emphasis is on careful integration rather than on turning every statement into a final theorem of the same status. Some results are rigorous under explicit assumptions, some are proof sketches, and some are clearly marked as conjectural or empirical signatures.

<a id="pdf-0594"></a>
## PDF 594 · Book page 558

**Status of results and illustrative tables.** Following the audit of this chapter, the status of each family of results is made explicit. The central compressed-sensing, deflation, rank-adaptive, and deployment statements are treated as conditional theorems under the hypotheses stated in the text. PAC, single-time-scale, optimiser, in-context, and diffusion statements are intentionally presented as proof sketches or conditional extensions when their full proofs require a separate appendix or a companion paper. The empirical tables in this chapter are synthetic calibration tables: they are consistent with the predicted scaling laws on controlled examples, but they are not presented as external benchmark evidence.

<a id="table-16-1"></a>
| Result family | Status in this chapter | Role |
|---|---|---|
| Compressed-sensing and exact deflation | Conditional theorem | Core geometric guarantee for the extended INCRT programme. |
| Rank-adaptive deflation | Conditional theorem | Rigorous count–rank invariant under orthogonal spectral deflation. |
| Deployment and quantisation floors | Conditional theorem | Budget- and precision-aware stopping rules. |
| Persistent drift and homeostasis | Theorem/proof sketch mix | Continual-learning extension of the core controller. |
| Input-conditional expert count and transfer | Proof sketch / scaling law | Explains routing and reuse of architectural capacity. |
| Adaptive optimisers and single-time-scale recovery | Proof sketch | Shows how diagnostic averaging repairs collapsed time scales. |
| PAC, in-context learning, diffusion families | Conditional / conjectural extension | Exports open directions to Volume II or companion work. |

**Table 16.1:** Proof status of the main extension families. The table prevents the extension chapter from assigning the same epistemic status to fully proved conditional results, proof sketches, empirical signatures, and conjectures.

**Bibliographic completion note.** This standalone draft keeps bibliographic calls neutral so that it can compile without the master `.bib` file. Before editorial submission, the placeholder citations must be replaced by the master bibliography. The audit identified the following keys as particularly important for this chapter: `esteves2026spectrally`, `fei2024scaling`, `ddt2024decoupled`, `silverstein2026symmetry`, `gross2025lowlatency`, `mansour2025geometry`, `latmix2026`, `gao2026spectral`, `sheshukova2025nonasymptotic`, `bennett1948spectra`, and `marconeuhoff2005validity`.

**Status of the statements and empirical tables.** This chapter deliberately mixes results of different proof status. Results labelled as theorems or lemmas are used only under their displayed assumptions; proof sketches are retained when the complete argument would require a long appendix or a separate paper. The small calibration tables are illustrative synthetic checks of the predicted scalings, not benchmark claims. They should be read in the same spirit as the controller illustrations of INCRT I: they make the mechanism visible, while full empirical validation at scale is left to companion experimental work. The two explicit conjectures of the chapter are kept as forward pointers toward Volume II.

<a id="section-16-2"></a>
## 16.2 Static notation

The chapter uses the notation of Chapters 14–15. The residual task operator is written as $\mathcal B$, the residual spectral or directional energy as $A_{\mathrm{res}}$, the residual amplification as $\Gamma_{\mathrm{res}}$, and

<a id="pdf-0595"></a>
## PDF 595 · Book page 559

<a id="table-16-2"></a>
| Extension axis | Replacement | Controller effect |
|---|---|---|
| Task drift | $\mathcal B\mapsto\mathcal B_t$ | growth/pruning becomes homeostatic |
| Input-conditional complexity | $\mathcal B\mapsto\mathcal B(x)$ | head count becomes expert count |
| Transfer | $\mathcal B_A,\mathcal B_B$ | growth depends on spectral overlap |
| Adaptive optimisation | gradient flow preconditioned by $P_t$ | gate speed changes, architecture law is preserved |
| Single time scale | diagnostics and weights move together | averaging recovers slow diagnostics |
| Indefinite tasks | $\mathcal B=\mathcal B_+-\mathcal B_-$ | signed gates and bilateral residuals |
| Rank-adaptive heads | rank-one atoms become rank-$r$ atoms | count–rank invariant |
| Deployment | budget and quantisation constraints | growth stops at an external floor |
| ICL | prompt induces implicit $\mathcal B_{\mathrm{ICL}}$ | context length acts as estimation budget |
| Diffusion | $\mathcal B\mapsto\mathcal B_\tau$ | heads specialise over denoising time |

**Table 16.2:** The extension map of INCRT II. Every row keeps the INCRT I controller but changes the object on which the controller acts.

the effective architecture size as $K^*$. When a quantity previously appeared as a “condition number”, it is interpreted here as a *directional task complexity index*: it measures how many stable residual directions must be represented, not the conditioning of a numerical linear system in the classical sense.

The extensions repeatedly use the same structural template. A core INCRT quantity is replaced by a family-indexed, time-indexed, input-indexed, or constraint-indexed quantity:

<a id="display-0595-indexed-task-operators"></a>
$$
\mathcal B\rightsquigarrow
\mathcal B_t,\ \mathcal B(x),\ \mathcal B_\tau,\
\mathcal B_+-\mathcal B_-,\ \widehat{\mathcal B}_b.
$$

The question is then whether the INCRT I growth law continues to control the residual demand after the replacement. Each section answers this question for one extension axis.

<a id="section-16-3"></a>
## 16.3 The compressed-sensing bound

The growth mechanism of INCRT I specifies *when* INCRT adds a head. The question of *how many* heads it adds in total is answered by a key quantitative result inherited from the original INCRT analysis and reformulated here in rationalised notation.

The result has the structure of a compressed-sensing bound: the number of heads needed to represent the task geometry is determined by the ratio of two spectral quantities, multiplied by a logarithmic factor that measures the required precision.

**The task operator.** For a given dataset $X\in\mathbb R^{n\times d}$ and a fixed antisymmetric motor $M_a\in\mathfrak{so}(d)$, the positive task-energy operator used in the compressed-sensing bound is defined as

<a id="equation-16-1"></a>
$$
\mathcal B=(XM_a)^\top(XM_a)\in\mathbb R^{d\times d}.
\tag{16.1}
$$

This operator is symmetric positive semidefinite by construction. The signed quantity $\operatorname{sym}(X^\top XM_a)$ is not positive in general and is therefore treated later in the indefinite-operator extension. The Frobenius norm $\Gamma_0=\|\mathcal B\|_F$ measures the total directional energy of

<a id="pdf-0596"></a>
## PDF 596 · Book page 560

the task as seen by the architecture. After $k$ heads have been added in directions $u_1,\ldots,u_k$ (the columns of the orthonormal matrix $Q^{(k)}\in\mathbb R^{d\times k}$), the residual operator is

<a id="equation-16-2"></a>
$$
A_{\mathrm{res}}^{(k)}=P_\perp^{(k)}\mathcal B P_\perp^{(k)},
\qquad P_\perp^{(k)}=I-Q^{(k)}Q^{(k)\top},
\tag{16.2}
$$

and the residual directional energy is $\Gamma_{\mathrm{res}}^{(k)}=\|A_{\mathrm{res}}^{(k)}\|_F$.

**The deflation lemma.** Each growth step removes the dominant residual direction. The statement is written in the Frobenius-energy convention used throughout this chapter: $\Gamma_{\mathrm{res}}^{(k)}=\|A_{\mathrm{res}}^{(k)}\|_F$. Normalised quantities, when needed, are introduced explicitly as ratios with respect to $\Gamma_0$.

<a id="lemma-16-1"></a>
**Lemma 16.1 (Exact rank-one deflation).** Let $A_{\mathrm{res}}^{(k-1)}\succeq0$ be the residual operator on $\operatorname{range}(P_\perp^{(k-1)})$, and let $u_k=v_1(A_{\mathrm{res}}^{(k-1)})$ be a unit leading eigenvector with eigenvalue $\lambda_1(A_{\mathrm{res}}^{(k-1)})$. Define $P_\perp^{(k)}=P_\perp^{(k-1)}-u_ku_k^\top$. Then

<a id="equation-16-3"></a>
$$
\bigl(\Gamma_{\mathrm{res}}^{(k)}\bigr)^2
=\bigl(\Gamma_{\mathrm{res}}^{(k-1)}\bigr)^2
-\lambda_1\bigl(A_{\mathrm{res}}^{(k-1)}\bigr)^2.
\tag{16.3}
$$

*Proof.* Since $A_{\mathrm{res}}^{(k-1)}=P_\perp^{(k-1)}\mathcal B P_\perp^{(k-1)}$, its range is contained in $\operatorname{range}(P_\perp^{(k-1)})$. Hence the leading eigenvector $u_k$ lies in this range and is orthogonal to all previously selected directions. Therefore $P_\perp^{(k)}$ is again an orthogonal projector. In the eigenbasis of $A_{\mathrm{res}}^{(k-1)}$, replacing $P_\perp^{(k-1)}$ by $P_\perp^{(k)}$ removes the rank-one component $\lambda_1u_ku_k^\top$ and leaves the remaining eigenspaces unchanged. Thus the squared Frobenius norm, which is the sum of squared eigenvalues, decreases by exactly $\lambda_1^2$. This is the rank-one case of the rank-$r$ deflation lemma proved later in this chapter. $\square$

The logarithmic compressed-sensing law requires a *relative* deflation assumption, not merely an additive lower bound on $\lambda_1$. The correct condition is that each accepted growth event captures a fixed fraction of the remaining Frobenius energy:

<a id="equation-16-4"></a>
$$
\lambda_1\bigl(A_{\mathrm{res}}^{(k)}\bigr)^2
\ge\frac1{\kappa_A^2}\bigl\|A_{\mathrm{res}}^{(k)}\bigr\|_F^2.
\tag{16.4}
$$

Under this condition, Lemma 16.1 gives

<a id="equation-16-5"></a>
$$
\bigl(\Gamma_{\mathrm{res}}^{(k)}\bigr)^2
\le\left(1-\frac1{\kappa_A^2}\right)^k\Gamma_0^2.
\tag{16.5}
$$

A weaker additive assumption $\lambda_1(A_{\mathrm{res}}^{(k)})\ge\delta_0$ would only imply a finite additive bound of order $(\Gamma_0^2-\theta_w^2)/\delta_0^2$, without the logarithmic factor.

<a id="theorem-16-2"></a>
**Theorem 16.2 (Compressed-sensing bound).** Suppose that:

- (B1) $\mathcal B\succeq0$ and $\|\mathcal B\|_F=\Gamma_0>\theta_w$;
- (B2) the relative deflation condition (16.4) holds for every accepted growth step before termination;
- (B3) each new head is added in the leading residual direction $v_1(A_{\mathrm{res}}^{(k-1)})$.

Then the number of heads $K^*$ sufficient to reduce $\Gamma_{\mathrm{res}}^{(K^*)}\le\theta_w$ satisfies

<a id="equation-16-6"></a>
$$
K^*\le2\kappa_A^2\log\frac{\Gamma_0}{\theta_w}.
\tag{16.6}
$$

<a id="pdf-0597"></a>
## PDF 597 · Book page 561

Equivalently, ignoring universal constants, $K^*=O\bigl(\kappa_A^2\log(\Gamma_0/\theta_w)\bigr)$. Moreover, the logarithmic dependence is essentially tight for hard families whose residual spectra maintain the relative-deflation ratio along the greedy trajectory.

*Proof.* From (16.5), it is enough that

<a id="display-0597-target-residual-inequality"></a>
$$
\left(1-\frac1{\kappa_A^2}\right)^K\Gamma_0^2\le\theta_w^2.
$$

Taking logarithms gives

<a id="display-0597-logarithmic-event-count"></a>
$$
K\ge\frac{2\log(\Gamma_0/\theta_w)}
{\log\bigl(1/(1-1/\kappa_A^2)\bigr)}.
$$

For $\kappa_A\ge1$, $\log(1/(1-x))\ge x$ for $x\in(0,1)$, hence the displayed condition is satisfied by $K=2\kappa_A^2\log(\Gamma_0/\theta_w)$. The lower-bound statement is the standard hard-spectrum construction: choose spectra whose leading eigenvalue remains a fixed $1/\kappa_A$ fraction of the Frobenius norm along the greedy path. Then no method restricted to one new one-dimensional atom per event can reduce the residual faster than the same geometric rate, up to constants. $\square$

**Interpretation.** The formula $K^*=O(\kappa_A^2\log\Gamma_0/\theta_w)$ has a natural reading. The logarithm $\log(\Gamma_0/\theta_w)$ counts the number of orders of magnitude by which the residual energy must decrease — it measures the difficulty of the precision requirement. The factor $\kappa_A^2$ counts how many steps are needed per order of magnitude — it measures how broadly the residual directional energy is spread across architectural directions. A task with small $\kappa_A$ is easy: each head captures a large fraction of the remaining energy. A task with large $\kappa_A$ is hard: many small steps are needed before the residual energy falls below threshold.

The formula says that these are the *only* two factors that matter. Details of the architecture — depth, width, head dimension — affect $K^*$ only through their impact on $\kappa_A$.

**Experimental calibration.** On the SARS-CoV-2 variant classification task of INCRT I, the directional task complexity index is $\kappa_T\approx9.1$, computed from $\Gamma_0=1.77$ and $\delta_0=\theta_w=0.175$. The theoretical prediction is $K_{\mathrm{pred}}^*=9.1^2\times\log(1.77/0.175)\approx191$ heads. The observed value is $K_{\mathrm{obs}}^*=191$, giving a ratio of exactly 1.00 — the bound is tight.

**The intrinsic complexity of a task.** Two consequences of the formula deserve emphasis. The first is the per-event guarantee: each growth event reduces $\Gamma_{\mathrm{res}}^2$ by at least $\delta_0^2$ (the lemma stated above), so the residual contracts strictly and uniformly. A head added on the dominant eigenvector of the current $A_{\mathrm{res}}$ not only reduces the residual — the size of the reduction is controlled. The second is the role of $\kappa_A=\Gamma_0/\delta_0$. This ratio is a property of the task itself, not of the model or of the choice of diagnostic. A task with small $\kappa_A$ is well-conditioned: a few heads suffice. A task with large $\kappa_A$ is ill-conditioned: many small steps are required. The head budget therefore depends on the task only through $\kappa_A$, and within that dependency it is sharp.

**Geometric decay, schematically.** the figure illustrates the contraction on a synthetic task with $\Gamma_0=1$, $\delta_0=\Gamma_0/3$, and growth threshold $\theta_w=0.05\Gamma_0$. The directional task complexity index is then $\kappa_A=3$, and the predicted head count is $K^*=\kappa_A^2\log(\Gamma_0/\theta_w)=9\log(20)\approx27$. The squared residual under orthogonal deflation tracks the envelope $\Gamma_0^2\rho^k$ with $\rho=1-1/\kappa_A^2$ and reaches the threshold at exactly the predicted $k=27$. A baseline that adds heads in

