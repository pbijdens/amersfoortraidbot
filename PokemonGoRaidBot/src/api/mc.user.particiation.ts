import { McTelegramUser } from "./mc.telegram.user";

export class McUserParticipation {
    public Unknown: McUserParticipationEntry[];
    public Valor: McUserParticipationEntry[];
    public Mystic: McUserParticipationEntry[];
    public Instinct: McUserParticipationEntry[];
}

export class McUserParticipationEntry {
    public User: McTelegramUser;
    public UtcWhen: string;
    public UtcArrived: string;
    public Extra: number;
}
