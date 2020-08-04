import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'isoDateAsLocalDateTime' })
export class IsoDateAsLocalDateTimePipe implements PipeTransform {
    transform(value: string): string {
        if (!value) return '';
        try {
            let date = new Date(value);
            return date.toLocaleString("nl-NL", { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric', hour: 'numeric', minute: 'numeric' });
        }
        catch (e)
        {
            console.log(`Error converting date '${value}': ${e}`);
            return "error";
        }
    }
}
