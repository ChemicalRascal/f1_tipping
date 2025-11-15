
var ButtonDataset = document.querySelector(".notification-toggler").dataset;
ButtonDataset.subState = "unknown";

const pushApiMainUrl = "api/PushSubscriptions";
const pushApiValidateUrl = "api/PushSubscriptions/validate";
const pushApiCheckUrl = "api/PushSubscriptions/userHasSub";

var serverExpectsSubscription = false;

window.addEventListener("serviceWorkerLoaded", async _ => InitPageData());
async function InitPageData() {
    await _updateState();
}

async function ProgressNotificationState() {
    if (ButtonDataset.subState == "subscription_successful") {
        var subData = await _getSubscriptionData();
        await _purgeSubscriptionFromServer(subData.sub);
        await _updateState();
    } else {
        const requestResult = await Notification.requestPermission();
        await RunSubscriptionProcess(requestResult);
    }
}

async function RunSubscriptionProcess() {
    ButtonDataset.subState = "pending_network";

    if (!(await _updateState())) {
        // We're not able to go on with the subscription process.
        return;
    }

    ButtonDataset.subState = "pending_network";
    try {
        var subData = await _getSubscriptionData() ?? await _makeSubscriptionData();
    }
    catch (e) {
        ButtonDataset.subState = "subscription_failed";
        console.log("Could not get or make subscription.", e);
        return;
    }

    if (subData.pushToServer) {
        try {
            await _postSubscriptionToServer(subData.sub);
            ButtonDataset.subState = "subscription_successful";
            serverExpectsSubscription = true;
        } catch (e) {
            console.log("Could not post subscription.", e);
            ButtonDataset.subState = "subscription_failed";
            serverExpectsSubscription = false;
            await _removeSubscription();
        }
    } else {
        ButtonDataset.subState = "subscription_successful";
        serverExpectsSubscription = true;
    }
};

/**
 * Sets subState on button depending on local state, and determines if we're good
 * to go ahead with creating a subscription.
 * 
 * @returns true if browser is good to create/post a subscription, false otherwise.
 */
async function _updateState() {
    ButtonDataset.subState = "pending_network";
    serverExpectsSubscription = await _checkSubscriptionExpected();

    // See note on https://notifications.spec.whatwg.org/#dom-notification-permission
    const permission = (await navigator.permissions.query({ name: "notifications" })).state;

    if (permission === "denied") {
        ButtonDataset.subState = "permission_denied";
        return false;
    }

    if (permission !== "granted") {
        ButtonDataset.subState = serverExpectsSubscription
            ? "expected_subscription" : "no_subscription";
        return false;
    }

    var subData = await _getSubscriptionData();
    if (subData === null) {
        ButtonDataset.subState = serverExpectsSubscription
            ? "expected_subscription" : "no_subscription";
        return true;
    }

    ButtonDataset.subState = "subscription_successful";
    return true;
}

async function _validateSubscription(sub) {
    var validationResult = await $.ajax({
        method: "GET",
        url: pushApiValidateUrl,
        data: { endpoint: sub.endpoint, },
    });
    return validationResult;
}

async function _checkSubscriptionExpected() {
    var checkResult = await $.ajax({
        method: "GET",
        url: pushApiCheckUrl,
    })
    return checkResult;
}

async function _getSubscriptionData() {
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

async function _makeSubscriptionData() {
    const registration = await navigator.serviceWorker.getRegistration();
    return {
        sub: await registration.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: vapidPublicKey }),
        pushToServer: true,
    };
}

async function _postSubscriptionToServer(sub) {
    return await $.ajax({
        method: "POST",
        url: pushApiMainUrl,
        data: JSON.stringify(
            {
                DeviceEndpoint: sub.endpoint,
                PublicKey: _bufferToString(sub.getKey("p256dh")),
                AuthSecret: _bufferToString(sub.getKey("auth")),
            }),
        contentType: "application/json; charset=utf-8",
    });
}

async function _purgeSubscriptionFromServer(sub) {
    return await $.ajax({
        method: "DELETE",
        url: pushApiMainUrl,
        data: JSON.stringify(
            {
                DeviceEndpoint: sub.endpoint,
                PublicKey: _bufferToString(sub.getKey("p256dh")),
                AuthSecret: _bufferToString(sub.getKey("auth")),
            }),
        contentType: "application/json; charset=utf-8",
    });
}

async function _triggerDebugNotification() {
    return await $.ajax({
        method: "HEAD",
        url: pushApiMainUrl,
    });
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
