import {Component, Input, OnInit} from '@angular/core';
import {ClientContext} from '@cmi/lesesaal-web-core';

@Component({
	selector: 'cmi-viaduc-synonyme-quelle',
	templateUrl: 'searchSynonymeQuelleList.component.html',
	styleUrls: ['searchSynonymeQuelleList.component.less']
})

export class SearchSynonymeQuelleListComponent implements OnInit {
	constructor(public _context: ClientContext) {
	}

	@Input()
	public synonymeQuelleList: string[];

	public showQuelle: boolean;

	public ngOnInit(): void {
		this.showQuelle = false;
	}

	public toggleQuelle() {
		this.showQuelle = !this.showQuelle;
	}
}
