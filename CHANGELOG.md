# Changelog
Running changelog of releases since `2.0.1`

## v4.0.8

### Bug Fixes

- Scoped credentials are now set on each request instead of on the shared `HttpClient.DefaultRequestHeaders.Authorization`, so a scoped call can no longer replace the parent client's token or leak a token into concurrent requests. The raw `Authorization-Value` header is no longer sent on the wire (OKTA-1275144)
- Request URIs that would resolve to a host other than the configured Okta domain (e.g. network-path references such as `//attacker.example.com/x`) are now rejected, preventing the API token from being sent off-origin. The Okta domain prefix is also matched case-insensitively (OKTA-1250467)
- `DefaultRequestExecutor` no longer stores the API token or Okta domain on the `HttpClient` it is given. They are kept on the executor and applied to each request, so several clients can share one `HttpClient` without overwriting each other's token or base address (PSIRT-327, OKTA-1251806)

### Breaking Changes

- The SDK no longer sets `HttpClient.BaseAddress` or `HttpClient.DefaultRequestHeaders.Authorization`. Callers who relied on those values for their own requests must set them themselves

## v4.0.7

### Updates

- Remove unused `System.Interactive.Async` dependency that caused conflicts with Entity Framework Core async LINQ methods
- Update `Microsoft.Extensions.*` packages to v10.0.1
- Update `System.Text.Json` to v10.0.1
- Update `Newtonsoft.Json` to v13.0.4
- Update `YamlDotNet` to v16.3.0
- Fix StyleCop warnings (SA1305, SA1505, SA1507, SA1508, SA1024, SA1028, SA1638)
- Fix CS0108 warnings for `StatusCode` property hiding

## v4.0.6

### Updates

- Update `Newtonsoft.Json` package to v13.0.1

## v4.0.5

### Bug Fixes

-  Set default authorization header if token is specified in configuration

## v4.0.4

### Updates

- Update `Microsoft.Extensions.Configuration` packages to v6

## v4.0.3

### Bug Fixes

- Fix for [Okta.Sdk issue #526](https://github.com/okta/okta-sdk-dotnet/issues/526) *Unable to create OktaClient on linux build server after 5.2.1 upgrade*.

## v4.0.2

### Bug Fixes

- Fix for null properties treated as empty strings issue (`ObjectExtension`).
- Exception was thrown when null object was passed in (`ObjectExtension`).

## v4.0.1

### Updates

- Remove [FlexibleConfiguration](https://github.com/nbarbettini/FlexibleConfiguration) dependency and use Microsoft Configuration providers instead.

## v4.0.0

### Features

- Add `OktaOAuthException` to represent OAuth errors.

### Updates

- Moved `StatusCode` property to `OktaException` to be accessible by derived classes.
- Refactor `OktaIonApiException` to show the message that comes from the server instead of `(StatusCode):(Message)`.

### Breaking changes

- Change in default behavior when serializing resources (JSON objects). Previously, null resource properties would result in a resource object with all its properties set to `null`. Now, null resource properties would result in `null` property value. 

_Before:_

```
{                                                 deserializedResource.Prop1.Should().Be("Hello World!");          
    prop1 : "Hello World!",         =>            deserializedResource.NestedObject.Should().NotBeNull();
    nestedObject: null                            deserializedResource.NestedObject.Prop1.Should().BeNull();
}

```

_Now:_

```
{                                                 deserializedResource.Prop1.Should().Be("Hello World!");          
    prop1 : "Hello World!",         =>            deserializedResource.NestedObject.Should().BeNull();
    nestedObject: null                            
}

```


## v3.0.3
- Add support for `json+ion` forms validation error responses 

## v3.0.2
- Add sanity check for null headers when parsing errors.

## v3.0.1
- Sanitize runtime version token in `UserAgentBuilder`. Bracket symbols are replaced with dashes on sanitization.  

## v3.0.0

- Add support for `json+ion` error responses.
- Add support to configure Authorization Headers. 
- Rename `CreatedScoped` to `CreateScoped`.
- Add `SendAsync` method in `BaseOktaClient`.

## v2.0.1


### Bug Fixes

- Preserve timezone when date-formatted strings are deserialized as DateTime.
