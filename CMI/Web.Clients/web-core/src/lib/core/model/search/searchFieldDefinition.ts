import {FieldType} from './fieldType';

export class SearchFieldDefinition {
	public type: FieldType;
	public key: string;
	public displayName: string;
	public items: SearchItem[];

	public constructor(type: FieldType, key?: string, displayName?: string, items?: SearchItem[]) {
		this.type = type;
		this.key = key;
		this.displayName = displayName;
		if (this.type === FieldType.Dropdown) {
			this.items = items;
		}
	}
}

export class SearchItem {
	public searchKey: string;
	public defaultLabel: string;
	public translationKey: string;

	public constructor(searchKey: string, defaultLabel: string, translationKey?: string) {
		this.searchKey = searchKey;
		this.defaultLabel = defaultLabel;
		this.translationKey = translationKey;
	}
}
