import {Ordering} from '@cmi/lesesaal-web-core';

export interface OrderCreationRequest extends Ordering {
	orderIdsToExclude: number[];
}
