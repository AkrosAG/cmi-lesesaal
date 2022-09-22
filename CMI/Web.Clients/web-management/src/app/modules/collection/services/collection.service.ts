import { Injectable } from '@angular/core';
import {Collection, CollectionListItem, CoreOptions, HttpService} from '@cmi/lesesaal-web-core';
import {map} from 'rxjs/operators';
import {Observable} from 'rxjs';

@Injectable()
export class CollectionService {
	private readonly _createBaseUrl: string;

	constructor(private _options: CoreOptions, private http: HttpService) {
		this._createBaseUrl = this._options.serverUrl + this._options.publicPort + '/api/Collections/';
	}

	public getAll(): Observable<CollectionListItem[] | null> {
		let url = this._createBaseUrl + 'GetAll';

		return this.http.get<CollectionListItem[]>(url, this.http.noCaching).pipe(map(arr =>  arr.map(item => CollectionListItem.fromJS(item))));
	}

	public getAllowedParents(currentItemId: number): Observable<any[] | null> {
		let url = this._createBaseUrl + 'GetPossibleParents/{id}';
		url = url.replace('{id}', '' + currentItemId);
		return this.http.get<any[]>(url, this.http.noCaching);
	}

	public get(id: number): Observable<Collection | null> {
		let url = this._createBaseUrl + 'Get/{id}';
		if (id === undefined || id === null) {
			throw new Error('The parameter ' + id + ' must be defined.');
		}
		url = url.replace('{id}', '' + id);
		return this.http.get(url, this.http.noCaching).pipe(map(r => Collection.fromJS(r)));
	}

	public create(value: Collection | null): Observable<any> {
		let url = this._createBaseUrl + 'Create';

		const content = value.toJSON();
		return this.http.post(url, content, this.http.noCaching);
	}

	public update(id: number, value: Collection | null): Observable<any> {
		let url = this._createBaseUrl + 'Update/{id}';
		if (id === undefined || id === null) {
			throw new Error('The parameter ' + id + ' must be defined.');
		}
		url = url.replace('{id}', '' + id);

		return this.http.put(url, value, this.http.noCaching);
	}

	public delete(id: number): Observable<any> {
		let url = this._createBaseUrl + 'Delete/{id}';
		if (id === undefined || id === null) {
			throw new Error('The parameter ' + id + ' must be defined.');
		}
		url = url.replace('{id}', '' + id);
		return this.http.delete(url);
	}

	public batchDelete(ids: number[]): Observable<any> {
		let url = this._createBaseUrl + 'BatchDelete';
		if (ids === undefined || ids.length === 0) {
			throw new Error('The parameter ' + ids + ' must be defined and contain values.');
		}
		return this.http.post(url, ids, this.http.noCaching);
	}

	public getImageURL(collectionId: number):string {
		let url = this._createBaseUrl + 'GetImage/{id}?usePrecalculatedThumbnail=true';
		if (collectionId === undefined || collectionId === null) {
			throw new Error('The parameter ' + collectionId + ' must be defined.');
		}
		url = url.replace('{id}', '' + collectionId);
		return url;
	}
}
