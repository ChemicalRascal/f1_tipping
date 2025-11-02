window.addEventListener("serviceWorkerLoaded", InitPushNotificationSub);

var PushNotificationState = "unknown";

async function InitPushNotificationSub(permission) {
    // See note on https://notifications.spec.whatwg.org/#dom-notification-permission
    if (permission === null) {
        const permission = (await navigator.permissions.query({ name: "notifications" })).state;
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
    console.log(sub);
    console.log(sub.endpoint);
    console.log("p256dh:", bufferToString(sub.getKey("p256dh")));
    console.log("auth:  ", bufferToString(sub.getKey("auth")));
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
