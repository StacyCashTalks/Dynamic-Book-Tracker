let webSocket = null;
let dotNetRef = null;

export function initialize(connectionUrl, userId, dotNetReference) {
    dotNetRef = dotNetReference;

    // Connect to Web PubSub
    webSocket = new WebSocket(connectionUrl);

    webSocket.onopen = () => {
        console.log('WebSocket connected');
    };

    webSocket.onmessage = (event) => {
        console.log('Notification received:', event.data);

        // Parse the message
        try {
            const message = JSON.parse(event.data);

            // Web PubSub sends different message types
            if (message.type === 'message' && message.from === 'server') {
                // This is a server message containing our notification
                dotNetRef.invokeMethodAsync('ReceiveNotification', message.data);
            } else if (typeof event.data === 'string' && event.data.startsWith('{')) {
                // Direct notification message
                dotNetRef.invokeMethodAsync('ReceiveNotification', event.data);
            }
        } catch (e) {
            console.error('Error processing notification:', e);
        }
    };

    webSocket.onerror = (error) => {
        console.error('WebSocket error:', error);
    };

    webSocket.onclose = () => {
        console.log('WebSocket disconnected');
        // Attempt to reconnect after 5 seconds
        setTimeout(() => {
            if (dotNetRef) {
                initialize(connectionUrl, userId, dotNetRef);
            }
        }, 5000);
    };
}

export function disconnect() {
    if (webSocket) {
        webSocket.close();
        webSocket = null;
    }
    dotNetRef = null;
}
