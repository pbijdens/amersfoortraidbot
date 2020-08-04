// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

declare const require;
export const environment = {
    production: false,
    translations: {
        "en-US": require(`raw-loader!../resources/i18n/messages.en-US.xlf`),
        "nl-NL": require(`raw-loader!../resources/i18n/messages.nl-NL.xlf`),
    }
};
