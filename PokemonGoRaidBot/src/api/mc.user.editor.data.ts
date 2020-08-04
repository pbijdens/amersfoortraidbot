export class McUserEditorData {
    Id: string;
    DisplayName: string;
    Email: string;
    NormalizedEmail: string;
    UserName: string;
    NormalizedUserName: string;
    EmailConfirmed: boolean;
    LockoutEnabled: boolean;
    ProfilePictureBase64: string;
    CreationDateUTC: string;
    LastModificationDateUTC: string;

    Password: string;
    IsAdministrator: boolean;
    IsFinance: boolean;
    IsStaff: boolean;
    IsVisitor: boolean;
}
