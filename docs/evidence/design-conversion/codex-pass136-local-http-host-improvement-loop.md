# Codex Pass136 - Local HTTP Host Improvement Loop

claim_scope:
One bounded PASS136 implementation and verification slice for the DesignConversion ModelServer improvement-loop route. This pass adds a local loopback HTTP host adapter/test helper path and proves an actual `HttpClient` POST to `127.0.0.1:{ephemeral}` reaches `POST /api/design-conversion/improvement-loop`, dispatches through the existing endpoint registry and handler, and returns a refs-only response. This is not an ASP.NET Core host proof, not a public/production deployment, not cross-repo caller wiring, not external AI/network execution outside loopback, not model training/upload, and not full product readiness.

evidence_scope:
Current workspace `C:\Projects\llm_mvp_core`; required AGENTS/session/skill read/check; requested ModelServer source/test/prior-evidence reads; no existing ASP.NET/TestHost seam found in `25.ModelServer`; new ModelServer loopback-host response adapter; new focused raw TCP loopback host test using `HttpClient`; focused new-test pass; requested adjacent route-test filter pass; pending post-write marker/mojibake/diff/readback checks recorded in the final packet.

count_claimed:
1 local loopback HTTP host proof for the improvement-loop route. 0 ASP.NET host proofs. 0 public-host proofs. 0 external-network provider calls. 0 product-readiness passes.

independent_evidence_count:
8 evidence groups before final hygiene checks: required skill-stack read/check; requested source/test/prior-evidence reads; existing host seam scan; loopback-host adapter edit; focused local HTTP host test edit; single new-test dotnet pass; requested adjacent route-filter dotnet pass; this evidence artifact.

subagent_id_or_NONE:
UNKNOWN_IN_SUBAGENT

requested_model: gpt-5.5
requested_reasoning_effort: xhigh

implementation_summary:
- Added `DesignConversionModelServerLocalHttpHostAdapter`.
- The adapter dispatches a `DesignConversionModelServerHttpRequest` through `DesignConversionModelServerEndpointRegistry.InvokeAsync`.
- The adapter annotates response boundary metadata and headers with loopback-only socket proof: `local_http_host_verified=True`, `local_loopback_host_verified=True`, `localhost_loopback_only=True`, `loopback_host=127.0.0.1`, `bound_port_mode=ephemeral`, `socket_opened=True`, and `loopback_socket_opened=True`.
- The adapter preserves negative boundaries: `asp_net_host_verified=False`, `public_http_endpoint_verified=False`, `cross_repo_caller_verified=False`, `deployment_verified=False`, `public_production_default_ready=False`, `external_provider_called=False`, `network_call_executed=False`, `external_network_call_executed=False`, and `product_ready=False`.
- Added `DesignConversionModelServerLocalHttpHostTests.PostImprovementLoop_LocalHttpHost_ReturnsRefsOnlyEnvelope`.
- The test starts a test-only `TcpListener` on `IPAddress.Loopback` with port `0`, sends an actual `HttpClient.PostAsync` request to the improvement-loop route, parses the HTTP response body, and disposes the listener.

local_http_request_response_chain:
`HttpClient.PostAsync` -> `127.0.0.1:{ephemeral}` raw loopback HTTP listener -> `DesignConversionModelServerLocalHttpHostAdapter.DispatchAsync` -> `DesignConversionModelServerEndpointRegistry.InvokeAsync` -> `DesignConversionModelServerImprovementLoopHandler.HandleAsync` -> `DesignConversionImprovementLoopCoordinator.RunAsync` -> explicit local service composition -> refs-only JSON response.

safe_boundary_assertions:
- The test asserts the response status is `200 OK`.
- The test asserts the response headers include `x-design-conversion-local-http-host-boundary` and `x-design-conversion-socket-opened=True`.
- The test asserts boundary metadata contains `local_http_host_verified=True`, `local_loopback_host_verified=True`, `socket_opened=True`, `loopback_socket_opened=True`, `localhost_loopback_only=True`, and `bound_port_mode=ephemeral`.
- The test asserts public/product/network boundaries remain false.
- The test asserts the response body does not contain the private marker, raw HTML, raw CSS, design-tree payload, or original-tree payload.

test_results:
- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionModelServerLocalHttpHostTests.PostImprovementLoop_LocalHttpHost_ReturnsRefsOnlyEnvelope" --no-restore --logger "console;verbosity=minimal"` passed: Failed 0, Passed 1, Skipped 0, Total 1.
- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionModelServerLocalHttpHost|FullyQualifiedName~DesignConversionModelServerImprovementLoop|FullyQualifiedName~DesignConversionModelServerLocalCallerAdapter" --no-restore --logger "console;verbosity=minimal"` passed: Failed 0, Passed 25, Skipped 0, Total 25.

coverage_delta:
This pass covers original requirements 1 and 2 only as local loopback HTTP proof: a local user/system-shaped caller can POST package data to the ModelServer improvement-loop API and receive a safe refs-only JSON response through an actual loopback HTTP request/response path. It stops before ASP.NET Core host proof, public/production deployment, external callers, real external AI/provider execution, model training, upload, and full readiness.

remaining_gaps:
- ASP.NET/minimal API host routing remains not proven.
- Public/production HTTP deployment remains not proven.
- External caller repos and cross-repo integration remain not proven.
- Real external AI/provider execution remains not proven.
- Model upload, training promotion, and product readiness remain not proven.

downgrade_needed:
No downgrade for the narrow local loopback HTTP host proof after focused tests passed. Downgrade remains required for ASP.NET host, public deployment, external network/provider calls, cross-repo caller wiring, model training/upload, and full product readiness claims.

verdict: PASS
