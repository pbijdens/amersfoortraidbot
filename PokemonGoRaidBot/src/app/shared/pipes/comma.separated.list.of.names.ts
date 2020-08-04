import { Pipe, PipeTransform } from '@angular/core';
import { McTelegramUser } from '../../../api/mc.telegram.user';

@Pipe({ name: 'commaSeparatedListOfNames' })
export class CommaSeparatedListOfNamesPipe implements PipeTransform {
    transform(value: McTelegramUser[]): string {
        if (!value) return '';
        try {
            return value.map(x => x.Username).join(', ');
        }
        catch (e) {
            console.log(`Error converting date '${value}': ${e}`);
            return "error";
        }
    }
}
