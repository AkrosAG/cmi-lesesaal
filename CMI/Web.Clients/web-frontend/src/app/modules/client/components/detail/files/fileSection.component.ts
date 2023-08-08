import {Component, Input, OnInit} from '@angular/core';
import {Entity} from '@cmi/lesesaal-web-core';

@Component({
	selector: 'cmi-viaduc-file-section',
	templateUrl: 'fileSection.component.html',
	styleUrls: ['./fileSection.component.less']
})
export class FileSectionComponent implements OnInit {

	@Input()
	public entity: Entity;
    public files: any[];

	constructor() {
	}

	public ngOnInit(): void {
        this.files = this.entity.files;
	}
}
