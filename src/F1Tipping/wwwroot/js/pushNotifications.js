window.addEventListener("serviceWorkerLoaded", async _ => InitPushNotificationSub(null));

var ButtonDataset = document.querySelector(".notification-toggler").dataset;
ButtonDataset.subState = "unknown";

const pushApiPostUri = "api/PushSubscriptions";
const pushApiValidateUrl = "api/PushSubscriptions/validate";

async function InitPushNotificationSub(permission) {
    // See note on https://notifications.spec.whatwg.org/#dom-notification-permission
    if (permission === null) {
        permission = (await navigator.permissions.query({ name: "notifications" })).state;
    }
    if (permission === "denied") {
        ButtonDataset.subState = "permission_denied";
        return;
    }
    if (permission !== "granted") {
        ButtonDataset.subState = "permission_pending";
        return;
    }

    var subData = await _getSubscription() ?? await _makeSubscription();

    if (subData.pushToServer) {
        try {
            var result = await $.ajax({
                method: "POST",
                url: pushApiPostUri,
                data: JSON.stringify(
                    {
                        DeviceEndpoint: subData.sub.endpoint,
                        PublicKey: _bufferToString(subData.sub.getKey("p256dh")),
                        AuthSecret: _bufferToString(subData.sub.getKey("auth")),
                    }),
                contentType: "application/json; charset=utf-8",
            });
            ButtonDataset.subState = "subscription_successful";
        } catch (e) {
            ButtonDataset.subState = "subscription_failed";
            await _removeSubscription();
        }
    } else {
        ButtonDataset.subState = "subscription_successful";
    }
};

async function RequestPushPermission() {
    const requestResult = await Notification.requestPermission();
    await InitPushNotificationSub(requestResult);
}

async function _getSubscription() {
    const registration = await navigator.serviceWorker.getRegistration();
    const sub = await registration.pushManager.getSubscription();
    if (sub === null) {
        return sub;
    }

    if (!(await _validateSubscription(sub))) {
        await _removeSubscription();
        return null;
    }

    return { sub: sub, pushToServer: false, };
}

async function _validateSubscription(sub) {
    var validationResult = await $.ajax({
        method: "GET",
        url: pushApiValidateUrl,
        data: { endpoint: sub.endpoint, },
    });
    return validationResult;
}

async function _makeSubscription() {
    const registration = await navigator.serviceWorker.getRegistration();
    return {
        sub: await registration.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: vapidPublicKey }),
        pushToServer: true,
    };
}

async function _removeSubscription() {
    const registration = await navigator.serviceWorker.getRegistration();
    (await registration.pushManager.getSubscription())?.unsubscribe();
}

function _bufferToString(buffer) {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}
