self.addEventListener('fetch', function (event) { });

self.addEventListener('push', function (event)
{
    event.waitUntil(
        self.registration.showNotification("Push Notification",
            {
                body: event.data.text(),
            }
        )
    );
});