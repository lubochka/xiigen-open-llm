import http from "node:http";

const listenPort = Number(process.env.PASS149_PROXY_PORT || "0");
const upstreamPort = Number(process.env.PASS149_UPSTREAM_PORT || "0");

if (!Number.isInteger(listenPort) || listenPort <= 0) {
  throw new Error("PASS149_PROXY_PORT must be a positive integer.");
}

if (!Number.isInteger(upstreamPort) || upstreamPort <= 0) {
  throw new Error("PASS149_UPSTREAM_PORT must be a positive integer.");
}

const upstreamOrigin = `http://127.0.0.1:${upstreamPort}`;

const server = http.createServer((clientRequest, clientResponse) => {
  const requestUrl = new URL(clientRequest.url || "/", upstreamOrigin);
  const upstreamRequest = http.request(
    {
      hostname: "127.0.0.1",
      port: upstreamPort,
      method: clientRequest.method,
      path: `${requestUrl.pathname}${requestUrl.search}`,
      headers: {
        ...clientRequest.headers,
        host: `127.0.0.1:${upstreamPort}`,
        "x-roles": "tenant_ops",
        "x-tenant-id": "tenant-a",
        "x-user-id": "pass149-browser-user",
        "x-idempotency-key": "pass149-browser-proxy",
        "x-correlation-id": "corr-pass149-browser-proxy"
      }
    },
    upstreamResponse => {
      const responseHeaders = { ...upstreamResponse.headers };
      delete responseHeaders["transfer-encoding"];
      clientResponse.writeHead(upstreamResponse.statusCode || 502, responseHeaders);
      upstreamResponse.pipe(clientResponse);
    });

  upstreamRequest.on("error", error => {
    clientResponse.writeHead(502, { "content-type": "text/plain; charset=utf-8" });
    clientResponse.end(`pass149 proxy upstream error: ${error.message}`);
  });

  clientRequest.pipe(upstreamRequest);
});

server.listen(listenPort, "127.0.0.1", () => {
  console.log(`pass149 header proxy listening on http://127.0.0.1:${listenPort}`);
  console.log(`pass149 header proxy upstream ${upstreamOrigin}`);
});
