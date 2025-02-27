import { Component } from '@angular/core';
import { TranslationService } from '@cmi/lesesaal-web-core';

@Component({
	selector: 'cmi-status-info-user',
	templateUrl: './status-info-user.component.html',
	styleUrls: ['./status-info-user.component.less']
})
export class StatusInfoUserComponent {

	constructor(private _txt: TranslationService) {
	}

	public getInfoMessage(): string {
		return this._txt.get('account.digitalOnboarding.statusInfo',
			// eslint-disable-next-line
			'Sofern Sie noch nicht den Status Identifiziert haben, schauen Sie bitte unter <a href=\"#/de/informationen/registrieren-und-identifizieren\">Registrieren und Identifizieren</a>.');
	}
}
