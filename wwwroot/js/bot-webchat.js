class BotFrameworkWebChat {
    constructor() {
        this.isOpen = false;
        this.directLine = null;
        this.init();
    }

    async init() {
        this.setupEventListeners();
        await this.initializeWebChat();
    }

    setupEventListeners() {
        const toggleButton = document.getElementById('webchatToggle');
        const closeButton = document.getElementById('webchatClose');

        toggleButton?.addEventListener('click', () => this.toggleChat());
        closeButton?.addEventListener('click', () => this.closeChat());

        // Close on outside click
        document.addEventListener('click', (e) => {
            const container = document.getElementById('webchatContainer');
            const toggle = document.getElementById('webchatToggle');

            if (this.isOpen &&
                !container?.contains(e.target) &&
                !toggle?.contains(e.target)) {
                this.closeChat();
            }
        });
    }

    async initializeWebChat() {
        try {
            // Get DirectLine token
            const tokenResponse = await fetch('/api/directline/token', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!tokenResponse.ok) {
                throw new Error('Failed to get DirectLine token');
            }

            const { token } = await tokenResponse.json();

            // Create DirectLine connection
            this.directLine = window.WebChat.createDirectLine({ token });

            // Style options for Bot Framework Web Chat
            const styleOptions = {
                // Bot and user styling
                botAvatarImage: '',
                botAvatarInitials: '🤖',
                userAvatarImage: '',
                userAvatarInitials: '👤',

                // Color scheme
                accent: '#0f62fe',
                backgroundColor: '#f9f9f9',
                bubbleBackground: '#0f62fe',
                bubbleFromUserBackground: '#e0e0e0',
                bubbleFromUserTextColor: '#000',
                bubbleTextColor: '#fff',

                // Layout
                bubbleBorderRadius: 16,
                bubbleFromUserBorderRadius: 16,
                paddingWide: 16,
                paddingRegular: 12,

                // Hide certain elements
                hideUploadButton: true,
                hideScrollToEndButton: false,

                // Timestamp
                timestampColor: '#999',
                timestampFormat: 'relative'
            };

            // Activity middleware for custom handling
            const activityMiddleware = () => next => ({ activity, nextVisibleActivity }) => {
                // Add custom logic for activities if needed
                return next({ activity, nextVisibleActivity });
            };

            // Store customization
            const store = window.WebChat.createStore({}, ({ dispatch }) => next => action => {
                if (action.type === 'DIRECT_LINE/CONNECT_FULFILLED') {
                    // Send welcome event when connected
                    dispatch({
                        type: 'WEB_CHAT/SEND_EVENT',
                        payload: {
                            name: 'webchat/join',
                            value: { language: window.navigator.language }
                        }
                    });
                }
                return next(action);
            });

            // Render Web Chat
            window.WebChat.renderWebChat(
                {
                    directLine: this.directLine,
                    styleOptions: styleOptions,
                    store: store,
                    activityMiddleware: activityMiddleware
                },
                document.getElementById('webchat')
            );

            console.log('Bot Framework Web Chat initialized successfully');

        } catch (error) {
            console.error('Error initializing Web Chat:', error);
            this.showError('Failed to initialize chat. Please try again later.');
        }
    }

    toggleChat() {
        if (this.isOpen) {
            this.closeChat();
        } else {
            this.openChat();
        }
    }

    openChat() {
        this.isOpen = true;
        const container = document.getElementById('webchatContainer');
        const toggle = document.getElementById('webchatToggle');

        container?.classList.add('show');
        toggle?.classList.add('active');

        // Focus on Web Chat input
        setTimeout(() => {
            const input = document.querySelector('[data-testid="textbox"] input');
            input?.focus();
        }, 300);
    }

    closeChat() {
        this.isOpen = false;
        const container = document.getElementById('webchatContainer');
        const toggle = document.getElementById('webchatToggle');

        container?.classList.remove('show');
        toggle?.classList.remove('active');
    }

    showError(message) {
        const webchatElement = document.getElementById('webchat');
        if (webchatElement) {
            webchatElement.innerHTML = `
                <div style="padding: 20px; text-align: center; color: #666;">
                    <p>${message}</p>
                    <button onclick="location.reload()" class="btn btn-primary btn-sm">Retry</button>
                </div>
            `;
        }
    }

    // Method to programmatically send a message
    sendMessage(text) {
        if (this.directLine) {
            this.directLine.postActivity({
                from: { id: 'user', name: 'User' },
                type: 'message',
                text: text
            }).subscribe();
        }
    }
}

// Initialize when page loads
document.addEventListener('DOMContentLoaded', () => {
    window.botWebChat = new BotFrameworkWebChat();
});

// Global function for easy access
function openChatWithMessage(message) {
    if (window.botWebChat) {
        window.botWebChat.openChat();
        setTimeout(() => {
            window.botWebChat.sendMessage(message);
        }, 500);
    }
}