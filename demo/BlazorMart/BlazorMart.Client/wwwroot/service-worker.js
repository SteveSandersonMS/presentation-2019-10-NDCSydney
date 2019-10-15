const cacheName = 'offline-cache';
let handleAsOfflineUntil = 0;

self.addEventListener('install', async event => {
    console.log('Installing service worker...');
    await Promise.all((await caches.keys()).map(key => caches.delete(key)));
    await (await caches.open(cacheName)).addAll(['loading.gif']);
    self.skipWaiting();
});

self.addEventListener('fetch', event => {
    // Don't interfere with API calls
    if (event.request.method !== 'GET' || event.request.url.indexOf('/_framework/debug') >= 0) {
        return;
    }

    event.respondWith(getFromNetworkOrCache(event.request));
});
  
async function getFromNetworkOrCache(request) {
    if (new Date().valueOf() > handleAsOfflineUntil) {
        try {
            const networkResponse = await fetchWithTimeout(request, 1000);
            (await caches.open(cacheName)).put(request, networkResponse.clone());
            console.info('Fetched from network: ' + request.url);
            return networkResponse;
        } catch (ex) {
            handleAsOfflineUntil = new Date().valueOf() + 3000; // Next 3 seconds
        }
    }

    // Fall back on cache
    console.info('Fetching from cache: ' + request.url);
    return caches.match(request);
}

function fetchWithTimeout(request, timeoutMs) {
    return new Promise((resolve, reject) => {
        setTimeout(() => reject('Timed out'), timeoutMs);
        fetch(request).then(resolve, reject);
    });
}