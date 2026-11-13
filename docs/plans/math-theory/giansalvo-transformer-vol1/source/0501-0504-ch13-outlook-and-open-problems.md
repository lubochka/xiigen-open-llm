---
title: "Chapter 13: residual conjectures, open problems, and closing outlook"
source_pdf_pages: [501, 504]
book_pages: [465, 468]
batch: "011"
language: en
type: source-transcription
status: complete-for-range
---

# Chapter 13: residual conjectures, open problems, and closing outlook

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous pages](0496-0500-ch13-pruning-growth-and-synthesis.md#pdf-0500)

> Faithful English transcription of physical PDF pages 501–504 / printed pages 465–468. The file continues §13.12.1 with Conjecture 13.45 and completes Chapter 13. The source's unresolved references, missing OP9 label, incomplete “the theorem to .” phrase, and blank status words are retained literally. Editorial analysis is separate in Russian notes 171–172.

<a id="pdf-0501"></a>
## PDF 501 · Book 465

<a id="conjecture-13-45"></a>
**Conjecture 13.45 (Orbit crowding implies channel-destruction equivalence).** *Under the standing hypotheses of §13.1 and the nested observability hypotheses of Definition 13.23, for every pair of heads $(h,h')$ at layer $\ell$,*

<a id="equation-13-75"></a>
$$
d_{\mathrm{orb}}\!\left(\mathcal O_h^{(\ell)},\mathcal O_{h'}^{(\ell)}\right)\longrightarrow0
\quad\Longrightarrow\quad
\pi_{hh'}\!\left(\theta(X,i)\right)\longrightarrow0
\quad\forall\ i,X,
\tag{13.75}
$$

*where $\pi_{hh'}$ is the projection onto the inter-head block $\mathfrak m_{hh'}\subseteq\mathfrak{so}(d_{\mathrm{model}})$ of §13.7, and the convergence is in the appropriate functional norm uniformly in $X\sim\mu_X$.*

**Status.** The conjecture asserts that orbit-coincidence and dictionary-atom collapse are tightly coupled. Three pieces of partial evidence support it. First, by Theorem 13.24 of this chapter, dynamic equivalence implies functional equivalence under the nested-observability hypotheses; the conjecture extends the implication from functional to dictionary content. Second, by Theorem 13.31, head specialisation drives dictionary diversity; orbit coincidence implies specialisation coincidence, hence dictionary coincidence on the relevant inter-head block. Third, by the rank contraction analysis of §13.8, two heads writing into the same row space contribute identical rows to $\sum_h R_h^{(\ell)}$, giving a structural mechanism by which orbit-crowded pairs collapse onto the same dictionary direction. A rigorous proof would require quantitative bounds on each of the three implications and is left to the next volume.

**Architectural significance.** If proved, the conjecture would establish that the orbital redundancy notion of §13.2 is quantitatively equivalent, not merely qualitatively related, to the dictionary-geometric notion of redundancy of §13.7. The two notions are introduced from different starting points — orbits from Chapter 11, dictionary atoms from the dictionary-geometric analysis, and their equivalence would unify the trajectory-based and the algebraic-geometric pictures of non-redundancy.

<a id="section-13-12-2"></a>
### 13.12.2 Cross-layer redundancy

The chapter's diagnostics are stated layer by layer. Two heads at the same layer with similar signatures and orbits constitute within-layer redundancy. Two heads at different layers, however, can also play similar roles: a directional-tail head at layer $\ell$ may carry the same orientation as a head at layer $\ell+2$, with the intermediate layers acting as identity-like transitions. This is *cross-layer redundancy*, and it is beyond the scope of the within-layer functionals defined here.

<a id="conjecture-13-46"></a>
**Conjecture 13.46 (Cross-layer signature persistence).** *For trained transformers under the standing hypotheses, the expected number of head pairs $(h,h')$ at distinct layers $\ell\ne\ell'$ with signature similarity below a threshold scales linearly with the model depth $L$, with the proportionality constant determined by the re-extraction profile of Chapter ??.*

**Status.** The cross-layer extension is conceptually a re-application of the within-layer functionals to cross-layer pairs. The technical challenge is the lack of a canonical comparison map between signatures at different layers: a signature at layer $\ell$ lives in $\Sigma$, and so does a signature at layer $\ell+2$, but the two embeddings are not naturally isomorphic. The signature transport of Chapter ?? (§8.6) provides a candidate comparison; making it quantitative is the open task. The conjecture's specific form, with linear scaling in $L$, is suggested by empirical observations on BertBase: the layer-wise re-extraction cost $\mathcal R^{(\ell)}$ has a non-monotonic profile with maxima at transition layers, and the cross-layer similarities concentrate on layers between two consecutive maxima. A quantitative theorem would convert this empirical observation into a structural statement.

<a id="pdf-0502"></a>
## PDF 502 · Book 466

<a id="section-13-12-3"></a>
### 13.12.3 Task-specific redundancy

The third direction concerns redundancy detected on a downstream task distribution rather than on the pre-training distribution $\mu_X$. As noted at the end of §13.2, the three redundancy notions are data-dependent, and a head that is non-redundant on $\mu_X$ can become redundant on a narrower $\mu'_X$. The fine-tuning practice of head-pruning exploits this fact, but without a theoretical framework that distinguishes finetuning-induced redundancy from intrinsic redundancy.

- A possible framework is based on the marginal-value functionals $\mathcal V_\Phi$ of §13.5 computed on the downstream distribution. The extension is straightforward notationally; the substantive question is whether the thresholds $\eta,c$ of the pruning rule (Definition 13.42) admit task-specific rescalings that preserve the structural protection of Principle P1 against directional-tail compression.
- A more ambitious framework would establish a *transferability bound*: a non-redundancy verdict on $\mu_X$ implies a non-redundancy verdict on $\mu'_X$ as long as the divergence $D(\mu'_X\Vert\mu_X)$ is below a threshold. No candidate for the threshold is currently available; the question is open.

<a id="section-13-12-4"></a>
### 13.12.4 A catalogue of open problems

The rigorisation of the chapter's central propositions has produced a complementary set of open problems. Each is a question whose resolution would strengthen, extend, or quantitatively refine a result of the chapter. The catalogue is intended as a working reference: items are numbered **OP1–OP10** and grouped by the type of intervention they require.

<a id="open-problems-residual-conjectures"></a>
#### Open problems on residual conjectures

Three results in the chapter remain in conjectural form. A proof or a counterexample for each would close the corresponding gap in the chapter's logical architecture.

<a id="open-problem-op1"></a>
- **OP1** (Conjecture 13.16: approximate loss–coverage–energy implication). The residual term $R^{(\ell)}(h)$ in (13.24) captures the loss sensitivity not detected by either coverage or directional-energy diagnostics. Showing that $R^{(\ell)}(h)$ is small at trained configurations of realistic transformers — as opposed to generic configurations — would extend the operational reach of Theorem 13.15 beyond the strict H-rep hypothesis. The natural test bed is a sequence of trained checkpoints of BertBase or similar models, with $R^{(\ell)}(h)$ measured directly.

<a id="open-problem-op2"></a>
- **OP2** (Conjecture 13.45: orbit crowding implies channel-destruction equivalence). The conjecture asserts that orbit-coincidence and dictionary-atom collapse are tightly coupled. A proof requires three quantitative bounds: dynamic-equivalence to functional equivalence (already partially established by Theorem 13.24); functional equivalence to dictionary-coincidence on the relevant inter-head block; and dictionary-coincidence to channel-destruction equivalence in the sense of head-channel gauge symmetry.

<a id="open-problem-op3"></a>
- **OP3** (Conjecture 13.46: cross-layer signature persistence). A quantitative form requires a canonical comparison map between signatures at different layers. The signature transport of Chapter ?? (§8.6) is a candidate; making it quantitative is the technical core of the open task.

<a id="pdf-0503"></a>
## PDF 503 · Book 467

<a id="open-problems-quantitative-calibration"></a>
#### Open problems on quantitative calibration

Three results are rigorous but contain constants whose value depends on regime-specific factors. A precise calibration in each case would convert qualitative theorems into computable diagnostics for the training criterion.

<a id="open-problem-op4"></a>
- **OP4 (resolved).** *Lipschitz constant of softmax for degenerate attention.* The Lipschitz constant $L_\sigma$ admits the explicit value $L_\sigma=1/2$, *uniform in $\sigma_{\min}$*, as recorded in equation (13.39) of the proof of Theorem 13.18. The bound is tight at the limit $p\to(\tfrac12,\tfrac12,0,\ldots,0)$ on the simplex. The single anomaly recorded in Table 13.3 ($\rho\approx2.10$) is fully accounted for by the $\sigma_{\min}^{-2}$ factor in the inverse map $L'_\sigma=1/\sigma_{\min}$, not by $L_\sigma$ itself. The original concern that $L_\sigma$ depended on $\sigma_{\min}$ is dispelled.

<a id="open-problem-op5"></a>
- **OP5 (resolved).** *Conditioning factor $\beta_\sigma\beta'_\sigma$ in the joint-intervention bound.* With OP4 settled, the explicit form $\beta_\sigma\beta'_\sigma=\kappa(V)/(2\sigma_{\min})$ is computed in (13.45), where $\kappa(V)=\lVert V\rVert_{\mathrm{op}}/\sigma_{\min}(V)$ is the condition number of the value-projection matrix. The bound becomes $\lVert D_{\mathrm{both}}\rVert_F^2\le\kappa(V)^2/(2\sigma_{\min}^2)(\lVert D_{\mathrm{TrA}}\rVert_F^2+\lVert D_{\mathrm{TrB}}\rVert_F^2)$, a deterministic per-configuration estimate that no longer relies on random-initialisation averaging.

<a id="open-problem-op6"></a>
- **OP6** (Genericity threshold at borderline saturation). Theorem 13.17 establishes the iff between $\mathcal V_{\mathrm{Cov}}$ and $\mathcal V_\Gamma$ in the strict non-saturation regime $H\,d_k<d_{\mathrm{model}}$, and identifies the saturated regime $H\,d_k>d_{\mathrm{model}}$ as the failure regime. The borderline case $H\,d_k=d_{\mathrm{model}}$, which corresponds to BertBase ($H=12$, $d_k=64$, $d_{\mathrm{model}}=768$), is not directly addressed: the genericity hypothesis on value projections has measure-zero failure set in the non-saturated regime, but on the boundary the measure-zero set may have positive measure in relative-volume sense. A precise statement of the iff in the borderline regime would clarify the architectural status of BertBase-like models.

<a id="open-problems-structural-extension"></a>
#### Open problems on structural extension

Four problems concern extensions of the chapter's framework to broader settings. Each is structural rather than calibrational.

<a id="open-problem-op7"></a>
- **OP7 (resolved).** *Joint-intervention bound under bare convexity.* Corollary 13.19, established immediately after Theorem 13.18, gives the sought-for bound under bare convexity plus $M$-smoothness: $\mathcal V_I^{\mathrm{both}}(h)\le M\,\kappa(V)^2/(2\sigma_{\min}^2)\cdot(\lVert D_{\mathrm{TrA}}\rVert_F^2+\lVert D_{\mathrm{TrB}}\rVert_F^2)$. The strong convexity assumption is not needed; the conclusion is expressed in terms of the geometric output deviations rather than the $\mathcal V_I^s,\mathcal V_I^a$ functionals. The bound is computable from a single forward pass with three score-matrix interventions.

<a id="open-problem-op8"></a>
- **OP8** (Architectural realisations of H-rep). Theorem 13.15 requires the representation-invariance hypothesis H-rep on the loss. Standard transformer architectures with LayerNorm break H-rep; the open question is which architectural modifications preserve it. RMSNorm, scaled-dot-product normalisation without bias, and certain weight-tying schemes are candidates. A characterisation theorem of the form “transformer architecture $\mathcal A$ satisfies H-rep iff [structural condition]” would identify the class of models for which the loss–coverage–energy implication is rigorous.

<a id="open-problem-strict-convexity-closure"></a>
- **Strict-convexity closure** (strict convexity on $G_{\mathrm{channel}}$ orbits). The training criterion of Theorem 13.30 is currently conditional on the strict convexity of the loss on the orbits of the channel-destruction group $G_{\mathrm{channel}}$. A proof of strict convexity would promote the theorem to . The problem reduces, by the head-channel symmetry analysis, to a statement about the second variation of the loss along the channel-destruction directions;

<a id="pdf-0504"></a>
## PDF 504 · Book 468

explicit calculation of this second variation in trained transformers is feasible but has not been done.

<a id="open-problem-op10"></a>
- **OP10** (Spectral calibration of $c_\Gamma^{(\ell)}$). The original conjectural form of the coverage–directional inequality used a constant $c_\Gamma^{(\ell)}$ relating directional-energy units to dimension counts. Theorem 13.17 replaces the conjecture with an iff that does not require a calibration; however, in regimes where the iff fails (saturated layers), a quantitative statement of the form $\mathcal V_\Gamma(h)\propto c_\Gamma^{(\ell)}\mathcal V_{\mathrm{Cov}}(h)$ may still hold over an appropriate spectral subset of heads. The calibration of $c_\Gamma^{(\ell)}$ via the median squared singular value of $\{M_a^{(\ell,h)}\}$ is the candidate; the regime of validity is the open question.

**How the catalogue is used.** The next chapter relies on Theorems 13.17, 13.18, 13.15 as black boxes; the open problems above do not block the construction of the rationalised block. They do, however, constrain its diagnostic precision. In particular, the training-criterion thresholds of §13.10 become quantitatively sharp only after OP6 is resolved, and the dynamic generalisation in Chapter 15 (INCRT) becomes loss-aware only after OP1, OP8 are resolved. The catalogue therefore serves both as a research programme and as an accountability record: every , , and unbounded constant in the chapter has a corresponding entry above.

Of the ten problems originally identified, three — **OP4** (Lipschitz constant of the softmax), **OP5** (conditioning factor of the joint-intervention bound), and **OP7** (joint-intervention bound under bare convexity) — have been resolved within this chapter, with the resolutions incorporated into the proofs of Theorem 13.18 and Corollary 13.19. Seven problems remain open and are taken up in subsequent chapters or in companion work.

<a id="section-13-12-5"></a>
### 13.12.5 Closing remark

The chapter's principal claim is that an active head is not necessarily a useful head. The chapter's principal tool for distinguishing the two is the family of marginal-value functionals, supported by the crowding diagnostics, the specialisation analysis, and the dictionary-geometric framework. The chapter's principal limitation is that the analysis is within-layer and quasi-static. The three outlook items above identify the routes by which the analysis would extend to a fully cross-layer, fully dynamic, fully task-aware redundancy framework. The next chapter takes the first step toward that extension by constructing the static rationalised block in which the within-layer prescriptions of this chapter are realised explicitly. The fully dynamic extension is taken up in Chapter 15.

---

[Next pages: Chapter 14](0505-0511-ch14-motivation-design-and-block.md#pdf-0505) · [Russian note 171](../notes/171-orbit-conjectures-cross-layer-counting-and-task-transfer.md) · [Russian note 172](../notes/172-open-problem-status-softmax-conditioning-and-stationarity.md)
