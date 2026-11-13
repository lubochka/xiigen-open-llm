# Call-Transit Scenarios
# Run: local-legacy-bottom-up-transit / batch-001

These scenarios are acceptance gates for documentation orchestration. They are
not training rows.

---

## Scenario 1 - wizGameserver Dynamic Document Storage

Expected route:

- `server x NET_CORE x netcoreapp3.1 x CSharp8 x NETCORE_WEBAPI`

Bottom-up order:

1. D1 `ParseSimpleObject(object)` at `WebApplication2\Controllers\DynamicController.cs:264`
2. D1 `ParseJpropertyObject(JProperty)` at `DynamicController.cs:243`
3. D2/D3 `ParseObjectAlternative(object)` at `DynamicController.cs:204`
4. D3 `StoreDocument(string,string,object)` at `DynamicController.cs:27`

Transit expectation:

`StoreDocument` may say it normalizes the posted object into typed values only
because `ParseObjectAlternative` is supplied as a callee summary. The controller
action itself is responsible for request binding, calling the parser, indexing
the parsed object through NEST, and returning success/error JSON.

Rejected behavior:

Documenting `StoreDocument` as if it locally parses every primitive type is
wrong; that behavior belongs to the parser callee summaries.

---

## Scenario 2 - GenieDynamicServer Object Parsing And Elastic Service

Expected route:

- `server x NET_CORE x netcoreapp3.1 x CSharp8 x NETCORE_WEBAPI`
- `server x NET_CORE x netcoreapp3.1 x CSharp8 x LIBRARY_INFRASTRUCTURE`

Bottom-up order:

1. D1 `ParseSimpleObject(object)` at `TestXLS\BusinessLogic\ObjectProcess.cs:139`
2. D1 `ParseJpropertyObject(JProperty)` at `ObjectProcess.cs:116`
3. D2 `ParseObjectAlternative(object)` at `ObjectProcess.cs:18`
4. D1 `IndexAttribute.Index` at `TestXLS\Services\Base\ElasticBaseService.cs`
5. D2 `ElasticBaseService<T>` constructor and CRUD methods

Transit expectation:

Object parsing documentation must propagate primitive conversion behavior upward
to recursive object parsing. Elastic service documentation must state that index
selection is derived from `IndexAttribute`, not invented from class name alone.

Rejected behavior:

Treating the generic CRUD service as a repository with EF behavior is wrong; it
uses NEST `ElasticClient`.

---

## Scenario 3 - geniesoftGameMVPServer Thin Template Surface

Expected route:

- `server x NET_CORE x netcoreapp3.1 x CSharp8 x NETCORE_WEBAPI`

Bottom-up order:

1. D1/D2 document the thin template-level API only.

Transit expectation:

The correct documentation route recognizes a low-complexity ASP.NET Core 3.1
template-like server and does not invent a game workflow from the repository
name.

Rejected behavior:

Writing D4 game-server workflow documentation without call-tree evidence is a
hard fail.

---

## Scenario 4 - genietasks Legacy Auth Workflow

Expected route:

- `mixed x NET_FRAMEWORK x net46 x CSharp5_or_CSharp6_review_required x NETFX_WEBAPI`
- frontend context: `AngularJS 1.4.8`
- server framework context: `System.Web.Http 5.2.3`, Breeze WebApi2, ASMX files present

Bottom-up order:

1. D1 `ConvertToMD5(this string)` at `AuthController.cs:114`
2. D1 `JwtManager.GenerateToken(UserBack,int)` at `JwtManager.cs:12`
3. D1 `UserBack(Member)` at `AuthController.cs:40`
4. D3 `CreateBackUser(Member)` at `AuthController.cs:211`
5. D3 `Authenticate(LoginDetails)` at `AuthController.cs:135`
6. D3 `Register(LoginDetails)` at `AuthController.cs:161`

Transit expectation:

`Authenticate` may claim password hashing only through the `ConvertToMD5`
callee. It may claim JWT issuance only through `CreateBackUser`, `UserBack`,
and `JwtManager.GenerateToken` summaries. The documentation must preserve
legacy WebApi2 behavior and must not rewrite the flow as ASP.NET Core Identity.

Rejected behavior:

Calling this a modern ASP.NET Core auth controller or skipping the token
generation transit is wrong.
