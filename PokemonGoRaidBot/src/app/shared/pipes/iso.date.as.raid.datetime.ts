import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'isoDateAsRaidDateTime' })
export class IsoDateAsRaidDateTimePipe implements PipeTransform {
    transform(value: string): string {
        if (!value) return '';
        try {
            let date = new Date(value);
            let now = new Date();
            if (now.getUTCFullYear() === date.getUTCFullYear() && now.getUTCMonth() === date.getUTCMonth() && now.getUTCDay() == date.getUTCDay()) {
                return date.toLocaleString("nl-NL", { hour: 'numeric', minute: 'numeric' });
            }
            else {
                return date.toLocaleString("nl-NL", { month: 'long', day: 'numeric', hour: 'numeric', minute: 'numeric' });
            }
        }
        catch (e) {
            console.log(`Error converting date '${value}': ${e}`);
            return "error";
        }
    }
}
