window.inactivityTimer = {
    timerStage1: null,
    timerStage2: null,
    dotNetRef: null,
    hasTriggeredInactivity: false, // Optimization: only notify activity if we were inactive
    
    // Base times in milliseconds
    baseTimeStage1: 15000, 
    baseTimeStage2: 30000,

    // Frustration Detection State
    clickHistory: [],
    typingHistory: [],

    initialize: function (dotNetReference) {
        this.dotNetRef = dotNetReference;
        
        // Save bound references for proper removal
        this.boundHandleInput = this.handleInput.bind(this);
        this.boundHandleClick = this.handleClick.bind(this);

        document.addEventListener('keydown', this.boundHandleInput);
        document.addEventListener('click', this.boundHandleClick);
        
        // REMOVED: document.addEventListener('mousedown', ...);
        // Requirement: "The timer should be sensitive to keystrokes in the editor pane only."
        // Clicking around (including on the Alert Alice image) should NOT reset the timer.

        console.log("Inactivity timer initialized. Listening for keystrokes in .ql-editor.");
    },

    handleClick: function(event) {
        // Rage Click Detection (>3 clicks in 1s within 20px)
        const now = Date.now();
        const x = event.clientX;
        const y = event.clientY;

        this.clickHistory.push({ x, y, time: now });
        // Clean up old clicks
        this.clickHistory = this.clickHistory.filter(c => now - c.time < 1000);

        if (this.clickHistory.length >= 3) {
            const last = this.clickHistory[this.clickHistory.length - 1];
            const isRage = this.clickHistory.every(c => 
                Math.abs(c.x - last.x) < 20 && 
                Math.abs(c.y - last.y) < 20
            );

            if (isRage) {
                console.log("Unstickd: Rage Click Detected");
                if (this.dotNetRef) this.dotNetRef.invokeMethodAsync('OnFrustrationDetected');
                this.clickHistory = []; // Reset to prevent double firing
            }
        }
    },

    handleInput: function (event) {
        // FILTER: Only proceed if the event originated from the Quill Editor.
        // The Quill editor content div always has the class 'ql-editor'.
        if (!event.target.classList.contains('ql-editor') && !event.target.closest('.ql-editor')) {
            return;
        }

        this.detectButtonMashing(event);

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

    detectButtonMashing: function(event) {
        // High Velocity Deletion: > 5 deletions in 1s
        const now = Date.now();
        
        if (event.key === 'Backspace' || event.key === 'Delete') {
             this.typingHistory.push({ type: 'delete', time: now });
             
             const recentDeletions = this.typingHistory.filter(t => t.type === 'delete' && now - t.time < 1000);
             if (recentDeletions.length > 5) {
                 console.log("Unstickd: Frantic Deletion Detected");
                 if (this.dotNetRef) this.dotNetRef.invokeMethodAsync('OnFrustrationDetected');
                 this.typingHistory = []; // Reset
             }
        } else {
             // Keep history clean (remove items older than 1s)
             this.typingHistory = this.typingHistory.filter(t => now - t.time < 1000);
        }
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
        if (this.boundHandleInput) document.removeEventListener('keydown', this.boundHandleInput);
        if (this.boundHandleClick) document.removeEventListener('click', this.boundHandleClick);
        if (this.timerStage1) clearTimeout(this.timerStage1);
        if (this.timerStage2) clearTimeout(this.timerStage2);
    }
};
