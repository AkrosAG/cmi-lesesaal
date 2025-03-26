import {NgModule} from '@angular/core';
import * as wjcCore from '@mescius/wijmo';
import * as wjcGrid from '@mescius/wijmo.angular2.grid';
import { WjGridModule } from '@mescius/wijmo.angular2.grid';
import { WjInputModule } from '@mescius/wijmo.angular2.input';
import { WjGridFilterModule } from '@mescius/wijmo.angular2.grid.filter';
import {WjGridGrouppanelModule} from '@mescius/wijmo.angular2.grid.grouppanel';
import { WjGridSearchModule } from '@mescius/wijmo.angular2.grid.search';
import {WjCoreModule} from '@mescius/wijmo.angular2.core';
import {ALL_SERVICES} from './services/_all';
import {ALL_COMPONENTS} from './components/_all';
import JSZip from 'jszip';
import {CommonModule} from '@angular/common';
window['JSZip'] = JSZip;
import {WIJMO_LICENSEKEY} from './wijmo.licensekey';

@NgModule({
	declarations: [ALL_COMPONENTS],
	imports: [
		CommonModule,
		WjInputModule,
		WjGridFilterModule,
		WjGridModule,
		WjGridSearchModule
	],
	exports: [
		WjInputModule,
		WjCoreModule,
		WjGridModule,
		WjGridFilterModule,
		WjGridGrouppanelModule,
		WjGridFilterModule,
		wjcGrid.WjFlexGridCellTemplate,
		...ALL_COMPONENTS
	],
	providers: [
		...ALL_SERVICES
	]
})

export class WijmoModule {
	constructor() {
		wjcCore.setLicenseKey(WIJMO_LICENSEKEY);
	}
}
