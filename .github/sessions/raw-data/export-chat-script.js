// Export Copilot Chat History - VS Code Extension Script
// This script attempts to export the current chat session

const vscode = require('vscode');
const fs = require('fs');
const path = require('path');

/**
 * Attempts to export the current Copilot chat session
 * Run this from VS Code Developer Tools Console or as part of an extension
 */
async function exportCopilotChat() {
    try {
        // Attempt 1: Check if Copilot extension exposes chat API
        const copilotExt = vscode.extensions.getExtension('GitHub.copilot-chat');
        
        if (copilotExt && copilotExt.isActive) {
            console.log('✅ Copilot Chat extension found and active');
            
            // Check for exported API
            const api = copilotExt.exports;
            if (api && api.getChatHistory) {
                const chatHistory = await api.getChatHistory();
                return chatHistory;
            } else {
                console.log('⚠️ Copilot Chat API does not expose getChatHistory method');
            }
        }
        
        // Attempt 2: Access through workspace state (if available)
        const workspaceState = vscode.workspace.getConfiguration('github.copilot');
        console.log('Copilot config:', workspaceState);
        
        // Attempt 3: Command-based approach
        const commands = await vscode.commands.getCommands();
        const copilotCommands = commands.filter(cmd => 
            cmd.includes('copilot') && cmd.includes('chat')
        );
        console.log('Available Copilot commands:', copilotCommands);
        
        return null;
    } catch (error) {
        console.error('Error exporting chat:', error);
        return null;
    }
}

/**
 * Alternative: Capture visible chat panel content
 * Note: This requires VS Code API access from Developer Tools
 */
function captureVisibleChatContent() {
    // This would need to run in VS Code's webview context
    const chatPanel = document.querySelector('[class*="chat"]');
    if (chatPanel) {
        return chatPanel.innerText;
    }
    return null;
}

/**
 * Save chat history to file
 */
function saveChatHistory(content, outputPath) {
    const timestamp = new Date().toISOString().replace(/:/g, '-').split('.')[0];
    const filename = `copilot-chat-${timestamp}.json`;
    const fullPath = path.join(outputPath, filename);
    
    const data = {
        timestamp: new Date().toISOString(),
        session: 'TDD CalculationService Integration',
        project: 'interstellar-tracker',
        content: content
    };
    
    fs.writeFileSync(fullPath, JSON.stringify(data, null, 2), 'utf8');
    console.log(`✅ Chat history saved to: ${fullPath}`);
    
    return fullPath;
}

// Export for use in VS Code extension or developer console
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        exportCopilotChat,
        saveChatHistory
    };
}
