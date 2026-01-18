window.inactivityTimer = {
    timerStage1: null,
    timerStage2: null,
    dotNetRef: null,
    hasTriggeredInactivity: false, // Optimization: only notify activity if we were inactive
    
    // Base times in milliseconds
    baseTimeStage1: 15000, 
    baseTimeStage2: 30000,

    initialize: function (dotNetReference) {
        this.dotNetRef = dotNetReference;
        
        // Listen for keydown events globally, but we will filter in the handler
        // to ensure we only care about the editor.
        document.addEventListener('keydown', this.handleInput.bind(this));
        
        // REMOVED: document.addEventListener('mousedown', ...);
        // Requirement: "The timer should be sensitive to keystrokes in the editor pane only."
        // Clicking around (including on the Alert Alice image) should NOT reset the timer.

        console.log("Inactivity timer initialized. Listening for keystrokes in .ql-editor.");
    },

    handleInput: function (event) {
        // FILTER: Only proceed if the event originated from the Quill Editor.
        // The Quill editor content div always has the class 'ql-editor'.
        if (!event.target.classList.contains('ql-editor') && !event.target.closest('.ql-editor')) {
            return;
        }

        // notify C# that user is active (to reset UI state immediately)
        // Optimization: Only call if we previously triggered an inactivity state
        if (this.hasTriggeredInactivity) {
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnActivityDetected'); 
            }
            this.hasTriggeredInactivity = false;
        }

        this.resetTimers(event);
    },

    resetTimers: function (event) {
        // Clear existing timers
        if (this.timerStage1) clearTimeout(this.timerStage1);
        if (this.timerStage2) clearTimeout(this.timerStage2);

        let multiplier = 1;

        // Check if event is a keydown and if the key is a sentence terminator
        if (event && event.type === 'keydown') {
            const key = event.key;
            // "Double the times... if the last non-whitespace character typed is a punctuation mark"
            if (['.', '!', '?', ';'].includes(key)) {
                multiplier = 2;
                console.log("Punctuation detected. Doubling inactivity timer.");
            }
        }

        const time1 = this.baseTimeStage1 * multiplier;
        const time2 = this.baseTimeStage2 * multiplier;

        this.timerStage1 = setTimeout(() => {
            this.hasTriggeredInactivity = true;
            if (this.dotNetRef) this.dotNetRef.invokeMethodAsync('OnInactivityStage1');
        }, time1);

        this.timerStage2 = setTimeout(() => {
            this.hasTriggeredInactivity = true;
            if (this.dotNetRef) this.dotNetRef.invokeMethodAsync('OnInactivityStage2');
        }, time2);
    },

    dispose: function () {
        document.removeEventListener('keydown', this.handleInput.bind(this));
        if (this.timerStage1) clearTimeout(this.timerStage1);
        if (this.timerStage2) clearTimeout(this.timerStage2);
    }
};
