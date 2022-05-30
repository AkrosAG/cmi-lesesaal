export class CollectionListItem implements ICollectionListItem {
	public collectionId: number;
	public parentId?: number | undefined;
	public title?: string | undefined;
	public language?: string | undefined;
	public descriptionShort?: string | undefined;
	public description?: string | undefined;
	public validFrom: Date;
	public validTo: Date;
	public collectionTypeId: number;
	public imageAltText?: string | undefined;
	public imageMimeType?: string | undefined;
	public link?: string | undefined;
	public collectionPath?: string | undefined;
	public sortOrder: number;
	public createdOn: Date;
	public createdBy?: string | undefined;
	public modifiedOn?: Date | undefined;
	public modifiedBy?: string | undefined;
	public parent?: string | undefined;
	public childCollections?: string | undefined;

	constructor(data?: ICollectionListItem) {
		if (data) {
			for (let property in data) {
				if (data.hasOwnProperty(property)) {
					(<any>this)[property] = (<any>data)[property];
				}
			}
		}
	}

	public init(_data?: any) {
		if (_data) {
			this.collectionId = _data['collectionId'];
			this.parentId = _data['parentId'];
			this.title = _data['title'];
			this.language = _data['language'];
			this.descriptionShort = _data['descriptionShort'];
			this.description = _data['description'];
			this.validFrom = _data['validFrom'] ? new Date(_data['validFrom'].toString()) : <any>undefined;
			this.validTo = _data['validTo'] ? new Date(_data['validTo'].toString()) : <any>undefined;
			this.collectionTypeId = _data['collectionTypeId'];
			this.imageAltText = _data['imageAltText'];
			this.imageMimeType = _data['imageMimeType'];
			this.link = _data['link'];
			this.collectionPath = _data['collectionPath'];
			this.sortOrder = _data['sortOrder'];
			this.createdOn = _data['createdOn'] ? new Date(_data['createdOn'].toString()) : <any>undefined;
			this.createdBy = _data['createdBy'];
			this.modifiedOn = _data['modifiedOn'] ? new Date(_data['modifiedOn'].toString()) : <any>undefined;
			this.modifiedBy = _data['modifiedBy'];
			this.parent = _data['parent'];
			this.childCollections = _data['childCollections'];
		}
	}

	public static fromJS(data: any): CollectionListItem {
		data = typeof data === 'object' ? data : {};
		let result = new CollectionListItem();
		result.init(data);
		return result;
	}

	public toJSON(data?: any) {
		data = typeof data === 'object' ? data : {};
		data['collectionId'] = this.collectionId;
		data['parentId'] = this.parentId;
		data['title'] = this.title;
		data['language'] = this.language;
		data['descriptionShort'] = this.descriptionShort;
		data['description'] = this.description;
		data['validFrom'] = this.validFrom ? this.validFrom.toISOString() : <any>undefined;
		data['validTo'] = this.validTo ? this.validTo.toISOString() : <any>undefined;
		data['collectionTypeId'] = this.collectionTypeId;
		data['imageAltText'] = this.imageAltText;
		data['imageMimeType'] = this.imageMimeType;
		data['link'] = this.link;
		data['collectionPath'] = this.collectionPath;
		data['sortOrder'] = this.sortOrder;
		data['createdOn'] = this.createdOn ? this.createdOn.toISOString() : <any>undefined;
		data['createdBy'] = this.createdBy;
		data['modifiedOn'] = this.modifiedOn ? this.modifiedOn.toISOString() : <any>undefined;
		data['modifiedBy'] = this.modifiedBy;
		data['parent'] = this.parent;
		data['childCollections'] = this.childCollections;
		return data;
	}
}

export interface ICollectionListItem {
	collectionId: number;
	parentId?: number | undefined;
	title?: string | undefined;
	language?: string | undefined;
	descriptionShort?: string | undefined;
	description?: string | undefined;
	validFrom: Date;
	validTo: Date;
	collectionTypeId: number;
	imageAltText?: string | undefined;
	imageMimeType?: string | undefined;
	link?: string | undefined;
	collectionPath?: string | undefined;
	sortOrder: number;
	createdOn: Date;
	createdBy?: string | undefined;
	modifiedOn?: Date | undefined;
	modifiedBy?: string | undefined;
	parent?: string | undefined;
	childCollections?: string | undefined;
}
