
export interface ICollection {
	collectionId: number;
	parentId?: number | undefined;
	language?: string | undefined;
	title?: string | undefined;
	descriptionShort?: string | undefined;
	description?: string | undefined;
	validFrom: Date;
	validTo: Date;
	collectionTypeId: number;
	image?: string | undefined;
	thumbnail?: string | undefined;
	imageAltText?: string | undefined;
	imageMimeType?: string | undefined;
	link?: string | undefined;
	collectionPath: string | undefined;
	sortOrder: number;
	createdOn: Date;
	createdBy?: string | undefined;
	modifiedOn?: Date | undefined;
	modifiedBy?: string | undefined;
	childCollections?: Collection[] | undefined;
	parent?: Collection | undefined;
}

export class Collection implements ICollection {
	public collectionId: number;
	public parentId?: number | undefined;
	public language?: string | undefined;
	public title?: string | undefined;
	public descriptionShort?: string | undefined;
	public description?: string | undefined;
	public validFrom!: Date;
	public validTo!: Date;
	public collectionTypeId!: number;
	public image?: string | undefined;
	public thumbnail?: string | undefined;
	public imageAltText?: string | undefined;
	public imageMimeType?: string | undefined;
	public link?: string | undefined;
	public collectionPath!: string | undefined;
	public sortOrder!: number;
	public createdOn!: Date;
	public createdBy?: string | undefined;
	public modifiedOn?: Date | undefined;
	public modifiedBy?: string | undefined;
	public childCollections?: Collection[] | undefined;
	public parent?: Collection | undefined;

	constructor(data?: ICollection) {
		if (data) {
			for (const property in data) {
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
			this.language = _data['language'];
			this.title = _data['title'];
			this.descriptionShort = _data['descriptionShort'];
			this.description = _data['description'];
			this.validFrom = _data['validFrom'] ? new Date(_data['validFrom'].toString()) : <any>undefined;
			this.validTo = _data['validTo'] ? new Date(_data['validTo'].toString()) : <any>undefined;
			this.collectionTypeId = _data['collectionTypeId'];
			this.image = _data['image'];
			this.thumbnail = _data['thumbnail'];
			this.imageAltText = _data['imageAltText'];
			this.imageMimeType = _data['imageMimeType'];
			this.link = _data['link'];
			this.collectionPath = _data['collectionPath'];
			this.sortOrder = _data['sortOrder'];
			this.createdOn = _data['createdOn'] ? new Date(_data['createdOn'].toString()) : <any>undefined;
			this.createdBy = _data['createdBy'];
			this.modifiedOn = _data['modifiedOn'] ? new Date(_data['modifiedOn'].toString()) : <any>undefined;
			this.modifiedBy = _data['modifiedBy'];
			if (Array.isArray(_data['childCollections'])) {
				this.childCollections = [] as any;
				for (const item of _data['childCollections']) {
					this.childCollections!.push(Collection.fromJS(item));
				}
			}
			this.parent = _data['parent'] ? Collection.fromJS(_data['parent']) : <any>undefined;
		}
	}

	public static fromJS(data: any): Collection {
		data = typeof data === 'object' ? data : {};
		const result = new Collection();
		result.init(data);
		return result;
	}

	public toJSON(data?: any) {
		data = typeof data === 'object' ? data : {};
		data['collectionId'] = this.collectionId;
		data['parentId'] = this.parentId;
		data['language'] = this.language;
		data['title'] = this.title;
		data['descriptionShort'] = this.descriptionShort;
		data['description'] = this.description;
		data['validFrom'] = this.validFrom ? this.validFrom.toISOString() : <any>undefined;
		data['validTo'] = this.validTo ? this.validTo.toISOString() : <any>undefined;
		data['collectionTypeId'] = this.collectionTypeId;
		data['image'] = this.image;
		data['thumbnail'] = this.thumbnail;
		data['imageAltText'] = this.imageAltText;
		data['imageMimeType'] = this.imageMimeType;
		data['link'] = this.link;
		data['collectionPath'] = this.collectionPath;
		data['sortOrder'] = this.sortOrder;
		data['createdOn'] = this.createdOn ? this.createdOn.toISOString() : <any>undefined;
		data['createdBy'] = this.createdBy;
		data['modifiedOn'] = this.modifiedOn ? this.modifiedOn.toISOString() : <any>undefined;
		data['modifiedBy'] = this.modifiedBy;
		if (Array.isArray(this.childCollections)) {
			data['childCollections'] = [];
			for (const item of this.childCollections) {
				data['childCollections'].push(item.toJSON());
			}
		}
		data['parent'] = this.parent ? this.parent.toJSON() : <any>undefined;
		return data;
	}
}
