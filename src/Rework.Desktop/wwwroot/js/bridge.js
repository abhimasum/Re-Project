// Bridge between the Razor component (C#) and the rework-panel Angular custom element.
//   C# -> Angular: set the element's `items` property.
//   Angular -> C#: listen for the `actionRequested` event and forward it with DotNet interop.
window.reworkBridge = {
    _dotNet: null,
    _listening: false,

    init(dotNetRef) {
        this._dotNet = dotNetRef;
        if (this._listening) return;
        this._listening = true;

        // Angular Elements events do not bubble, so listen in the capture phase on the document.
        document.addEventListener('actionRequested', (event) => {
            const { id, action } = event.detail;
            this._dotNet?.invokeMethodAsync('OnPanelAction', id, action);
        }, true);
    },

    setItems(items) {
        const panel = document.querySelector('rework-panel');
        if (panel) {
            // Map the C# PascalCase DTO fields to the camelCase shape the Angular component expects.
            panel.items = (items ?? []).map(i => ({
                id: i.id, defectDescription: i.defectDescription, status: i.status, assignedTo: i.assignedTo
            }));
        }
    }
};
