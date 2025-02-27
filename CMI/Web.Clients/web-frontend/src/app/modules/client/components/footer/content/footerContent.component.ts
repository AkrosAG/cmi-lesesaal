import {Component, ElementRef, AfterViewInit} from '@angular/core';
import {Utilities as _util} from '@cmi/lesesaal-web-core';
import {ClientContext} from '@cmi/lesesaal-web-core';

@Component({
	selector: 'cmi-viaduc-footer-content',
	templateUrl: 'footerContent.component.html'
})
export class FooterContentComponent implements AfterViewInit {
	private _elem: any;

	constructor(private _context: ClientContext,
				private _elemRef: ElementRef) {
		this._elem = this._elemRef.nativeElement;
	}

	public ngAfterViewInit(): void {
		_util.initJQForElement(this._elem);
	}

	public get language(): string {
		return this._context.language;
	}

	public get versionInfo(): string {
		const v = this._context.client.version;
		return v ? `${v.major}.${v.minor}.${v.revision}.${v.build}` : void 0;
	}

}
