# McUserEditorData

|Field|Type|Description|
|-----|----|-----------|
|Id|string|ID of the user entity, leave empty when creating a user.|
|Email|string|Email address of the user.|
|UserName|string|Username or login name, must not be empty.|
|DisplayName|string|Display name, msut not be empty.|
|ProfilePictureBase64|string|Base64-encoded profile picture.|
|CreationDateUTC|string|ISO8601 timestamp indicating when the object was created.|
|LastModificationDateUTC|string|ISO8601 timestamp indicating when the object was last modified.|
|LockoutEnabled|boolean|Set to true when the user account is considered disabled.|
|Password|string|Input only, set to change the password.|
|IsAdministrator|boolean|Is the user in the Administrator role?|
|IsFinance|boolean|Is the user in the Finance role?|
|IsStaff|boolean|Is the user in the Staff role?|
|IsVisitor|boolean|Is the user in the Visitor role?|
