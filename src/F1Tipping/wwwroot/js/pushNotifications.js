window.addEventListener("serviceWorkerLoaded", InitPushNotificationSub);

var PushNotificationState = "unknown";

const pushApiPostUri = "api/PushSubscriptions";

async function InitPushNotificationSub(permission) {
    // See note on https://notifications.spec.whatwg.org/#dom-notification-permission
    if (permission === null) {
        permission = (await navigator.permissions.query({ name: "notifications" })).state;
    }
    if (permission === "denied") {
        PushNotificationState = "permission_denied";
        return;
    }
    if (permission !== "granted") {
        PushNotificationState = "permission_pending";
        return;
    }

    var sub = await getSubscription() ?? await makeSubscription();

    try {
        var result = await $.ajax({
            type: "POST",
            url: pushApiPostUri,
            data: JSON.stringify(
                {
                    DeviceEndpoint: sub.endpoint,
                    PublicKey: bufferToString(sub.getKey("p256dh")),
                    AuthSecret: bufferToString(sub.getKey("auth")),
                }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
        });
        console.log("Push Notification subscription succeeded:", result.responseJSON.title, result.status);
        PushNotificationState = "subscription_successful";
    } catch (e) {
        console.log("Push Notification subscription failed:", e.responseJSON.title, e.status);
        PushNotificationState = "subscription_failed";
        await removeSubscription();
    }
};

async function RequestPushPermission() {
    const requestResult = await Notification.requestPermission();
    await InitPushNotificationSub(requestResult);
}

async function getSubscription() {
    const registration = await navigator.serviceWorker.getRegistration();
    return registration.pushManager.getSubscription();
}

async function makeSubscription() {
    const registration = await navigator.serviceWorker.getRegistration();
    return registration.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: vapidPublicKey });
}

async function removeSubscription() {
    return await registration.pushManager.removeSubscription();
}

function bufferToString(buffer) {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}
