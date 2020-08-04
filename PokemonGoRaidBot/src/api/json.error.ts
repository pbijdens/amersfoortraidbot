import { IdentityError } from "./identity.error";

export class JsonError {
    success: boolean;
    error: string;
    message: string;
    validationErrors: string[];
    identityErrors: IdentityError[];
}
