import { 
    DataSourceType, 
    IDataSourcesProps, 
    IconName, 
    IconStyle, 
    IThemeOptions 
} from '@microsoft/sharepointembedded-copilotchat-react';
import { IContainer } from "../../../common/schemas/ContainerSchemas";

export class ChatController {
    public static readonly instance = new ChatController();
    private constructor() { }

    public get dataSources(): IDataSourcesProps[] {
        const sources: IDataSourcesProps[] = [];

        for (const container of this._selectedContainers) {
            if (!container || !container.drive) {
                continue;
            }
            
            // Instead of embedding the URL directly, use the proper SharePoint Embedded approach
            // which uses the container's ID and other metadata
            sources.push({
                type: DataSourceType.DocumentLibrary,
                value: {
                    name: container.displayName,
                    // Don't use webUrl directly as it may trigger CSP errors
                    // Instead, reference by ID and let the SharePoint Embedded component handle access
                    url: this.getSharePointEmbeddedUrl(container)
                }
            });
        }

        return sources;
    }

    // Helper method to create a properly formatted URL for SharePoint Embedded
    private getSharePointEmbeddedUrl(container: IContainer): string {
        // Use container ID or other unique identifier rather than direct SharePoint URL
        // This approach avoids direct iframe embedding which triggers CSP errors
        // The actual format will depend on SharePoint Embedded's requirements
        return `spembed://container/${container.id}`;
    }

    private _dataSourceSubscribers: ((dataSources: IDataSourcesProps[]) => void)[] = [];
    public addDataSourceSubscriber(subscriber: (dataSources: IDataSourcesProps[]) => void) {
        this._dataSourceSubscribers.push(subscriber);
    }
    public removeDataSourceSubscriber(subscriber: (dataSources: IDataSourcesProps[]) => void) {
        this._dataSourceSubscribers = this._dataSourceSubscribers.filter(s => s !== subscriber);
    }

    private _selectedContainers: IContainer[] = [];
    public get selectedContainers(): IContainer[] {
        return this._selectedContainers;
    }
    public set selectedContainers(value: IContainer[]) { console.log(value);
        this._selectedContainers = value;
        this._dataSourceSubscribers.forEach(subscriber => subscriber(this.dataSources));
    }


    public readonly header = "SharePoint Embedded Chat";
    public readonly theme: IThemeOptions = {
        useDarkMode: false,
        customTheme: {
            themePrimary: '#4854EE',
            themeSecondary: '#4854EE',
            themeDark: '#4854EE',
            themeDarker: '#4854EE',
            themeTertiary: '#4854EE',
            themeLight: '#dddeef',
            themeDarkAlt: '#4854EE',
            themeLighter: '#dddeef',
            themeLighterAlt: '#dddeef',
            themeDarkAltTransparent: '#4854EE',
            themeLighterTransparent: '#dddeef',
            themeLighterAltTransparent: '#dddeef',
            themeMedium: '#4854EE',
            neutralSecondary: '#4854EE',
            neutralSecondaryAlt: '#4854EE',
            neutralTertiary: '#4854EE',
            neutralTertiaryAlt: '#4854EE',
            neutralQuaternary: '#4854EE',
            neutralQuaternaryAlt: '#4854EE',
            neutralPrimaryAlt: '#4854EE',
            neutralDark: '#4854EE',
            themeBackground: 'white',
        }
    };

    public readonly zeroQueryPrompts = {
        headerText: "SharePoint Embedded Chat: How can I help you today?",
        promptSuggestionList: [
            {
                suggestionText: 'Show me recent files',
                iconRegular: { name: IconName.ChatBubblesQuestion, style: IconStyle.Regular },
                iconHover: { name: IconName.ChatBubblesQuestion, style: IconStyle.Filled },
            },
            {
                suggestionText: 'What is SharePoint Embedded?',
                iconRegular: { name: IconName.DocumentCatchUp, style: IconStyle.Regular },
                iconHover: { name: IconName.DocumentCatchUp, style: IconStyle.Filled },
            }
        ]
    };

    public readonly suggestedPrompts = [
        "List and summarize recent files",
    ];

    public readonly pirateMetaPrompt = "Response must be in the tone of a pirate. Yarrr!";

    public readonly locale = "en";
}