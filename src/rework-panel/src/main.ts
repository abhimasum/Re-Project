import { createApplication } from '@angular/platform-browser';
import { createCustomElement } from '@angular/elements';
import { ReworkPanelComponent } from './app/rework-panel.component';

// Angular Elements: registers <rework-panel> so Razor/JavaScript can use it like any HTML element.
(async () => {
  const app = await createApplication({ providers: [] });
  const element = createCustomElement(ReworkPanelComponent, { injector: app.injector });
  if (!customElements.get('rework-panel')) {
    customElements.define('rework-panel', element);
  }
})();
