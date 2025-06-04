import React, { useEffect } from "react";
import { ChatAuthProvider } from "../providers/ChatAuthProvider";
import { ChatController } from "../providers/ChatController";
import { ChatEmbedded, ChatEmbeddedAPI, ChatLaunchConfig } from '@microsoft/sharepointembedded-copilotchat-react';
import { IContainer } from '../../../common/schemas/ContainerSchemas';

interface ChatSidebarProps {
    container: IContainer;
}

export const ChatSidebar: React.FunctionComponent<ChatSidebarProps> = ({ container }) => {
    const [chatAuthProvider, setChatAuthProvider] = React.useState<ChatAuthProvider | undefined>();
    
    const [chatConfig] = React.useState<ChatLaunchConfig>({
        header: ChatController.instance.header,
        theme: ChatController.instance.theme,
        zeroQueryPrompts: ChatController.instance.zeroQueryPrompts,
        suggestedPrompts: ChatController.instance.suggestedPrompts,
        instruction: ChatController.instance.pirateMetaPrompt,
        locale: ChatController.instance.locale,
    });

    // Set up auth provider
    useEffect(() => {
        const setupAuthProvider = async () => {
            try {
                const provider = await ChatAuthProvider.getInstance();
                setChatAuthProvider(provider);
            } catch (error) {
                console.error("Error setting up auth provider:", error);
            }
        };
        
        setupAuthProvider();
    }, []);
    
    const onApiReady = async (api: ChatEmbeddedAPI) => {
        try {
            // Configure API before opening chat
            await api.openChat(chatConfig);
            
            // Add container as data source
            ChatController.instance.selectedContainers = [container];
            
            // Subscribe to data source changes
            ChatController.instance.addDataSourceSubscriber(dataSources => {
                api.setDataSources(dataSources);
            });
        } catch (error) {
            console.error("Error in ChatEmbedded onApiReady:", error);
        }
    }

    return (<>
    {chatAuthProvider && (
        <ChatEmbedded
            authProvider={chatAuthProvider}
            onApiReady={onApiReady}
            containerId={container.id}
        />
    )}
    </>);
}
