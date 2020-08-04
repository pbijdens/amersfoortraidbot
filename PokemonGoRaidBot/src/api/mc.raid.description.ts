import { McTelegramUser } from "./mc.telegram.user";
import { McLocation } from "./mc.location";
import { McExternalSource } from "./mc.external.source";
import { McPublicationEntry } from "./mc.publication.entry";

export class McRaidDescription {
    public UniqueID: string;
    public PublicID: string;
    public User: McTelegramUser;
    public Location: McLocation;
    public Address: string;
    public Raid: string;
    public Gym: string;
    public Alignment: string;
    public RaidUnlockTime: string;
    public RaidEndTime: string;
    public UpdateCount: number;
    public Remarks: string;
    public TelegramMessageID: string;
    public Sources: McExternalSource[];
    public Publications: McPublicationEntry[];
    public Valor: number;
    public Mystic: number;
    public Instinct: number;
    public Unknown: number;
    public Maybe: number;
    public Total: number;
}
