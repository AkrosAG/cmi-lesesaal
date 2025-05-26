import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import { StaticContentService} from '../../modules/client';

@Component({
	selector: 'cmi-benutzertabelle-static-page',
	templateUrl: 'staticPageBenutzertable.component.html'
})
export class StaticPageBenutzertableComponent implements OnInit, OnDestroy {
	@Input()
	public activeColumn: string;
	@Input()
	public contentUrl: string;

	public loadedHtml: string;
	private _navigationSubscription: any = null;

	private  activeHeader = 'id="header_${0}" class="col-header text-center"';
	private  activeRow =  'id="column_${0}_row_\\d" class="text-center';
	private  activeLastRow =  'id="column_${0}_row_last" class="text-center';

	constructor(private _static: StaticContentService) {
	}

	public ngOnInit(): void {
		this._loadContent();
	}

	private _loadContent() {
		const routeInfo = this._static.getStaticRouteInfo(this.contentUrl);
		const subscription = this._static.getContent(routeInfo.route)
			.subscribe(
				html => {
					this.activeHeader =  this.activeHeader.replace( '${0}', this.activeColumn);
					this.activeRow =  this.activeRow.replace( '${0}', this.activeColumn);
					this.activeLastRow =  this.activeLastRow.replace( '${0}', this.activeColumn);
					html = html.replace(this.activeHeader, 'class="active-col-header text-center"');
					html = html.replace(this.activeLastRow, 'class="active-col-row-last text-center"');
					const regExp = new RegExp(this.activeRow, 'g');
					html = html.replace(regExp, 'class="active-col-row text-center');
					this.loadedHtml = html;
					subscription.unsubscribe();
				},
				() => {
					this.loadedHtml = null;
					subscription.unsubscribe();
				}
			);
	}

	public ngOnDestroy(): void {
		if (this._navigationSubscription) {
			this._navigationSubscription.unsubscribe();
		}
		this._navigationSubscription = null;
	}
}
