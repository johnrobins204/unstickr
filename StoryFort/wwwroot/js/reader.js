window.reader = (function () {
    let timer = null;
    let dotnetRef = null;

    function init() {
        console.debug('reader.js initialized');
    }

    function onSentenceClicked(sid) {
        console.debug('sentence clicked', sid);
        const el = document.querySelector(`[data-sid='${sid}']`);
        const rect = el ? el.getBoundingClientRect() : { x: 100, y: 100 };
        // Show context menu for this sentence
        const content = el ? el.innerHTML : '';
        showContextMenu(rect.left + 10, rect.top + 10, content, null, sid);
    }

    function animateGhostBook(srcRect, destRect, options) {
        // Placeholder: implement smooth clone + transform animation
        console.debug('animateGhostBook', srcRect, destRect, options);
        return Promise.resolve();
    }

    async function animateGhostFromElement(elementId, storyId, dotnetRef) {
        const src = document.getElementById(elementId);
        if (!src) return;

        const rect = src.getBoundingClientRect();
        const clone = src.cloneNode(true);

        // Prepare clone styles for GPU-accelerated transform animation
        clone.style.position = 'fixed';
        clone.style.left = rect.left + 'px';
        clone.style.top = rect.top + 'px';
        clone.style.width = rect.width + 'px';
        clone.style.height = rect.height + 'px';
        clone.style.margin = '0';
        clone.style.zIndex = 99999;
        clone.style.boxShadow = '0 20px 40px rgba(0,0,0,0.45)';
        clone.style.borderRadius = '6px';
        clone.style.overflow = 'hidden';
        clone.style.transformOrigin = 'center center';
        clone.style.willChange = 'transform, left, top, width, height, opacity';
        clone.style.transition = 'transform 700ms cubic-bezier(.2,.8,.2,1), opacity 300ms';
        document.body.appendChild(clone);

        // destination: center of viewport with modal-ish size
        const destWidth = Math.min(window.innerWidth * 0.8, 900);
        const destHeight = Math.min(window.innerHeight * 0.8, 800);
        const destLeft = (window.innerWidth - destWidth) / 2;
        const destTop = (window.innerHeight - destHeight) / 2;

            const quick = document.createElement('div');
            quick.className = 'reader-menu-item';
            quick.textContent = 'Quick Pin';
            quick.style.padding = '6px 8px';
            quick.style.cursor = 'pointer';
            quick.onclick = async (e) => {
                e.stopPropagation();
                await postPin(null, storyId, contentHtml, sentenceId);
                removeContextMenu();
                // mark UI
                if (sentenceId) {
                    const el = document.querySelector(`[data-sid='${sentenceId}']`);
                    if (el) el.classList.add('pinned');
                }
            };
            menu.appendChild(quick);
        // apply transform: translate + scale + rotation
        const rotate = -8 + (Math.random() * 16 - 8); // slight randomized tilt
        clone.style.transform = `translate(${deltaX}px, ${deltaY}px) scale(${scale}) rotate(${rotate}deg)`;
        clone.style.opacity = '0.99';

        // wait for transform to finish
        await new Promise(resolve => {
            const onEnd = (e) => { if (e.propertyName === 'transform') { clone.removeEventListener('transitionend', onEnd); resolve(); } };
            clone.addEventListener('transitionend', onEnd);
        });

        // short pause to let the modal open behind it
        await new Promise(r => setTimeout(r, 80));

        // fade & shrink slightly then remove
        clone.style.transition = 'opacity 200ms ease-out, transform 200ms ease-out';
        clone.style.opacity = '0.0';
        clone.style.transform = `translate(${deltaX}px, ${deltaY}px) scale(${scale * 0.98}) rotate(${rotate}deg)`;
        setTimeout(() => { try { document.body.removeChild(clone); } catch (e) { } }, 240);

        // Notify .NET that animation completed
        try {
            await dotnetRef.invokeMethodAsync('OnGhostAnimationComplete', storyId);
        } catch (e) {
            console.error('dotnet callback failed', e);
        }
    }

    // Utility: fetch notebooks for account
    async function fetchNotebooks(accountId = 1) {
        const res = await fetch(`/api/notebooks?accountId=${accountId}`);
        if (!res.ok) return [];
        return await res.json();
    }

    async function postPin(notebookId, storyId, content, sentenceId) {
        const body = { notebookId, storyId, content, sentenceId };
        const res = await fetch('/api/pins', {
            method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body)
        });
        return res.ok;
    }

    function removeContextMenu() {
        const existing = document.getElementById('reader-context-menu');
        if (existing) existing.remove();
    }

    async function showContextMenu(x, y, contentHtml, storyId = null, sentenceId = null) {
        removeContextMenu();
        const menu = document.createElement('div');
        menu.id = 'reader-context-menu';
        menu.style.position = 'fixed';
        menu.style.left = x + 'px';
        menu.style.top = y + 'px';
        menu.style.zIndex = 20000;
        menu.style.background = 'white';
        menu.style.border = '1px solid rgba(0,0,0,0.12)';
        menu.style.boxShadow = '0 6px 18px rgba(0,0,0,0.12)';
        menu.style.padding = '8px';
        menu.style.minWidth = '180px';

        const quick = document.createElement('div');
        quick.className = 'reader-menu-item';
        quick.textContent = 'Quick Pin';
        quick.style.padding = '6px 8px';
        quick.style.cursor = 'pointer';
        quick.onclick = async (e) => {
            e.stopPropagation();
            await postPin(null, storyId, contentHtml, sentenceId);
            removeContextMenu();
            // mark UI
            if (sentenceId) {
                const el = document.querySelector(`[data-sid='${sentenceId}']`);
                if (el) el.classList.add('pinned');
            }
        };
        menu.appendChild(quick);

        const choose = document.createElement('div');
        choose.className = 'reader-menu-item';
        choose.style.padding = '6px 8px';
        choose.textContent = 'Pin to...';
        menu.appendChild(choose);

        // notebooks list
        const list = document.createElement('div');
        list.style.maxHeight = '200px';
        list.style.overflow = 'auto';
        list.style.marginTop = '6px';
        menu.appendChild(list);

        document.body.appendChild(menu);

        const notebooks = await fetchNotebooks();
        if (window.reader && window.reader.dotnetRef && typeof window.reader.dotnetRef.invokeMethodAsync === 'function') {
            try {
                await window.reader.dotnetRef.invokeMethodAsync('OnContextMenuRequested', x, y, contentHtml, storyId, sentenceId);
                return;
            }
            catch (e) { console.debug('dotnet context menu call failed', e); }
        }

        if (notebooks && notebooks.length) {
            notebooks.forEach(nb => {
                const it = document.createElement('div');
                it.textContent = nb.name;
                it.style.padding = '6px 8px';
                it.style.cursor = 'pointer';
                it.onclick = async (e) => {
                    e.stopPropagation();
                    await postPin(nb.id, storyId, contentHtml, sentenceId);
                    removeContextMenu();
                    if (sentenceId) {
                        const el = document.querySelector(`[data-sid='${sentenceId}']`);
                        if (el) el.classList.add('pinned');
                    }
                };
                list.appendChild(it);
            });
        } else {
            const none = document.createElement('div');
            none.textContent = 'No notebooks';
            none.style.padding = '6px 8px';
            list.appendChild(none);
        }

        // close on outside click
        setTimeout(() => {
            const onDoc = (e) => { removeContextMenu(); document.removeEventListener('click', onDoc); };
            document.addEventListener('click', onDoc);
        }, 10);
    }

    // Selection handling: single-click selects a word, drag selects multiple words, double-click captures sentence
    document.addEventListener('dblclick', function (e) {
        const sel = window.getSelection();
        if (!sel || sel.isCollapsed) return;
        const anchor = sel.anchorNode;
        const sent = anchor && anchor.parentElement ? anchor.parentElement.closest('.reader-sentence') : null;
        if (sent) {
            const sid = sent.getAttribute('data-sid');
            const rect = sent.getBoundingClientRect();
            showContextMenu(rect.left + 10, rect.top + 10, sent.innerHTML, null, sid);
            sel.removeAllRanges();
        }
    });

    document.addEventListener('mouseup', function (e) {
        const sel = window.getSelection();
        if (!sel) return;
        if (sel.isCollapsed) return;
        const range = sel.getRangeAt(0);
        const container = document.createElement('div');
        container.appendChild(range.cloneContents());
        const html = container.innerHTML;
        if (!html) return;
        const rect = range.getBoundingClientRect();
        showContextMenu(rect.left + 10, rect.top + 10, html, null, null);
        sel.removeAllRanges();
    });

    async function flipCover(coverElementId) {
        const el = document.getElementById(coverElementId);
        if (!el) return;

        // Ensure element is visible
        el.classList.add('reader-cover');
        // Force reflow
        void el.offsetWidth;
        return new Promise(resolve => {
            const onEnd = (e) => {
                el.removeEventListener('transitionend', onEnd);
                resolve();
            };
            el.addEventListener('transitionend', onEnd);
            // Trigger flip
            el.classList.add('flip');
        });
    }

    function startReaderTimer(dotnetHelper) {
        dotnetRef = dotnetHelper;
        if (timer) clearInterval(timer);
        timer = setInterval(() => {
            // Notify .NET of elapsed seconds if needed
            if (dotnetRef) {
                try { dotnetRef.invokeMethodAsync('OnReaderTick'); } catch (e) { }
            }
        }, 1000);
    }

    function stopReaderTimer() {
        if (timer) { clearInterval(timer); timer = null; }
        dotnetRef = null;
    }

    function prevPage(storyId) { console.debug('prevPage', storyId); }
    function nextPage(storyId) { console.debug('nextPage', storyId); }

    return { init, onSentenceClicked, animateGhostFromElement, animateGhostBook, flipCover, fetchNotebooks, postPin, startReaderTimer, stopReaderTimer, prevPage, nextPage };
})();
