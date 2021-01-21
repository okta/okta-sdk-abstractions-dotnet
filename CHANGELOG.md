# Changelog
Running changelog of releases since `2.0.1`

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
