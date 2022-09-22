import {Paging} from '@cmi/lesesaal-web-core';

export interface PagedResult<T> {
	items: T[];
	paging: Paging;

	dynamicColumns: any[];
}

export interface DetailResult<T> {
	item: T;
}
