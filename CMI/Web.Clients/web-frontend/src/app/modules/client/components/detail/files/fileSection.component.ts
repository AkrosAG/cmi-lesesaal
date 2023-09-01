import {Component, Input, OnInit} from '@angular/core';
import {CoreOptions, Entity} from '@cmi/lesesaal-web-core';

@Component({
	selector: 'cmi-viaduc-file-section',
	templateUrl: 'fileSection.component.html',
	styleUrls: ['./fileSection.component.less']
})

export class FileSectionComponent implements OnInit {

	@Input()
	public entity: Entity;
	public files: any[];

	constructor(private _options: CoreOptions) {
	}

	public ngOnInit(): void {
		this.files = this.entity.files;
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

	public getFileUrl(name: string)	{
		const apiDataUrl = this._options.serverUrl + this._options.publicPort + '/api/File';
		const url = `${apiDataUrl}/GetMetadataFile?id=${this.entity.archiveRecordId}&name=${name}`;
		return url;
	}
}
