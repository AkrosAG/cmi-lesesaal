import {Component, Input, OnInit} from '@angular/core';
import {CoreOptions, Entity, ClientContext} from '@cmi/lesesaal-web-core';
import {AuthenticationService} from '../../../services/authentication.service';
import {ShoppingCartService} from '../../../services/shoppingCart.service';

@Component({
	selector: 'cmi-viaduc-file-section',
	templateUrl: 'fileSection.component.html',
	styleUrls: ['./fileSection.component.less']
})

export class FileSectionComponent implements OnInit {

	@Input()
	public entity: Entity;
	public files: any[];
	public showHasProtectedFiles: boolean = false;

	constructor(private _options: CoreOptions,
		private _context: ClientContext,
		private _authentication: AuthenticationService,
		private _scs: ShoppingCartService) {
	}

	public ngOnInit(): void {
		this.files = this.entity.files;
		this.showHasProtectedFiles = this.entity.hasProtectedFiles;
	}

	public getSymbolClass(extension: string): string {

		let cssClass: string;
		switch (extension) {
			case '.jpg':
				cssClass = 'fa-solid fa-file-image fa-2xl';
				break;
			case '.pdf':
				cssClass = 'fa-solid fa-file-pdf fa-2xl';
				break;
			case '.tif':
				cssClass = 'fa-solid fa-file-image fa-2xl';
				break;
			default:
				cssClass = 'fa-solid fa-file fa-2xl';
				break;
		}
		return cssClass;
	}

	public getFileUrl(name: string, download: boolean)	{
		const apiDataUrl = this._options.serverUrl + this._options.publicPort + '/api/File';
		const url = `${apiDataUrl}/GetMetadataFile?id=${this.entity.archiveRecordId}&name=${name}&download=${download}`;
		return url;
	}

	public orderVe(): void {
		this.loginIfNotAuthenticated();
	}

	public loginIfNotAuthenticated() {
		if (!this._context.authenticated) {
			this._authentication.login();
		} else {
			this._scs.addToCart(this.entity).subscribe();
		}
	}
}
