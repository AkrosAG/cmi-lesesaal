import {Component, EventEmitter, Input, Output} from '@angular/core';
import {OrderService} from '../../../services';
import {ToastrService} from 'ngx-toastr';
import {ErrorService} from '../../../../shared/services';

@Component({
	selector: 'cmi-viaduc-digitalisierung-starten-modal',
	templateUrl: 'digitalisierungStartenModal.component.html',
	styleUrls: ['./digitalisierungStartenModal.component.less']
})
export class DigitalisierungStartenModalComponent {

	@Input()
	public ids: number[] = [];
	@Input()
	public set open(val: boolean) {
		this._open = val;
		this.openChange.emit(val);
	}
	public get open(): boolean {
		return this._open;
	}

	@Output()
	public openChange: EventEmitter<boolean> = new EventEmitter<boolean>();
	@Output()
	public onSubmitted: EventEmitter<boolean> = new EventEmitter<boolean>();

	public isLoading = false;
	private _open = true;

	constructor(private _ord: OrderService,
				private _err: ErrorService,
				private _toastr: ToastrService) {
	}

	public cancel() {
		this.open = false;
	}

	public ok() {
		this.isLoading = true;
		this._ord.digitalisierungStarten(this.ids).subscribe(() => {
			const msg = `Die Digitalisierung wurde gestartet.`;
			this._toastr.success(msg, 'Erfolgreich', { timeOut: 5000});
				this.open = false;
				this.onSubmitted.emit(true);
				this.isLoading = false;
			}, (e) => {
				this._err.showError(e);
				this.isLoading = false;
		});
	}
}
