let timerStage1 = null;
let timerStage2 = null;
let dotNetRef = null;
let hasTriggeredInactivity = false;
let clickHistory = [];
let typingHistory = [];
let boundHandleInput = null;
let boundHandleClick = null;

const baseTimeStage1 = 15000;
const baseTimeStage2 = 30000;

export function initialize(dotNetReference) {
    dotNetRef = dotNetReference;
    
    boundHandleInput = handleInput;
    boundHandleClick = handleClick;

    document.addEventListener('keydown', boundHandleInput);
    document.addEventListener('click', boundHandleClick);
    
    console.log("Inactivity timer module initialized.");
}

function handleClick(event) {
    const now = Date.now();
    const x = event.clientX;
    const y = event.clientY;

    clickHistory.push({ x, y, time: now });
    clickHistory = clickHistory.filter(c => now - c.time < 1000);

    if (clickHistory.length >= 3) {
        const last = clickHistory[clickHistory.length - 1];
        const isRage = clickHistory.every(c => 
            Math.abs(c.x - last.x) < 20 && 
            Math.abs(c.y - last.y) < 20
        );

        if (isRage) {
            console.log("StoryFort: Rage Click Detected");
            if (dotNetRef) dotNetRef.invokeMethodAsync('OnFrustrationDetected');
            clickHistory = []; 
        }
    }
}

function handleInput(event) {
    if (!event.target.classList.contains('ql-editor') && !event.target.closest('.ql-editor')) {
        return;
    }

    detectButtonMashing(event);

    if (hasTriggeredInactivity) {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('OnActivityDetected'); 
        }
        hasTriggeredInactivity = false;
    }

    resetTimers(event);
}

function detectButtonMashing(event) {
    const now = Date.now();
    
    if (event.key === 'Backspace' || event.key === 'Delete') {
         typingHistory.push({ type: 'delete', time: now });
         const recentDeletions = typingHistory.filter(t => t.type === 'delete' && now - t.time < 1000);
         if (recentDeletions.length > 5) {
             console.log("StoryFort: Frantic Deletion Detected");
             if (dotNetRef) dotNetRef.invokeMethodAsync('OnFrustrationDetected');
             typingHistory = [];
         }
    } else {
         typingHistory = typingHistory.filter(t => now - t.time < 1000);
    }
}

function resetTimers(event) {
    if (timerStage1) clearTimeout(timerStage1);
    if (timerStage2) clearTimeout(timerStage2);

    let multiplier = 1;

    if (event && event.type === 'keydown') {
        const key = event.key;
        if (['.', '!', '?', ';'].includes(key)) {
            multiplier = 2;
        }
    }

    const time1 = baseTimeStage1 * multiplier;
    const time2 = baseTimeStage2 * multiplier;

    timerStage1 = setTimeout(() => {
        hasTriggeredInactivity = true;
        if (dotNetRef) dotNetRef.invokeMethodAsync('OnInactivityStage1');
    }, time1);

    timerStage2 = setTimeout(() => {
        hasTriggeredInactivity = true;
        if (dotNetRef) dotNetRef.invokeMethodAsync('OnInactivityStage2');
    }, time2);
}

export function dispose() {
    if (boundHandleInput) document.removeEventListener('keydown', boundHandleInput);
    if (boundHandleClick) document.removeEventListener('click', boundHandleClick);
    if (timerStage1) clearTimeout(timerStage1);
    if (timerStage2) clearTimeout(timerStage2);
}


