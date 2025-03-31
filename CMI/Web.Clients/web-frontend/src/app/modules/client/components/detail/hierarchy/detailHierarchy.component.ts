import {Component, Input} from '@angular/core';
import {Router} from '@angular/router';
import {UrlService} from '../../../services/url.service';
import {Entity} from '@cmi/lesesaal-web-core';

@Component({
	selector: 'cmi-viaduc-detail-hierarchy',
	templateUrl: 'detailHierarchy.component.html',
	styleUrls: ['./detailHierarchy.component.less']
})
export class DetailHierarchyComponent {
	@Input()
	public items: Entity[];

	public inset = 1;

	constructor(private _url: UrlService,
		private _router: Router) {
	}

	public open(item: Entity): void {
		this._router.navigate([this._url.getDetailUrl(item.archiveRecordId, item.title)]);
	}
}
