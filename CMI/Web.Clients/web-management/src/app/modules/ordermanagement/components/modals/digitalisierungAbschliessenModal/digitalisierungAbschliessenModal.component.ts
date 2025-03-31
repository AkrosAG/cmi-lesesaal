import {Component, EventEmitter, Input, Output} from '@angular/core';
import {OrderService} from '../../../services';
import {ToastrService} from 'ngx-toastr';
import {ErrorService} from '../../../../shared/services';

@Component({
	selector: 'cmi-viaduc-digitalisierung-abschliessen-modal',
	templateUrl: 'digitalisierungAbschliessenModal.component.html',
	styleUrls: ['./digitalisierungAbschliessenModal.component.less']
})
export class DigitalisierungAbschliessenModalComponent {

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
					console.log('ctor');
	}

	public cancel() {
		this.open = false;
	}

	public ok() {
		this.isLoading = true;
		this._ord.digitalisierungAbschliessen(this.ids).subscribe(() => {
			const msg = `Die Digitalisierung wurde abgeschlossen.`;
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
