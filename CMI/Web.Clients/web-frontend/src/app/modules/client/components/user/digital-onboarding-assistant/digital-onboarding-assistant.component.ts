import { Component, OnInit, AfterViewInit } from '@angular/core';

@Component({
	selector: 'cmi-digital-onboarding-assistant',
	templateUrl: './digital-onboarding-assistant.component.html',
	styleUrls: ['./digital-onboarding-assistant.component.less']
})
export class DigitalOnboardingAssistantComponent implements OnInit, AfterViewInit {

	constructor() { }

	public ngOnInit() {
	}

	public ngAfterViewInit(): void {}

	public getUserMail(): string {
		return 'benutzer-admin@bar.admin.ch';
	}
}
