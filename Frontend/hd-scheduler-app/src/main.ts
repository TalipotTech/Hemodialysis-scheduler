import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { registerLicense } from '@syncfusion/ej2-base';

// Register Syncfusion license key
registerLicense('Ngo9BigBOggjGyl/Vkd+XU9FcVRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS3tSdERjW39dcHdTTmJZWE91Xg==');

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
