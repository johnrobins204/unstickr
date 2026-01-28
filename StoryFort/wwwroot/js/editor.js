window.editorJs = {
    autoSaveTimer: null,
    dotNetRef: null,

    initAutoSave: function (dotNetReference, debounceMs) {
        this.dotNetRef = dotNetReference;
        const editor = document.querySelector('.ql-editor');
        
        if (editor) {
            // Auto-focus the editor
            editor.focus();
            editor.setAttribute('spellcheck', 'false'); // Disable spellcheck by default as per req

            // Use 'input' event to catch all changes (typing, paste, cut)
            // 'keydown' misses paste/cut via mouse
            editor.addEventListener('input', () => this.handleInput(debounceMs));
            
            // Also listen to keydown to catch some edge cases or immediate feedback if needed
            editor.addEventListener('keydown', () => this.handleInput(debounceMs));

            // === PILOT NETWORK RESILIENCE ===
            window.addEventListener('offline', () => this.handleConnectivityChange(false));
            window.addEventListener('online', () => this.handleConnectivityChange(true));

        } else {
            console.warn("Editor element .ql-editor not found for auto-save initialization.");
        }
    },

    handleConnectivityChange: function(isOnline) {
        console.log("Connection status change: " + (isOnline ? "ONLINE" : "OFFLINE"));
        if (!isOnline) {
            // Signal loss to user (Simple alert for pilot, replace with toast later)
            // Implementation: Change editor border or show overlay
            const editorContainer = document.querySelector('.ql-container');
            if (editorContainer) {
                editorContainer.style.border = "2px solid red";
                editorContainer.title = "You are OFFLINE. Changes may not save.";
            }
        } else {
            // Restore visual state
            const editorContainer = document.querySelector('.ql-container');
            if (editorContainer) {
                editorContainer.style.border = "";
                editorContainer.title = "";
            }
            // Trigger an immediate save when back online
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('TriggerAutoSave');
            }
        }
    },

    handleInput: function (debounceMs) {
        // notify C# that we are "dirty" / typing immediately? 
        // Or just wait for debounce?
        // Requirement: "State 1 (Editing): While typing + during the 2s debounce window... Visual: Saving..."
        // To achieve "Saving..." while typing, we need to notify start of typing?
        // That might flood the connection.
        // Better: The Client (JS) knows we are waiting. Maybe we can update a UI element directly?
        // Or we just send one "I'm dirty" signal if not already dirty?
        
        // For now, let's keep it simple: Just debounce the save trigger.
        // The C# side sets status to "Saving..." when TriggerAutoSave is called.
        // If we want "Typing..." state, we'd need more logic.
        
        if (this.autoSaveTimer) {
            clearTimeout(this.autoSaveTimer);
        }

        this.autoSaveTimer = setTimeout(() => {
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('TriggerAutoSave');
            }
        }, debounceMs);
    }
    ,
    focusEditor: function () {
        const editor = document.querySelector('.ql-editor');
        if (editor) {
            editor.focus();
        }
    }
};
