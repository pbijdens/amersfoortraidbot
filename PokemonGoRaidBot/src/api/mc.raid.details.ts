import { McRaidDescription } from "./mc.raid.description";
import { McUserParticipation } from "./mc.user.particiation";
import { McTelegramUser } from "./mc.telegram.user";

export class McRaidDetails {
    public UniqueID: string;
    public PublicID: string;
    public Raid: McRaidDescription;
    public IsPublished: boolean;
    public LastRefresh: string;
    public LastModificationTime: string;
    public Participants: McUserParticipation;
    public Rejected: McTelegramUser[];
    public Done: McTelegramUser[];
    public Maybe: McTelegramUser[];
    //
    public NumberOfParticipants: number;
}
