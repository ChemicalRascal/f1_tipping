self.addEventListener('fetch', function (event) { });

self.addEventListener('push', function (event)
{
    event.waitUntil(
        self.registration.showNotification("F1 Tipping",
            {
                body: event.data.text(),
            }
        )
    );
});