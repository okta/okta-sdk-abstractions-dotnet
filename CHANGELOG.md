# Changelog
Running changelog of releases since `2.0.1`

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
