import { ParamMap} from '@angular/router';

export class MockUserSettingsParamMap implements ParamMap {
	public readonly keys: string[];
	// eslint-disable-next-line
	public get(name: string): string | null {
		return 'new';
	}
	// eslint-disable-next-line
	public getAll(name: string): string[] {
		return [];
	}
	// eslint-disable-next-line
	public has(name: string): boolean {
		return false;
	}
}
