import {Injectable} from '@angular/core';
import {ClientContext, CoreOptions, HttpService} from '@cmi/viaduc-web-core';
import {INewsForOneLanguage} from '../model';

@Injectable()
export class NewsService {
	private _url: string;

	constructor(private _options: CoreOptions,
				private _context: ClientContext,
				private _http: HttpService) {
	}

	public async getRelevantNews(): Promise<INewsForOneLanguage[]> {
		this._url = this._createBaseUrl() + '/GetRelevantNews?lang=' + this._context.language;
		return await this._http.get<INewsForOneLanguage[]>(this._url, this._http.noCaching).toPromise();
	}

	private _createBaseUrl(): string {
		return this._options.serverUrl + this._options.publicPort + '/api/News';
	}
}
