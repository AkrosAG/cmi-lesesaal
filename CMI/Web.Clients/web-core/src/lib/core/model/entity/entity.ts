import {EntityMetadata} from './entityMetadata';
import {Highlight} from '../search/highlight';
import {DetailData} from './detailData';

export interface Entity {
	_context?: any;
	_metadata?: EntityMetadata;
	highlight: Highlight;
	creationPeriod: any[];
	archiveRecordId: string;
	level?: string;
	title: string;
	treeSequence?: number;
	isAnonymized?: boolean;
	HasImage?: boolean;
	HasAudioVideo?: boolean;
	nichtOnlineRecherchierbareDossiers?: string;
	isWithinProtectionRange?: boolean;
	canBeOrdered?: boolean;
	isPhysicalyUsable?: boolean;
	containsPersonRelatedInformation?: boolean;
	detailData: DetailData[];
	permission: string;
	primaryDataLink?: any[];
	itemClasses?: string;
	iconClasses?: string;
	childCount?: number;
	referenceCode: string;
	isDownloadAllowed: boolean;
	images?: string[];
	primaryDataDownloadAccessTokens: string[];

	explanation: {
		value: string,
		explanation: string,
	};
}
