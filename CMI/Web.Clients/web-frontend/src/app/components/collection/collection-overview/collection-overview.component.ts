import {Component, Input, OnInit} from '@angular/core';
import {CollectionService} from '../../../modules/client/services/collection.service';
import {CollectionListItem, UiService} from '@cmi/lesesaal-web-core';
import {ICollectionListItem} from '@cmi/lesesaal-web-core/lib/core/model/collection/collectionListItem';

@Component({
	selector: 'cmi-collection-overview',
	templateUrl: './collection-overview.component.html',
	styleUrls: ['./collection-overview.component.less']
})
export class CollectionOverviewComponent implements OnInit {
	private _parentId: number;
	@Input()
	public  set parentId(newValue: number | null) {
		if (!isNaN(newValue)) {
			this._parentId = newValue;
			this.loadCollectionList();
		}
		this.isOnMainPage = this._parentId === null;
	}
	@Input()
	public headerText: string;
	@Input()
	public centerContent = false;
	public collections: ICollectionListItem[];
	public url: string;
	public isOnMainPage: boolean;

	constructor(private collectionService: CollectionService,
				private _ui: UiService) { }

	public ngOnInit(): void {
		if (this.isOnMainPage) {
			this.loadCollectionHeader();
		}
		this.loadCollectionList();
		this.url = this.collectionService.getBaseUrl;
	}

	private loadCollectionList(): void {
		if (!isNaN(this._parentId)) {
			this.collectionService.getActiveCollections(this._parentId).subscribe(
				res => this.prepareResult(res),
				err => this._ui.showError(err));
		}
	}

	private prepareResult(result: CollectionListItem[]) {
		this.collections = result;
	}

	private loadCollectionHeader() {
		this.collectionService.getCollectionHeader().subscribe(
			res => this.setHeader(res));
	}

	private setHeader(res: string) {
		this.headerText = res;
	}
}
