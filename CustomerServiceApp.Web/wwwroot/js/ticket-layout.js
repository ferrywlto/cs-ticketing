// Ticket layout JavaScript functions for scrolling and UI enhancements

window.initializeTicketLayout = () => {
    console.log('Ticket layout initialized');
};

window.scrollMessagesToBottom = () => {
    const messagesContainer = document.querySelector('.ticket-messages-container');
    if (messagesContainer) {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }
};

window.scrollToBottomIfNearEnd = () => {
    const messagesContainer = document.querySelector('.ticket-messages-container');
    if (messagesContainer) {
        const threshold = 100; // pixels from bottom
        const isNearBottom = messagesContainer.scrollTop + messagesContainer.clientHeight >= messagesContainer.scrollHeight - threshold;
        
        if (isNearBottom) {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
    }
};
