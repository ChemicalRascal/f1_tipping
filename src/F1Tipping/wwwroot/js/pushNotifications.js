window.addEventListener("serviceWorkerLoaded", InitPushNotificationSub);

var PushNotificationState = "unknown";

const pushApiPostUri = "api/PushSubscriptions";

async function InitPushNotificationSub(permission) {
    // See note on https://notifications.spec.whatwg.org/#dom-notification-permission
    if (permission === null) {
        permission = (await navigator.permissions.query({ name: "notifications" })).state;
    }
    if (permission === "denied") {
        PushNotificationState = "denied";
        return;
    }
    if (permission !== "granted") {
        return;
    }

    var sub = await getSubscription();
    if (sub === null) {
        sub = await makeSubscription();
    }

    var aRes = $.ajax({
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
    console.log(await aRes);
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

function bufferToString(buffer) {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}
