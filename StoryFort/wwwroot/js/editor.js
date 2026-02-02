let autoSaveTimer = null;
let dotNetRef = null;

export function initAutoSave(dotNetReference, debounceMs) {
    dotNetRef = dotNetReference;
    const editor = document.querySelector('.ql-editor');
    
    if (editor) {
        // Auto-focus the editor
        editor.focus();
        editor.setAttribute('spellcheck', 'false');

        const listener = () => handleInput(debounceMs);
        editor.addEventListener('input', listener);
        editor.addEventListener('keydown', listener);

        window.addEventListener('offline', () => handleConnectivityChange(false));
        window.addEventListener('online', () => handleConnectivityChange(true));

    } else {
        console.warn("Editor element .ql-editor not found for auto-save initialization.");
    }
}

export function focusEditor() {
    const editor = document.querySelector('.ql-editor');
    if (editor) {
        editor.focus();
    }
}

function handleInput(debounceMs) {
    if (autoSaveTimer) clearTimeout(autoSaveTimer);
    autoSaveTimer = setTimeout(() => {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('TriggerAutoSave');
        }
    }, debounceMs);
}

function handleConnectivityChange(isOnline) {
    console.log("Connection status change: " + (isOnline ? "ONLINE" : "OFFLINE"));
    const editorContainer = document.querySelector('.ql-container');
    if (!isOnline) {
        if (editorContainer) {
            editorContainer.style.border = "2px solid red";
            editorContainer.title = "You are OFFLINE. Changes may not save.";
        }
    } else {
        if (editorContainer) {
            editorContainer.style.border = "";
            editorContainer.title = "";
        }
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('TriggerAutoSave');
        }
    }
}

