# MatchedContracts REST API

MatchdContracts uses a JSON-based REST API for all interactions between the frontend and the backend. Third parties may also use this API to integrate with the system.

## Errors

In addition to their normal response, all API methods can return an error block instead of their normal response. This can be detected by reading the success attribute which must have value false for an error to be returned.

|Field|Type|Description|
|-----|----|-----------|
|success|boolean|Will have fixed value of `false` when an error occurred, otherwise not specified |
|error|string|Short description of the error condition|
|message|string|Detailed multi-line description of the error condition|
|validationErrors|string[]|List of error codes for describing validation errors in the incoming data. Which codes are used is described for the individual methods|
|identityErrors|[IdentityError[]](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identityerror?view=aspnetcore-2.0)|List of identity errors, in case of failed identity operations only. Contains a Code and a Description field.|

When an error is returned, normally also the response's HTTP status code will have a specific value. Whcih values those are is documented in the API method's documentation.

## Authentication

The API methods use the bearer-header for authentication. The basic flow is as follows:
- Log in using `/api/token/auth` to obtain a token structure containing a token, a refreskh token, and information about token expiry.
- To successive API requests, add a header `Authorization` with value `Bearer <token>`. This will tell the system which user we're logging in as.
- If the token is expired the request withh return a `401`-error. A call to `/api/token/auth/` mey be placed using the refresh_token, to obtain a new token.

### Endpoint: POST `/api/token/auth`

Use this endpoint to authenticate a user to the system.

#### URL Parameters:

None.

#### Request body

|Field|Type|Description|
|-----|----|-----------|
|grant_type|string|Can be either `password` or `refresh_token` to indicate whether this is a username/password authentication request, or a token extension request.|
|client_id|string|Unique identification of the client that makes the authentication request.|
|client_secret|string|Secret belonging to the calling client. Currently not used, leave empty.|
|username|string|When performing password authentication, contains the user's login name or email address.|
|password|string|When performing password authentication, contains the user's password.|
|refresh_token|string|When perforing token refresh, specify the refresh token here.|

#### Response

Response codes:

|Code|Meaning|
|----|-------|
|200|Authenticating succeeded|
|500|Authentication failed|

Response body:

|Field|Type|Description|
|-----|----|-----------|
|token|string|Token code to be used  in the Authorization header on API requests to identify the user.|
|expiration|number|Number of minutes from the moment of generation until the token will expire.|
|refresh_token|string|Refresh token that can be used to extend the token as long as the underlying user account has access to the system.|

This endpoint does not require an authenticated user.

## User Management

### Endpoint: GET `/api/user/me`

Use this endpoint to obtain detailed information about the currently logged-in user.

#### URL Parameters:

None.

#### Request body

None

#### Response

Response codes:

|Code|Meaning|
|----|-------|
|200|Success|
|401|The user is not authenticated, or the token has expired|
|403|Forbidden: The currently authenticated user is not allowed to access this method.|

Response body:

|Field|Type|Description|
|-----|----|-----------|
|userId|string|Name claim for the logged-in user.|
|userClaims|[McCaim[]](./McClaim.md)|All claims for the logged-in user|
|user|[McUser](./McUser.md)||
|isAdministrator|boolean|True if the user is in the administrator role|
|isFinance|boolean|True if the user is in the finance role|
|isStaff|boolean|True if the user is in the staff role|
|isVisitor|boolean|True if the user is in the visitor role|

This endpoint does not require an authenticated user.

### Endpoint: GET `/api/user/roles`

Use this endpoint to obtain the roles for the logged-in user. Mainly useful for debugging purposes.

#### URL Parameters:

None.

#### Request body

None

#### Response

Response codes:

|Code|Meaning|
|----|-------|
|200|Success|
|401|The user is not authenticated, or the token has expired|
|403|Forbidden: The currently authenticated user is not allowed to access this method.|

Response body:

|Field|Type|Description|
|-----|----|-----------|
||string[]|Returns as body a list of roles to which the user is assigned.|

### Endpoint: GET `/api/user/list`

Use this endpoint to obtain the list of user accounts.

#### URL Parameters:

|Parameter|Type|Description|
|---------|----|-----------|
|start|number|Default: 0. Index of the first item to be returned.|
|num|number|Default: Int32.MaxValue|Number of items to return|
|query|string|Default: null. Search query to filter down the list of returned rows.|
|includeDeleted|boolean|Default: false. If set to true, also inactive or deleted items are returned.|

#### Request body

None

#### Response

Response codes:

|Code|Meaning|
|----|-------|
|200|Success|
|401|The user is not authenticated, or the token has expired|
|403|Forbidden: The currently authenticated user is not allowed to access this method.|

Response body:

|Field|Type|Description|
|-----|----|-----------|
||[McUser[]](./McUser.md)|Returns as body a list of users matching the specified filter.|

### Endpoint: GET `/api/user/user`

Use this endpoint to obtain information on a specific user.

#### URL Parameters:

|Parameter|Type|Description|
|---------|----|-----------|
|id|string|User ID to get information on|

#### Request body

None

#### Response

Response codes:

|Code|Meaning|
|----|-------|
|200|Success|
|401|The user is not authenticated, or the token has expired|
|403|Forbidden: The currently authenticated user is not allowed to access this method.|

Response body:

|Field|Type|Description|
|-----|----|-----------|
||[McUserEditorData](./McUserEditorData.md)|Returns as body a single McUserEditorData structure.|

### Endpoint: PUT `/api/user/user`

Use this endpoint to create a new user.

#### URL Parameters:

None.

#### Request body

|Field|Type|Description|
|-----|----|-----------|
|Email|string|Email address of the user.|
|UserName|string|Username or login name, must not be empty.|
|DisplayName|string|Display name, msut not be empty.|
|ProfilePictureBase64|string|Base64-encoded profile picture.|
|Password|string|Input only, set to change the password.|
|IsAdministrator|boolean|Is the user in the Administrator role?|
|IsFinance|boolean|Is the user in the Finance role?|
|IsStaff|boolean|Is the user in the Staff role?|
|IsVisitor|boolean|Is the user in the Visitor role?|

#### Response

Response codes:

|Code|Meaning|
|----|-------|
|200|Success|
|400|Bad request: The request failed to process because of invalid input.|
|401|The user is not authenticated, or the token has expired|
|403|Forbidden: The currently authenticated user is not allowed to access this method.|
|500|Error: An error occured|

Response body:

|Field|Type|Description|
|-----|----|-----------|
||[McUserEditorData](./McUserEditorData.md)|Returns as body a single McUserEditorData structure of the created users.|

Validation errors:

|Error|Meaning|
|-----|-------|
|ID_INVALID|An ID was specified during add.|
|USERNAME_UNAVAILABLE|The username is not available.|
|EMAIL_UNAVAILABLE|The email address is not available.|
|EMAIL_INVALID|The email address is invalid.|
|USERNAME_INVALID|The username is invalid.|
|NAME_INVALID|The name is invalid.|
|PASSWORD_INVALID|The password is invalid (additional password errors will be identityErrors!)|

### Endpoint: POST `/api/user/user`

Use this endpoint to update an existing user.

#### URL Parameters:

None.

#### Request body

|Field|Type|Description|
|-----|----|-----------|
|Id|string|ID of the user entity that must be updated.|
|Email|string|Email address of the user.|
|UserName|string|Username or login name, must not be empty.|
|DisplayName|string|Display name, msut not be empty.|
|ProfilePictureBase64|string|Base64-encoded profile picture.|
|LockoutEnabled|boolean|Set to true when the user account is considered disabled.|
|Password|string|Input only, set to change the password. If not set, the password will not be changed. |
|IsAdministrator|boolean|Is the user in the Administrator role?|
|IsFinance|boolean|Is the user in the Finance role?|
|IsStaff|boolean|Is the user in the Staff role?|
|IsVisitor|boolean|Is the user in the Visitor role?|

#### Response

Response codes:

|Code|Meaning|
|----|-------|
|200|Success|
|400|Bad request: The request failed to process because of invalid input.|
|401|The user is not authenticated, or the token has expired|
|403|Forbidden: The currently authenticated user is not allowed to access this method.|
|500|Error: An error occured|

Response body:

|Field|Type|Description|
|-----|----|-----------|
||[McUserEditorData](./McUserEditorData.md)|Returns as body a single McUserEditorData structure of the created users.|

Validation errors:

|Error|Meaning|
|-----|-------|
|INTERNAL_ERROR|Something bad happens, e.g. the Id was not valid or not present.|
|CANNOT_LOCKOUT_SELF|An attempt was made to lock out the logged-in user.|
|ADMIN_ROLE_CHANGE_FOR_SELF|An attempt was made to remove one's own admin rights.|
|USERNAME_UNAVAILABLE|The username is not available.|
|EMAIL_UNAVAILABLE|The email address is not available.|
|EMAIL_INVALID|The email address is invalid.|
|USERNAME_INVALID|The username is invalid.|
|NAME_INVALID|The name is invalid.|

## Candidates

## Customers

## Contracts

## Messages and notifications

## Miscellaneous

# Types

## McUser
